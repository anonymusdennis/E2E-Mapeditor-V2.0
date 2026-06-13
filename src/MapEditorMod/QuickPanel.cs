using E2EApi.Editor;
using E2EApi.Features;
using E2EApi.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MapEditorMod
{
    /// <summary>
    /// Draggable in-game settings panel for the editor. Collapses to a header bar
    /// whose hitbox matches its visible size.
    /// </summary>
    internal class QuickPanel
    {
        private const float HeaderHeight = 26f;
        private const float BodyHeight = 330f;
        private const float Width = 240f;

        private GameObject _root;
        private RectTransform _rootRect;
        private GameObject _body;
        private Text _titleLabel;
        private Text _paintLabel;
        private Text _eraseLabel;
        private Text _linkLabel;
        private Text _connectedSettingsLabel;
        private Text _connectedViewLabel;
        private Text _toolHint;
        private bool _collapsed;

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
            _collapsed = Plugin.CfgQuickPanelCollapsed.Value;

            _root = new GameObject("E2E_QuickPanel");
            _root.transform.SetParent(ModWindow.OverlayCanvas.transform, false);
            _rootRect = _root.AddComponent<RectTransform>();
            _rootRect.anchorMin = new Vector2(1f, 1f);
            _rootRect.anchorMax = new Vector2(1f, 1f);
            _rootRect.pivot = new Vector2(1f, 1f);
            _rootRect.anchoredPosition = new Vector2(
                Plugin.CfgQuickPanelX.Value, Plugin.CfgQuickPanelY.Value);
            ApplyRootSize();

            var headerGo = new GameObject("Header");
            headerGo.transform.SetParent(_root.transform, false);
            var headerRect = headerGo.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.anchoredPosition = Vector2.zero;
            headerRect.sizeDelta = new Vector2(0f, HeaderHeight);
            var headerImage = headerGo.AddComponent<Image>();
            headerImage.color = new Color(0.16f, 0.16f, 0.22f, 0.92f);

            var dragGo = new GameObject("Drag");
            dragGo.transform.SetParent(headerGo.transform, false);
            var dragRect = dragGo.AddComponent<RectTransform>();
            dragRect.anchorMin = Vector2.zero;
            dragRect.anchorMax = Vector2.one;
            dragRect.offsetMin = Vector2.zero;
            dragRect.offsetMax = new Vector2(-30f, 0f);
            var dragImage = dragGo.AddComponent<Image>();
            dragImage.color = new Color(0f, 0f, 0f, 0.01f);
            var drag = dragGo.AddComponent<PanelDrag>();
            drag.Target = _rootRect;
            _titleLabel = ModWindow.MakeText(dragGo.transform,
                _collapsed ? "E2E tools ▸" : "E2E tools ▾", 13, TextAnchor.MiddleLeft);
            var titleRect = _titleLabel.rectTransform;
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = new Vector2(8f, 0f);
            titleRect.offsetMax = Vector2.zero;
            _titleLabel.raycastTarget = false;

            var collapseGo = new GameObject("Collapse");
            collapseGo.transform.SetParent(headerGo.transform, false);
            var collapseRect = collapseGo.AddComponent<RectTransform>();
            collapseRect.anchorMin = new Vector2(1f, 0f);
            collapseRect.anchorMax = new Vector2(1f, 1f);
            collapseRect.pivot = new Vector2(1f, 0.5f);
            collapseRect.sizeDelta = new Vector2(30f, 0f);
            var collapseImage = collapseGo.AddComponent<Image>();
            collapseImage.color = new Color(0.20f, 0.20f, 0.28f, 0.92f);
            var collapseButton = collapseGo.AddComponent<Button>();
            collapseButton.targetGraphic = collapseImage;
            var collapseText = ModWindow.MakeText(collapseGo.transform, "▾", 12, TextAnchor.MiddleCenter);
            ModWindow.Stretch(collapseText.rectTransform);
            collapseButton.onClick.AddListener(ToggleCollapsed);

            _body = new GameObject("Body");
            _body.transform.SetParent(_root.transform, false);
            var bodyRect = _body.AddComponent<RectTransform>();
            bodyRect.anchorMin = new Vector2(0f, 1f);
            bodyRect.anchorMax = new Vector2(1f, 1f);
            bodyRect.pivot = new Vector2(0.5f, 1f);
            bodyRect.anchoredPosition = new Vector2(0f, -HeaderHeight);
            bodyRect.sizeDelta = new Vector2(0f, BodyHeight);
            var bodyImage = _body.AddComponent<Image>();
            bodyImage.color = new Color(0.10f, 0.10f, 0.14f, 0.92f);

            var list = UiFactory.VerticalList(bodyRect, 24f, 2f);

            var restrictions = UiFactory.Toggle(list, "Ignore ALL restrictions",
                Restrictions.IgnoreAll, v =>
                {
                    Restrictions.IgnoreAll = v;
                    Plugin.CfgIgnoreAllRestrictions.Value = v;
                });
            UiFactory.FixHeight(restrictions, 24f);

            var xray = UiFactory.Toggle(list, "X-ray hidden blocks",
                XRay.Enabled, v =>
                {
                    XRay.Enabled = v;
                    Plugin.CfgXRay.Value = v;
                });
            UiFactory.FixHeight(xray, 24f);

            var fences = UiFactory.Toggle(list, "Electric fence overlay",
                FenceOverlay.Enabled, v =>
                {
                    FenceOverlay.Enabled = v;
                    Plugin.CfgFenceOverlay.Value = v;
                });
            UiFactory.FixHeight(fences, 24f);

            var arrows = UiFactory.Toggle(list, "Show logic connections",
                TriggerArrows.Enabled, v =>
                {
                    TriggerArrows.Enabled = v;
                    Plugin.CfgTriggerArrows.Value = v;
                });
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

            var connectedSettings = UiFactory.Button(list, "",
                () => Plugin.ShowConnectedBrushWindow());
            UiFactory.FixHeight(connectedSettings, 24f);
            _connectedSettingsLabel = connectedSettings.GetComponentInChildren<Text>();

            var connectedView = UiFactory.Button(list, "",
                () =>
                {
                    EditorTools.SetShowConnectedColors(!EditorTools.ShowConnectedColors);
                });
            UiFactory.FixHeight(connectedView, 24f);
            _connectedViewLabel = connectedView.GetComponentInChildren<Text>();

            _toolHint = UiFactory.Label(list, "", 10);
            _toolHint.color = new Color(1f, 0.9f, 0.5f, 0.85f);
            UiFactory.FixHeight(_toolHint, 16f);

            _body.SetActive(!_collapsed);
            EditorTools.Changed += RefreshToolRows;
            RefreshToolRows();
        }

        private void ApplyRootSize()
        {
            float h = _collapsed ? HeaderHeight : HeaderHeight + BodyHeight;
            _rootRect.sizeDelta = new Vector2(Width, h);
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
            var ct = EditorTools.ConnectedStamp;
            _connectedSettingsLabel.text = ct == null
                ? "Connected texture settings"
                : "Connected settings: " + ct.Name;
            _connectedViewLabel.text = EditorTools.ShowConnectedColors
                ? "■ View tileset colors" : "View tileset colors";
            _toolHint.text = EditorTools.HintText();
        }

        private void ToggleCollapsed()
        {
            _collapsed = !_collapsed;
            _body.SetActive(!_collapsed);
            _titleLabel.text = _collapsed ? "E2E tools ▸" : "E2E tools ▾";
            Plugin.CfgQuickPanelCollapsed.Value = _collapsed;
            ApplyRootSize();
        }

        private class PanelDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
        {
            public RectTransform Target;
            private Vector2 _offset;

            public void OnBeginDrag(PointerEventData eventData)
            {
                var parent = Target.parent as RectTransform;
                Vector2 local;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parent, eventData.position, eventData.pressEventCamera, out local);
                _offset = Target.anchoredPosition - local;
            }

            public void OnDrag(PointerEventData eventData)
            {
                var parent = Target.parent as RectTransform;
                Vector2 local;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parent, eventData.position, eventData.pressEventCamera, out local);
                Target.anchoredPosition = local + _offset;
            }

            public void OnEndDrag(PointerEventData eventData)
            {
                Plugin.CfgQuickPanelX.Value = Target.anchoredPosition.x;
                Plugin.CfgQuickPanelY.Value = Target.anchoredPosition.y;
            }
        }
    }
}
