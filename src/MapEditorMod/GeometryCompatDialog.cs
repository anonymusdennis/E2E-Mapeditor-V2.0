using E2EApi.Features;
using E2EApi.UI;
using UnityEngine;

namespace MapEditorMod
{
    /// <summary>
    /// Warns when a map's virtual geometry sidecar looks incompatible with
    /// this mod build (hash mismatch or non-native layout).
    /// </summary>
    internal static class GeometryCompatDialog
    {
        private static bool _shownThisSession;

        internal static void MaybeShow()
        {
            if (_shownThisSession || !MapGeometry.HasCompatibilityIssue)
            {
                return;
            }
            _shownThisSession = true;

            var window = ModWindow.Create("Map Geometry Compatibility", 460f, 170f);
            window.SetVisible(true);
            var list = UiFactory.VerticalList(window.Content);

            var label = UiFactory.Label(list, MapGeometry.CompatibilityWarning, 12);
            UiFactory.FixHeight(label, 72f);

            var ok = UiFactory.Button(list, "OK", () =>
            {
                Object.Destroy(window.gameObject);
            });
            UiFactory.FixHeight(ok, 28f);
        }
    }
}
