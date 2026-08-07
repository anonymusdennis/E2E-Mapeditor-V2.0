using System;
using System.Collections.Generic;
using E2EApi.Editor;
using E2EApi.Features;
using E2EApi.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MapEditorMod
{
    /// <summary>
    /// Radial quick-tool menu. Hold Alt to open it under the cursor, pick an
    /// entry by pointing at it or scrolling the mouse wheel, release Alt to
    /// run it. Gives fast access to undo/redo, brush settings and overlays
    /// without opening any windows.
    /// </summary>
    internal static class ToolWheel
    {
        private sealed class WheelItem
        {
            public Func<string> Label;
            public Func<string> Detail;
            public Func<bool> Enabled;
            public Action Execute;
        }

        private const float Radius = 170f;
        private const float ItemWidth = 128f;
        private const float ItemHeight = 40f;
        private const float DeadZone = 42f;
        private const float PointerTakeoverPixels = 10f;

        public static bool IsOpen { get; private set; }

        private static readonly List<WheelItem> Items = new List<WheelItem>();
        private static GameObject _root;
        private static Image[] _itemBackgrounds;
        private static Text[] _itemLabels;
        private static Text _centerLabel;
        private static Text _detailLabel;
        private static int _selected = -1;
        private static bool _wheelSelection;
        private static Vector2 _lastPointer;
        private static float _scrollAccumulator;
        private static bool _mouseWasSuppressed;

        /// <summary>Call every frame while the level editor is open.</summary>
        public static void Tick()
        {
            bool alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            if (alt && !IsOpen && !UiInput.IsTypingInInputField())
            {
                Open();
            }
            else if (!alt && IsOpen)
            {
                int chosen = _selected;
                Close();
                if (chosen >= 0 && chosen < Items.Count &&
                    Items[chosen].Enabled() && Items[chosen].Execute != null)
                {
                    Items[chosen].Execute();
                }
            }
            if (IsOpen)
            {
                UpdateSelection();
            }
        }

        /// <summary>Force-close (editor exit safety).</summary>
        public static void Close()
        {
            if (!IsOpen)
            {
                return;
            }
            IsOpen = false;
            VanillaEditor.MouseSuppressed = _mouseWasSuppressed;
            if (_root != null)
            {
                _root.SetActive(false);
            }
        }

        private static void Open()
        {
            BuildItems();
            EnsureUi();
            _root.SetActive(true);
            IsOpen = true;
            _selected = -1;
            _wheelSelection = false;
            _scrollAccumulator = 0f;
            _lastPointer = (Vector2)Input.mousePosition;
            _mouseWasSuppressed = VanillaEditor.MouseSuppressed;
            VanillaEditor.MouseSuppressed = true;
            RefreshVisuals();
        }

        private static void BuildItems()
        {
            Items.Clear();
            Items.Add(new WheelItem
            {
                Label = () => "Undo",
                Detail = () => UndoHistory.CanUndo
                    ? "undo " + UndoHistory.PeekUndoLabel : "nothing to undo",
                Enabled = () => UndoHistory.CanUndo,
                Execute = () => UndoHistory.Undo(),
            });
            Items.Add(new WheelItem
            {
                Label = () => "Redo",
                Detail = () => UndoHistory.CanRedo
                    ? "redo " + UndoHistory.PeekRedoLabel : "nothing to redo",
                Enabled = () => UndoHistory.CanRedo,
                Execute = () => UndoHistory.Redo(),
            });
            Items.Add(new WheelItem
            {
                Label = () => EditorTools.ConnectedStamp != null &&
                    EditorTools.ConnectedStamp.CollideWithPlayers
                    ? "Collision: ON" : "Collision: OFF",
                Detail = () => "toggle player collision of the current CTM brush",
                Enabled = () => EditorTools.ConnectedStamp != null,
                Execute = EditorTools.ConnectedToggleCollision,
            });
            Items.Add(new WheelItem
            {
                Label = () => EditorTools.ConnectedStamp != null &&
                    EditorTools.ConnectedStamp.Damaging
                    ? "Damage: ON" : "Damage: OFF",
                Detail = () => "toggle damaging tiles for the current CTM brush",
                Enabled = () => EditorTools.ConnectedStamp != null,
                Execute = EditorTools.ConnectedToggleDamaging,
            });
            Items.Add(new WheelItem
            {
                Label = () => EditorTools.ConnectedStamp != null &&
                    EditorTools.ConnectedStamp.Auto
                    ? "Connect: AUTO" : "Connect: MANUAL",
                Detail = () => "toggle automatic edge connection of the CTM brush",
                Enabled = () => EditorTools.ConnectedStamp != null,
                Execute = EditorTools.ConnectedToggleAuto,
            });
            Items.Add(new WheelItem
            {
                Label = () => EditorTools.ConnectedStamp != null
                    ? "Color: " + EditorTools.ConnectedStamp.Color : "Color",
                Detail = () => "cycle the CTM connection color group",
                Enabled = () => EditorTools.ConnectedStamp != null,
                Execute = EditorTools.ConnectedNextColor,
            });
            Items.Add(new WheelItem
            {
                Label = () => "Normal brush",
                Detail = () => "turn the current CTM/animated brush into a plain tile brush",
                Enabled = () => EditorTools.ConnectedStamp != null ||
                    EditorTools.AnimatedStamp != null,
                Execute = () => EditorTools.ConvertToNormalBrush(),
            });
            Items.Add(new WheelItem
            {
                Label = () => "Clear brush",
                Detail = () => "put away the current tool (Esc)",
                Enabled = () => true,
                Execute = EditorTools.ClearBrush,
            });
            Items.Add(new WheelItem
            {
                Label = () => MapGeometry.IsolateSelectedFloor
                    ? "Floor iso: ON" : "Floor iso: OFF",
                Detail = () => "only show the selected floor while editing",
                Enabled = () => true,
                Execute = () => MapGeometry.IsolateSelectedFloor =
                    !MapGeometry.IsolateSelectedFloor,
            });
            Items.Add(new WheelItem
            {
                Label = () => XRay.Enabled ? "X-Ray: ON" : "X-Ray: OFF",
                Detail = () => "see through walls",
                Enabled = () => true,
                Execute = () => XRay.Enabled = !XRay.Enabled,
            });
            Items.Add(new WheelItem
            {
                Label = () => FenceOverlay.Enabled ? "Fences: ON" : "Fences: OFF",
                Detail = () => "toggle the electric fence overlay",
                Enabled = () => true,
                Execute = () => FenceOverlay.Enabled = !FenceOverlay.Enabled,
            });
            Items.Add(new WheelItem
            {
                Label = () => TriggerArrows.Enabled ? "Triggers: ON" : "Triggers: OFF",
                Detail = () => "toggle trigger link arrows",
                Enabled = () => true,
                Execute = () => TriggerArrows.Enabled = !TriggerArrows.Enabled,
            });
        }

        private static void EnsureUi()
        {
            if (_root != null)
            {
                // rebuild if item count changed
                if (_itemLabels != null && _itemLabels.Length == Items.Count)
                {
                    return;
                }
                UnityEngine.Object.Destroy(_root);
                _root = null;
            }

            _root = new GameObject("E2EApi_ToolWheel");
            _root.transform.SetParent(ModWindow.OverlayCanvas.transform, false);
            var rootRect = _root.AddComponent<RectTransform>();
            ModWindow.Stretch(rootRect);

            // dim + click blocker
            var dim = _root.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.45f);
            dim.raycastTarget = true;

            int n = Items.Count;
            _itemBackgrounds = new Image[n];
            _itemLabels = new Text[n];
            for (int i = 0; i < n; i++)
            {
                var go = new GameObject("Item" + i);
                go.transform.SetParent(_root.transform, false);
                var rect = go.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(ItemWidth, ItemHeight);
                float angle = ItemAngle(i, n);
                rect.anchoredPosition = new Vector2(
                    Mathf.Cos(angle) * Radius, Mathf.Sin(angle) * Radius);
                var bg = go.AddComponent<Image>();
                bg.color = new Color(0.16f, 0.16f, 0.2f, 0.92f);
                bg.raycastTarget = false;
                _itemBackgrounds[i] = bg;

                var textGo = new GameObject("Label");
                textGo.transform.SetParent(go.transform, false);
                var textRect = textGo.AddComponent<RectTransform>();
                ModWindow.Stretch(textRect);
                var text = textGo.AddComponent<Text>();
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                text.fontSize = 13;
                text.alignment = TextAnchor.MiddleCenter;
                text.color = Color.white;
                text.raycastTarget = false;
                _itemLabels[i] = text;
            }

            var centerGo = new GameObject("Center");
            centerGo.transform.SetParent(_root.transform, false);
            var centerRect = centerGo.AddComponent<RectTransform>();
            centerRect.sizeDelta = new Vector2(240f, 44f);
            centerRect.anchoredPosition = Vector2.zero;
            _centerLabel = centerGo.AddComponent<Text>();
            _centerLabel.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _centerLabel.fontSize = 15;
            _centerLabel.fontStyle = FontStyle.Bold;
            _centerLabel.alignment = TextAnchor.MiddleCenter;
            _centerLabel.color = Color.white;
            _centerLabel.raycastTarget = false;

            var detailGo = new GameObject("Detail");
            detailGo.transform.SetParent(_root.transform, false);
            var detailRect = detailGo.AddComponent<RectTransform>();
            detailRect.sizeDelta = new Vector2(300f, 40f);
            detailRect.anchoredPosition = new Vector2(0f, -34f);
            _detailLabel = detailGo.AddComponent<Text>();
            _detailLabel.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _detailLabel.fontSize = 12;
            _detailLabel.alignment = TextAnchor.MiddleCenter;
            _detailLabel.color = new Color(0.8f, 0.8f, 0.85f, 1f);
            _detailLabel.raycastTarget = false;
        }

        private static float ItemAngle(int index, int count)
        {
            // start at the top, clockwise
            return (90f - index * (360f / count)) * Mathf.Deg2Rad;
        }

        private static void UpdateSelection()
        {
            int n = Items.Count;
            if (n == 0)
            {
                return;
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.0001f)
            {
                _scrollAccumulator += scroll;
                int steps = 0;
                while (_scrollAccumulator >= 0.1f)
                {
                    steps--;
                    _scrollAccumulator -= 0.1f;
                }
                while (_scrollAccumulator <= -0.1f)
                {
                    steps++;
                    _scrollAccumulator += 0.1f;
                }
                if (steps != 0)
                {
                    _wheelSelection = true;
                    _lastPointer = (Vector2)Input.mousePosition;
                    _selected = _selected < 0
                        ? (steps > 0 ? 0 : n - 1)
                        : ((_selected + steps) % n + n) % n;
                }
            }

            var pointer = (Vector2)Input.mousePosition;
            if (_wheelSelection &&
                (pointer - _lastPointer).sqrMagnitude >
                    PointerTakeoverPixels * PointerTakeoverPixels)
            {
                _wheelSelection = false;
            }

            if (!_wheelSelection)
            {
                var center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                var delta = pointer - center;
                if (delta.magnitude >= DeadZone)
                {
                    float pointerAngle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
                    // invert of ItemAngle: index = (90 - angle) / step
                    float step = 360f / n;
                    int index = Mathf.RoundToInt((90f - pointerAngle) / step);
                    _selected = ((index % n) + n) % n;
                }
            }

            RefreshVisuals();
        }

        private static void RefreshVisuals()
        {
            if (_itemLabels == null)
            {
                return;
            }
            for (int i = 0; i < Items.Count && i < _itemLabels.Length; i++)
            {
                bool enabled = Items[i].Enabled();
                bool selected = i == _selected;
                _itemLabels[i].text = Items[i].Label();
                _itemLabels[i].color = enabled
                    ? (selected ? Color.white : new Color(0.85f, 0.85f, 0.9f, 1f))
                    : new Color(0.5f, 0.5f, 0.55f, 1f);
                _itemBackgrounds[i].color = selected
                    ? (enabled
                        ? new Color(0.25f, 0.45f, 0.7f, 0.97f)
                        : new Color(0.3f, 0.3f, 0.34f, 0.95f))
                    : new Color(0.16f, 0.16f, 0.2f, 0.92f);
                _itemBackgrounds[i].rectTransform.localScale =
                    selected ? new Vector3(1.08f, 1.08f, 1f) : Vector3.one;
            }
            if (_selected >= 0 && _selected < Items.Count)
            {
                _centerLabel.text = Items[_selected].Label();
                _detailLabel.text = Items[_selected].Detail();
            }
            else
            {
                _centerLabel.text = "Tool wheel";
                _detailLabel.text = "point or scroll, release Alt to apply";
            }
        }
    }
}
