using E2EApi.Editor;
using E2EApi.Features;
using E2EApi.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MapEditorMod
{
    /// <summary>
    /// In-game tooltip shown while X-ray mode is on and the editor cursor sits
    /// on an X-rayed tile: same researched info as the web UI tooltips.
    /// </summary>
    internal class HoverTooltip
    {
        private GameObject _root;
        private Text _text;
        private BlockInfo _shownInfo;

        public void Tick(bool inEditor)
        {
            if (!inEditor || !XRay.Enabled)
            {
                Hide();
                return;
            }
            int x, y;
            if (!Placement.GetCursorTile(out x, out y))
            {
                Hide();
                return;
            }
            var info = XRay.GetInfoAt(x, y);
            if (info == null)
            {
                Hide();
                return;
            }
            Show(info);
            Position();
        }

        private void Show(BlockInfo info)
        {
            if (_root == null)
            {
                Build();
            }
            _root.SetActive(true);
            if (_shownInfo == info)
            {
                return;
            }
            _shownInfo = info;
            _text.text = Compose(info);
        }

        internal static string Compose(BlockInfo info)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("<b>").Append(info.DisplayName ?? "?").Append("</b>  #").Append(info.Id);
            sb.Append("\n").Append(info.Kind);
            if (info.EditorOnly)
            {
                sb.Append("  [dev-only]");
            }
            if (info.IsZone)
            {
                sb.Append("  [zone]");
            }
            if (!string.IsNullOrEmpty(info.InternalName))
            {
                sb.Append("\ninternal: ").Append(info.InternalName);
            }
            if (!string.IsNullOrEmpty(info.PrefabName))
            {
                sb.Append("\nprefab: ").Append(info.PrefabName);
            }
            if (!string.IsNullOrEmpty(info.ValidLayers))
            {
                sb.Append("\nlayers: ").Append(info.ValidLayers);
            }
            if (!string.IsNullOrEmpty(info.LimitGroup))
            {
                sb.Append("\ngroup: ").Append(info.LimitGroup);
            }
            if (!string.IsNullOrEmpty(info.Purpose))
            {
                sb.Append("\npurpose: ").Append(info.Purpose);
            }
            if (!string.IsNullOrEmpty(info.Description) && !info.Description.StartsWith("Text."))
            {
                sb.Append("\n").Append(info.Description);
            }
            if (!string.IsNullOrEmpty(info.Notes))
            {
                sb.Append("\n\n").Append(info.Notes);
            }
            return sb.ToString();
        }

        private void Position()
        {
            var rect = (RectTransform)_root.transform;
            Vector2 mouse = Input.mousePosition;
            bool right = mouse.x < Screen.width * 0.6f;
            bool up = mouse.y < Screen.height * 0.6f;
            rect.pivot = new Vector2(right ? 0f : 1f, up ? 0f : 1f);
            rect.position = mouse + new Vector2(right ? 16f : -16f, up ? 16f : -16f);
        }

        private void Hide()
        {
            if (_root != null)
            {
                _root.SetActive(false);
            }
            _shownInfo = null;
        }

        private void Build()
        {
            _root = new GameObject("E2E_HoverTooltip");
            _root.transform.SetParent(ModWindow.OverlayCanvas.transform, false);
            var rect = _root.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(330f, 230f);
            var image = _root.AddComponent<Image>();
            image.color = new Color(0.05f, 0.05f, 0.08f, 0.94f);
            image.raycastTarget = false;

            _text = ModWindow.MakeText(_root.transform, "", 12, TextAnchor.UpperLeft);
            _text.raycastTarget = false;
            var textRect = _text.rectTransform;
            ModWindow.Stretch(textRect);
            textRect.offsetMin = new Vector2(8f, 8f);
            textRect.offsetMax = new Vector2(-8f, -8f);
        }
    }
}
