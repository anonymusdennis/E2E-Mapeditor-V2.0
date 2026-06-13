using System;
using UnityEngine;
using UnityEngine.UI;

namespace E2EApi.UI
{
    /// <summary>Small factory for sprite-less uGUI controls (Unity 5.5-safe).</summary>
    public static class UiFactory
    {
        public static Text Label(Transform parent, string text, int size = 13)
        {
            var label = ModWindow.MakeText(parent, text, size, TextAnchor.MiddleLeft);
            return label;
        }

        public static Button Button(Transform parent, string caption, Action onClick)
        {
            var go = new GameObject($"Button_{caption}");
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.color = new Color(0.25f, 0.25f, 0.33f, 1f);
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            if (onClick != null)
            {
                button.onClick.AddListener(() => onClick());
            }
            var text = ModWindow.MakeText(go.transform, caption, 13, TextAnchor.MiddleCenter);
            ModWindow.Stretch(text.rectTransform);
            return button;
        }

        public static Toggle Toggle(Transform parent, string caption, bool initial, Action<bool> onChanged)
        {
            var go = new GameObject($"Toggle_{caption}");
            go.transform.SetParent(parent, false);
            var toggle = go.AddComponent<Toggle>();

            var boxGo = new GameObject("Box");
            boxGo.transform.SetParent(go.transform, false);
            var boxRect = boxGo.AddComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0f, 0.5f);
            boxRect.anchorMax = new Vector2(0f, 0.5f);
            boxRect.pivot = new Vector2(0f, 0.5f);
            boxRect.sizeDelta = new Vector2(16f, 16f);
            boxRect.anchoredPosition = new Vector2(2f, 0f);
            var boxImage = boxGo.AddComponent<Image>();
            boxImage.color = new Color(0.30f, 0.30f, 0.38f, 1f);

            var checkGo = new GameObject("Check");
            checkGo.transform.SetParent(boxGo.transform, false);
            var checkRect = checkGo.AddComponent<RectTransform>();
            checkRect.anchorMin = Vector2.zero;
            checkRect.anchorMax = Vector2.one;
            checkRect.offsetMin = new Vector2(3f, 3f);
            checkRect.offsetMax = new Vector2(-3f, -3f);
            var checkImage = checkGo.AddComponent<Image>();
            checkImage.color = new Color(0.45f, 0.85f, 0.45f, 1f);

            var text = ModWindow.MakeText(go.transform, caption, 13, TextAnchor.MiddleLeft);
            var textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(24f, 0f);
            textRect.offsetMax = Vector2.zero;

            toggle.targetGraphic = boxImage;
            toggle.graphic = checkImage;
            toggle.isOn = initial;
            if (onChanged != null)
            {
                toggle.onValueChanged.AddListener(v => onChanged(v));
            }
            return toggle;
        }

        /// <summary>A vertical auto-layout list (rows of fixed height).</summary>
        public static RectTransform VerticalList(Transform parent, float rowHeight = 26f, float spacing = 4f)
        {
            var go = new GameObject("VerticalList");
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            ModWindow.Stretch(rect);
            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = new RectOffset(6, 6, 6, 6);
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            var fitter = go.AddComponent<LayoutElement>();
            fitter.minHeight = rowHeight;
            return rect;
        }

        /// <summary>Give a control a fixed height inside a layout group.</summary>
        public static void FixHeight(Component control, float height)
        {
            var element = control.gameObject.GetComponent<LayoutElement>();
            if (element == null)
            {
                element = control.gameObject.AddComponent<LayoutElement>();
            }
            element.minHeight = height;
            element.preferredHeight = height;
        }
    }
}
