using System.Collections.Generic;
using E2EApi.Editor;
using E2EApi.Features;
using UnityEngine;

namespace E2EApi.Features
{
    /// <summary>
    /// Renders <see cref="AnimatedModTiles"/> placements as world-space sprites
    /// that cycle through their frames at each placement's configured FPS.
    ///
    /// Rebuilds all renderers when <see cref="AnimatedModTiles.Version"/> changes,
    /// then advances frame indices every tick without a full rebuild.
    /// </summary>
    internal static class AnimatedModTileOverlay
    {
        public static bool Enabled = true;

        private const int FloorSortingOrder = 41;   // just above static mod tiles (40)
        private const int DecorSortingOrder = 22001; // just above static decor (22000)
        private const float RotationEpsilon = 0.001f;

        // ---- render entries ----

        private class RenderEntry
        {
            public GameObject PivotOrRoot; // the outermost GO (destroyed on rebuild)
            public SpriteRenderer Renderer;
            public Sprite[] Sprites;        // one per frame (null = missing atlas)
            public float Fps;
            public bool Loop;
            public bool PingPong;

            private float _nextFrameAt;
            private int _frameIndex;
            private int _direction = 1;

            public void InitTiming()
            {
                _nextFrameAt = Time.time + (Fps > 0 ? 1f / Fps : 1f);
                _frameIndex = 0;
                _direction = 1;
                if (Renderer != null && Sprites != null && Sprites.Length > 0)
                    Renderer.sprite = Sprites[0];
            }

            /// <summary>Advance the animation frame if the time interval has elapsed.</summary>
            public void Tick()
            {
                if (Sprites == null || Sprites.Length <= 1 || Renderer == null) return;
                float now = Time.time;
                if (now < _nextFrameAt) return;
                _nextFrameAt = now + (Fps > 0 ? 1f / Fps : 1f);

                if (PingPong)
                {
                    _frameIndex += _direction;
                    if (_frameIndex >= Sprites.Length - 1)
                    {
                        _frameIndex = Sprites.Length - 1;
                        _direction = -1;
                    }
                    else if (_frameIndex <= 0)
                    {
                        _frameIndex = 0;
                        _direction = 1;
                    }
                }
                else if (Loop)
                {
                    _frameIndex = (_frameIndex + 1) % Sprites.Length;
                }
                else
                {
                    _frameIndex = Mathf.Min(_frameIndex + 1, Sprites.Length - 1);
                }

                Renderer.sprite = Sprites[_frameIndex];
            }
        }

        private static readonly List<RenderEntry> Entries = new List<RenderEntry>();
        private static int _builtVersion = -1;
        private static int _builtGeometryVersion = -1;
        private static BaseLevelManager _builtFor;
        private static int _builtLayer = -1;

        // ---- public API ----

        internal static void Tick()
        {
            var level = BaseLevelManager.GetInstance();
            if (!Enabled || level == null || AnimatedModTiles.Count == 0)
            {
                if (Entries.Count > 0)
                {
                    DestroyAll();
                    _builtFor = null;
                }
                return;
            }
            int editorLayer = Grid.CurrentEditorLayer;
            if (_builtVersion != AnimatedModTiles.Version ||
                _builtFor != level || _builtLayer != editorLayer ||
                _builtGeometryVersion != MapGeometry.Version)
            {
                Rebuild(editorLayer);
                _builtVersion = AnimatedModTiles.Version;
                _builtGeometryVersion = MapGeometry.Version;
                _builtFor = level;
                _builtLayer = editorLayer;
            }

            // advance animation frames each frame
            foreach (var e in Entries)
                e.Tick();
        }

        // ---- private ----

        private static void Rebuild(int editorLayer)
        {
            DestroyAll();
            float tileSize = FenceOverlay.TileWorldSize();

            foreach (var p in AnimatedModTiles.All())
            {
                if (editorLayer >= 0 && p.Layer > editorLayer)
                    continue;

                // build sprite array (null elements = missing atlas frame)
                var sprites = new Sprite[p.Frames.Count];
                for (int i = 0; i < p.Frames.Count; i++)
                {
                    var f = p.Frames[i];
                    sprites[i] = TileSets.GetSprite(f.Atlas, f.Rx, f.Ry, f.Rw, f.Rh);
                }

                // use frame-0 sprite (or missing placeholder) for the initial display
                Sprite displaySprite = sprites[0] ?? MissingSprite(p.WTiles, p.HTiles);

                Vector3? world = Grid.TileToWorld(p.X, p.Y, p.Layer);
                if (world == null) continue;

                var go = new GameObject("E2E_AnimTile");
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = displaySprite;
                renderer.sharedMaterial = OverlayLib.LitSpriteMaterial;
                renderer.sortingOrder = (p.Decor ? DecorSortingOrder : FloorSortingOrder)
                    + p.Layer;

                float zOffset = p.Decor ? -0.6f : -0.05f;
                if (editorLayer >= 0)
                    zOffset = p.Decor ? -0.6f : -0.45f;
                if (editorLayer >= 0 && p.Layer < editorLayer)
                    renderer.color = new Color(1f, 1f, 1f, 0.45f);

                Vector3 bottomLeft = new Vector3(
                    world.Value.x - tileSize * 0.5f,
                    world.Value.y - tileSize * 0.5f,
                    world.Value.z + zOffset);
                go.transform.localScale = new Vector3(tileSize, tileSize, 1f);

                var entry = new RenderEntry
                {
                    Renderer = renderer,
                    Sprites = sprites,
                    Fps = p.Fps,
                    Loop = p.Loop,
                    PingPong = p.PingPong,
                };

                if (Mathf.Abs(p.Rotation) > RotationEpsilon)
                {
                    float halfW = p.WTiles * tileSize * 0.5f;
                    float halfH = p.HTiles * tileSize * 0.5f;
                    var pivot = new GameObject("E2E_AnimTilePivot");
                    pivot.transform.position = new Vector3(
                        bottomLeft.x + halfW, bottomLeft.y + halfH, bottomLeft.z);
                    pivot.transform.eulerAngles = new Vector3(0f, 0f, p.Rotation);
                    go.transform.SetParent(pivot.transform, false);
                    go.transform.localPosition = new Vector3(-halfW, -halfH, 0f);
                    entry.PivotOrRoot = pivot;
                }
                else
                {
                    go.transform.position = bottomLeft;
                    entry.PivotOrRoot = go;
                }

                entry.InitTiming();
                Entries.Add(entry);
            }
        }

        private static readonly Dictionary<long, Sprite> MissingSprites =
            new Dictionary<long, Sprite>();

        private static Sprite MissingSprite(int wTiles, int hTiles)
        {
            wTiles = Mathf.Max(wTiles, 1);
            hTiles = Mathf.Max(hTiles, 1);
            long key = ((long)wTiles << 32) | (uint)hTiles;
            Sprite s;
            if (MissingSprites.TryGetValue(key, out s) && s != null) return s;
            var tex = new Texture2D(wTiles, hTiles, TextureFormat.ARGB32, false);
            var magenta = new Color32(255, 50, 220, 160);
            var dark = new Color32(60, 0, 60, 160);
            for (int y = 0; y < hTiles; y++)
                for (int x2 = 0; x2 < wTiles; x2++)
                    tex.SetPixel(x2, y, ((x2 + y) % 2 == 0) ? (Color)magenta : (Color)dark);
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            s = Sprite.Create(tex, new Rect(0, 0, wTiles, hTiles), new Vector2(0f, 0f), 1f);
            MissingSprites[key] = s;
            return s;
        }

        private static void DestroyAll()
        {
            foreach (var e in Entries)
            {
                if (e.PivotOrRoot != null)
                    Object.Destroy(e.PivotOrRoot);
            }
            Entries.Clear();
        }
    }
}
