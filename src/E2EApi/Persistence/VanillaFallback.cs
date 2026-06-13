using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;

namespace E2EApi.Persistence
{
    /// <summary>
    /// Vanilla-player fallback for maps that need this mod.
    ///
    /// When a map with mod content is saved as finished (the version vanilla
    /// players download and play), the real Level_Finished.dat is stowed
    /// base64-encoded in the Level.e2e sidecar ([realmap] section) and the
    /// file on disk is replaced by a "disclaimer variant": the same map with
    /// "NEEDS E2E MAPEDITOR MOD - SEE WORKSHOP PAGE" painted onto the ground
    /// floor in tiles. The variant is guaranteed playable because it IS the
    /// real (checker-validated) map — only the modded overlays are missing,
    /// which the painted message explains. A fabricated empty map could miss
    /// zones the game needs at runtime, so this is deliberately safer than
    /// the literal "empty dummy map".
    ///
    /// Modded clients reverse the swap transparently: when the game resolves
    /// a custom level file for play, the sidecar next to it is checked for
    /// [realmap] and the original bytes are served from a scratch file
    /// instead. The same hook also loads the sidecar itself, so fences,
    /// triggers and modded tiles work for maps downloaded from the Workshop
    /// (which never go through SaveManager.LoadTheLevel).
    /// </summary>
    public static class VanillaFallback
    {
        /// <summary>Master switch (config "VanillaFallbackMap", default on).</summary>
        public static bool Enabled = true;

        /// <summary>Painted on the ground floor of the disclaimer variant.</summary>
        public static string DisclaimerText = "NEEDS E2E\nMAPEDITOR\nMOD";

        private const string RealMapSection = "realmap";
        private const string ScratchFileName = "Level_Real.e2etmp";

        // Level.dat constants (see docs/leveldat-format.md)
        private const byte ChunkEnd = 0x66;
        private const byte FlagInstructions = 201;
        private const byte FinishedBlob = 100;
        private const byte OpDrawOnce = 101;
        private const byte OpChangeLayer = 103;
        private const byte BodyXorKey = 71;   // LevelType.Finished
        private const byte BodySeed = 15;
        private const byte ChecksumXor = 0x5F;

        /// <summary>Install the save/load/upload hooks (idempotent).</summary>
        public static void Initialise() => PatchRegistry.EnsurePatched(typeof(Patches));

        // ---- save side: stash real map, write disclaimer variant ----

        /// <summary>
        /// True when the current extras carry actual mod content (ignoring a
        /// previously stowed real map).
        /// </summary>
        private static bool HasModContent(ModExtras extras)
        {
            foreach (var name in new[] { "fences", "triggers", "tiles" })
            {
                if (extras.Section(name).Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        private static void AfterFinishedSave(string directory)
        {
            string finishedPath = directory + "Level_Finished.dat";
            string sidecarPath = directory + ModExtras.FileName;
            var extras = ModExtras.Current;
            if (!Enabled || !HasModContent(extras))
            {
                // no fallback wanted: make sure no stale stowed map survives
                if (extras.Section(RealMapSection).Count > 0)
                {
                    extras.ClearSection(RealMapSection);
                    if (extras.IsEmpty)
                    {
                        if (File.Exists(sidecarPath))
                        {
                            File.Delete(sidecarPath);
                        }
                    }
                    else
                    {
                        File.WriteAllText(sidecarPath, extras.Serialize());
                    }
                }
                return;
            }
            byte[] real = File.ReadAllBytes(finishedPath);
            byte[] variant;
            try
            {
                variant = BuildDisclaimerVariant(real);
            }
            catch (Exception e)
            {
                Log.Error("VanillaFallback: building disclaimer variant failed, " +
                    "keeping the real map: " + e);
                return;
            }
            var section = extras.Section(RealMapSection);
            section.Clear();
            section.Add(Convert.ToBase64String(real));
            extras.RequiresMod = true;
            File.WriteAllText(sidecarPath, extras.Serialize());
            File.WriteAllBytes(finishedPath, variant);
            Log.Info("VanillaFallback: Level_Finished.dat is now the disclaimer variant (" +
                variant.Length + " bytes, real map " + real.Length + " bytes in sidecar)");
        }

        // ---- load side: restore the real map for modded clients ----

        private static string SwapInRealMap(string finishedPath)
        {
            try
            {
                string dir = Path.GetDirectoryName(finishedPath);
                var extras = ModExtras.Current;
                var section = extras.Section(RealMapSection);
                if (section.Count == 0)
                {
                    return finishedPath;
                }
                byte[] real = Convert.FromBase64String(string.Concat(section.ToArray()));
                string scratch = Path.Combine(dir, ScratchFileName);
                File.WriteAllBytes(scratch, real);
                Log.Info("VanillaFallback: serving real map (" + real.Length +
                    " bytes) instead of the disclaimer variant");
                return scratch;
            }
            catch (Exception e)
            {
                Log.Error("VanillaFallback: real-map swap failed: " + e);
                return finishedPath;
            }
        }

        // ---- disclaimer variant builder ----

        /// <summary>
        /// Copy of a finished Level.dat with the disclaimer painted onto the
        /// ground floor (extra Draw_Once instructions appended to the
        /// FinishedLevelInstructions blob).
        /// </summary>
        internal static byte[] BuildDisclaimerVariant(byte[] file)
        {
            // locate the LevelInstructions_V1 chunk
            int pos = 0;
            int chunkStart = -1;
            int chunkLen = 0;
            while (pos + 5 <= file.Length)
            {
                byte flag = file[pos];
                int len = ReadInt(file, pos + 1);
                if (flag == FlagInstructions)
                {
                    chunkStart = pos;
                    chunkLen = len;
                    break;
                }
                pos += 5 + len;
            }
            if (chunkStart < 0)
            {
                throw new InvalidDataException("no LevelInstructions_V1 chunk");
            }

            // decrypt body (everything except the trailing ChunkEnd)
            int bodyStart = chunkStart + 5;
            int bodyLen = chunkLen - 1;
            var body = new byte[bodyLen];
            byte rolling = BodySeed;
            for (int i = 0; i < bodyLen; i++)
            {
                byte c = file[bodyStart + i];
                body[i] = (byte)((c ^ BodyXorKey) - rolling);
                rolling = c;
            }

            // body = checksum:i32 + FinishedLevelInstructions blob
            if (body.Length < 14 || body[4] != FinishedBlob)
            {
                throw new InvalidDataException("unexpected finished body layout");
            }
            int blobLen = ReadInt(body, 5); // bytes after the len field, incl. ChunkEnd
            int count = ReadInt(body, 9);
            int instrEnd = 9 + blobLen - 1; // index of the blob's ChunkEnd byte
            if (instrEnd >= body.Length || body[instrEnd] != ChunkEnd)
            {
                throw new InvalidDataException("finished blob terminator not found");
            }

            // build the extra instructions: switch to ground floor, draw text
            var extra = new List<byte>();
            extra.Add(OpChangeLayer);
            extra.Add(1); // LevelLayers.GroundFloor
            int added = 1;
            int tileId = ResolveDisclaimerTileId();
            foreach (var cell in TextTiles(DisclaimerText))
            {
                extra.Add(OpDrawOnce);
                extra.Add((byte)cell.Key);
                extra.Add((byte)cell.Value);
                AddInt(tileId, extra);
                AddInt(0, extra); // seed
                added++;
            }

            // splice: bigger blob, updated count/lengths, fresh checksum
            var newBody = new List<byte>(body.Length + extra.Count);
            newBody.AddRange(new byte[4]); // checksum placeholder
            newBody.Add(FinishedBlob);
            AddInt(blobLen + extra.Count, newBody);
            AddInt(count + added, newBody);
            for (int i = 13; i < instrEnd; i++)
            {
                newBody.Add(body[i]);
            }
            newBody.AddRange(extra);
            for (int i = instrEnd; i < body.Length; i++)
            {
                newBody.Add(body[i]); // blob ChunkEnd + anything after it
            }
            int checksum = 0;
            for (int i = 4; i < newBody.Count; i++)
            {
                checksum += newBody[i] ^ ChecksumXor;
            }
            WriteInt(newBody, 0, checksum);

            // re-encrypt and reassemble the file
            var result = new List<byte>(file.Length + extra.Count);
            for (int i = 0; i < chunkStart; i++)
            {
                result.Add(file[i]);
            }
            result.Add(FlagInstructions);
            AddInt(newBody.Count + 1, result);
            rolling = BodySeed;
            for (int i = 0; i < newBody.Count; i++)
            {
                byte c = (byte)((newBody[i] + rolling) ^ BodyXorKey);
                result.Add(c);
                rolling = c;
            }
            result.Add(ChunkEnd);
            int tailStart = chunkStart + 5 + chunkLen;
            for (int i = tailStart; i < file.Length; i++)
            {
                result.Add(file[i]);
            }
            return result.ToArray();
        }

        /// <summary>
        /// A ground-floor tile block used to paint the disclaimer. Prefers
        /// visually loud tiles, falls back to the first valid one.
        /// </summary>
        private static int ResolveDisclaimerTileId()
        {
            // plain ground tiles only — "special" tiles like water/lava spawn
            // region renderers at play time (a misplaced water pool covered
            // the map in a giant broken-material quad during testing)
            string[] preferred = { "sand", "tarmac", "concrete", "path", "dirt" };
            BuildingBlock_Tile fallback = null;
            foreach (var tile in UnityEngine.Resources.FindObjectsOfTypeAll<BuildingBlock_Tile>())
            {
                if (tile == null || tile.m_ID < 0 || tile.m_EditorOnly ||
                    !tile.IsValidForLayer(BaseLevelManager.LevelLayers.GroundFloor))
                {
                    continue;
                }
                if (fallback == null)
                {
                    fallback = tile;
                }
                string name = tile.name.ToLowerInvariant();
                foreach (var hint in preferred)
                {
                    if (name.Contains(hint))
                    {
                        return tile.m_ID;
                    }
                }
            }
            if (fallback == null)
            {
                throw new InvalidDataException("no ground tile available for the disclaimer");
            }
            return fallback.m_ID;
        }

        // ---- 3x5 tile font ----

        private static readonly Dictionary<char, int> Font = new Dictionary<char, int>
        {
            // 15-bit glyphs, rows top→bottom, 3 bits per row (msb = left)
            { 'A', Glyph("010,101,111,101,101") }, { 'B', Glyph("110,101,110,101,110") },
            { 'C', Glyph("011,100,100,100,011") }, { 'D', Glyph("110,101,101,101,110") },
            { 'E', Glyph("111,100,110,100,111") }, { 'F', Glyph("111,100,110,100,100") },
            { 'G', Glyph("011,100,101,101,011") }, { 'H', Glyph("101,101,111,101,101") },
            { 'I', Glyph("111,010,010,010,111") }, { 'J', Glyph("001,001,001,101,010") },
            { 'K', Glyph("101,110,100,110,101") }, { 'L', Glyph("100,100,100,100,111") },
            { 'M', Glyph("101,111,111,101,101") }, { 'N', Glyph("101,111,111,111,101") },
            { 'O', Glyph("010,101,101,101,010") }, { 'P', Glyph("110,101,110,100,100") },
            { 'Q', Glyph("010,101,101,011,001") }, { 'R', Glyph("110,101,110,110,101") },
            { 'S', Glyph("011,100,010,001,110") }, { 'T', Glyph("111,010,010,010,010") },
            { 'U', Glyph("101,101,101,101,011") }, { 'V', Glyph("101,101,101,010,010") },
            { 'W', Glyph("101,101,111,111,101") }, { 'X', Glyph("101,010,010,010,101") },
            { 'Y', Glyph("101,101,010,010,010") }, { 'Z', Glyph("111,001,010,100,111") },
            { '0', Glyph("010,101,101,101,010") }, { '1', Glyph("010,110,010,010,111") },
            { '2', Glyph("110,001,010,100,111") }, { '3', Glyph("110,001,010,001,110") },
            { '4', Glyph("101,101,111,001,001") }, { '5', Glyph("111,100,110,001,110") },
            { '6', Glyph("011,100,110,101,010") }, { '7', Glyph("111,001,010,010,010") },
            { '8', Glyph("010,101,010,101,010") }, { '9', Glyph("010,101,011,001,110") },
            { '!', Glyph("010,010,010,000,010") }, { '.', Glyph("000,000,000,000,010") },
            { '-', Glyph("000,000,111,000,000") },
        };

        private static int Glyph(string rows)
        {
            int bits = 0;
            foreach (char c in rows)
            {
                if (c == '0' || c == '1')
                {
                    bits = (bits << 1) | (c - '0');
                }
            }
            return bits;
        }

        /// <summary>Tile coordinates spelling out the text, centred on the map.</summary>
        private static IEnumerable<KeyValuePair<int, int>> TextTiles(string text)
        {
            string[] lines = text.Replace("\r", "").Split('\n');
            const int charW = 4;  // 3 glyph + 1 space
            const int lineH = 7;  // 5 glyph + 2 space
            int totalH = lines.Length * lineH;
            int topY = 60 + totalH / 2; // map y grows downward visually; centre block
            for (int li = 0; li < lines.Length; li++)
            {
                string line = lines[li].ToUpperInvariant();
                int width = line.Length * charW;
                int startX = Math.Max(1, 60 - width / 2);
                int baseY = topY - li * lineH;
                for (int ci = 0; ci < line.Length; ci++)
                {
                    int glyph;
                    if (!Font.TryGetValue(line[ci], out glyph))
                    {
                        continue; // space and unknown chars
                    }
                    for (int row = 0; row < 5; row++)
                    {
                        for (int col = 0; col < 3; col++)
                        {
                            int bit = 14 - (row * 3 + col);
                            if ((glyph >> bit & 1) == 0)
                            {
                                continue;
                            }
                            int x = startX + ci * charW + col;
                            int y = baseY - row;
                            if (x > 0 && x < 119 && y > 0 && y < 119)
                            {
                                yield return new KeyValuePair<int, int>(x, y);
                            }
                        }
                    }
                }
            }
        }

        // ---- byte helpers ----

        private static int ReadInt(byte[] data, int offset)
        {
            return data[offset] | (data[offset + 1] << 8) |
                (data[offset + 2] << 16) | (data[offset + 3] << 24);
        }

        private static int ReadInt(List<byte> data, int offset)
        {
            return data[offset] | (data[offset + 1] << 8) |
                (data[offset + 2] << 16) | (data[offset + 3] << 24);
        }

        private static void AddInt(int value, List<byte> data)
        {
            data.Add((byte)value);
            data.Add((byte)(value >> 8));
            data.Add((byte)(value >> 16));
            data.Add((byte)(value >> 24));
        }

        private static void WriteInt(List<byte> data, int offset, int value)
        {
            data[offset] = (byte)value;
            data[offset + 1] = (byte)(value >> 8);
            data[offset + 2] = (byte)(value >> 16);
            data[offset + 3] = (byte)(value >> 24);
        }

        // ---- Harmony integration ----

        [HarmonyPatch]
        private static class Patches
        {
            private static string DirectoryFor(SaveManager manager)
            {
                if (manager == null || PlatformIO.GetInstance() == null)
                {
                    return null;
                }
                string owner = manager.GetOwnerName(manager.m_iCurrentPrison);
                return string.IsNullOrEmpty(owner)
                    ? null : PlatformIO.GetInstance().GetPath(owner);
            }

            /// <summary>Finished save → stash real map + write disclaimer variant.</summary>
            [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.SaveUserLevel))]
            [HarmonyPostfix]
            private static void AfterSaveUserLevel(SaveManager __instance, bool __result,
                bool bIsFinishedVersion)
            {
                if (!__result || !bIsFinishedVersion)
                {
                    return;
                }
                try
                {
                    string dir = DirectoryFor(__instance);
                    if (dir != null)
                    {
                        AfterFinishedSave(dir);
                    }
                }
                catch (Exception e)
                {
                    Log.Error("VanillaFallback save hook failed: " + e);
                }
            }

            /// <summary>
            /// Play-mode level resolution: pick up the sidecar from the map's
            /// folder (covers Workshop downloads) and serve the real map when
            /// the file on disk is the disclaimer variant.
            /// </summary>
            [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.GetCustomLevelFilePath))]
            [HarmonyPostfix]
            private static void AfterGetCustomLevelFilePath(ref string __result,
                bool bWithoutFinal)
            {
                if (string.IsNullOrEmpty(__result))
                {
                    return;
                }
                try
                {
                    string dir = Path.GetDirectoryName(__result);
                    ModExtras.LoadFromDirectory(dir);
                    if (!bWithoutFinal)
                    {
                        __result = SwapInRealMap(__result);
                    }
                }
                catch (Exception e)
                {
                    Log.Error("VanillaFallback load hook failed: " + e);
                }
            }

            /// <summary>Workshop upload staging: include the sidecar.</summary>
            [HarmonyPatch(typeof(SteamPlatform), nameof(SteamPlatform.UploadUGCItem))]
            [HarmonyPrefix]
            private static void BeforeUploadUGCItem(Platform.UGCUploadData uploadData)
            {
                try
                {
                    if (uploadData == null ||
                        uploadData.m_ugcType != Platform.UGCType.eCustomLevel ||
                        string.IsNullOrEmpty(uploadData.m_strContentPath) ||
                        !Directory.Exists(uploadData.m_strContentPath))
                    {
                        return;
                    }
                    string parent = Path.GetDirectoryName(
                        uploadData.m_strContentPath.TrimEnd('/', '\\'));
                    string sidecar = Path.Combine(parent, ModExtras.FileName);
                    if (File.Exists(sidecar))
                    {
                        File.Copy(sidecar,
                            Path.Combine(uploadData.m_strContentPath, ModExtras.FileName),
                            overwrite: true);
                        Log.Info("VanillaFallback: staged " + ModExtras.FileName +
                            " for Workshop upload");
                    }
                }
                catch (Exception e)
                {
                    Log.Error("VanillaFallback upload hook failed: " + e);
                }
            }
        }
    }
}
