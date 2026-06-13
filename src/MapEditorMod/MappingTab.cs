using System.Collections.Generic;
using E2EApi.Editor;
using E2EApi.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MapEditorMod
{
    /// <summary>
    /// The "Mapping" tab: search box + kind filter + scrollable icon grid of
    /// every spawnable block. Clicking a block makes it the active brush.
    /// </summary>
    internal class MappingTab
    {
        private const float CellSize = 64f;

        private readonly RectTransform _panel;
        private InputField _search;
        private RectTransform _grid;
        private Text _status;
        private BlockKind? _kindFilter;
        private List<BlockInfo> _blocks = new List<BlockInfo>();

        public MappingTab(RectTransform panel)
        {
            _panel = panel;
            Build();
        }

        public void Refresh()
        {
            _blocks = Blocks.GetSpawnList();
            Repopulate();
        }

        private void Build()
        {
            // top bar: search + filters
            var topGo = new GameObject("TopBar");
            topGo.transform.SetParent(_panel, false);
            var top = topGo.AddComponent<RectTransform>();
            top.anchorMin = new Vector2(0f, 1f);
            top.anchorMax = new Vector2(1f, 1f);
            top.pivot = new Vector2(0.5f, 1f);
            top.sizeDelta = new Vector2(0f, 30f);
            var topLayout = topGo.AddComponent<HorizontalLayoutGroup>();
            topLayout.spacing = 4f;
            topLayout.padding = new RectOffset(4, 4, 2, 2);
            topLayout.childForceExpandWidth = false;
            topLayout.childForceExpandHeight = true;

            _search = MakeSearchField(top);
            var searchElement = _search.gameObject.AddComponent<LayoutElement>();
            searchElement.preferredWidth = 160f;
            searchElement.minWidth = 120f;

            AddFilterButton(top, "All", null);
            AddFilterButton(top, "Tile", BlockKind.Tile);
            AddFilterButton(top, "Wall", BlockKind.Wall);
            AddFilterButton(top, "Obj", BlockKind.Object);
            AddFilterButton(top, "Deco", BlockKind.Decoration);
            AddFilterButton(top, "Room", BlockKind.Room);

            var refresh = UiFactory.Button(top, "↻", Refresh);
            var refreshElement = refresh.gameObject.AddComponent<LayoutElement>();
            refreshElement.preferredWidth = 28f;

            // status line at the bottom
            _status = UiFactory.Label(_panel, "open the level editor, then refresh", 11);
            var statusRect = _status.rectTransform;
            statusRect.anchorMin = new Vector2(0f, 0f);
            statusRect.anchorMax = new Vector2(1f, 0f);
            statusRect.pivot = new Vector2(0.5f, 0f);
            statusRect.sizeDelta = new Vector2(-8f, 18f);
            statusRect.anchoredPosition = new Vector2(4f, 2f);

            // scroll view with grid
            var scrollGo = new GameObject("Scroll");
            scrollGo.transform.SetParent(_panel, false);
            var scrollRect = scrollGo.AddComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = new Vector2(0f, 22f);
            scrollRect.offsetMax = new Vector2(0f, -32f);
            scrollGo.AddComponent<Image>().color = new Color(0.10f, 0.10f, 0.12f, 0.8f);
            scrollGo.AddComponent<Mask>().showMaskGraphic = true;
            var scroll = scrollGo.AddComponent<ScrollRect>();
            scroll.horizontal = false;

            var gridGo = new GameObject("Grid");
            gridGo.transform.SetParent(scrollGo.transform, false);
            _grid = gridGo.AddComponent<RectTransform>();
            _grid.anchorMin = new Vector2(0f, 1f);
            _grid.anchorMax = new Vector2(1f, 1f);
            _grid.pivot = new Vector2(0.5f, 1f);
            var gridLayout = gridGo.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(CellSize, CellSize + 16f);
            gridLayout.spacing = new Vector2(4f, 4f);
            gridLayout.padding = new RectOffset(4, 4, 4, 4);
            var fitter = gridGo.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = _grid;

            _search.onValueChanged.AddListener(_ => Repopulate());
        }

        private InputField MakeSearchField(Transform parent)
        {
            var go = new GameObject("Search");
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.color = new Color(0.08f, 0.08f, 0.10f, 1f);
            var input = go.AddComponent<InputField>();
            var text = ModWindowText(go.transform, "", TextAnchor.MiddleLeft);
            var placeholder = ModWindowText(go.transform, "search…", TextAnchor.MiddleLeft);
            placeholder.color = new Color(1f, 1f, 1f, 0.35f);
            input.textComponent = text;
            input.placeholder = placeholder;
            return input;
        }

        private static Text ModWindowText(Transform parent, string value, TextAnchor anchor)
        {
            var text = UiFactory.Label(parent, value, 13);
            text.alignment = anchor;
            var rect = text.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(6f, 0f);
            rect.offsetMax = new Vector2(-6f, 0f);
            return text;
        }

        private void AddFilterButton(Transform parent, string label, BlockKind? kind)
        {
            var button = UiFactory.Button(parent, label, () => { _kindFilter = kind; Repopulate(); });
            var element = button.gameObject.AddComponent<LayoutElement>();
            element.preferredWidth = 44f;
        }

        private void Repopulate()
        {
            for (int i = _grid.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(_grid.GetChild(i).gameObject);
            }

            string query = _search.text != null ? _search.text.ToLowerInvariant() : "";
            int shown = 0;
            foreach (var block in _blocks)
            {
                if (_kindFilter.HasValue && block.Kind != _kindFilter.Value)
                {
                    continue;
                }
                if (query.Length > 0 &&
                    (block.DisplayName == null || !block.DisplayName.ToLowerInvariant().Contains(query)))
                {
                    continue;
                }
                AddCell(block);
                shown++;
            }
            _status.text = _blocks.Count == 0
                ? "no blocks — open the level editor, then refresh"
                : $"{shown}/{_blocks.Count} blocks";
        }

        private void AddCell(BlockInfo block)
        {
            var cellGo = new GameObject($"Block_{block.Id}");
            cellGo.transform.SetParent(_grid, false);
            var image = cellGo.AddComponent<Image>();
            image.color = block.EditorOnly
                ? new Color(0.30f, 0.22f, 0.22f, 1f)
                : new Color(0.20f, 0.20f, 0.25f, 1f);
            var button = cellGo.AddComponent<Button>();
            button.targetGraphic = image;
            int id = block.Id;
            string name = block.DisplayName;
            button.onClick.AddListener(() =>
            {
                bool ok = Blocks.SelectBrush(id);
                _status.text = ok ? $"brush: {name} (#{id})" : "editor not open";
            });

            if (block.Icon != null && block.Icon.mainTexture != null)
            {
                var iconGo = new GameObject("Icon");
                iconGo.transform.SetParent(cellGo.transform, false);
                var icon = iconGo.AddComponent<RawImage>();
                icon.texture = block.Icon.mainTexture;
                icon.uvRect = new Rect(
                    block.Icon.mainTextureOffset.x, block.Icon.mainTextureOffset.y,
                    block.Icon.mainTextureScale.x, block.Icon.mainTextureScale.y);
                var iconRect = icon.rectTransform;
                iconRect.anchorMin = new Vector2(0f, 0.25f);
                iconRect.anchorMax = Vector2.one;
                iconRect.offsetMin = new Vector2(4f, 0f);
                iconRect.offsetMax = new Vector2(-4f, -4f);
            }

            var label = UiFactory.Label(cellGo.transform, block.DisplayName, 9);
            label.alignment = TextAnchor.LowerCenter;
            var labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = new Vector2(1f, 0.25f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
        }
    }
}
