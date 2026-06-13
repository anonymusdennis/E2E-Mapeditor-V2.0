using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace E2EApi.Editor
{
    /// <summary>
    /// Extracts block icons as PNG bytes. Icon materials point into texture
    /// atlases via mainTextureOffset/Scale; the atlas usually isn't
    /// CPU-readable, so it is blitted through a RenderTexture once and cached.
    /// Blocks whose atlas icon is the red "TEMP" placeholder (or missing) get
    /// a live render of their actual visual rep / gameplay prefab instead.
    /// Must be called on the main thread (use <see cref="MainThread"/>).
    /// </summary>
    public static class BlockIcons
    {
        private const int VisualIconSize = 96;
        // Unused by the game's own content; isolates the temp camera + light
        // from the live scene during the one-off manual render.
        private const int IconLayer = 31;

        private static readonly Dictionary<int, Texture2D> ReadableAtlases =
            new Dictionary<int, Texture2D>();
        private static readonly Dictionary<int, byte[]> PngCache =
            new Dictionary<int, byte[]>();

        /// <summary>PNG for a block's icon, or null if it has none.</summary>
        public static byte[] GetPng(int blockId)
        {
            byte[] cached;
            if (PngCache.TryGetValue(blockId, out cached))
            {
                return cached;
            }
            byte[] png = Render(blockId);
            PngCache[blockId] = png;
            return png;
        }

        public static void ClearCache()
        {
            PngCache.Clear();
            foreach (var pair in ReadableAtlases)
            {
                if (pair.Value != null)
                {
                    Object.Destroy(pair.Value);
                }
            }
            ReadableAtlases.Clear();
        }

        /// <summary>
        /// True when this cropped atlas icon is the red "TEMP" dev
        /// placeholder: a mostly pure-red (#FF0000) square with dark text.
        /// Detected by pixel statistics — the placeholder cell is shared by
        /// many blocks but its material name carries no hint.
        /// </summary>
        public static bool LooksLikeTempPlaceholder(Texture2D icon)
        {
            float redFraction;
            ComputeRedStats(icon, out redFraction);
            return redFraction > 0.4f;
        }

        private static void ComputeRedStats(Texture2D icon, out float redFraction)
        {
            redFraction = 0f;
            Color32[] pixels = icon.GetPixels32();
            if (pixels.Length == 0)
            {
                return;
            }
            int red = 0;
            for (int i = 0; i < pixels.Length; i++)
            {
                Color32 c = pixels[i];
                if (c.r > 200 && c.g < 60 && c.b < 60)
                {
                    red++;
                }
            }
            redFraction = red / (float)pixels.Length;
        }

        private static byte[] Render(int blockId)
        {
            var block = Blocks.Get(blockId);
            if (block == null)
            {
                return null;
            }

            Texture2D icon = CropAtlasIcon(block.m_UIImage);
            bool placeholder = icon == null || LooksLikeTempPlaceholder(icon);
            if (placeholder)
            {
                byte[] real = null;
                try
                {
                    real = RenderVisual(block);
                }
                catch (System.Exception e)
                {
                    Log.Warn("BlockIcons: visual render for block " + blockId + " failed: " + e.Message);
                }
                if (real != null)
                {
                    if (icon != null)
                    {
                        Object.Destroy(icon);
                    }
                    return real;
                }
                // truly invisible marker → fall through to the TEMP atlas icon
            }
            if (icon == null)
            {
                return null;
            }
            byte[] png = icon.EncodeToPNG();
            Object.Destroy(icon);
            return png;
        }

        // ---- atlas crop (normal path) ----

        private static Texture2D CropAtlasIcon(Material material)
        {
            if (material == null)
            {
                return null;
            }
            var atlas = material.mainTexture as Texture2D;
            if (atlas == null)
            {
                return null;
            }

            Texture2D readable = GetReadableAtlas(atlas);
            if (readable == null)
            {
                return null;
            }

            Vector2 offset = material.mainTextureOffset;
            Vector2 scale = material.mainTextureScale;
            int x = Mathf.Clamp(Mathf.RoundToInt(offset.x * readable.width), 0, readable.width - 1);
            int y = Mathf.Clamp(Mathf.RoundToInt(offset.y * readable.height), 0, readable.height - 1);
            int w = Mathf.Clamp(Mathf.RoundToInt(Mathf.Abs(scale.x) * readable.width), 1, readable.width - x);
            int h = Mathf.Clamp(Mathf.RoundToInt(Mathf.Abs(scale.y) * readable.height), 1, readable.height - y);

            var icon = new Texture2D(w, h, TextureFormat.ARGB32, false);
            icon.SetPixels(readable.GetPixels(x, y, w, h));
            icon.Apply();
            return icon;
        }

        private static Texture2D GetReadableAtlas(Texture2D atlas)
        {
            int id = atlas.GetInstanceID();
            Texture2D readable;
            if (ReadableAtlases.TryGetValue(id, out readable) && readable != null)
            {
                return readable;
            }

            var rt = RenderTexture.GetTemporary(atlas.width, atlas.height, 0,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            var previous = RenderTexture.active;
            try
            {
                Graphics.Blit(atlas, rt);
                RenderTexture.active = rt;
                readable = new Texture2D(atlas.width, atlas.height, TextureFormat.ARGB32, false);
                readable.ReadPixels(new Rect(0, 0, atlas.width, atlas.height), 0, 0);
                readable.Apply();
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(rt);
            }
            ReadableAtlases[id] = readable;
            return readable;
        }

        // ---- live prefab render (TEMP placeholder path) ----

        /// <summary>
        /// Renders the block's editor visual rep (or, failing that, its real
        /// gameplay prefab) with a temporary orthographic camera and returns
        /// the PNG. Returns null when the block has nothing renderable.
        /// </summary>
        private static byte[] RenderVisual(BaseBuildingBlock block)
        {
            bool useReal = false;
            GameObject source = block.GetVisualRep(0);
            if (!HasRenderer(source))
            {
                source = block.GetRealObject(0);
                useReal = true;
                if (!HasRenderer(source))
                {
                    return null;
                }
            }

            GameObject clone = Object.Instantiate(source);
            GameObject camGo = null;
            RenderTexture rt = null;
            Texture2D tex = null;
            var previousActive = RenderTexture.active;
            try
            {
                clone.name = "E2E Icon Subject";
                clone.transform.parent = null;
                clone.transform.position = new Vector3(-2000f, -2000f, 0f);
                if (useReal)
                {
                    // real prefabs carry gameplay scripts (Photon, AI, …);
                    // remove them so activating the clone has no side effects
                    StripBehaviours(clone);
                }
                else
                {
                    foreach (var mb in clone.GetComponentsInChildren<MonoBehaviour>(true))
                    {
                        if (mb != null)
                        {
                            mb.enabled = false;
                        }
                    }
                }
                SetLayerRecursive(clone.transform, IconLayer);
                clone.SetActive(true);

                Bounds bounds = new Bounds();
                bool hasBounds = false;
                foreach (var renderer in clone.GetComponentsInChildren<Renderer>())
                {
                    if (renderer == null || !renderer.enabled)
                    {
                        continue;
                    }
                    if (!hasBounds)
                    {
                        bounds = renderer.bounds;
                        hasBounds = true;
                    }
                    else
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }
                }
                if (!hasBounds || (bounds.size.x < 0.001f && bounds.size.y < 0.001f))
                {
                    return null;
                }

                camGo = new GameObject("E2E Icon Camera");
                camGo.transform.position = new Vector3(
                    bounds.center.x, bounds.center.y, bounds.min.z - 10f);
                var cam = camGo.AddComponent<Camera>();
                cam.enabled = false;
                cam.orthographic = true;
                cam.orthographicSize = Mathf.Max(
                    Mathf.Max(bounds.extents.x, bounds.extents.y) * 1.05f, 0.05f);
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.nearClipPlane = 0.01f;
                cam.farClipPlane = 20f + bounds.size.z + 100f;
                cam.cullingMask = 1 << IconLayer;
                cam.useOcclusionCulling = false;

                // sprites are unlit, but mesh-based blocks need some light or
                // they come out black
                var lightGo = new GameObject("E2E Icon Light");
                lightGo.transform.parent = camGo.transform;
                lightGo.transform.rotation = Quaternion.Euler(40f, -25f, 0f);
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1f;
                light.color = Color.white;
                light.cullingMask = 1 << IconLayer;

                // Sprite shaders don't write destination alpha, so a single
                // render comes out fully transparent. Render on black and on
                // white and recover per-pixel alpha from the difference.
                rt = RenderTexture.GetTemporary(VisualIconSize, VisualIconSize, 16,
                    RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
                cam.targetTexture = rt;

                cam.backgroundColor = new Color(0f, 0f, 0f, 1f);
                cam.Render();
                RenderTexture.active = rt;
                tex = new Texture2D(VisualIconSize, VisualIconSize, TextureFormat.ARGB32, false);
                tex.ReadPixels(new Rect(0, 0, VisualIconSize, VisualIconSize), 0, 0);
                Color32[] onBlack = tex.GetPixels32();

                cam.backgroundColor = new Color(1f, 1f, 1f, 1f);
                cam.Render();
                tex.ReadPixels(new Rect(0, 0, VisualIconSize, VisualIconSize), 0, 0);
                Color32[] onWhite = tex.GetPixels32();
                cam.targetTexture = null;

                var combined = new Color32[onBlack.Length];
                for (int i = 0; i < onBlack.Length; i++)
                {
                    Color32 b = onBlack[i];
                    Color32 w = onWhite[i];
                    // alpha = 1 - (white_result - black_result); average channels
                    int alpha = 255 - (((w.r - b.r) + (w.g - b.g) + (w.b - b.b)) / 3);
                    alpha = alpha < 0 ? 0 : (alpha > 255 ? 255 : alpha);
                    if (alpha == 0)
                    {
                        combined[i] = new Color32(0, 0, 0, 0);
                    }
                    else
                    {
                        // un-premultiply against the black background
                        int r = b.r * 255 / alpha;
                        int g = b.g * 255 / alpha;
                        int bl = b.b * 255 / alpha;
                        combined[i] = new Color32(
                            (byte)(r > 255 ? 255 : r),
                            (byte)(g > 255 ? 255 : g),
                            (byte)(bl > 255 ? 255 : bl),
                            (byte)alpha);
                    }
                }
                tex.SetPixels32(combined);
                tex.Apply();
                return tex.EncodeToPNG();
            }
            finally
            {
                RenderTexture.active = previousActive;
                if (rt != null)
                {
                    RenderTexture.ReleaseTemporary(rt);
                }
                if (tex != null)
                {
                    Object.DestroyImmediate(tex);
                }
                if (camGo != null)
                {
                    Object.DestroyImmediate(camGo);
                }
                if (clone != null)
                {
                    Object.DestroyImmediate(clone);
                }
            }
        }

        private static bool HasRenderer(GameObject go)
        {
            return go != null && go.GetComponentsInChildren<Renderer>(true).Length > 0;
        }

        private static void SetLayerRecursive(Transform t, int layer)
        {
            t.gameObject.layer = layer;
            for (int i = t.childCount - 1; i >= 0; i--)
            {
                SetLayerRecursive(t.GetChild(i), layer);
            }
        }

        /// <summary>
        /// Destroys all MonoBehaviours on an (inactive) clone so that
        /// activating it never runs gameplay code. RequireComponent chains can
        /// block destruction, hence multiple passes; stragglers get disabled.
        /// </summary>
        private static void StripBehaviours(GameObject root)
        {
            for (int pass = 0; pass < 8; pass++)
            {
                var behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
                int destroyed = 0;
                foreach (var mb in behaviours)
                {
                    if (mb == null)
                    {
                        continue;
                    }
                    try
                    {
                        Object.DestroyImmediate(mb);
                        if (mb == null)
                        {
                            destroyed++;
                        }
                    }
                    catch
                    {
                        // kept alive by a RequireComponent dependency
                    }
                }
                if (destroyed == 0)
                {
                    break;
                }
            }
            foreach (var mb in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb != null)
                {
                    mb.enabled = false;
                }
            }
        }

        // ---- diagnostics ----

        /// <summary>
        /// JSON report over all registered blocks: icon material/texture
        /// names, whether the icon is the TEMP placeholder, and what visual
        /// source (editor rep / real prefab / none) is available. Main thread.
        /// </summary>
        public static string DiagnoseJson()
        {
            var all = Blocks.All;
            var sb = new StringBuilder("[");
            if (all != null)
            {
                bool first = true;
                foreach (var block in all)
                {
                    if (block == null)
                    {
                        continue;
                    }
                    var material = block.m_UIImage;
                    Texture2D icon = CropAtlasIcon(material);
                    float redFraction = 0f;
                    if (icon != null)
                    {
                        ComputeRedStats(icon, out redFraction);
                    }
                    bool temp = icon == null || redFraction > 0.4f;
                    if (icon != null)
                    {
                        Object.Destroy(icon);
                    }
                    string visual = "untested";
                    if (temp)
                    {
                        if (HasRenderer(block.GetVisualRep(0)))
                        {
                            visual = "rep";
                        }
                        else if (HasRenderer(block.GetRealObject(0)))
                        {
                            visual = "real";
                        }
                        else
                        {
                            visual = "none";
                        }
                    }
                    if (!first)
                    {
                        sb.Append(",");
                    }
                    first = false;
                    sb.Append("{\"id\":").Append(block.m_ID);
                    sb.Append(",\"name\":\"").Append(Escape(block.gameObject != null ? block.gameObject.name : ""));
                    sb.Append("\",\"mat\":\"").Append(Escape(material != null ? material.name : ""));
                    sb.Append("\",\"tex\":\"").Append(Escape(
                        material != null && material.mainTexture != null ? material.mainTexture.name : ""));
                    sb.Append("\",\"red\":").Append(redFraction.ToString("0.000",
                        System.Globalization.CultureInfo.InvariantCulture));
                    sb.Append(",\"temp\":").Append(temp ? "true" : "false");
                    sb.Append(",\"visual\":\"").Append(visual).Append("\"}");
                }
            }
            sb.Append("]");
            return sb.ToString();
        }

        private static string Escape(string s)
        {
            return s == null ? "" : s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
