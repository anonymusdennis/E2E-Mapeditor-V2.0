using UnityEngine;

namespace E2EApi.UI
{
    /// <summary>Display-mode helpers (the mod's "windowed mode" feature).</summary>
    public static class WindowMode
    {
        /// <summary>
        /// Switch the game to windowed mode at the given resolution.
        /// Pass 0/0 to keep the current resolution.
        /// </summary>
        public static void ForceWindowed(int width = 0, int height = 0)
        {
            int w = width > 0 ? width : Screen.width;
            int h = height > 0 ? height : Screen.height;
            if (!Screen.fullScreen && Screen.width == w && Screen.height == h)
            {
                return;
            }
            Screen.SetResolution(w, h, fullscreen: false);
            Log.Info($"windowed mode forced ({w}x{h})");
        }

        public static void SetFullscreen(int width = 0, int height = 0)
        {
            int w = width > 0 ? width : Screen.width;
            int h = height > 0 ? height : Screen.height;
            Screen.SetResolution(w, h, fullscreen: true);
        }

        public static bool IsWindowed => !Screen.fullScreen;
    }
}
