using System;
using System.Collections.Generic;
using E2EApi.Editor;
using E2EApi.Features;
using UnityEngine;

namespace MapEditorMod
{
    internal enum EditorToolMode
    {
        None,
        PaintElectric,
        EraseElectric,
        LinkSwitch,
        PaintTile,
        EraseTile,
        PaintAnimatedTile,
        EraseAnimatedTile,
    }

    /// <summary>The atlas region currently armed for tile painting.</summary>
    internal class TileStamp
    {
        public string Atlas;
        public int X, Y, W, H; // atlas pixel rect, bottom-left origin
        public bool Decor;
        /// <summary>
        /// Layer override (-1 = follow the editor's current layer).
        /// 0=Underground … 5=Roof.
        /// </summary>
        public int Layer = -1;
        /// <summary>
        /// When true, each stamp placement also places the currently
        /// selected vanilla block at the same position (adds collision /
        /// gameplay behaviour alongside the visual tile).
        /// </summary>
        public bool SpamIncludesCollision;

        public int WTiles => (W + 31) / 32;
        public int HTiles => (H + 31) / 32;
    }

    /// <summary>Multi-frame animated stamp armed for <see cref="EditorToolMode.PaintAnimatedTile"/>.</summary>
    internal class AnimatedTileStamp
    {
        public List<E2EApi.Features.AnimatedModTiles.AnimFrame> Frames;
        public float Fps = 4f;
        public bool Loop = true;
        public bool PingPong;
        public bool Decor;
        public string Name = "";
        /// <summary>Layer override (-1 = follow the editor's current layer).</summary>
        public int Layer = -1;
        /// <summary>When true, each placement also places the current vanilla brush block.</summary>
        public bool SpamIncludesCollision;

        public int WTiles => Frames != null && Frames.Count > 0
            ? (Frames[0].Rw + 31) / 32 : 1;
        public int HTiles => Frames != null && Frames.Count > 0
            ? (Frames[0].Rh + 31) / 32 : 1;
    }

    /// <summary>
    /// Mutually exclusive editor tools (paint/erase electricity, link fence
    /// to switch). Ticked from Plugin.Update() while the level editor is open.
    /// </summary>
    internal static class EditorTools
    {
        public static EditorToolMode Mode { get; private set; }

        /// <summary>Pending switch tile in link mode (-1 = none selected yet).</summary>
        public static int SwitchX { get; private set; } = -1;
        public static int SwitchY { get; private set; } = -1;

        /// <summary>Raised whenever the mode or pending switch changes (main thread).</summary>
        public static event Action Changed;

        /// <summary>Stamp used by PaintTile (set from the web UI before arming).</summary>
        public static TileStamp Stamp;

        /// <summary>Animated stamp used by PaintAnimatedTile.</summary>
        public static AnimatedTileStamp AnimatedStamp;

        private const float RotationStep = 11.25f;

        private static bool _arrowsWereEnabled;
        private static bool _fenceOverlayWasEnabled;

        public static void SetMode(EditorToolMode mode)
        {
            if (Mode == mode)
            {
                return;
            }
            if (Mode == EditorToolMode.LinkSwitch)
            {
                TriggerArrows.Enabled = _arrowsWereEnabled;
            }
            if (Mode == EditorToolMode.PaintElectric || Mode == EditorToolMode.EraseElectric)
            {
                FenceOverlay.Enabled = _fenceOverlayWasEnabled;
            }
            Mode = mode;
            SwitchX = SwitchY = -1;
            if (mode == EditorToolMode.LinkSwitch)
            {
                _arrowsWereEnabled = TriggerArrows.Enabled;
                TriggerArrows.Enabled = true;
            }
            if (mode == EditorToolMode.PaintElectric || mode == EditorToolMode.EraseElectric)
            {
                _fenceOverlayWasEnabled = FenceOverlay.Enabled;
                FenceOverlay.Enabled = true;
            }

            // a mod tool owns the mouse: stop the vanilla editor from also
            // placing/selecting, and hide its block brush preview
            bool toolActive = mode != EditorToolMode.None;
            VanillaEditor.MouseSuppressed = toolActive;
            VanillaEditor.SetBrushVisible(!toolActive);
            if (!toolActive)
            {
                TileMarker.Hide();
            }
            OnChanged();
        }

        /// <summary>Activate the mode, or deactivate it if already active.</summary>
        public static void Toggle(EditorToolMode mode)
            => SetMode(Mode == mode ? EditorToolMode.None : mode);

        public static string HintText()
        {
            string selHint = TileSelector.Count > 0
                ? $" [{TileSelector.Count} selected — Q/E rotate]"
                : " [hover tile + Q/E to rotate; Shift-drag or Ctrl-click to select]";
            switch (Mode)
            {
                case EditorToolMode.PaintElectric:
                    return "hold LMB to electrify tiles";
                case EditorToolMode.EraseElectric:
                    return "hold LMB to remove electricity";
                case EditorToolMode.LinkSwitch:
                    return SwitchX < 0
                        ? "click the SWITCH tile first"
                        : $"switch ({SwitchX},{SwitchY}) — click fence tiles to link, Esc resets";
                case EditorToolMode.PaintTile:
                    if (Stamp == null) return "pick a stamp in the Tilesets tab first";
                    {
                        string layerInfo = Stamp.Layer >= 0 ? $" layer {Stamp.Layer}" : " auto-layer";
                        string collInfo = Stamp.SpamIncludesCollision ? " +collision" : "";
                        return $"LMB stamps {Stamp.WTiles}x{Stamp.HTiles} tile(s) ({(Stamp.Decor ? "over" : "under")} characters,{layerInfo}{collInfo}){selHint}";
                    }
                case EditorToolMode.EraseTile:
                    return "click modded tiles to remove them";
                case EditorToolMode.PaintAnimatedTile:
                    if (AnimatedStamp == null) return "pick an animated tile from the building blocks first";
                    {
                        string aLayerInfo = AnimatedStamp.Layer >= 0 ? $" layer {AnimatedStamp.Layer}" : " auto-layer";
                        string aCollInfo = AnimatedStamp.SpamIncludesCollision ? " +collision" : "";
                        return $"LMB places animated tile \"{AnimatedStamp.Name}\" {AnimatedStamp.WTiles}x{AnimatedStamp.HTiles} ({AnimatedStamp.Frames.Count} frames @ {AnimatedStamp.Fps:0.#} FPS,{aLayerInfo}{aCollInfo}){selHint}";
                    }
                case EditorToolMode.EraseAnimatedTile:
                    return "click animated tiles to remove them";
                default:
                    return selHint.TrimStart();
            }
        }

        /// <summary>Call every frame while the level editor is open.</summary>
        public static void Tick()
        {
            // Selection and rotation are available in all tool modes
            TickSelection();
            TickRotation();

            switch (Mode)
            {
                case EditorToolMode.PaintElectric:
                    UpdateBrushCursor(new Color(1f, 0.85f, 0.1f, 0.95f));
                    Paint(true);
                    break;
                case EditorToolMode.EraseElectric:
                    UpdateBrushCursor(new Color(1f, 0.25f, 0.2f, 0.95f));
                    Paint(false);
                    break;
                case EditorToolMode.LinkSwitch:
                    UpdateBrushCursor(new Color(0.3f, 0.8f, 1f, 0.95f));
                    TickLink();
                    break;
                case EditorToolMode.PaintTile:
                    UpdateBrushCursor(new Color(0.35f, 1f, 0.5f, 0.95f));
                    TickPaintTile();
                    break;
                case EditorToolMode.EraseTile:
                    UpdateBrushCursor(new Color(1f, 0.45f, 0.1f, 0.95f));
                    TickEraseTile();
                    break;
                case EditorToolMode.PaintAnimatedTile:
                    UpdateBrushCursor(new Color(0.5f, 0.8f, 1f, 0.95f));
                    TickPaintAnimatedTile();
                    break;
                case EditorToolMode.EraseAnimatedTile:
                    UpdateBrushCursor(new Color(1f, 0.6f, 0.1f, 0.95f));
                    TickEraseAnimatedTile();
                    break;
            }
        }

        private static void TickSelection()
        {
            // Selection input is only active when no exclusive tool is running,
            // so it does not interfere with electricity paint / link / etc.
            if (Mode == EditorToolMode.None || Mode == EditorToolMode.PaintTile ||
                Mode == EditorToolMode.PaintAnimatedTile)
            {
                TileSelector.Tick();
            }
        }

        private static void TickRotation()
        {
            bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            // Q = counter-clockwise (+11.25°), E = clockwise (-11.25°)
            bool rotCCW = Input.GetKeyDown(KeyCode.Q);
            bool rotCW  = Input.GetKeyDown(KeyCode.E);
            if (!rotCCW && !rotCW) return;

            // E is the vanilla "activate trigger" key in play mode, but here we
            // are inside the editor so it is safe to reuse.
            float delta = rotCCW ? RotationStep : -RotationStep;

            int layer = Grid.CurrentEditorLayer < 0 ? 1 : Grid.CurrentEditorLayer;

            if (TileSelector.Count > 0)
            {
                // Rotate every selected placement
                foreach (var key in TileSelector.All())
                {
                    var p = ModTiles.GetAt(key.X, key.Y, key.Layer);
                    if (p != null)
                    {
                        ModTiles.Rotate(p, delta);
                    }
                }
                // Version already bumped inside Rotate() for each call; ensure a
                // single redraw by the overlay the next frame.
            }
            else
            {
                // Rotate the tile under the cursor
                int x, y;
                if (Placement.GetCursorTile(out x, out y))
                {
                    ModTiles.RotateAt(x, y, layer, delta);
                }
            }
        }

        private static int _lastStampX = int.MinValue;
        private static int _lastStampY = int.MinValue;

        private static void TickPaintTile()
        {
            if (Stamp == null || !Input.GetMouseButton(0))
            {
                _lastStampX = _lastStampY = int.MinValue;
                return;
            }
            int x, y;
            if (!Placement.GetCursorTile(out x, out y))
            {
                return;
            }
            // multi-tile stamps anchor on the hovered tile; drag-paint snaps to
            // the stamp grid so dragging tiles a floor seamlessly
            int w = Stamp.WTiles;
            int h = Stamp.HTiles;
            if (w > 1 || h > 1)
            {
                x = (x / w) * w;
                y = (y / h) * h;
            }
            if (x == _lastStampX && y == _lastStampY)
            {
                return;
            }
            _lastStampX = x;
            _lastStampY = y;
            int layer = Stamp.Layer >= 0 ? Stamp.Layer
                : (Grid.CurrentEditorLayer < 0 ? 1 : Grid.CurrentEditorLayer);
            ModTiles.Place(x, y, layer, Stamp.Decor,
                Stamp.Atlas, Stamp.X, Stamp.Y, Stamp.W, Stamp.H);
            if (Stamp.SpamIncludesCollision)
            {
                int brush = Blocks.CurrentBrush;
                if (brush >= 0)
                {
                    Placement.PlaceBlock(brush, x, y);
                }
            }
        }

        private static void TickEraseTile()
        {
            if (!Input.GetMouseButton(0))
            {
                return;
            }
            int x, y;
            if (Placement.GetCursorTile(out x, out y))
            {
                int layer = Grid.CurrentEditorLayer;
                ModTiles.EraseAt(x, y, layer < 0 ? 1 : layer);
            }
        }

        private static int _lastAnimStampX = int.MinValue;
        private static int _lastAnimStampY = int.MinValue;

        private static void TickPaintAnimatedTile()
        {
            if (AnimatedStamp == null || !Input.GetMouseButton(0))
            {
                _lastAnimStampX = _lastAnimStampY = int.MinValue;
                return;
            }
            int x, y;
            if (!Placement.GetCursorTile(out x, out y)) return;
            int w = AnimatedStamp.WTiles;
            int h = AnimatedStamp.HTiles;
            if (w > 1 || h > 1)
            {
                x = (x / w) * w;
                y = (y / h) * h;
            }
            if (x == _lastAnimStampX && y == _lastAnimStampY) return;
            _lastAnimStampX = x;
            _lastAnimStampY = y;
            int layer = AnimatedStamp.Layer >= 0 ? AnimatedStamp.Layer
                : (Grid.CurrentEditorLayer < 0 ? 1 : Grid.CurrentEditorLayer);
            AnimatedModTiles.Place(x, y, layer,
                AnimatedStamp.Decor, AnimatedStamp.Fps,
                AnimatedStamp.Loop, AnimatedStamp.PingPong,
                AnimatedStamp.Frames);
            if (AnimatedStamp.SpamIncludesCollision)
            {
                int brush = Blocks.CurrentBrush;
                if (brush >= 0)
                {
                    Placement.PlaceBlock(brush, x, y);
                }
            }
        }

        private static void TickEraseAnimatedTile()
        {
            if (!Input.GetMouseButton(0)) return;
            int x, y;
            if (Placement.GetCursorTile(out x, out y))
            {
                int layer = Grid.CurrentEditorLayer;
                AnimatedModTiles.EraseAt(x, y, layer < 0 ? 1 : layer);
            }
        }

        private static void UpdateBrushCursor(Color color)
        {
            int x, y;
            if (Placement.GetCursorTile(out x, out y))
            {
                TileMarker.Show(x, y, color);
            }
            else
            {
                TileMarker.Hide();
            }
        }

        private static void Paint(bool on)
        {
            if (!Input.GetMouseButton(0))
            {
                return;
            }
            int x, y;
            if (Placement.GetCursorTile(out x, out y))
            {
                ElectricFences.SetElectrified(x, y, on);
            }
        }

        private static void TickLink()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && SwitchX >= 0)
            {
                SwitchX = SwitchY = -1;
                OnChanged();
                return;
            }
            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }
            int x, y;
            if (!Placement.GetCursorTile(out x, out y))
            {
                return;
            }
            if (SwitchX < 0)
            {
                SwitchX = x;
                SwitchY = y;
            }
            else
            {
                Triggers.AddLink(SwitchX, SwitchY, x, y, Triggers.TriggerAction.ToggleFence);
            }
            OnChanged();
        }

        private static void OnChanged()
        {
            var handler = Changed;
            if (handler != null)
            {
                handler();
            }
        }
    }
}

