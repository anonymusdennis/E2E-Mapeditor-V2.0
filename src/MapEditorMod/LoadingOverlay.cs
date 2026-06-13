using E2EApi.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MapEditorMod
{
    /// <summary>
    /// Full-screen "mod is loading" overlay: dimmed backdrop (which also
    /// swallows every mouse click), a progress bar and a status line. Shown
    /// while the mod does its own asset work — tileset harvesting and atlas
    /// preloading after a map load — and removed the moment that finishes.
    /// Game-driven loading flows are never blocked by this.
    /// </summary>
    internal static class LoadingOverlay
    {
        private static GameObject _root;
        private static RectTransform _barFill;
        private static Text _label;

        public static bool Visible => _root != null && _root.activeSelf;

        public static void Show(string status, float progress01)
        {
            if (_root == null)
            {
                Build();
            }
            _root.SetActive(true);
            _label.text = status;
            _barFill.anchorMax = new Vector2(Mathf.Clamp01(progress01), 1f);
        }

        public static void Hide()
        {
            if (_root != null && _root.activeSelf)
            {
                _root.SetActive(false);
            }
        }

        private static void Build()
        {
            _root = new GameObject("E2E_LoadingOverlay");
            _root.transform.SetParent(ModWindow.OverlayCanvas.transform, false);
            var rootRect = _root.AddComponent<RectTransform>();
            ModWindow.Stretch(rootRect);
            _root.transform.SetAsLastSibling(); // above every other mod window

            // dimmed, click-eating backdrop
            var dim = _root.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.65f);
            dim.raycastTarget = true;

            var panel = new GameObject("Panel");
            panel.transform.SetParent(_root.transform, false);
            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(460f, 92f);
            var panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.10f, 0.10f, 0.14f, 0.97f);

            var title = ModWindow.MakeText(panel.transform, "E2E Map Editor", 13,
                TextAnchor.MiddleCenter);
            var titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -6f);
            titleRect.sizeDelta = new Vector2(0f, 18f);
            title.color = new Color(1f, 1f, 1f, 0.6f);

            _label = ModWindow.MakeText(panel.transform, "loading…", 14,
                TextAnchor.MiddleCenter);
            var labelRect = _label.rectTransform;
            labelRect.anchorMin = new Vector2(0f, 0.5f);
            labelRect.anchorMax = new Vector2(1f, 0.5f);
            labelRect.anchoredPosition = new Vector2(0f, 2f);
            labelRect.sizeDelta = new Vector2(0f, 22f);

            var barBack = new GameObject("BarBack");
            barBack.transform.SetParent(panel.transform, false);
            var backRect = barBack.AddComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0f, 0f);
            backRect.anchorMax = new Vector2(1f, 0f);
            backRect.pivot = new Vector2(0.5f, 0f);
            backRect.anchoredPosition = new Vector2(0f, 12f);
            backRect.offsetMin = new Vector2(16f, 12f);
            backRect.offsetMax = new Vector2(-16f, 26f);
            var backImage = barBack.AddComponent<Image>();
            backImage.color = new Color(0.22f, 0.22f, 0.30f, 1f);

            var fill = new GameObject("BarFill");
            fill.transform.SetParent(barBack.transform, false);
            _barFill = fill.AddComponent<RectTransform>();
            _barFill.anchorMin = Vector2.zero;
            _barFill.anchorMax = new Vector2(0f, 1f);
            _barFill.offsetMin = Vector2.zero;
            _barFill.offsetMax = Vector2.zero;
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.45f, 0.75f, 1f, 1f);
        }
    }
}
