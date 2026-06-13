using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace E2EApi.UI
{
    /// <summary>
    /// A <see cref="ModWindow"/> with a horizontal tab strip. Each tab owns a
    /// content panel; exactly one panel is active at a time.
    /// </summary>
    public class TabbedWindow
    {
        private const float TabHeight = 26f;

        public ModWindow Window { get; }
        private readonly RectTransform _tabStrip;
        private readonly List<KeyValuePair<Button, RectTransform>> _tabs =
            new List<KeyValuePair<Button, RectTransform>>();

        private TabbedWindow(ModWindow window)
        {
            Window = window;

            var stripGo = new GameObject("TabStrip");
            stripGo.transform.SetParent(window.Content, false);
            _tabStrip = stripGo.AddComponent<RectTransform>();
            _tabStrip.anchorMin = new Vector2(0f, 1f);
            _tabStrip.anchorMax = new Vector2(1f, 1f);
            _tabStrip.pivot = new Vector2(0.5f, 1f);
            _tabStrip.sizeDelta = new Vector2(0f, TabHeight);
            _tabStrip.anchoredPosition = Vector2.zero;
            var layout = stripGo.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 2f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
        }

        public static TabbedWindow Create(string title, float width = 460f, float height = 380f)
            => new TabbedWindow(ModWindow.Create(title, width, height));

        /// <summary>Add a tab; returns the panel to fill with content.</summary>
        public RectTransform AddTab(string label)
        {
            var buttonGo = new GameObject($"Tab_{label}");
            buttonGo.transform.SetParent(_tabStrip, false);
            var image = buttonGo.AddComponent<Image>();
            image.color = new Color(0.22f, 0.22f, 0.28f, 1f);
            var button = buttonGo.AddComponent<Button>();
            button.targetGraphic = image;
            var text = ModWindow.MakeText(buttonGo.transform, label, 13, TextAnchor.MiddleCenter);
            ModWindow.Stretch(text.rectTransform);

            var panelGo = new GameObject($"Panel_{label}");
            panelGo.transform.SetParent(Window.Content, false);
            var panel = panelGo.AddComponent<RectTransform>();
            panel.anchorMin = Vector2.zero;
            panel.anchorMax = Vector2.one;
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = new Vector2(0f, -(TabHeight + 2f));

            int index = _tabs.Count;
            _tabs.Add(new KeyValuePair<Button, RectTransform>(button, panel));
            button.onClick.AddListener(() => Select(index));

            if (index == 0)
            {
                Select(0);
            }
            else
            {
                panelGo.SetActive(false);
            }
            return panel;
        }

        public void Select(int index)
        {
            for (int i = 0; i < _tabs.Count; i++)
            {
                bool active = i == index;
                _tabs[i].Value.gameObject.SetActive(active);
                _tabs[i].Key.targetGraphic.color = active
                    ? new Color(0.32f, 0.32f, 0.42f, 1f)
                    : new Color(0.22f, 0.22f, 0.28f, 1f);
            }
        }
    }
}
