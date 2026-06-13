using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace E2EApi.UI
{
    /// <summary>
    /// A movable in-game window built from raw uGUI (Unity 5.5-safe):
    /// dark panel, draggable title bar, close button, content area.
    /// Create with <see cref="Create"/>, add content under <see cref="Content"/>.
    /// </summary>
    public class ModWindow : MonoBehaviour
    {
        public RectTransform Root { get; private set; }
        public RectTransform Content { get; private set; }
        public Text TitleText { get; private set; }

        private static Canvas _canvas;

        /// <summary>Shared overlay canvas for all API windows (created on demand).</summary>
        public static Canvas OverlayCanvas
        {
            get
            {
                if (_canvas == null)
                {
                    var go = new GameObject("E2EApi_UICanvas");
                    Object.DontDestroyOnLoad(go);
                    _canvas = go.AddComponent<Canvas>();
                    _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    _canvas.sortingOrder = 30000;
                    go.AddComponent<CanvasScaler>();
                    go.AddComponent<GraphicRaycaster>();
                }
                return _canvas;
            }
        }

        public static ModWindow Create(string title, float width = 420f, float height = 320f)
        {
            var winGo = new GameObject($"E2EApi_Window_{title}");
            winGo.transform.SetParent(OverlayCanvas.transform, worldPositionStays: false);

            var window = winGo.AddComponent<ModWindow>();
            var root = winGo.AddComponent<RectTransform>();
            window.Root = root;
            root.sizeDelta = new Vector2(width, height);
            root.anchoredPosition = Vector2.zero;

            var background = winGo.AddComponent<Image>();
            background.color = new Color(0.13f, 0.13f, 0.16f, 0.96f);

            // title bar
            var barGo = new GameObject("TitleBar");
            barGo.transform.SetParent(winGo.transform, false);
            var barRect = barGo.AddComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0f, 1f);
            barRect.anchorMax = new Vector2(1f, 1f);
            barRect.pivot = new Vector2(0.5f, 1f);
            barRect.sizeDelta = new Vector2(0f, 28f);
            barRect.anchoredPosition = Vector2.zero;
            var barImage = barGo.AddComponent<Image>();
            barImage.color = new Color(0.20f, 0.20f, 0.26f, 1f);
            var drag = barGo.AddComponent<DragHandle>();
            drag.Target = root;

            window.TitleText = MakeText(barGo.transform, title, 14, TextAnchor.MiddleLeft);
            var titleRect = window.TitleText.rectTransform;
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = new Vector2(8f, 0f);
            titleRect.offsetMax = new Vector2(-30f, 0f);

            // close button
            var closeGo = new GameObject("Close");
            closeGo.transform.SetParent(barGo.transform, false);
            var closeRect = closeGo.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1f, 0.5f);
            closeRect.anchorMax = new Vector2(1f, 0.5f);
            closeRect.pivot = new Vector2(1f, 0.5f);
            closeRect.sizeDelta = new Vector2(24f, 24f);
            closeRect.anchoredPosition = new Vector2(-2f, 0f);
            var closeImage = closeGo.AddComponent<Image>();
            closeImage.color = new Color(0.55f, 0.16f, 0.16f, 1f);
            var closeButton = closeGo.AddComponent<Button>();
            closeButton.targetGraphic = closeImage;
            closeButton.onClick.AddListener(() => window.SetVisible(false));
            var x = MakeText(closeGo.transform, "X", 12, TextAnchor.MiddleCenter);
            Stretch(x.rectTransform);

            // content area
            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(winGo.transform, false);
            window.Content = contentGo.AddComponent<RectTransform>();
            window.Content.anchorMin = Vector2.zero;
            window.Content.anchorMax = Vector2.one;
            window.Content.offsetMin = new Vector2(4f, 4f);
            window.Content.offsetMax = new Vector2(-4f, -32f);

            return window;
        }

        public void SetVisible(bool visible) => gameObject.SetActive(visible);
        public bool IsVisible => gameObject.activeSelf;
        public void SetTitle(string title) => TitleText.text = title;

        public static Text MakeText(Transform parent, string value, int size, TextAnchor anchor)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = size;
            text.alignment = anchor;
            text.color = Color.white;
            text.text = value;
            return text;
        }

        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        /// <summary>Title-bar drag behaviour.</summary>
        private class DragHandle : MonoBehaviour, IBeginDragHandler, IDragHandler
        {
            public RectTransform Target;
            private Vector2 _offset;

            public void OnBeginDrag(PointerEventData eventData)
            {
                _offset = (Vector2)Target.position - eventData.position;
            }

            public void OnDrag(PointerEventData eventData)
            {
                Target.position = eventData.position + _offset;
            }
        }
    }
}
