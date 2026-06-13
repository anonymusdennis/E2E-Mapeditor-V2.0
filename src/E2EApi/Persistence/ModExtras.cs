using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HarmonyLib;

namespace E2EApi.Persistence
{
    /// <summary>
    /// Mod-extras sidecar for custom maps: a "Level.e2e" file stored next to
    /// the vanilla Level.dat. Vanilla never touches it, so maps stay 100%
    /// vanilla-compatible; modded clients get the extra content back.
    ///
    /// Format (UTF-8 text):
    ///   E2EX1
    ///   requiresMod=true|false
    ///   [sectionName]
    ///   one line per entry (section-defined syntax)
    /// </summary>
    public class ModExtras
    {
        public const string FileName = "Level.e2e";
        public const string Magic = "E2EX1";

        /// <summary>Extras of the currently loaded/edited custom map.</summary>
        public static ModExtras Current { get; private set; } = new ModExtras();

        /// <summary>Fired before saving — features should write their sections now.</summary>
        public static event Action<ModExtras> Saving;

        /// <summary>Fired after a sidecar (or its absence) was processed on load.</summary>
        public static event Action<ModExtras> Loaded;

        /// <summary>If true, the map advertises that it needs this mod to play.</summary>
        public bool RequiresMod;

        private readonly Dictionary<string, List<string>> _sections =
            new Dictionary<string, List<string>>();

        public List<string> Section(string name)
        {
            List<string> lines;
            if (!_sections.TryGetValue(name, out lines))
            {
                lines = new List<string>();
                _sections[name] = lines;
            }
            return lines;
        }

        public void ClearSection(string name) => _sections.Remove(name);

        public bool IsEmpty
        {
            get
            {
                if (RequiresMod)
                {
                    return false;
                }
                foreach (var pair in _sections)
                {
                    if (pair.Value.Count > 0)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private static bool _editorHooked;

        internal static void EnsurePatched()
        {
            PatchRegistry.EnsurePatched(typeof(SaveLoadPatches));
            if (!_editorHooked)
            {
                _editorHooked = true;
                // the editor loads its map straight from GlobalStart's custom
                // level file (no SaveManager involved), so pick up the sidecar
                // whenever an editor session starts
                Events.GameEvents.EditorEntered += OnEditorEntered;
            }
        }

        private static void OnEditorEntered()
        {
            try
            {
                var globalStart = GlobalStart.GetInstance();
                string file = globalStart != null ? globalStart.m_strCustomLevelFile : null;
                if (!string.IsNullOrEmpty(file))
                {
                    LoadFromDirectory(Path.GetDirectoryName(file));
                }
                else
                {
                    // brand-new map: start from a clean slate
                    Current = new ModExtras();
                    var handler = Loaded;
                    if (handler != null)
                    {
                        handler(Current);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("ModExtras editor-entry load failed: " + e);
            }
        }

        /// <summary>
        /// Load the sidecar of an arbitrary map directory (e.g. a Workshop
        /// item folder) into <see cref="Current"/> and notify features. Used
        /// when a map is loaded for play, which bypasses LoadTheLevel.
        /// </summary>
        public static void LoadFromDirectory(string directory)
        {
            try
            {
                string path = Path.Combine(directory, FileName);
                Current = File.Exists(path)
                    ? Deserialize(File.ReadAllText(path))
                    : new ModExtras();
                if (File.Exists(path))
                {
                    Log.Info($"mod extras loaded ← {path}");
                }
                var handler = Loaded;
                if (handler != null)
                {
                    handler(Current);
                }
            }
            catch (Exception e)
            {
                Log.Error("ModExtras directory load failed: " + e);
            }
        }

        // ---- serialization ----

        public string Serialize()
        {
            var sb = new StringBuilder();
            sb.AppendLine(Magic);
            sb.AppendLine("requiresMod=" + (RequiresMod ? "true" : "false"));
            foreach (var pair in _sections)
            {
                if (pair.Value.Count == 0)
                {
                    continue;
                }
                sb.AppendLine("[" + pair.Key + "]");
                foreach (var line in pair.Value)
                {
                    sb.AppendLine(line);
                }
            }
            return sb.ToString();
        }

        public static ModExtras Deserialize(string text)
        {
            var extras = new ModExtras();
            if (string.IsNullOrEmpty(text))
            {
                return extras;
            }
            string[] lines = text.Replace("\r\n", "\n").Split('\n');
            if (lines.Length == 0 || lines[0].Trim() != Magic)
            {
                Log.Warn("ModExtras: bad magic, ignoring sidecar");
                return extras;
            }
            List<string> current = null;
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Length == 0)
                {
                    continue;
                }
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    current = extras.Section(line.Substring(1, line.Length - 2));
                }
                else if (line.StartsWith("requiresMod="))
                {
                    extras.RequiresMod = line.Substring("requiresMod=".Length) == "true";
                }
                else if (current != null)
                {
                    current.Add(line);
                }
            }
            return extras;
        }

        // ---- save/load integration ----

        [HarmonyPatch]
        private static class SaveLoadPatches
        {
            private static string DirectoryFor(SaveManager manager)
            {
                if (manager == null || PlatformIO.GetInstance() == null)
                {
                    return null;
                }
                string owner = manager.GetOwnerName(manager.m_iCurrentPrison);
                if (string.IsNullOrEmpty(owner))
                {
                    return null;
                }
                return PlatformIO.GetInstance().GetPath(owner);
            }

            [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.SaveUserLevel))]
            [HarmonyPostfix]
            private static void AfterSaveUserLevel(SaveManager __instance, bool __result, bool bIsFinishedVersion)
            {
                if (!__result || bIsFinishedVersion)
                {
                    return; // write the sidecar once, alongside Level.dat
                }
                try
                {
                    var handler = Saving;
                    if (handler != null)
                    {
                        handler(Current);
                    }
                    string dir = DirectoryFor(__instance);
                    if (dir == null)
                    {
                        return;
                    }
                    string path = dir + FileName;
                    if (Current.IsEmpty)
                    {
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                        return;
                    }
                    File.WriteAllText(path, Current.Serialize());
                    Log.Info($"mod extras saved → {path}");
                }
                catch (Exception e)
                {
                    Log.Error("ModExtras save failed: " + e);
                }
            }

            [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LoadTheLevel))]
            [HarmonyPostfix]
            private static void AfterLoadTheLevel(SaveManager __instance, bool __result)
            {
                try
                {
                    Current = new ModExtras();
                    if (__result)
                    {
                        string dir = DirectoryFor(__instance);
                        string path = dir != null ? dir + FileName : null;
                        if (path != null && File.Exists(path))
                        {
                            Current = Deserialize(File.ReadAllText(path));
                            Log.Info($"mod extras loaded ← {path}");
                        }
                    }
                    var handler = Loaded;
                    if (handler != null)
                    {
                        handler(Current);
                    }
                }
                catch (Exception e)
                {
                    Log.Error("ModExtras load failed: " + e);
                }
            }
        }
    }
}
