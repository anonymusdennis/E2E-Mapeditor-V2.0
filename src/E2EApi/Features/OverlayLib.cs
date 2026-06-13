using UnityEngine;

namespace E2EApi.Features
{
    /// <summary>Shared helpers for world-space overlay visuals.</summary>
    internal static class OverlayLib
    {
        private static Texture2D _checker;
        private static Texture2D _outline;
        private static Material _spriteMaterial;
        private static Material _litSpriteMaterial;
        private static bool _litSearchAttempted;

        /// <summary>Prefix used for all mod-owned GameObjects.</summary>
        internal const string GameObjectPrefix = "E2E_";

        public const int SortingOrder = 31000;

        public static Material SpriteMaterial
        {
            get
            {
                if (_spriteMaterial == null)
                {
                    var shader = Shader.Find("Sprites/Default");
                    _spriteMaterial = new Material(shader);
                }
                return _spriteMaterial;
            }
        }

        /// <summary>
        /// A lit sprite material that matches the vanilla map sprite rendering,
        /// so that custom-tile stamps respond to the game's day/night lighting.
        /// Attempts to borrow the material from an existing vanilla
        /// <see cref="SpriteRenderer"/> in the scene so the shader is an exact
        /// match; falls back to <c>Sprites/Diffuse</c> (Unity's built-in lit
        /// sprite shader) when no vanilla renderer is found.
        /// </summary>
        public static Material LitSpriteMaterial
        {
            get
            {
                if (_litSpriteMaterial == null && !_litSearchAttempted)
                {
                    _litSearchAttempted = true;
                    foreach (var r in Object.FindObjectsOfType<SpriteRenderer>())
                    {
                        if (r != null && r.sharedMaterial != null &&
                            !r.gameObject.name.StartsWith(GameObjectPrefix))
                        {
                            _litSpriteMaterial = r.sharedMaterial;
                            break;
                        }
                    }
                }
                if (_litSpriteMaterial == null)
                {
                    var shader = Shader.Find("Sprites/Diffuse") ?? Shader.Find("Sprites/Default");
                    _litSpriteMaterial = new Material(shader);
                }
                return _litSpriteMaterial;
            }
        }

        /// <summary>8×8 yellow/transparent checker, point-filtered.</summary>
        public static Texture2D CheckerTexture
        {
            get
            {
                if (_checker == null)
                {
                    _checker = new Texture2D(8, 8, TextureFormat.ARGB32, false);
                    var on = new Color32(255, 220, 0, 150);
                    var off = new Color32(255, 220, 0, 30);
                    for (int y = 0; y < 8; y++)
                    {
                        for (int x = 0; x < 8; x++)
                        {
                            _checker.SetPixel(x, y, ((x / 2 + y / 2) % 2 == 0) ? (Color)on : (Color)off);
                        }
                    }
                    _checker.filterMode = FilterMode.Point;
                    _checker.wrapMode = TextureWrapMode.Repeat;
                    _checker.Apply();
                }
                return _checker;
            }
        }

        /// <summary>16×16 transparent square with a 2px border.</summary>
        public static Texture2D OutlineTexture
        {
            get
            {
                if (_outline == null)
                {
                    _outline = new Texture2D(16, 16, TextureFormat.ARGB32, false);
                    var border = new Color32(120, 220, 255, 230);
                    var fill = new Color32(120, 220, 255, 25);
                    for (int y = 0; y < 16; y++)
                    {
                        for (int x = 0; x < 16; x++)
                        {
                            bool edge = x < 2 || y < 2 || x > 13 || y > 13;
                            _outline.SetPixel(x, y, edge ? (Color)border : (Color)fill);
                        }
                    }
                    _outline.filterMode = FilterMode.Point;
                    _outline.Apply();
                }
                return _outline;
            }
        }

        /// <summary>
        /// Sprite-quad covering one tile, centred on the tile. <paramref name="z"/>
        /// is an offset relative to the tile's own world z — the editor scene sits
        /// at a different depth than the play-mode world, so absolute z values
        /// would put the quad behind the map geometry there.
        /// </summary>
        public static GameObject MakeTileQuad(string name, Texture2D texture, int x, int y, float z)
        {
            Vector3? world = Editor.Grid.TileToWorld(x, y);
            if (world == null)
            {
                return null;
            }
            var go = new GameObject(name);
            var sprite = go.AddComponent<SpriteRenderer>();
            sprite.sprite = Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), texture.width);
            sprite.sharedMaterial = SpriteMaterial;
            sprite.sortingOrder = SortingOrder;
            go.transform.position = new Vector3(world.Value.x, world.Value.y, world.Value.z + z);
            return go;
        }

        /// <summary>Small world-space text label.</summary>
        public static GameObject MakeLabel(string text, Vector3 position, float size, Color color)
        {
            var go = new GameObject("E2E_Label");
            var mesh = go.AddComponent<TextMesh>();
            mesh.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            mesh.fontSize = 48;
            mesh.characterSize = size;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = color;
            mesh.text = text;
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = mesh.font.material;
            renderer.sortingOrder = SortingOrder + 1;
            go.transform.position = position;
            return go;
        }

        /// <summary>Distance from point to segment, in the same (screen) space.</summary>
        public static float PointToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float len = ab.sqrMagnitude;
            if (len < 0.0001f)
            {
                return Vector2.Distance(p, a);
            }
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / len);
            return Vector2.Distance(p, a + ab * t);
        }
    }
}
