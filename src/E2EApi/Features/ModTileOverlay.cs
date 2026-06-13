using System.Collections.Generic;
using E2EApi.Editor;
using UnityEngine;

namespace E2EApi.Features
{
    /// <summary>
    /// Renders <see cref="ModTiles"/> placements as world-space sprites.
    /// In the editor only the current building layer and the ones below it
    /// are shown (matching the vanilla layer view); in play mode every layer
    /// renders at its own depth. Placements whose atlas is not cached on
    /// this machine show as magenta placeholders.
    /// </summary>
    public static class ModTileOverlay
    {
        public static bool Enabled = true;

        // floor stamps sit just above the baked map, decor stamps above actors
        private const int FloorSortingOrder = 40;
        private const int DecorSortingOrder = 22000;

        private static readonly List<GameObject> Markers = new List<GameObject>();
        private static int _builtVersion = -1;
        private static BaseLevelManager _builtFor;
        private static int _builtLayer = -1;

        // Minimum rotation in degrees before we apply a pivot parent for rendering.
        private const float RotationEpsilon = 0.001f;

        internal static void Tick()
        {
            var level = BaseLevelManager.GetInstance();
            if (!Enabled || level == null || ModTiles.Count == 0)
            {
                if (Markers.Count > 0)
                {
                    DestroyMarkers();
                    _builtFor = null;
                }
                return;
            }
            int editorLayer = Grid.CurrentEditorLayer;
            if (_builtVersion == ModTiles.Version && _builtFor == level &&
                _builtLayer == editorLayer)
            {
                return;
            }
            Rebuild(editorLayer);
            _builtVersion = ModTiles.Version;
            _builtFor = level;
            _builtLayer = editorLayer;
        }

        private static void Rebuild(int editorLayer)
        {
            DestroyMarkers();
            float tileSize = FenceOverlay.TileWorldSize();
            foreach (var p in ModTiles.All())
            {
                if (editorLayer >= 0 && p.Layer > editorLayer)
                {
                    continue; // editor: hide layers above the current one
                }
                var sprite = TileSets.GetSprite(p.Atlas, p.Rx, p.Ry, p.Rw, p.Rh);
                bool missing = sprite == null;
                if (missing)
                {
                    sprite = MissingSprite(p.WTiles, p.HTiles);
                }
                Vector3? world = Grid.TileToWorld(p.X, p.Y, p.Layer);
                if (world == null)
                {
                    continue;
                }
                var go = new GameObject("E2E_ModTile");
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sharedMaterial = OverlayLib.LitSpriteMaterial;
                renderer.sortingOrder = (p.Decor ? DecorSortingOrder : FloorSortingOrder)
                    + p.Layer;
                // sprite pivot is bottom-left; anchor at the tile's bottom-left corner
                float zOffset = p.Decor ? -0.6f : -0.05f;
                if (editorLayer >= 0)
                {
                    zOffset = p.Decor ? -0.6f : -0.45f; // editor plane sits at -50 already
                }
                Vector3 bottomLeft = new Vector3(
                    world.Value.x - tileSize * 0.5f,
                    world.Value.y - tileSize * 0.5f,
                    world.Value.z + zOffset);
                go.transform.localScale = new Vector3(tileSize, tileSize, 1f);
                if (editorLayer >= 0 && p.Layer < editorLayer)
                {
                    renderer.color = new Color(1f, 1f, 1f, 0.45f); // dim lower layers
                }

                if (Mathf.Abs(p.Rotation) > RotationEpsilon)
                {
                    // Rotate the sprite around the stamp centre using a pivot parent.
                    float halfW = p.WTiles * tileSize * 0.5f;
                    float halfH = p.HTiles * tileSize * 0.5f;
                    var pivot = new GameObject("E2E_ModTilePivot");
                    pivot.transform.position = new Vector3(
                        bottomLeft.x + halfW,
                        bottomLeft.y + halfH,
                        bottomLeft.z);
                    pivot.transform.eulerAngles = new Vector3(0f, 0f, p.Rotation);
                    go.transform.SetParent(pivot.transform, false);
                    go.transform.localPosition = new Vector3(-halfW, -halfH, 0f);
                    Markers.Add(pivot);
                }
                else
                {
                    go.transform.position = bottomLeft;
                    Markers.Add(go);
                }
            }
        }

        private static readonly Dictionary<long, Sprite> MissingSprites =
            new Dictionary<long, Sprite>();

        private static Sprite MissingSprite(int wTiles, int hTiles)
        {
            wTiles = Mathf.Max(wTiles, 1);
            hTiles = Mathf.Max(hTiles, 1);
            long key = ((long)wTiles << 32) | (uint)hTiles;
            Sprite sprite;
            if (MissingSprites.TryGetValue(key, out sprite) && sprite != null)
            {
                return sprite;
            }
            // one magenta/dark checker pixel per tile, 1 px = 1 tile
            var tex = new Texture2D(wTiles, hTiles, TextureFormat.ARGB32, false);
            var magenta = new Color32(255, 0, 220, 160);
            var dark = new Color32(60, 0, 60, 160);
            for (int y = 0; y < hTiles; y++)
            {
                for (int x = 0; x < wTiles; x++)
                {
                    tex.SetPixel(x, y, ((x + y) % 2 == 0) ? (Color)magenta : (Color)dark);
                }
            }
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            sprite = Sprite.Create(tex, new Rect(0, 0, wTiles, hTiles),
                new Vector2(0f, 0f), 1f);
            MissingSprites[key] = sprite;
            return sprite;
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
    }
}
