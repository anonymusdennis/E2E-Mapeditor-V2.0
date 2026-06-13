using System.Collections.Generic;
using UnityEngine;

namespace E2EApi.Features
{
    /// <summary>
    /// Checkered translucent highlight over every electrified tile, so electric
    /// fences are clearly marked both in the editor and in play mode.
    /// </summary>
    public static class FenceOverlay
    {
        public static bool Enabled = true;

        private static readonly List<GameObject> Markers = new List<GameObject>();
        private static int _builtVersion = -1;
        private static BaseLevelManager _builtFor;

        internal static void Tick()
        {
            var level = BaseLevelManager.GetInstance();
            if (!Enabled || level == null)
            {
                if (Markers.Count > 0)
                {
                    DestroyMarkers();
                }
                return;
            }
            if (_builtVersion == ElectricFences.Version && _builtFor == level)
            {
                return;
            }
            Rebuild();
            _builtVersion = ElectricFences.Version;
            _builtFor = level;
        }

        private static void Rebuild()
        {
            DestroyMarkers();
            float scale = TileWorldSize();
            foreach (var tile in ElectricFences.All())
            {
                var quad = OverlayLib.MakeTileQuad("E2E_FenceMark", OverlayLib.CheckerTexture,
                    tile.Key, tile.Value, -0.55f);
                if (quad == null)
                {
                    continue;
                }
                quad.transform.localScale = new Vector3(scale, scale, 1f);
                Markers.Add(quad);
            }
        }

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
        }

        /// <summary>World-space size of one tile (distance between tile centres).</summary>
        internal static float TileWorldSize()
        {
            Vector3? a = Editor.Grid.TileToWorld(10, 10);
            Vector3? b = Editor.Grid.TileToWorld(11, 10);
            if (a == null || b == null)
            {
                return 1f;
            }
            float d = Vector3.Distance(a.Value, b.Value);
            return d > 0.001f ? d : 1f;
        }
    }
}
