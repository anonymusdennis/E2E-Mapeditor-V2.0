using E2EApi.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MapEditorMod
{
    /// <summary>
    /// A small modal window that presents the available update to the player
    /// with three choices: update now, skip this release, or dismiss.
    /// </summary>
    internal static class UpdateDialog
    {
        internal static void Show(string newVersion, string releaseUrl)
        {
            var window = ModWindow.Create("Update Available", 400f, 185f);
            window.SetVisible(true);

            var list = UiFactory.VerticalList(window.Content);

            var label = UiFactory.Label(list,
                "E2E Map Editor " + newVersion + " is available  (you have " + PluginInfo.Version + ")",
                13);
            UiFactory.FixHeight(label, 42f);

            var updateBtn = UiFactory.Button(list, "Update Now", () =>
            {
                OpenUrl(releaseUrl);
                Object.Destroy(window.gameObject);
            });
            UiFactory.FixHeight(updateBtn, 30f);

            var skipBtn = UiFactory.Button(list, "Do not ask for this release", () =>
            {
                Plugin.CfgSkipVersion.Value = newVersion;
                Plugin.Log.LogInfo("Update check: will no longer prompt for version " + newVersion + ".");
                Object.Destroy(window.gameObject);
            });
            UiFactory.FixHeight(skipBtn, 30f);

            var laterBtn = UiFactory.Button(list, "Later", () =>
            {
                Object.Destroy(window.gameObject);
            });
            UiFactory.FixHeight(laterBtn, 30f);
        }

        private static void OpenUrl(string url)
        {
            try
            {
                if (Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    System.Diagnostics.Process.Start(url);
                }
                else
                {
                    System.Diagnostics.Process.Start("xdg-open", url);
                }
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogWarning("Could not open browser (" + e.Message + ") — visit " + url + " manually.");
            }
        }
    }
}
