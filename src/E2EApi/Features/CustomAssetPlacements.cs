using System;
using System.Collections.Generic;
using E2EApi.Editor;
using E2EApi.Events;
using E2EApi.Persistence;
using UnityEngine;

namespace E2EApi.Features
{
    /// <summary>
    /// Tracks custom AssetBundle prefab placements in a map. Each placement
    /// records the bundle filename (relative to the bundles folder), the
    /// prefab asset name and the tile coordinate. Placements are spawned as
    /// GameObjects when the editor is entered and destroyed on exit.
    ///
    /// Sidecar section: [custom_assets]
    /// Line format: bundleName|assetName|x,y,layer[|off:dx,dy,dz][|rot:ry]
    /// </summary>
    public static class CustomAssetPlacements
    {
        private const string SectionName = "custom_assets";

        // ---- data type ----

        public class Placement
        {
            public string BundleName;  // filename only, e.g. "myassets.bundle"
            public string AssetName;   // prefab name inside the bundle
            public int X, Y, Layer;
            /// <summary>Sub-tile world-space offset (default 0,0,0).</summary>
            public float OffX, OffY, OffZ;
            /// <summary>Y-axis rotation in degrees (default 0).</summary>
            public float RotY;

            public string Serialize()
            {
                string line = BundleName + "|" + AssetName + "|" +
                    X + "," + Y + "," + Layer;
                if (OffX != 0f || OffY != 0f || OffZ != 0f)
                {
                    line += "|off:" +
                        OffX.ToString("G", System.Globalization.CultureInfo.InvariantCulture) + "," +
                        OffY.ToString("G", System.Globalization.CultureInfo.InvariantCulture) + "," +
                        OffZ.ToString("G", System.Globalization.CultureInfo.InvariantCulture);
                }
                if (RotY != 0f)
                {
                    line += "|rot:" +
                        RotY.ToString("G", System.Globalization.CultureInfo.InvariantCulture);
                }
                return line;
            }

            public static Placement Parse(string line)
            {
                string[] parts = line.Split('|');
                if (parts.Length < 3)
                {
                    return null;
                }
                var p = new Placement();
                p.BundleName = parts[0];
                p.AssetName = parts[1];
                string[] coords = parts[2].Split(',');
                if (coords.Length < 3 ||
                    !int.TryParse(coords[0], out p.X) ||
                    !int.TryParse(coords[1], out p.Y) ||
                    !int.TryParse(coords[2], out p.Layer))
                {
                    return null;
                }
                for (int i = 3; i < parts.Length; i++)
                {
                    string token = parts[i];
                    if (token.StartsWith("off:"))
                    {
                        string[] off = token.Substring(4).Split(',');
                        if (off.Length == 3)
                        {
                            float.TryParse(off[0], System.Globalization.NumberStyles.Float,
                                System.Globalization.CultureInfo.InvariantCulture, out p.OffX);
                            float.TryParse(off[1], System.Globalization.NumberStyles.Float,
                                System.Globalization.CultureInfo.InvariantCulture, out p.OffY);
                            float.TryParse(off[2], System.Globalization.NumberStyles.Float,
                                System.Globalization.CultureInfo.InvariantCulture, out p.OffZ);
                        }
                    }
                    else if (token.StartsWith("rot:"))
                    {
                        float.TryParse(token.Substring(4), System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out p.RotY);
                    }
                }
                return p;
            }
        }

        // ---- state ----

        private static readonly List<Placement> _placements = new List<Placement>();
        private static readonly Dictionary<string, GameObject> _instances =
            new Dictionary<string, GameObject>();
        private static bool _initialised;

        /// <summary>Bumped on every list change.</summary>
        public static int Version { get; private set; }

        public static int Count => _placements.Count;

        public static IEnumerable<Placement> All() => _placements;

        // ---- lifecycle ----

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
            GameEvents.EditorEntered += SpawnAll;
            GameEvents.EditorExited += DestroyAll;
            ApiRunner.Ensure();
        }

        // ---- CRUD ----

        /// <summary>
        /// Place a custom asset at a tile. If a placement with the same
        /// bundle+asset+coord already exists it is replaced.
        /// </summary>
        public static void Place(string bundleName, string assetName,
            int x, int y, int layer,
            float offX = 0f, float offY = 0f, float offZ = 0f, float rotY = 0f)
        {
            Initialise();
            _placements.RemoveAll(p =>
                p.X == x && p.Y == y && p.Layer == layer);
            var placement = new Placement
            {
                BundleName = bundleName,
                AssetName = assetName,
                X = x,
                Y = y,
                Layer = layer,
                OffX = offX,
                OffY = offY,
                OffZ = offZ,
                RotY = rotY,
            };
            _placements.Add(placement);
            Version++;
            if (GameEvents.IsEditorActive)
            {
                DestroyInstanceAt(x, y, layer);
                SpawnOne(placement);
            }
        }

        /// <summary>Remove any placement at the tile and destroy its instance.</summary>
        public static int EraseAt(int x, int y, int layer)
        {
            DestroyInstanceAt(x, y, layer);
            int removed = _placements.RemoveAll(p =>
                p.X == x && p.Y == y && p.Layer == layer);
            if (removed > 0)
            {
                Version++;
            }
            return removed;
        }

        public static Placement GetAt(int x, int y, int layer)
        {
            for (int i = _placements.Count - 1; i >= 0; i--)
            {
                var p = _placements[i];
                if (p.X == x && p.Y == y && p.Layer == layer)
                {
                    return p;
                }
            }
            return null;
        }

        public static void Clear()
        {
            DestroyAll();
            if (_placements.Count > 0)
            {
                _placements.Clear();
                Version++;
            }
        }

        // ---- spawn / destroy ----

        /// <summary>Instantiate all placements in the editor scene.</summary>
        public static void SpawnAll()
        {
            DestroyAll();
            foreach (var p in _placements)
            {
                SpawnOne(p);
            }
        }

        /// <summary>Destroy all live instances (editor exit / clear).</summary>
        public static void DestroyAll()
        {
            foreach (var pair in _instances)
            {
                if (pair.Value != null)
                {
                    UnityEngine.Object.Destroy(pair.Value);
                }
            }
            _instances.Clear();
        }

        private static string InstanceKey(int x, int y, int layer)
            => x + "_" + y + "_" + layer;

        private static void DestroyInstanceAt(int x, int y, int layer)
        {
            string key = InstanceKey(x, y, layer);
            GameObject go;
            if (_instances.TryGetValue(key, out go) && go != null)
            {
                UnityEngine.Object.Destroy(go);
            }
            _instances.Remove(key);
        }

        private static void SpawnOne(Placement p)
        {
            try
            {
                string bundlePath = System.IO.Path.Combine(
                    CustomAssets.BundlesFolder, p.BundleName);
                Vector3? center = Grid.TileToWorld(p.X, p.Y, p.Layer);
                if (!center.HasValue)
                {
                    Log.Warn($"CustomAssetPlacements: no world pos for tile " +
                        $"({p.X},{p.Y},{p.Layer}) — skipping spawn of {p.AssetName}");
                    return;
                }
                Vector3 pos = center.Value + new Vector3(p.OffX, p.OffY, p.OffZ);
                var go = CustomAssets.Spawn(bundlePath, p.AssetName, pos);
                if (go == null)
                {
                    Log.Warn($"CustomAssetPlacements: spawn failed for " +
                        $"{p.BundleName}/{p.AssetName} — bundle missing or wrong Unity version?");
                    return;
                }
                if (p.RotY != 0f)
                {
                    go.transform.rotation = Quaternion.Euler(0f, p.RotY, 0f);
                }
                _instances[InstanceKey(p.X, p.Y, p.Layer)] = go;
            }
            catch (Exception e)
            {
                Log.Error("CustomAssetPlacements.SpawnOne failed: " + e);
            }
        }

        // ---- sidecar integration ----

        private static void OnSaving(ModExtras extras)
        {
            extras.ClearSection(SectionName);
            if (_placements.Count == 0)
            {
                return;
            }
            var section = extras.Section(SectionName);
            foreach (var p in _placements)
            {
                section.Add(p.Serialize());
            }
            extras.RequiresMod = true;
        }

        private static void OnLoaded(ModExtras extras)
        {
            DestroyAll();
            _placements.Clear();
            foreach (var line in extras.Section(SectionName))
            {
                var p = Placement.Parse(line);
                if (p != null)
                {
                    _placements.Add(p);
                }
            }
            Version++;
            if (_placements.Count > 0)
            {
                Log.Info("custom asset placements: " + _placements.Count + " loaded");
                // spawning happens via EditorEntered if we're in the editor
                if (GameEvents.IsEditorActive)
                {
                    SpawnAll();
                }
            }
        }
    }
}
