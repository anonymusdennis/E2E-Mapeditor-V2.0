using System.Collections.Generic;
using E2EApi.Editor;
using E2EApi.Features;
using UnityEngine;

namespace MapEditorMod
{
    /// <summary>
    /// Tracks a multi-tile selection of mod-tile placements and renders a
    /// per-tile highlight overlay for every selected tile.
    ///
    /// Usage:
    ///   • Shift + LMB drag  → rubber-band area selection (replaces selection)
    ///   • Ctrl  + LMB click → toggle a single tile in/out of the selection
    ///   • Esc               → clear selection
    ///
    /// Call <see cref="Tick"/> every editor frame and <see cref="Clear"/> when
    /// leaving the editor.
    /// </summary>
    internal static class TileSelector
    {
        // Each entry is the anchor tile (X, Y) plus layer of a placement.
        private static readonly HashSet<TileKey> Selected = new HashSet<TileKey>();

        // Rubber-band drag state
        private static bool _dragging;
        private static int _dragStartX, _dragStartY;

        // World-space selection highlight quads (one per selected tile cell)
        private static readonly List<GameObject> Highlights = new List<GameObject>();
        private static int _highlightVersion = -1;

        // Rubber-band rectangle GO
        private static GameObject _bandGo;

        public static int Count => Selected.Count;
        public static IEnumerable<TileKey> All() => Selected;

        /// <summary>
        /// Process input for selection (Shift-drag, Ctrl-click, Esc to clear).
        /// Call every editor frame from <see cref="EditorTools.Tick"/>.
        /// </summary>
        public static void Tick()
        {
            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool ctrl  = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            // Esc clears selection
            if (Input.GetKeyDown(KeyCode.Escape) && Selected.Count > 0)
            {
                Clear();
                return;
            }

            int cx, cy;
            bool hasCursor = Placement.GetCursorTile(out cx, out cy);
            int layer = Grid.CurrentEditorLayer < 0 ? 1 : Grid.CurrentEditorLayer;

            // --- Ctrl + Click: toggle single tile ---
            if (ctrl && !shift && Input.GetMouseButtonDown(0) && hasCursor)
            {
                Toggle(cx, cy, layer);
                return;
            }

            // --- Shift + Drag: rubber-band area selection ---
            if (shift && !ctrl)
            {
                if (Input.GetMouseButtonDown(0) && hasCursor)
                {
                    _dragging = true;
                    _dragStartX = cx;
                    _dragStartY = cy;
                    Selected.Clear();
                    _highlightVersion = -1;
                }

                if (_dragging)
                {
                    // Update the rubber-band rectangle visual
                    if (hasCursor)
                    {
                        UpdateBand(_dragStartX, _dragStartY, cx, cy, layer);
                    }

                    if (Input.GetMouseButtonUp(0))
                    {
                        _dragging = false;
                        DestroyBand();
                        if (hasCursor)
                        {
                            SelectRect(_dragStartX, _dragStartY, cx, cy, layer);
                        }
                    }
                }
            }
            else if (_dragging)
            {
                // Modifier released mid-drag — cancel
                _dragging = false;
                DestroyBand();
            }

            // Rebuild highlight quads if selection changed
            RebuildHighlights();
        }

        public static void Clear()
        {
            Selected.Clear();
            _dragging = false;
            DestroyBand();
            RebuildHighlights();
        }

        // ------------------------------------------------------------------ //

        private static void Toggle(int x, int y, int layer)
        {
            // Only select tiles that actually have a mod placement
            if (ModTiles.GetAt(x, y, layer) == null) return;

            var key = new TileKey(x, y, layer);
            if (Selected.Contains(key))
                Selected.Remove(key);
            else
                Selected.Add(key);
            _highlightVersion = -1;
        }

        private static void SelectRect(int x0, int y0, int x1, int y1, int layer)
        {
            int minX = Mathf.Min(x0, x1);
            int maxX = Mathf.Max(x0, x1);
            int minY = Mathf.Min(y0, y1);
            int maxY = Mathf.Max(y0, y1);

            Selected.Clear();
            foreach (var p in ModTiles.All())
            {
                if (p.Layer != layer) continue;
                // Add if the placement's anchor tile falls within the rubber-band rect
                if (p.X >= minX && p.X <= maxX && p.Y >= minY && p.Y <= maxY)
                {
                    Selected.Add(new TileKey(p.X, p.Y, p.Layer));
                }
            }
            _highlightVersion = -1;
        }

        // ------------------------------------------------------------------ //
        // Visuals

        private static void RebuildHighlights()
        {
            // XOR the tile count in using a prime multiplier so that a selection
            // change (count change) produces a different hash even when Version is
            // unchanged (e.g. the user rotated a tile and the overlay rebuilt).
            int curVersion = ModTiles.Version ^ (Selected.Count * 397);
            if (curVersion == _highlightVersion) return;
            _highlightVersion = curVersion;

            foreach (var h in Highlights)
                if (h != null) Object.Destroy(h);
            Highlights.Clear();

            if (Selected.Count == 0) return;

            foreach (var key in Selected)
            {
                var go = OverlayLib.MakeTileQuad("E2E_SelectHighlight",
                    OverlayLib.OutlineTexture, key.X, key.Y, -0.65f);
                if (go == null) continue;
                float scale = FenceOverlay.TileWorldSize();
                go.transform.localScale = new Vector3(scale, scale, 1f);
                var sr = go.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = new Color(1f, 0.85f, 0.1f, 0.85f); // yellow
                Highlights.Add(go);
            }
        }

        private static void UpdateBand(int x0, int y0, int x1, int y1, int layer)
        {
            // Rebuild the rubber-band rectangle each frame
            DestroyBand();
            float tileSize = FenceOverlay.TileWorldSize();
            int minX = Mathf.Min(x0, x1);
            int maxX = Mathf.Max(x0, x1);
            int minY = Mathf.Min(y0, y1);
            int maxY = Mathf.Max(y0, y1);

            Vector3? worldMin = Grid.TileToWorld(minX, minY, layer < 0 ? 0 : layer);
            Vector3? worldMax = Grid.TileToWorld(maxX, maxY, layer < 0 ? 0 : layer);
            if (worldMin == null || worldMax == null) return;

            // Create a semi-transparent quad spanning the entire selection rect
            int wTiles = maxX - minX + 1;
            int hTiles = maxY - minY + 1;

            // Build a tiny texture (1×1 tinted)
            var tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            tex.SetPixel(0, 0, new Color(0.3f, 0.7f, 1f, 0.18f));
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0f, 0f), 1f);

            _bandGo = new GameObject("E2E_SelectBand");
            var sr = _bandGo.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sharedMaterial = OverlayLib.SpriteMaterial;
            sr.sortingOrder = OverlayLib.SortingOrder - 1;
            sr.color = new Color(0.3f, 0.7f, 1f, 0.18f);

            float z = worldMin.Value.z - 0.65f;
            _bandGo.transform.position = new Vector3(
                worldMin.Value.x - tileSize * 0.5f,
                worldMin.Value.y - tileSize * 0.5f,
                z);
            _bandGo.transform.localScale = new Vector3(wTiles * tileSize, hTiles * tileSize, 1f);
        }

        private static void DestroyBand()
        {
            if (_bandGo != null)
            {
                Object.Destroy(_bandGo);
                _bandGo = null;
            }
        }

        // ------------------------------------------------------------------ //

        internal struct TileKey
        {
            public readonly int X, Y, Layer;
            public TileKey(int x, int y, int layer) { X = x; Y = y; Layer = layer; }

            public override bool Equals(object obj)
            {
                if (!(obj is TileKey)) return false;
                var other = (TileKey)obj;
                return X == other.X && Y == other.Y && Layer == other.Layer;
            }

            public override int GetHashCode()
            {
                return X ^ (Y << 8) ^ (Layer << 16);
            }
        }
    }
}
