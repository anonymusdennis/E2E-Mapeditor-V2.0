using System.Collections.Generic;
using E2EApi.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MapEditorMod
{
    /// <summary>Dedicated in-game settings window for connected texture brushes.</summary>
    internal class ConnectedBrushWindow
    {
        private ModWindow _window;
        private Text _summary;
        private Text _autoLabel;
        private Text _lockLabel;
        private Text _colorLabel;
        private Text _flagsLabel;
        private Text _maskLabel;
        private Text _variantLabel;
        private readonly List<Text> _maskLabels = new List<Text>();
        private readonly List<Text> _variantLabels = new List<Text>();
        private int _selectedMask;

        public void Initialise()
        {
            EditorTools.Changed += RefreshForBrushState;
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

        private void RefreshForBrushState()
        {
            if (EditorTools.ConnectedStamp == null)
            {
                Hide();
                return;
            }
            if (EditorTools.Mode == EditorToolMode.PaintConnectedTile)
            {
                Show();
                return;
            }
            Refresh();
        }

        private void Ensure()
        {
            if (_window != null)
            {
                return;
            }

            _window = ModWindow.Create("Connected Texture Brush", 470f, 560f);
            var list = UiFactory.VerticalList(_window.Content, 24f, 4f);

            _summary = UiFactory.Label(list, "", 12);
            _summary.color = new Color(1f, 0.92f, 0.72f, 1f);
            UiFactory.FixHeight(_summary, 44f);

            _autoLabel = AddButton(list, "", () =>
                EditorTools.SetConnectedAuto(!(EditorTools.ConnectedStamp != null &&
                    EditorTools.ConnectedStamp.Auto)));
            _lockLabel = AddButton(list, "", () =>
                EditorTools.SetConnectedLock(!(EditorTools.ConnectedStamp != null &&
                    EditorTools.ConnectedStamp.LockPlaced)));
            _colorLabel = AddButton(list, "", () =>
                EditorTools.ConnectedNextColor());

            var colors = AddButton(list, "", () =>
                EditorTools.SetShowConnectedColors(!EditorTools.ShowConnectedColors));
            _flagsLabel = colors;

            AddFlagRow(list);
            AddMaskGrid(list);

            _maskLabel = UiFactory.Label(list, "", 12);
            UiFactory.FixHeight(_maskLabel, 24f);

            AddVariantRows(list);

            var auto = UiFactory.Button(list, "Return to auto-connected mode", () =>
                EditorTools.SetConnectedAuto(true));
            UiFactory.FixHeight(auto, 24f);
            var clear = UiFactory.Button(list, "Clear brush (Escape)", () =>
                EditorTools.ClearBrush());
            UiFactory.FixHeight(clear, 24f);

            _window.SetVisible(false);
        }

        private Text AddButton(Transform list, string caption, System.Action action)
        {
            var button = UiFactory.Button(list, caption, action);
            UiFactory.FixHeight(button, 24f);
            return button.GetComponentInChildren<Text>();
        }

        private void AddFlagRow(Transform list)
        {
            var row = new GameObject("ConnectedFlags");
            row.transform.SetParent(list, false);
            var rect = row.AddComponent<RectTransform>();
            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 4f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            UiFactory.FixHeight(rect, 24f);

            AddSmallButton(row.transform, "Damage", () => EditorTools.ConnectedToggleDamaging());
            AddSmallButton(row.transform, "Collision", () => EditorTools.ConnectedToggleCollision());
            AddSmallButton(row.transform, "Destruct", () => EditorTools.ConnectedToggleDestructible());
        }

        private void AddMaskGrid(Transform list)
        {
            var label = UiFactory.Label(list, "Single subtile selector", 12);
            UiFactory.FixHeight(label, 20f);

            var grid = new GameObject("MaskGrid");
            grid.transform.SetParent(list, false);
            var rect = grid.AddComponent<RectTransform>();
            var layout = grid.AddComponent<GridLayoutGroup>();
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 4;
            layout.cellSize = new Vector2(104f, 32f);
            layout.spacing = new Vector2(4f, 4f);
            UiFactory.FixHeight(rect, 144f);

            for (int mask = 0; mask < 16; mask++)
            {
                int captured = mask;
                var button = UiFactory.Button(grid.transform, "", () =>
                {
                    _selectedMask = captured;
                    EditorTools.SetConnectedManualMask(captured);
                });
                _maskLabels.Add(button.GetComponentInChildren<Text>());
            }
        }

        private void AddVariantRows(Transform list)
        {
            var title = UiFactory.Label(list, "Variations for selected subtile", 12);
            title.color = new Color(0.7f, 0.85f, 1f, 1f);
            UiFactory.FixHeight(title, 22f);
            _variantLabel = title;

            for (int i = 0; i < 4; i++)
            {
                int captured = i;
                var row = new GameObject("Variant" + i);
                row.transform.SetParent(list, false);
                var rect = row.AddComponent<RectTransform>();
                var layout = row.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = 4f;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = true;
                UiFactory.FixHeight(rect, 24f);

                var text = UiFactory.Label(row.transform, "", 11);
                _variantLabels.Add(text);
                var down = UiFactory.Button(row.transform, "-10", () => AdjustVariant(captured, -10));
                var up = UiFactory.Button(row.transform, "+10", () => AdjustVariant(captured, 10));
                UiFactory.FixHeight(down, 24f);
                UiFactory.FixHeight(up, 24f);
            }
        }

        private void AddSmallButton(Transform parent, string caption, System.Action action)
        {
            var button = UiFactory.Button(parent, caption, action);
            UiFactory.FixHeight(button, 24f);
        }

        private void AdjustVariant(int index, int delta)
        {
            var stamp = EditorTools.ConnectedStamp;
            if (stamp == null)
            {
                return;
            }
            List<ConnectedVariant> variants;
            if (!stamp.Variants.TryGetValue(_selectedMask, out variants) ||
                index < 0 || index >= variants.Count)
            {
                return;
            }
            EditorTools.SetConnectedVariantWeight(_selectedMask, index,
                variants[index].Weight + delta);
        }

        private void Refresh()
        {
            if (_summary == null)
            {
                return;
            }
            var stamp = EditorTools.ConnectedStamp;
            if (stamp == null)
            {
                _summary.text = "No connected texture brush armed.";
                return;
            }

            _summary.text = stamp.Name + " (" + stamp.Mode + ")\n" +
                "Left-click paints. Right-click erases this connected texture. Shift locks placed tiles.";
            _autoLabel.text = stamp.Auto
                ? "Auto-connect mode: ON"
                : "Single subtile mode: mask " + (stamp.ManualMask < 0 ? 15 : stamp.ManualMask);
            _lockLabel.text = stamp.LockPlaced ? "Lock brush: ON" : "Lock brush: off";
            _colorLabel.text = "Color group: " + stamp.Color + " (same colors connect)";
            _flagsLabel.text = EditorTools.ShowConnectedColors
                ? "View tileset colors: ON" : "View tileset colors: off";
            _maskLabel.text = "Selected mask " + _selectedMask +
                " (N/E/S/W bits: " + MaskBits(_selectedMask) + ")";
            _variantLabel.text = "Variations for mask " + _selectedMask;

            RefreshMaskLabels(stamp);
            RefreshVariantLabels(stamp);
        }

        private void RefreshMaskLabels(ConnectedTileStamp stamp)
        {
            for (int mask = 0; mask < _maskLabels.Count; mask++)
            {
                List<ConnectedVariant> variants;
                int count = stamp.Variants.TryGetValue(mask, out variants) ? variants.Count : 0;
                string marker = mask == _selectedMask ? "* " : "";
                _maskLabels[mask].text = marker + mask + " " + MaskBits(mask) +
                    (count > 1 ? " x" + count : count == 1 ? " x1" : " empty");
            }
        }

        private void RefreshVariantLabels(ConnectedTileStamp stamp)
        {
            List<ConnectedVariant> variants;
            stamp.Variants.TryGetValue(_selectedMask, out variants);
            for (int i = 0; i < _variantLabels.Count; i++)
            {
                if (variants != null && i < variants.Count)
                {
                    var v = variants[i];
                    _variantLabels[i].text = "Variant " + (i + 1) + ": " +
                        v.Weight + "% " + v.W + "x" + v.H;
                }
                else
                {
                    _variantLabels[i].text = "Variant " + (i + 1) + ": none";
                }
            }
        }

        private static string MaskBits(int mask)
        {
            return ((mask & 1) != 0 ? "N" : "-") +
                ((mask & 2) != 0 ? "E" : "-") +
                ((mask & 4) != 0 ? "S" : "-") +
                ((mask & 8) != 0 ? "W" : "-");
        }
    }
}
