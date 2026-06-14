using E2EApi.Editor;
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
        private Transform _trashContent;

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

            _window = ModWindow.Create("Configure Map Layers", 520f, 780f);
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
            UiFactory.FixHeight(_layersLabel, 160f);

            BuildTrashSection(list);

            var reloadBtn = UiFactory.Button(list, "Save & Reload Editor", OnSaveAndReload);
            reloadBtn.GetComponentInChildren<Text>().color = new Color(1f, 0.85f, 0.4f, 1f);
            UiFactory.FixHeight(reloadBtn, 26f);

            var reset = UiFactory.Button(list, "Reset to vanilla 6-layer layout", () =>
                MapGeometry.ResetDefault());
            UiFactory.FixHeight(reset, 26f);
            _window.SetVisible(false);
        }

        private void BuildTrashSection(Transform list)
        {
            var trashTitle = UiFactory.Label(list, "Trash Bin  (removed layers)", 11);
            trashTitle.color = new Color(1f, 0.65f, 0.35f, 1f);
            UiFactory.FixHeight(trashTitle, 20f);

            // Scrollable panel for trash entries
            var panelGo = new GameObject("TrashPanel");
            panelGo.transform.SetParent(list, false);
            var panelRect = panelGo.AddComponent<RectTransform>();
            var panelEl = panelGo.AddComponent<LayoutElement>();
            panelEl.preferredHeight = 110f;
            panelEl.minHeight = 110f;
            panelEl.flexibleHeight = 0f;

            var panelBg = panelGo.AddComponent<Image>();
            panelBg.color = new Color(0.10f, 0.10f, 0.14f, 0.85f);

            var scroll = panelGo.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 24f;

            var viewportGo = new GameObject("Viewport");
            viewportGo.transform.SetParent(panelGo.transform, false);
            var viewportRect = viewportGo.AddComponent<RectTransform>();
            ModWindow.Stretch(viewportRect);
            viewportGo.AddComponent<RectMask2D>();
            scroll.viewport = viewportRect;

            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRect = contentGo.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;
            var contentLayout = contentGo.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 2f;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.padding = new RectOffset(2, 2, 2, 2);
            var contentFitter = contentGo.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            scroll.content = contentRect;
            _trashContent = contentGo.transform;

            var clearRow = Row(list, "TrashClearRow");
            AddSmall(clearRow, "Clear Trash", () => MapGeometry.ClearTrash());
        }

        private void RefreshTrashPanel()
        {
            if (_trashContent == null)
            {
                return;
            }
            for (int i = _trashContent.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(_trashContent.GetChild(i).gameObject);
            }

            int count = MapGeometry.TrashCount;
            if (count == 0)
            {
                var empty = ModWindow.MakeText(_trashContent, "(empty)", 10, TextAnchor.MiddleLeft);
                empty.color = new Color(0.55f, 0.55f, 0.55f, 1f);
                UiFactory.FixHeight(empty, 22f);
                return;
            }

            var trash = MapGeometry.TrashBin;
            for (int i = 0; i < count; i++)
            {
                int capturedIndex = i;
                var layer = trash[i];

                var rowGo = new GameObject("TrashRow_" + i);
                rowGo.transform.SetParent(_trashContent, false);
                var rowRect = rowGo.AddComponent<RectTransform>();
                var rowEl = rowGo.AddComponent<LayoutElement>();
                rowEl.preferredHeight = 24f;
                rowEl.minHeight = 24f;
                var rowLayout = rowGo.AddComponent<HorizontalLayoutGroup>();
                rowLayout.spacing = 4f;
                rowLayout.childForceExpandWidth = false;
                rowLayout.childForceExpandHeight = true;
                rowLayout.childControlHeight = true;

                var labelGo = new GameObject("TrashLabel");
                labelGo.transform.SetParent(rowGo.transform, false);
                labelGo.AddComponent<RectTransform>();
                var labelEl = labelGo.AddComponent<LayoutElement>();
                labelEl.flexibleWidth = 1f;
                var label = labelGo.AddComponent<Text>();
                label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                label.fontSize = 10;
                label.color = new Color(0.85f, 0.75f, 0.65f, 1f);
                label.alignment = TextAnchor.MiddleLeft;
                label.text = i + ": " + layer.Name + " [" + layer.Type + "] native " + layer.BackingLayer;
                label.raycastTarget = false;

                var restoreBtn = UiFactory.Button(rowGo.transform, "Restore", () =>
                    MapGeometry.RestoreFromTrash(capturedIndex));
                var restoreEl = restoreBtn.gameObject.AddComponent<LayoutElement>();
                restoreEl.preferredWidth = 60f;
                restoreEl.flexibleWidth = 0f;
                UiFactory.FixHeight(restoreBtn, 24f);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_trashContent as RectTransform);
        }

        private static void OnSaveAndReload()
        {
            var dialog = ModWindow.Create("Reload Editor?", 360f, 140f);
            var list = UiFactory.VerticalList(dialog.Content, 24f, 6f);

            var msg = ModWindow.MakeText(list, "Save the level and exit to the menu?\nYou can re-open the level from the level list.", 11, TextAnchor.MiddleCenter);
            UiFactory.FixHeight(msg, 46f);

            var btnRow = new GameObject("BtnRow");
            btnRow.transform.SetParent(list, false);
            var rowRect = btnRow.AddComponent<RectTransform>();
            var rowLayout = btnRow.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 6f;
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = true;
            UiFactory.FixHeight(rowRect, 28f);

            UiFactory.Button(btnRow.transform, "Save & Exit", () =>
            {
                try
                {
                    EditorSession.Save();
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("[E2E] Save failed before reload: " + ex.Message);
                    return;
                }
                dialog.SetVisible(false);
                GlobalStart.GetInstance().EndEditorLevel();
            });
            UiFactory.Button(btnRow.transform, "Cancel", () => dialog.SetVisible(false));
            dialog.SetVisible(true);
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
            RefreshTrashPanel();
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
