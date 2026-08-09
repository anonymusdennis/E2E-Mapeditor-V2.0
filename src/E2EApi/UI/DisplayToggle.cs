using UnityEngine;

namespace E2EApi.UI
{
    /// <summary>
    /// Minimal fullscreen ↔ windowed control. Uses Unity's Screen.fullScreen only —
    /// no Win32 calls, no repositioning, no size forcing.
    /// </summary>
    public static class DisplayToggle
    {
        public static bool IsFullscreen => Screen.fullScreen;

        /// <summary>Leave fullscreen; the OS keeps the normal window chrome/size.</summary>
        public static void GoWindowed()
        {
            if (Screen.fullScreen)
            {
                Screen.fullScreen = false;
            }
        }

        public static void GoFullscreen()
        {
            if (!Screen.fullScreen)
            {
                Screen.fullScreen = true;
            }
        }

        public static void Toggle()
        {
            Screen.fullScreen = !Screen.fullScreen;
        }
    }
}
