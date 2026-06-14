using System.Collections.Generic;
using E2EApi.Events;
using E2EApi.Features;
using E2EApi.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace E2EApi.Editor
{
    /// <summary>
    /// Replaces the fixed six-layer vanilla tab strip with a scrollable list
    /// driven by <see cref="MapGeometry"/> virtual layers.
    /// Plain Button+Image+Text rows are used instead of cloning T17Button to
    /// avoid private-field initialization ordering issues and hitbox mismatches.
    /// </summary>
    public static class VirtualLayerListUi
    {
        private const float RowSpacing = 2f;
        private const float RowHeight = 34f;
        /// <summary>Visible height of the scroll root; ~20 rows at 34px each.</summary>
        private const float ScrollViewHeight = 680f;

        private static bool _built;
        private static GameObject _scrollRoot;
        private static ScrollRect _scroll;
        private static RectTransform _content;
        private static T17TabPanel _vanillaTabGroup;
        private static readonly List<Button> _buttons = new List<Button>();

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

            // Hide all vanilla tab buttons — we replace them entirely.
            foreach (var button in _vanillaTabGroup.m_Buttons)
            {
                if (button != null)
                {
                    button.gameObject.SetActive(false);
                }
            }

            // Parent our scroll root to the same container as the vanilla buttons,
            // filling it completely. LayoutElement.ignoreLayout prevents the parent's
            // VerticalLayoutGroup (if any) from fighting our stretch anchors.
            var anchor = _vanillaTabGroup.m_Buttons[0].transform.parent;
            if (anchor == null)
            {
                anchor = _vanillaTabGroup.transform;
            }

            _scrollRoot = new GameObject("E2E_VirtualLayerList");
            _scrollRoot.transform.SetParent(anchor, false);
            var rootRect = _scrollRoot.AddComponent<RectTransform>();
            // Anchor to top of parent container, extend down with an explicit large height
            // so ~20 floors are visible without excessive scrolling (10x the original 2-row height).
            rootRect.anchorMin = new Vector2(0f, 1f);
            rootRect.anchorMax = new Vector2(1f, 1f);
            rootRect.pivot = new Vector2(0.5f, 1f);
            rootRect.anchoredPosition = Vector2.zero;
            rootRect.sizeDelta = new Vector2(0f, ScrollViewHeight);
            var rootLayoutEl = _scrollRoot.AddComponent<LayoutElement>();
            rootLayoutEl.ignoreLayout = true;

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
            // DestroyImmediate avoids stale child references within the same frame.
            foreach (var btn in _buttons)
            {
                if (btn != null)
                {
                    Object.DestroyImmediate(btn.gameObject);
                }
            }
            _buttons.Clear();

            var state = MapGeometry.Current;
            int selected = MapGeometry.SelectedVirtualLayerIndex;

            for (int i = 0; i < state.Layers.Count; i++)
            {
                int capturedIndex = i;
                var layer = state.Layers[i];
                bool isSelected = (i == selected);
                bool isHidden = layer.Hidden;

                var rowGo = new GameObject("E2E_Layer_" + i);
                rowGo.transform.SetParent(_content, false);

                var le = rowGo.AddComponent<LayoutElement>();
                le.minHeight = RowHeight;
                le.preferredHeight = RowHeight;
                le.flexibleHeight = 0f;

                var bg = rowGo.AddComponent<Image>();

                var btn = rowGo.AddComponent<Button>();
                btn.targetGraphic = bg;
                ApplyButtonStyle(btn, bg, isSelected, isHidden);
                btn.onClick.AddListener(() => SelectLayer(capturedIndex));

                // Right-click to toggle hidden state
                var trigger = rowGo.AddComponent<EventTrigger>();
                var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
                entry.callback.AddListener(eventData =>
                {
                    var pe = eventData as PointerEventData;
                    if (pe != null && pe.button == PointerEventData.InputButton.Right)
                    {
                        MapGeometry.SetLayerHidden(capturedIndex,
                            !MapGeometry.GetLayer(capturedIndex).Hidden);
                    }
                });
                trigger.triggers.Add(entry);

                var textGo = new GameObject("Label");
                textGo.transform.SetParent(rowGo.transform, false);
                var textRect = textGo.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(8f, 2f);
                textRect.offsetMax = new Vector2(-4f, -2f);
                var label = textGo.AddComponent<Text>();
                label.text = FormatLabel(i, layer);
                label.fontSize = 11;
                label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                label.color = isHidden ? new Color(0.55f, 0.55f, 0.55f, 1f) : Color.white;
                label.alignment = TextAnchor.MiddleLeft;
                label.horizontalOverflow = HorizontalWrapMode.Overflow;
                label.verticalOverflow = VerticalWrapMode.Truncate;
                label.raycastTarget = false;

                _buttons.Add(btn);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
        }

        private static void ApplyButtonStyle(Button btn, Image bg, bool selected, bool hidden = false)
        {
            Color normalColor;
            Color highlightColor;
            if (selected)
            {
                normalColor = new Color(0.30f, 0.50f, 0.90f, 1f);
                highlightColor = new Color(0.40f, 0.60f, 1.00f, 1f);
            }
            else if (hidden)
            {
                normalColor = new Color(0.12f, 0.12f, 0.15f, 1f);
                highlightColor = new Color(0.20f, 0.20f, 0.25f, 1f);
            }
            else
            {
                normalColor = new Color(0.18f, 0.18f, 0.24f, 1f);
                highlightColor = new Color(0.28f, 0.28f, 0.36f, 1f);
            }
            bg.color = normalColor;
            var colors = btn.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = highlightColor;
            colors.pressedColor = new Color(0.20f, 0.40f, 0.80f, 1f);
            colors.fadeDuration = 0.05f;
            btn.colors = colors;
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
            string prefix = layer.Hidden ? "[H] " : "";
            return prefix + index + ": " + layer.Name;
        }
    }
}
