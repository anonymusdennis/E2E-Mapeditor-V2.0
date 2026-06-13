using E2EApi.Features;
using E2EApi.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MapEditorMod
{
    internal class MapLayersWindow
    {
        private ModWindow _window;
        private Text _summary;
        private Text _warningLabel;
        private Text _boundsLabel;
        private Text _selectedLabel;
        private Text _layersLabel;

        public void Initialise()
        {
            MapGeometry.Changed += Refresh;
        }

        public void Show()
        {
            Ensure();
            _window.SetVisible(true);
            Refresh();
        }

        public void Hide()
        {
            if (_window != null)
            {
                _window.SetVisible(false);
            }
        }

        private void Ensure()
        {
            if (_window != null)
            {
                return;
            }

            _window = ModWindow.Create("Configure Map Layers", 520f, 680f);
            var list = UiFactory.VerticalList(_window.Content, 24f, 4f);

            _summary = UiFactory.Label(list, "", 12);
            _summary.color = new Color(1f, 0.92f, 0.72f, 1f);
            UiFactory.FixHeight(_summary, 56f);

            _warningLabel = UiFactory.Label(list, "", 11);
            _warningLabel.color = new Color(1f, 0.55f, 0.45f, 1f);
            UiFactory.FixHeight(_warningLabel, 36f);

            _boundsLabel = UiFactory.Label(list, "", 12);
            UiFactory.FixHeight(_boundsLabel, 28f);
            AddBoundsRow(list);

            _selectedLabel = UiFactory.Label(list, "", 12);
            _selectedLabel.color = new Color(0.75f, 0.9f, 1f, 1f);
            UiFactory.FixHeight(_selectedLabel, 28f);
            AddSelectedRow(list);
            AddTypeRow(list);

            var addTitle = UiFactory.Label(list, "Add virtual layer", 12);
            UiFactory.FixHeight(addTitle, 22f);
            AddAddRow(list);

            _layersLabel = UiFactory.Label(list, "", 11);
            UiFactory.FixHeight(_layersLabel, 220f);

            var reset = UiFactory.Button(list, "Reset to vanilla 6-layer layout", () =>
                MapGeometry.ResetDefault());
            UiFactory.FixHeight(reset, 26f);
            _window.SetVisible(false);
        }

        private void AddBoundsRow(Transform list)
        {
            var row = Row(list, "BoundsRow");
            AddSmall(row, "Width -16", () =>
                MapGeometry.SetBounds(MapGeometry.Width - 16, MapGeometry.Height, MapGeometry.OriginX, MapGeometry.OriginY));
            AddSmall(row, "Width +16", () =>
                MapGeometry.SetBounds(MapGeometry.Width + 16, MapGeometry.Height, MapGeometry.OriginX, MapGeometry.OriginY));
            AddSmall(row, "Height -16", () =>
                MapGeometry.SetBounds(MapGeometry.Width, MapGeometry.Height - 16, MapGeometry.OriginX, MapGeometry.OriginY));
            AddSmall(row, "Height +16", () =>
                MapGeometry.SetBounds(MapGeometry.Width, MapGeometry.Height + 16, MapGeometry.OriginX, MapGeometry.OriginY));
            var originRow = Row(list, "OriginRow");
            AddSmall(originRow, "Origin X -16", () =>
                MapGeometry.SetBounds(MapGeometry.Width, MapGeometry.Height, MapGeometry.OriginX - 16, MapGeometry.OriginY));
            AddSmall(originRow, "Origin X +16", () =>
                MapGeometry.SetBounds(MapGeometry.Width, MapGeometry.Height, MapGeometry.OriginX + 16, MapGeometry.OriginY));
            AddSmall(originRow, "Origin Y -16", () =>
                MapGeometry.SetBounds(MapGeometry.Width, MapGeometry.Height, MapGeometry.OriginX, MapGeometry.OriginY - 16));
            AddSmall(originRow, "Origin Y +16", () =>
                MapGeometry.SetBounds(MapGeometry.Width, MapGeometry.Height, MapGeometry.OriginX, MapGeometry.OriginY + 16));
        }

        private void AddSelectedRow(Transform list)
        {
            var row = Row(list, "SelectedLayerRow");
            AddSmall(row, "Prev", () => MapGeometry.MoveSelected(-1));
            AddSmall(row, "Next", () => MapGeometry.MoveSelected(1));
            AddSmall(row, "Move up", () => MapGeometry.MoveLayer(MapGeometry.SelectedVirtualLayerIndex, -1));
            AddSmall(row, "Move down", () => MapGeometry.MoveLayer(MapGeometry.SelectedVirtualLayerIndex, 1));
            AddSmall(row, "Duplicate", () => MapGeometry.DuplicateLayer(MapGeometry.SelectedVirtualLayerIndex));
            AddSmall(row, "Remove", () => MapGeometry.RemoveLayer(MapGeometry.SelectedVirtualLayerIndex));
        }

        private void AddTypeRow(Transform list)
        {
            var row = Row(list, "TypeRow");
            AddSmall(row, "Underground", () => SetSelectedType(MapGeometry.VirtualLayerType.Underground));
            AddSmall(row, "Ground", () => SetSelectedType(MapGeometry.VirtualLayerType.Ground));
            AddSmall(row, "Vent", () => SetSelectedType(MapGeometry.VirtualLayerType.Vent));
            AddSmall(row, "Roof", () => SetSelectedType(MapGeometry.VirtualLayerType.Roof));
        }

        private void AddAddRow(Transform list)
        {
            var row = Row(list, "AddRow");
            AddSmall(row, "+ Underground", () => MapGeometry.AddLayer(MapGeometry.VirtualLayerType.Underground));
            AddSmall(row, "+ Ground", () => MapGeometry.AddLayer(MapGeometry.VirtualLayerType.Ground));
            AddSmall(row, "+ Vent", () => MapGeometry.AddLayer(MapGeometry.VirtualLayerType.Vent));
            AddSmall(row, "+ Roof", () => MapGeometry.AddLayer(MapGeometry.VirtualLayerType.Roof));
        }

        private void SetSelectedType(MapGeometry.VirtualLayerType type)
        {
            MapGeometry.SetLayerType(MapGeometry.SelectedVirtualLayerIndex, type);
        }

        private Transform Row(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 4f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            UiFactory.FixHeight(rect, 26f);
            return go.transform;
        }

        private void AddSmall(Transform parent, string text, System.Action action)
        {
            var button = UiFactory.Button(parent, text, action);
            UiFactory.FixHeight(button, 26f);
        }

        private void Refresh()
        {
            if (_summary == null)
            {
                return;
            }
            var selected = MapGeometry.GetLayer(MapGeometry.SelectedVirtualLayerIndex);
            _summary.text =
                "Virtual layers live in Level.e2e. Vanilla Level.dat stays 120x120 with six native layers.\n" +
                "Mod tiles and connected textures use virtual layers; vanilla blocks stay inside native bounds.";
            _warningLabel.text = MapGeometry.HasCompatibilityIssue
                ? MapGeometry.CompatibilityWarning
                : "";
            _boundsLabel.text = "Bounds: " + MapGeometry.Width + "x" + MapGeometry.Height +
                " origin (" + MapGeometry.OriginX + "," + MapGeometry.OriginY + ")" +
                "  hash " + MapGeometry.CompatibilityHash;
            _selectedLabel.text = "Selected: #" + MapGeometry.SelectedVirtualLayerIndex +
                " " + selected.Name + " type " + selected.Type +
                " backing native layer " + selected.BackingLayer;
            _layersLabel.text = BuildLayerList();
        }

        private string BuildLayerList()
        {
            var state = MapGeometry.Current;
            var text = "";
            for (int i = 0; i < state.Layers.Count; i++)
            {
                var layer = state.Layers[i];
                string mark = i == MapGeometry.SelectedVirtualLayerIndex ? "> " : "  ";
                text += mark + i + ": " + layer.Name + " [" + layer.Type +
                    "] native " + layer.BackingLayer + "\n";
            }
            return text;
        }
    }
}
