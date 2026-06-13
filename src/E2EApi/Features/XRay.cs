using System.Collections.Generic;
using E2EApi.Editor;
using UnityEngine;

namespace E2EApi.Features
{
    /// <summary>
    /// X-ray mode for the level editor: every placed block that is invisible
    /// or dev-only (no texture, zone markers, TEMP content…) gets a small
    /// outline + name label at its tile, so hidden content becomes editable.
    /// <see cref="GetInfoAt"/> backs the hover tooltips.
    /// </summary>
    public static class XRay
    {
        private static bool _enabled;

        public static bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                if (!value)
                {
                    DestroyMarkers();
                }
                else
                {
                    _nextScan = 0f; // rescan immediately
                }
                ApiRunner.Ensure();
            }
        }

        private const float RescanSeconds = 2f;

        private static readonly List<GameObject> Markers = new List<GameObject>();
        private static readonly Dictionary<long, BlockInfo> InfoByTile = new Dictionary<long, BlockInfo>();
        private static float _nextScan;

        private static long Key(int x, int y) => ((long)x << 32) | (uint)y;

        /// <summary>Researched info for the X-rayed block on a tile (any layer), or null.</summary>
        public static BlockInfo GetInfoAt(int x, int y)
        {
            BlockInfo info;
            return InfoByTile.TryGetValue(Key(x, y), out info) ? info : null;
        }

        internal static void Tick()
        {
            if (!_enabled)
            {
                return;
            }
            var level = BaseLevelManager.GetInstance();
            if (level == null || !Events.GameEvents.IsEditorActive)
            {
                DestroyMarkers();
                return;
            }
            if (Time.time < _nextScan)
            {
                return;
            }
            _nextScan = Time.time + RescanSeconds;
            Rebuild(level);
        }

        private static void Rebuild(BaseLevelManager level)
        {
            DestroyMarkers();
            var layers = level.m_BuildingLayers;
            if (layers == null)
            {
                return;
            }
            float scale = FenceOverlay.TileWorldSize();
            var seenObjects = new HashSet<GameObject>();
            var describedCache = new Dictionary<int, BlockInfo>();

            for (int layer = 0; layer < layers.Length; layer++)
            {
                ScanArray(layers[layer].m_TileTileIDs, layers[layer].m_TileTileObjects,
                    seenObjects, describedCache, scale);
                ScanArray(layers[layer].m_WallTileIDs, layers[layer].m_WallTileObjects,
                    seenObjects, describedCache, scale);
                ScanArray(layers[layer].m_ObjectTileIDs, layers[layer].m_ObjectTileObjects,
                    seenObjects, describedCache, scale);
            }
        }

        private static void ScanArray(BaseLevelManager.TileIDData[] ids, GameObject[] objects,
            HashSet<GameObject> seenObjects, Dictionary<int, BlockInfo> describedCache, float scale)
        {
            if (ids == null)
            {
                return;
            }
            for (int index = 0; index < ids.Length; index++)
            {
                int id = (int)(ids[index] & BaseLevelManager.TileIDData.IDMask);
                if (id == (int)BaseLevelManager.TileIDData.IDInvalid || id == 0)
                {
                    continue;
                }
                var placed = objects != null && index < objects.Length ? objects[index] : null;
                if (placed != null && !seenObjects.Add(placed))
                {
                    continue; // multi-tile footprint: only mark once
                }

                BlockInfo info;
                if (!describedCache.TryGetValue(id, out info))
                {
                    var block = Blocks.Get(id);
                    info = block != null ? Blocks.Describe(block) : null;
                    describedCache[id] = info;
                }
                if (info == null)
                {
                    continue;
                }

                bool invisible = placed == null || !HasVisibleRenderer(placed);
                if (!info.EditorOnly && !info.IsZone && !invisible)
                {
                    continue;
                }

                int x = index % 120;
                int y = index / 120;
                AddMarker(info, x, y, scale);
            }
        }

        private static bool HasVisibleRenderer(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                if (renderer != null && renderer.enabled)
                {
                    return true;
                }
            }
            return false;
        }

        private static void AddMarker(BlockInfo info, int x, int y, float scale)
        {
            InfoByTile[Key(x, y)] = info;

            var quad = OverlayLib.MakeTileQuad("E2E_XRay", OverlayLib.OutlineTexture, x, y, -0.6f);
            if (quad == null)
            {
                return;
            }
            quad.transform.localScale = new Vector3(scale * 0.92f, scale * 0.92f, 1f);
            Markers.Add(quad);

            string label = Shorten(info.DisplayName ?? info.InternalName ?? ("#" + info.Id), 14);
            var text = OverlayLib.MakeLabel(label,
                quad.transform.position + new Vector3(0f, 0f, -0.01f),
                scale * 0.06f, new Color(0.85f, 0.95f, 1f, 0.95f));
            Markers.Add(text);
        }

        private static string Shorten(string s, int max)
            => s.Length <= max ? s : s.Substring(0, max - 1) + "…";

        private static void DestroyMarkers()
        {
            foreach (var marker in Markers)
            {
                if (marker != null)
                {
                    Object.Destroy(marker);
                }
            }
            Markers.Clear();
            InfoByTile.Clear();
        }
    }
}
