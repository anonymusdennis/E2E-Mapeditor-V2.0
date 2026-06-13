using UnityEngine;

namespace E2EApi.Features
{
    /// <summary>
    /// A single highlight quad that follows a tile — used as the visible
    /// "brush" cursor for mod editor tools. Call <see cref="Show"/> every
    /// frame the marker should be visible; it is destroyed via <see cref="Hide"/>.
    /// </summary>
    public static class TileMarker
    {
        private static GameObject _marker;
        private static int _x = -1;
        private static int _y = -1;

        /// <summary>Place (or move) the marker on a tile with the given tint.</summary>
        public static void Show(int x, int y, Color color)
        {
            if (_marker == null)
            {
                _marker = OverlayLib.MakeTileQuad("E2E_TileMarker",
                    OverlayLib.OutlineTexture, x, y, -0.7f);
                if (_marker == null)
                {
                    return;
                }
                float scale = FenceOverlay.TileWorldSize();
                _marker.transform.localScale = new Vector3(scale, scale, 1f);
                _x = x;
                _y = y;
            }
            else if (_x != x || _y != y)
            {
                Vector3? world = Editor.Grid.TileToWorld(x, y);
                if (world != null)
                {
                    _marker.transform.position =
                        new Vector3(world.Value.x, world.Value.y, world.Value.z - 0.7f);
                    _x = x;
                    _y = y;
                }
            }
            var renderer = _marker != null ? _marker.GetComponent<SpriteRenderer>() : null;
            if (renderer != null)
            {
                renderer.color = color;
            }
        }

        public static void Hide()
        {
            if (_marker != null)
            {
                Object.Destroy(_marker);
                _marker = null;
            }
            _x = _y = -1;
        }
    }
}
