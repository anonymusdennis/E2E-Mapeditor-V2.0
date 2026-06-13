using E2EApi.UI;
using UnityEngine;

namespace MapEditorMod
{
    /// <summary>
    /// The mod's main window: tab 1 = settings, tab 2 = mapping UI.
    /// Toggled with the configured hotkey; auto-shown/hidden with the editor.
    /// </summary>
    internal class EditorWindow
    {
        private TabbedWindow _window;
        private RectTransform _settingsPanel;
        private RectTransform _mappingPanel;
        internal MappingTab Mapping { get; private set; }
        internal ExtrasTab Extras { get; private set; }

        public bool IsCreated => _window != null;

        public void Toggle()
        {
            if (_window == null)
            {
                Create();
                return;
            }
            _window.Window.SetVisible(!_window.Window.IsVisible);
        }

        public void Show()
        {
            if (_window == null)
            {
                Create();
            }
            else
            {
                _window.Window.SetVisible(true);
            }
        }

        public void Hide()
        {
            if (_window != null)
            {
                _window.Window.SetVisible(false);
            }
        }

        private void Create()
        {
            _window = TabbedWindow.Create("E2E Map Editor", 520f, 440f);
            _settingsPanel = _window.AddTab("Settings");
            _mappingPanel = _window.AddTab("Mapping");
            var extrasPanel = _window.AddTab("Extras");
            BuildSettingsTab();
            Mapping = new MappingTab(_mappingPanel);
            Extras = new ExtrasTab(extrasPanel);
        }

        private void BuildSettingsTab()
        {
            var list = UiFactory.VerticalList(_settingsPanel);

            var devBlocks = UiFactory.Toggle(list, "Unlock dev-only blocks",
                Plugin.CfgUnlockDevBlocks.Value, v => Plugin.CfgUnlockDevBlocks.Value = v);
            UiFactory.FixHeight(devBlocks, 24f);

            var layers = UiFactory.Toggle(list, "Ignore layer restrictions",
                Plugin.CfgIgnoreLayerRestrictions.Value, v => Plugin.CfgIgnoreLayerRestrictions.Value = v);
            UiFactory.FixHeight(layers, 24f);

            var completion = UiFactory.Toggle(list, "Show incomplete blocks",
                Plugin.CfgIgnoreCompletionState.Value, v => Plugin.CfgIgnoreCompletionState.Value = v);
            UiFactory.FixHeight(completion, 24f);

            var windowed = UiFactory.Toggle(list, "Force windowed mode",
                Plugin.CfgForceWindowed.Value, v => Plugin.CfgForceWindowed.Value = v);
            UiFactory.FixHeight(windowed, 24f);

            var layersConfig = UiFactory.Button(list, "Configure map layers",
                () => Plugin.ShowMapLayersWindow());
            UiFactory.FixHeight(layersConfig, 28f);

            var hint = UiFactory.Label(list,
                $"Toggle window: {Plugin.CfgWindowKey.Value} — guard/inmate cap: {Plugin.CfgGuardInmateCap.Value}", 11);
            UiFactory.FixHeight(hint, 20f);
        }
    }
}
