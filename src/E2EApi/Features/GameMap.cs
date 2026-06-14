using System.Collections.Generic;
using UnityEngine;

namespace E2EApi.Features
{
    /// <summary>
    /// Access to the game's per-floor map textures (the in-game "MAP" screen
    /// images) plus floor metadata — used by the web UI for click-to-teleport.
    /// Only available in play mode; the editor has no <c>FloorManager</c>.
    /// </summary>
    public static class GameMap
    {
        public class FloorInfo
        {
            public int Index;
            public string Name;
            public bool IsStartFloor;
            public bool HasTexture;
        }

        /// <summary>All valid floors of the current level (empty outside play mode).</summary>
        public static List<FloorInfo> GetFloors()
        {
            var result = new List<FloorInfo>();
            var manager = FloorManager.GetInstance();
            if (manager == null)
            {
                return result;
            }
            foreach (var floor in manager.GetValidFloors())
            {
                if (floor == null)
                {
                    continue;
                }
                result.Add(new FloorInfo
                {
                    Index = floor.m_FloorIndex,
                    Name = string.IsNullOrEmpty(floor.m_FloorName)
                        ? ("Floor " + floor.m_FloorIndex) : floor.m_FloorName,
                    IsStartFloor = floor.m_bIsStartFloor,
                    HasTexture = floor.m_MapTexture != null,
                });
            }
            return result;
        }

        /// <summary>
        /// PNG of a floor's map texture, or null. Works for non-readable
        /// textures too (official prisons) via a RenderTexture round-trip.
        /// </summary>
        public static byte[] GetFloorPng(int floorIndex)
        {
            var manager = FloorManager.GetInstance();
            var floor = manager != null ? manager.FindFloorbyIndex(floorIndex) : null;
            var texture = floor != null ? floor.m_MapTexture : null;
            if (texture == null)
            {
                return null;
            }
            try
            {
                return texture.EncodeToPNG();
            }
            catch (UnityException)
            {
                return EncodeUnreadable(texture);
            }
        }

        /// <summary>
        /// PNG for a virtual layer. Resolves the virtual index to its backing
        /// physical floor via <see cref="MapGeometry.GetBackingLayer"/> and
        /// returns the same texture as <see cref="GetFloorPng"/>.
        /// Returns <c>null</c> when the index is out-of-range or the texture
        /// is unavailable.
        /// </summary>
        public static byte[] GetVirtualLayerPng(int virtualIndex)
        {
            if (virtualIndex < 0 || virtualIndex >= MapGeometry.LayerCount) return null;
            int backing = MapGeometry.GetBackingLayer(virtualIndex);
            return GetFloorPng(backing);
        }
        private static byte[] EncodeUnreadable(Texture2D texture)
        {
            var rt = RenderTexture.GetTemporary(texture.width, texture.height, 0,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            var previous = RenderTexture.active;
            try
            {
                Graphics.Blit(texture, rt);
                RenderTexture.active = rt;
                var copy = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
                copy.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
                copy.Apply();
                byte[] png = copy.EncodeToPNG();
                Object.Destroy(copy);
                return png;
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(rt);
            }
        }
    }
}
