using System.Collections.Generic;
using E2EApi.Events;
using E2EApi.Features;
using E2EApi.UI;
using UnityEngine;
using UnityEngine.UI;

namespace E2EApi.Editor
{
    /// <summary>
    /// Replaces the fixed six-layer vanilla tab strip with a scrollable list
    /// driven by <see cref="MapGeometry"/> virtual layers.
    /// </summary>
    public static class VirtualLayerListUi
    {
        private const float RowSpacing = 2f;
        private const float MaxViewportHeight = 280f;

        private static bool _built;
        private static GameObject _scrollRoot;
        private static ScrollRect _scroll;
        private static RectTransform _content;
        private static T17Button _template;
        private static T17TabPanel _vanillaTabGroup;
        private static readonly List<T17Button> _buttons = new List<T17Button>();

        public static bool IsActive => _built;

        public static void Initialise()
        {
            MapGeometry.Changed += Refresh;
            GameEvents.EditorExited += Reset;
            Patches.VirtualLayerListPatch.EnsurePatched();
        }

        public static void Reset()
        {
            _buttons.Clear();
            if (_scrollRoot != null)
            {
                Object.Destroy(_scrollRoot);
                _scrollRoot = null;
                _scroll = null;
                _content = null;
            }
            if (_vanillaTabGroup != null && _vanillaTabGroup.m_Buttons != null)
            {
                foreach (var button in _vanillaTabGroup.m_Buttons)
                {
                    if (button != null)
                    {
                        button.gameObject.SetActive(true);
                    }
                }
            }
            _built = false;
            _template = null;
            _vanillaTabGroup = null;
        }

        public static void Refresh()
        {
            if (!GameEvents.IsEditorActive)
            {
                return;
            }
            var ui = LevelEditor_UIController.GetInstance();
            if (ui == null)
            {
                return;
            }
            if (!EnsureBuilt(ui))
            {
                return;
            }
            RebuildButtons();
            ScrollToSelected(MapGeometry.SelectedVirtualLayerIndex);
        }

        public static void SelectLayer(int index)
        {
            MapGeometry.SelectLayer(index);
            var ui = LevelEditor_UIController.GetInstance();
            if (ui != null)
            {
                ui.ExternalChangeLayer(
                    (BaseLevelManager.LevelLayers)MapGeometry.GetBackingLayer(index));
                ui.UpdateLayerChanges();
            }
            Refresh();
        }

        private static bool EnsureBuilt(LevelEditor_UIController ui)
        {
            if (_built)
            {
                return true;
            }
            _vanillaTabGroup = ui.m_LayerTabGroup;
            if (_vanillaTabGroup == null || _vanillaTabGroup.m_Buttons == null ||
                _vanillaTabGroup.m_Buttons.Length == 0)
            {
                return false;
            }
            _template = _vanillaTabGroup.m_Buttons[0];
            if (_template == null)
            {
                return false;
            }

            foreach (var button in _vanillaTabGroup.m_Buttons)
            {
                if (button != null)
                {
                    button.gameObject.SetActive(false);
                }
            }

            var anchor = _template.transform.parent;
            if (anchor == null)
            {
                anchor = _vanillaTabGroup.transform;
            }

            _scrollRoot = new GameObject("E2E_VirtualLayerList");
            _scrollRoot.transform.SetParent(anchor, false);
            var rootRect = _scrollRoot.AddComponent<RectTransform>();
            CopyRect(_template.GetComponent<RectTransform>(), rootRect);
            rootRect.sizeDelta = new Vector2(rootRect.sizeDelta.x, MaxViewportHeight);

            _scroll = _scrollRoot.AddComponent<ScrollRect>();
            _scroll.horizontal = false;
            _scroll.vertical = true;
            _scroll.movementType = ScrollRect.MovementType.Clamped;
            _scroll.scrollSensitivity = 24f;

            var viewportGo = new GameObject("Viewport");
            viewportGo.transform.SetParent(_scrollRoot.transform, false);
            var viewportRect = viewportGo.AddComponent<RectTransform>();
            ModWindow.Stretch(viewportRect);
            viewportGo.AddComponent<RectMask2D>();

            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(viewportGo.transform, false);
            _content = contentGo.AddComponent<RectTransform>();
            _content.anchorMin = new Vector2(0f, 1f);
            _content.anchorMax = new Vector2(1f, 1f);
            _content.pivot = new Vector2(0.5f, 1f);
            _content.anchoredPosition = Vector2.zero;
            _content.sizeDelta = new Vector2(0f, 0f);

            var layout = contentGo.AddComponent<VerticalLayoutGroup>();
            layout.spacing = RowSpacing;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.padding = new RectOffset(0, 0, 2, 2);

            var fitter = contentGo.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            _scroll.viewport = viewportRect;
            _scroll.content = _content;
            _built = true;
            return true;
        }

        private static void RebuildButtons()
        {
            foreach (var button in _buttons)
            {
                if (button != null)
                {
                    Object.Destroy(button.gameObject);
                }
            }
            _buttons.Clear();

            var state = MapGeometry.Current;
            int selected = MapGeometry.SelectedVirtualLayerIndex;
            float rowHeight = _template.GetComponent<RectTransform>().sizeDelta.y;
            if (rowHeight <= 0f)
            {
                rowHeight = 28f;
            }

            for (int i = 0; i < state.Layers.Count; i++)
            {
                int index = i;
                var layer = state.Layers[i];
                var clone = Object.Instantiate(_template.gameObject);
                clone.name = "E2E_Layer_" + i;
                clone.transform.SetParent(_content, false);
                clone.SetActive(true);

                var rect = clone.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(rect.sizeDelta.x, rowHeight);

                var button = clone.GetComponent<T17Button>();
                SetLabel(button, FormatLabel(i, layer));
                StyleButton(button, i == selected);

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => SelectLayer(index));
                _buttons.Add(button);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
        }

        private static void ScrollToSelected(int index)
        {
            if (_scroll == null || _content == null || _buttons.Count == 0)
            {
                return;
            }
            Canvas.ForceUpdateCanvases();
            float viewportHeight = _scroll.viewport.rect.height;
            float contentHeight = _content.rect.height;
            if (contentHeight <= viewportHeight + 1f)
            {
                _scroll.verticalNormalizedPosition = 1f;
                return;
            }

            var rowRect = _buttons[Mathf.Clamp(index, 0, _buttons.Count - 1)]
                .GetComponent<RectTransform>();
            float rowTop = -rowRect.anchoredPosition.y;
            float rowBottom = rowTop + rowRect.rect.height;
            float scrollY = (1f - _scroll.verticalNormalizedPosition) * (contentHeight - viewportHeight);

            if (rowTop < scrollY)
            {
                scrollY = rowTop;
            }
            else if (rowBottom > scrollY + viewportHeight)
            {
                scrollY = rowBottom - viewportHeight;
            }

            float maxScroll = contentHeight - viewportHeight;
            _scroll.verticalNormalizedPosition = 1f - Mathf.Clamp01(scrollY / maxScroll);
        }

        private static string FormatLabel(int index, MapGeometry.VirtualLayer layer)
        {
            return index + ": " + layer.Name;
        }

        private static void SetLabel(T17Button button, string text)
        {
            if (button == null)
            {
                return;
            }
            if (button.m_ButtonText != null)
            {
                button.m_ButtonText.text = text;
                return;
            }
            var label = button.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.text = text;
            }
        }

        private static void StyleButton(T17Button button, bool selected)
        {
            if (button == null)
            {
                return;
            }
            var colors = button.colors;
            if (selected)
            {
                colors.normalColor = new Color(0.45f, 0.65f, 0.95f, 1f);
                colors.highlightedColor = new Color(0.55f, 0.75f, 1f, 1f);
            }
            else
            {
                colors.normalColor = new Color(0.28f, 0.28f, 0.34f, 1f);
                colors.highlightedColor = new Color(0.38f, 0.38f, 0.46f, 1f);
            }
            button.colors = colors;
        }

        private static void CopyRect(RectTransform source, RectTransform target)
        {
            target.anchorMin = source.anchorMin;
            target.anchorMax = source.anchorMax;
            target.pivot = source.pivot;
            target.anchoredPosition = source.anchoredPosition;
            target.sizeDelta = source.sizeDelta;
            target.offsetMin = source.offsetMin;
            target.offsetMax = source.offsetMax;
        }
    }
}
