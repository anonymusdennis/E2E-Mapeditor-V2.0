using System.Collections.Generic;
using System.Text;
using E2EApi.Editor;
using E2EApi.Persistence;
using UnityEngine;

namespace E2EApi.Features
{
    /// <summary>
    /// Animated modded tile placements: multi-frame stamps cut from harvested
    /// atlases, cycling at a configurable FPS. Placements are persisted in the
    /// Level.e2e sidecar (section [animated_tiles]), one line per placement.
    ///
    /// Line format (bottom-left pixel origin for every rect):
    ///   x,y,layer,mode,fps,loop,pingpong[,rot:angle]|atlas:rx:ry:rw:rh[;atlas:rx:ry:rw:rh ...]
    /// where mode = 'f' (floor) or 'd' (decor), loop/pingpong = '1'/'0', 'p'/'n'.
    /// </summary>
    public static class AnimatedModTiles
    {
        private const string SectionName = "animated_tiles";

        // ---- data types ----

        public class AnimFrame
        {
            public string Atlas;
            public int Rx, Ry, Rw, Rh;
        }

        public class AnimatedPlacement
        {
            public int X, Y, Layer;
            public bool Decor;
            public float Rotation;
            public float Fps;
            public bool Loop;
            public bool PingPong;
            public List<AnimFrame> Frames = new List<AnimFrame>();

            public int WTiles => Frames.Count > 0
                ? (Frames[0].Rw + TileSets.TilePixels - 1) / TileSets.TilePixels : 1;
            public int HTiles => Frames.Count > 0
                ? (Frames[0].Rh + TileSets.TilePixels - 1) / TileSets.TilePixels : 1;

            public bool Covers(int x, int y, int layer)
            {
                return layer == Layer &&
                    x >= X && x < X + WTiles &&
                    y >= Y && y < Y + HTiles;
            }

            public string Serialize()
            {
                var sb = new StringBuilder();
                sb.Append(X).Append(',').Append(Y).Append(',').Append(Layer).Append(',');
                sb.Append(Decor ? 'd' : 'f').Append(',');
                sb.Append(Fps.ToString("0.##",
                    System.Globalization.CultureInfo.InvariantCulture)).Append(',');
                sb.Append(Loop ? '1' : '0').Append(',');
                sb.Append(PingPong ? 'p' : 'n');
                if (Mathf.Abs(Rotation) > 0.001f)
                {
                    sb.Append(",rot:").Append(
                        Rotation.ToString("G",
                            System.Globalization.CultureInfo.InvariantCulture));
                }
                sb.Append('|');
                for (int i = 0; i < Frames.Count; i++)
                {
                    if (i > 0) sb.Append(';');
                    var f = Frames[i];
                    sb.Append(f.Atlas).Append(':')
                      .Append(f.Rx).Append(':').Append(f.Ry).Append(':')
                      .Append(f.Rw).Append(':').Append(f.Rh);
                }
                return sb.ToString();
            }

            public static AnimatedPlacement Parse(string line)
            {
                int pipeIdx = line.IndexOf('|');
                if (pipeIdx < 0) return null;

                string header = line.Substring(0, pipeIdx);
                string framesStr = line.Substring(pipeIdx + 1);
                string[] hparts = header.Split(',');
                if (hparts.Length < 7) return null;

                var p = new AnimatedPlacement();
                if (!int.TryParse(hparts[0], out p.X) ||
                    !int.TryParse(hparts[1], out p.Y) ||
                    !int.TryParse(hparts[2], out p.Layer))
                {
                    return null;
                }
                p.Decor = hparts[3] == "d";
                float fps;
                p.Fps = float.TryParse(hparts[4],
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out fps)
                    ? fps : 4f;
                p.Loop = hparts[5] == "1";
                p.PingPong = hparts[6] == "p";
                // optional rotation token in positions 7+
                for (int i = 7; i < hparts.Length; i++)
                {
                    if (hparts[i].StartsWith("rot:"))
                    {
                        float rot;
                        if (float.TryParse(hparts[i].Substring(4),
                            System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out rot))
                        {
                            p.Rotation = rot;
                        }
                    }
                }

                // parse semicolon-separated frames: atlas:rx:ry:rw:rh
                foreach (var fstr in framesStr.Split(';'))
                {
                    if (string.IsNullOrEmpty(fstr)) continue;
                    string[] fparts = fstr.Split(':');
                    if (fparts.Length < 5) continue;
                    int rx, ry, rw, rh;
                    if (!int.TryParse(fparts[fparts.Length - 4], out rx) ||
                        !int.TryParse(fparts[fparts.Length - 3], out ry) ||
                        !int.TryParse(fparts[fparts.Length - 2], out rw) ||
                        !int.TryParse(fparts[fparts.Length - 1], out rh))
                    {
                        continue;
                    }
                    string atlas = string.Join(":", fparts, 0, fparts.Length - 4);
                    p.Frames.Add(new AnimFrame
                    {
                        Atlas = atlas, Rx = rx, Ry = ry, Rw = rw, Rh = rh
                    });
                }
                if (p.Frames.Count == 0) return null;
                return p;
            }
        }

        // ---- state ----

        private static readonly List<AnimatedPlacement> Placements =
            new List<AnimatedPlacement>();
        private static bool _initialised;

        /// <summary>Bumped on every change; the overlay rebuilds on it.</summary>
        public static int Version { get; private set; }

        public static int Count => Placements.Count;

        public static IEnumerable<AnimatedPlacement> All() => Placements;

        // ---- lifecycle ----

        public static void Initialise()
        {
            if (_initialised) return;
            _initialised = true;
            ModExtras.EnsurePatched();
            ModExtras.Saving += OnSaving;
            ModExtras.Loaded += OnLoaded;
            ApiRunner.Ensure();
        }

        // ---- mutation ----

        /// <summary>
        /// Place (or replace) an animated tile at a map position. Existing
        /// placements with the same anchor, mode, and layer are replaced.
        /// </summary>
        public static void Place(int x, int y, int layer, bool decor,
            float fps, bool loop, bool pingPong,
            List<AnimFrame> frames, float rotation = 0f)
        {
            Initialise();
            if (frames == null || frames.Count == 0) return;
            Placements.RemoveAll(p =>
                p.X == x && p.Y == y && p.Layer == layer && p.Decor == decor);
            Placements.Add(new AnimatedPlacement
            {
                X = x, Y = y, Layer = layer, Decor = decor,
                Fps = Mathf.Max(fps, 0.1f),
                Loop = loop, PingPong = pingPong,
                Frames = new List<AnimFrame>(frames),
                Rotation = rotation,
            });
            Version++;
        }

        /// <summary>Topmost animated placement covering the tile, or null.</summary>
        public static AnimatedPlacement GetAt(int x, int y, int layer)
        {
            for (int i = Placements.Count - 1; i >= 0; i--)
            {
                if (Placements[i].Covers(x, y, layer))
                    return Placements[i];
            }
            return null;
        }

        /// <summary>Remove every animated placement covering the tile. Returns count removed.</summary>
        public static int EraseAt(int x, int y, int layer)
        {
            int removed = Placements.RemoveAll(p => p.Covers(x, y, layer));
            if (removed > 0) Version++;
            return removed;
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
                foreach (var f in p.Frames)
                {
                    if (!TileSets.HasAtlas(f.Atlas) && !missing.Contains(f.Atlas))
                        missing.Add(f.Atlas);
                }
            }
            return missing;
        }

        // ---- persistence ----

        private static void OnSaving(ModExtras extras)
        {
            extras.ClearSection(SectionName);
            if (Placements.Count == 0) return;
            var section = extras.Section(SectionName);
            foreach (var p in Placements)
                section.Add(p.Serialize());
            extras.RequiresMod = true;
        }

        private static void OnLoaded(ModExtras extras)
        {
            Placements.Clear();
            foreach (var line in extras.Section(SectionName))
            {
                var p = AnimatedPlacement.Parse(line);
                if (p != null) Placements.Add(p);
            }
            Version++;
            if (Placements.Count > 0)
                Log.Info("animated mod tiles: " + Placements.Count + " placement(s) loaded");
        }
    }
}
