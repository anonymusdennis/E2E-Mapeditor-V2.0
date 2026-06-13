using System.Collections.Generic;
using E2EApi.Editor;
using E2EApi.Persistence;

namespace E2EApi.Features
{
    /// <summary>
    /// Modded tile placements: rectangular "stamps" cut from harvested
    /// atlases (see <see cref="TileSets"/>) painted onto the map grid.
    /// Placements are rendered as sprite overlays in the editor and in play
    /// mode, and persisted in the Level.e2e sidecar (section [tiles], lines
    /// "x,y,layer,mode,rx,ry,rw,rh,atlasName"). Atlases are referenced by
    /// stable asset name, never by index, so maps survive machine moves —
    /// a client that hasn't harvested the source set yet sees placeholder
    /// markers and a warning instead of broken art.
    /// </summary>
    public static class ModTiles
    {
        private const string SectionName = "tiles";

        public class Placement
        {
            public int X;          // anchor tile (bottom-left of the stamp)
            public int Y;
            public int Layer;      // 0..5 building layer
            public bool Decor;     // false = under characters, true = above
            public string Atlas;   // atlas asset name
            public int Rx, Ry, Rw, Rh; // atlas pixel rect, bottom-left origin
            public float Rotation; // clockwise degrees (0 = no rotation)

            public int WTiles => (Rw + TileSets.TilePixels - 1) / TileSets.TilePixels;
            public int HTiles => (Rh + TileSets.TilePixels - 1) / TileSets.TilePixels;

            public bool Covers(int x, int y, int layer)
            {
                return layer == Layer &&
                    x >= X && x < X + WTiles && y >= Y && y < Y + HTiles;
            }

            public string Serialize()
            {
                string line = X + "," + Y + "," + Layer + "," + (Decor ? "d" : "f") + "," +
                    Rx + "," + Ry + "," + Rw + "," + Rh + "," + Atlas;
                if (Rotation != 0f)
                {
                    line += ",rot:" + Rotation.ToString("G", System.Globalization.CultureInfo.InvariantCulture);
                }
                return line;
            }

            public static Placement Parse(string line)
            {
                string[] parts = line.Split(',');
                if (parts.Length < 9)
                {
                    return null;
                }
                var p = new Placement();
                if (!int.TryParse(parts[0], out p.X) || !int.TryParse(parts[1], out p.Y) ||
                    !int.TryParse(parts[2], out p.Layer) ||
                    !int.TryParse(parts[4], out p.Rx) || !int.TryParse(parts[5], out p.Ry) ||
                    !int.TryParse(parts[6], out p.Rw) || !int.TryParse(parts[7], out p.Rh))
                {
                    return null;
                }
                p.Decor = parts[3] == "d";
                // Detect optional trailing rotation token "rot:<value>" (backward-compatible)
                int atlasEnd = parts.Length;
                if (atlasEnd > 8 && parts[atlasEnd - 1].StartsWith("rot:"))
                {
                    float rot;
                    if (float.TryParse(parts[atlasEnd - 1].Substring(4),
                        System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out rot))
                    {
                        p.Rotation = rot;
                    }
                    atlasEnd--;
                }
                p.Atlas = string.Join(",", parts, 8, atlasEnd - 8);
                return p;
            }
        }

        private static readonly List<Placement> Placements = new List<Placement>();
        private static bool _initialised;

        /// <summary>Bumped on every change; the overlay rebuilds on it.</summary>
        public static int Version { get; private set; }

        /// <summary>True while atlases referenced by the loaded map are being read.</summary>
        public static bool Preloading { get; private set; }

        /// <summary>Atlas preload progress for UI.</summary>
        public static int PreloadCurrent { get; private set; }
        public static int PreloadTotal { get; private set; }

        public static int Count => Placements.Count;

        public static IEnumerable<Placement> All() => Placements;

        public static void Initialise()
        {
            if (_initialised)
            {
                return;
            }
            _initialised = true;
            ModExtras.EnsurePatched();
            ModExtras.Saving += OnSaving;
            ModExtras.Loaded += OnLoaded;
            ApiRunner.Ensure();
        }

        /// <summary>
        /// Stamp an atlas region at a tile. An identical-anchor placement of
        /// the same mode on the same layer is replaced.
        /// </summary>
        public static void Place(int x, int y, int layer, bool decor,
            string atlas, int rx, int ry, int rw, int rh, float rotation = 0f)
        {
            Initialise();
            Placements.RemoveAll(p =>
                p.X == x && p.Y == y && p.Layer == layer && p.Decor == decor);
            Placements.Add(new Placement
            {
                X = x,
                Y = y,
                Layer = layer,
                Decor = decor,
                Atlas = atlas,
                Rx = rx,
                Ry = ry,
                Rw = rw,
                Rh = rh,
                Rotation = rotation,
            });
            Version++;
        }

        /// <summary>Return the topmost placement covering the tile, or null.</summary>
        public static Placement GetAt(int x, int y, int layer)
        {
            for (int i = Placements.Count - 1; i >= 0; i--)
            {
                if (Placements[i].Covers(x, y, layer))
                {
                    return Placements[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Rotate every placement covering the tile by <paramref name="delta"/> degrees.
        /// Returns how many placements were rotated.
        /// </summary>
        public static int RotateAt(int x, int y, int layer, float delta)
        {
            int count = 0;
            foreach (var p in Placements)
            {
                if (p.Covers(x, y, layer))
                {
                    p.Rotation = NormalizeAngle(p.Rotation + delta);
                    count++;
                }
            }
            if (count > 0)
            {
                Version++;
            }
            return count;
        }

        /// <summary>Rotate a specific placement by <paramref name="delta"/> degrees.</summary>
        public static void Rotate(Placement placement, float delta)
        {
            placement.Rotation = NormalizeAngle(placement.Rotation + delta);
            Version++;
        }

        private static float NormalizeAngle(float angle)
        {
            angle %= 360f;
            if (angle < 0f) angle += 360f;
            return angle;
        }

        /// <summary>Remove every placement covering the tile. Returns how many.</summary>
        public static int EraseAt(int x, int y, int layer)
        {
            int removed = Placements.RemoveAll(p => p.Covers(x, y, layer));
            if (removed > 0)
            {
                Version++;
            }
            return removed;
        }

        /// <summary>Any placement covering the tile (topmost wins)?</summary>
        public static bool HasTileAt(int x, int y, int layer)
        {
            foreach (var p in Placements)
            {
                if (p.Covers(x, y, layer))
                {
                    return true;
                }
            }
            return false;
        }

        public static void Clear()
        {
            if (Placements.Count > 0)
            {
                Placements.Clear();
                Version++;
            }
        }

        /// <summary>Atlases referenced by the map but missing from the local cache.</summary>
        public static List<string> MissingAtlases()
        {
            var missing = new List<string>();
            foreach (var p in Placements)
            {
                if (!TileSets.HasAtlas(p.Atlas) && !missing.Contains(p.Atlas))
                {
                    missing.Add(p.Atlas);
                }
            }
            return missing;
        }

        private static void OnSaving(ModExtras extras)
        {
            extras.ClearSection(SectionName);
            if (Placements.Count == 0)
            {
                return;
            }
            var section = extras.Section(SectionName);
            foreach (var p in Placements)
            {
                section.Add(p.Serialize());
            }
            extras.RequiresMod = true;
        }

        private static void OnLoaded(ModExtras extras)
        {
            Placements.Clear();
            foreach (var line in extras.Section(SectionName))
            {
                var p = Placement.Parse(line);
                if (p != null)
                {
                    Placements.Add(p);
                }
            }
            Version++;
            if (Placements.Count > 0)
            {
                Log.Info("mod tiles: " + Placements.Count + " placement(s) loaded");
                var missing = MissingAtlases();
                if (missing.Count > 0)
                {
                    Log.Warn("mod tiles: missing atlases (harvest the matching sets): " +
                        string.Join(", ", missing.ToArray()));
                }
                ApiRunner.StartRoutine(PreloadAtlases());
            }
        }

        /// <summary>
        /// Read every atlas the map references into GPU memory, one per frame,
        /// so the loading overlay can show progress and the first render
        /// doesn't hitch.
        /// </summary>
        private static System.Collections.IEnumerator PreloadAtlases()
        {
            var names = new List<string>();
            foreach (var p in Placements)
            {
                if (!names.Contains(p.Atlas) && TileSets.HasAtlas(p.Atlas))
                {
                    names.Add(p.Atlas);
                }
            }
            if (names.Count == 0)
            {
                yield break;
            }
            Preloading = true;
            PreloadCurrent = 0;
            PreloadTotal = names.Count;
            foreach (var name in names)
            {
                TileSets.GetAtlasTexture(name);
                PreloadCurrent++;
                yield return null;
            }
            Preloading = false;
        }
    }
}
