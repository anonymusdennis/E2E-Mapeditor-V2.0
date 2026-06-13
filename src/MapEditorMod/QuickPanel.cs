using E2EApi.Editor;
using E2EApi.Features;
using E2EApi.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MapEditorMod
{
    /// <summary>
    /// Fixed-position (top-right) in-game settings panel for the editor:
    /// master restriction kill-switch, X-ray, fence overlay, logic arrows.
    /// Collapses to a small "E2E" button.
    /// </summary>
    internal class QuickPanel
    {
        private GameObject _root;
        private GameObject _body;
        private Text _collapseLabel;
        private Text _paintLabel;
        private Text _eraseLabel;
        private Text _linkLabel;
        private Text _toolHint;

        public void Show()
        {
            if (_root == null)
            {
                Build();
            }
            _root.SetActive(true);
        }

        public void Hide()
        {
            if (_root != null)
            {
                _root.SetActive(false);
            }
        }

        private void Build()
        {
            _root = new GameObject("E2E_QuickPanel");
            _root.transform.SetParent(ModWindow.OverlayCanvas.transform, false);
            var rootRect = _root.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(1f, 1f);
            rootRect.anchorMax = new Vector2(1f, 1f);
            rootRect.pivot = new Vector2(1f, 1f);
            rootRect.anchoredPosition = new Vector2(-8f, -8f);
            rootRect.sizeDelta = new Vector2(240f, 30f);

            // collapse header button
            var headerGo = new GameObject("Header");
            headerGo.transform.SetParent(_root.transform, false);
            var headerRect = headerGo.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.sizeDelta = new Vector2(0f, 26f);
            var headerImage = headerGo.AddComponent<Image>();
            headerImage.color = new Color(0.16f, 0.16f, 0.22f, 0.92f);
            var headerButton = headerGo.AddComponent<Button>();
            headerButton.targetGraphic = headerImage;
            _collapseLabel = ModWindow.MakeText(headerGo.transform, "E2E tools ▾", 13, TextAnchor.MiddleCenter);
            ModWindow.Stretch(_collapseLabel.rectTransform);
            headerButton.onClick.AddListener(ToggleCollapsed);

            // body
            _body = new GameObject("Body");
            _body.transform.SetParent(_root.transform, false);
            var bodyRect = _body.AddComponent<RectTransform>();
            bodyRect.anchorMin = new Vector2(0f, 1f);
            bodyRect.anchorMax = new Vector2(1f, 1f);
            bodyRect.pivot = new Vector2(0.5f, 1f);
            bodyRect.anchoredPosition = new Vector2(0f, -26f);
            bodyRect.sizeDelta = new Vector2(0f, 276f);
            var bodyImage = _body.AddComponent<Image>();
            bodyImage.color = new Color(0.10f, 0.10f, 0.14f, 0.92f);

            var list = UiFactory.VerticalList(bodyRect, 24f, 2f);

            var restrictions = UiFactory.Toggle(list, "Ignore ALL restrictions",
                Restrictions.IgnoreAll, v => Restrictions.IgnoreAll = v);
            UiFactory.FixHeight(restrictions, 24f);

            var xray = UiFactory.Toggle(list, "X-ray hidden blocks",
                XRay.Enabled, v => XRay.Enabled = v);
            UiFactory.FixHeight(xray, 24f);

            var fences = UiFactory.Toggle(list, "Electric fence overlay",
                FenceOverlay.Enabled, v => FenceOverlay.Enabled = v);
            UiFactory.FixHeight(fences, 24f);

            var arrows = UiFactory.Toggle(list, "Show logic connections",
                TriggerArrows.Enabled, v => TriggerArrows.Enabled = v);
            UiFactory.FixHeight(arrows, 24f);

            var cameraLock = UiFactory.Toggle(list, "Lock camera (WASD only)",
                EditorCamera.LockPan, v =>
                {
                    Plugin.CfgLockCameraPan.Value = v;
                    EditorCamera.LockPan = v;
                });
            UiFactory.FixHeight(cameraLock, 24f);

            var hint = UiFactory.Label(list, "click an arrow to jump along it", 10);
            hint.color = new Color(1f, 1f, 1f, 0.45f);
            UiFactory.FixHeight(hint, 16f);

            var paint = UiFactory.Button(list, "",
                () => EditorTools.Toggle(EditorToolMode.PaintElectric));
            UiFactory.FixHeight(paint, 24f);
            _paintLabel = paint.GetComponentInChildren<Text>();

            var erase = UiFactory.Button(list, "",
                () => EditorTools.Toggle(EditorToolMode.EraseElectric));
            UiFactory.FixHeight(erase, 24f);
            _eraseLabel = erase.GetComponentInChildren<Text>();

            var link = UiFactory.Button(list, "",
                () => EditorTools.Toggle(EditorToolMode.LinkSwitch));
            UiFactory.FixHeight(link, 24f);
            _linkLabel = link.GetComponentInChildren<Text>();

            _toolHint = UiFactory.Label(list, "", 10);
            _toolHint.color = new Color(1f, 0.9f, 0.5f, 0.85f);
            UiFactory.FixHeight(_toolHint, 16f);

            EditorTools.Changed += RefreshToolRows;
            RefreshToolRows();
        }

        private void RefreshToolRows()
        {
            if (_paintLabel == null)
            {
                return;
            }
            var mode = EditorTools.Mode;
            _paintLabel.text = mode == EditorToolMode.PaintElectric
                ? "■ Paint electricity (ON)" : "Paint electricity";
            _eraseLabel.text = mode == EditorToolMode.EraseElectric
                ? "■ Remove electricity (ON)" : "Remove electricity";
            _linkLabel.text = mode == EditorToolMode.LinkSwitch
                ? "■ Link fence → switch (ON)" : "Link fence → switch";
            _toolHint.text = EditorTools.HintText();
        }

        private void ToggleCollapsed()
        {
            bool collapsed = _body.activeSelf;
            _body.SetActive(!collapsed);
            _collapseLabel.text = collapsed ? "E2E tools ▸" : "E2E tools ▾";
        }
    }
}
