using E2EApi.Editor;
using E2EApi.Features;
using E2EApi.Persistence;
using E2EApi.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MapEditorMod
{
    /// <summary>
    /// The "Extras" tab: electric fences and trigger links (mod-only content
    /// persisted in the Level.e2e sidecar).
    /// </summary>
    internal class ExtrasTab
    {
        private Text _status;
        private int _pendingButtonX = -1;
        private int _pendingButtonY = -1;

        public ExtrasTab(RectTransform panel)
        {
            ElectricFences.Initialise();
            Triggers.Initialise();

            var list = UiFactory.VerticalList(panel);

            var help = UiFactory.Label(list,
                "Hover a tile in the editor, then press the hotkeys below.", 11);
            UiFactory.FixHeight(help, 20f);

            var fence = UiFactory.Button(list, "Toggle electric fence at cursor (F6)", ToggleFenceAtCursor);
            UiFactory.FixHeight(fence, 26f);

            var trigger = UiFactory.Button(list, "Link trigger: button → fence (F7 twice)", LinkStep);
            UiFactory.FixHeight(trigger, 26f);

            var clear = UiFactory.Button(list, "Clear all fences + triggers", () =>
            {
                ElectricFences.Clear();
                Triggers.Clear();
                UpdateStatus("cleared");
            });
            UiFactory.FixHeight(clear, 26f);

            var requiresMod = UiFactory.Toggle(list, "Map requires this mod",
                ModExtras.Current.RequiresMod, v => ModExtras.Current.RequiresMod = v);
            UiFactory.FixHeight(requiresMod, 24f);

            _status = UiFactory.Label(list, "", 11);
            UiFactory.FixHeight(_status, 36f);
            UpdateStatus(null);
        }

        public void ToggleFenceAtCursor()
        {
            int x, y;
            if (!Placement.GetCursorTile(out x, out y))
            {
                UpdateStatus("cursor not on the map");
                return;
            }
            bool on = ElectricFences.Toggle(x, y);
            UpdateStatus($"fence at ({x},{y}) {(on ? "ON" : "off")}");
        }

        public void LinkStep()
        {
            int x, y;
            if (!Placement.GetCursorTile(out x, out y))
            {
                UpdateStatus("cursor not on the map");
                return;
            }
            if (_pendingButtonX < 0)
            {
                _pendingButtonX = x;
                _pendingButtonY = y;
                UpdateStatus($"button set at ({x},{y}) — now aim at the fence and press F7 again");
            }
            else
            {
                Triggers.AddLink(_pendingButtonX, _pendingButtonY, x, y);
                UpdateStatus($"linked button ({_pendingButtonX},{_pendingButtonY}) → fence ({x},{y})");
                _pendingButtonX = _pendingButtonY = -1;
            }
        }

        private void UpdateStatus(string message)
        {
            string summary = $"fences: {ElectricFences.Count} — triggers: {Triggers.All.Count}";
            _status.text = string.IsNullOrEmpty(message) ? summary : message + "\n" + summary;
        }
    }
}
