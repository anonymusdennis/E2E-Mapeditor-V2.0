using System;
using System.Collections.Generic;
using E2EApi.Editor;
using E2EApi.Features;
using E2EApi.UI;
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
        PaintConnectedTile,
        LinkTeleport,
    }

    /// <summary>The atlas region currently armed for tile painting.</summary>
    internal class TileStamp
    {
        public string Atlas;
        public int X, Y, W, H; // atlas pixel rect, bottom-left origin
        public bool Decor;

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

        public int WTiles => Frames != null && Frames.Count > 0
            ? (Frames[0].Rw + 31) / 32 : 1;
        public int HTiles => Frames != null && Frames.Count > 0
            ? (Frames[0].Rh + 31) / 32 : 1;
    }

    internal class CompositeStampPart
    {
        public int Dx, Dy, Layer;
        public bool Decor;
        public string Atlas;
        public int X, Y, W, H;
        public bool Animated;
        public bool Vanilla;
        public int BlockId;
        public float Fps = 4f;
        public bool Loop = true;
        public bool PingPong;
        public List<E2EApi.Features.AnimatedModTiles.AnimFrame> Frames;
    }

    internal class CompositeStamp
    {
        public string Name = "";
        public int WTiles = 1;
        public int HTiles = 1;
        public List<CompositeStampPart> Parts = new List<CompositeStampPart>();
    }

    internal class ConnectedVariant
    {
        public string Atlas;
        public int X, Y, W, H;
        public int Weight = 100;
    }

    internal class ConnectedTileStamp
    {
        public string Id = "";
        public string Name = "";
        public string Mode = "floor";
        public bool Decor;
        public int Color = 1;
        public bool Auto = true;
        public bool LockPlaced;
        public bool ManualSingle;
        public int ManualMask = -1;
        public bool CollideWithPlayers;
        public bool Destructible;
        public bool Damaging;
        public bool Indestructible => !Destructible;
        public Dictionary<int, List<ConnectedVariant>> Variants =
            new Dictionary<int, List<ConnectedVariant>>();
    }

    internal class ConnectedCell
    {
        public ConnectedTileStamp Stamp;
        public int Color;
        public bool Locked;
        public bool Manual;
        public int ManualMask;
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

        /// <summary>Virtual layer of the pending teleport source (-1 = none).</summary>
        public static int SwitchLayer { get; private set; } = -1;

        /// <summary>Raised whenever the mode or pending switch changes (main thread).</summary>
        public static event Action Changed;

        /// <summary>Stamp used by PaintTile (set from the web UI before arming).</summary>
        public static TileStamp Stamp;

        /// <summary>Animated stamp used by PaintAnimatedTile.</summary>
        public static AnimatedTileStamp AnimatedStamp;

        /// <summary>Composite preset used by PaintTile.</summary>
        public static CompositeStamp Composite;

        public static ConnectedTileStamp ConnectedStamp;
        public static bool ShowConnectedColors;

        private static readonly Dictionary<string, ConnectedCell> ConnectedCells =
            new Dictionary<string, ConnectedCell>();

        /// <summary>
        /// Painted CTM cells keyed "layer:x:y" — exposed for map persistence
        /// (same assembly only).
        /// </summary>
        internal static Dictionary<string, ConnectedCell> ConnectedCellMap
            => ConnectedCells;

        /// <summary>Clear all painted CTM cells (map load/reset).</summary>
        internal static void ClearConnectedCells() => ConnectedCells.Clear();

        private const float RotationStep = 11.25f;

        private static bool _arrowsWereEnabled;
        private static bool _fenceOverlayWasEnabled;

        // ---- undo batching ---------------------------------------------------
        // While a mouse gesture (drag) is in progress, individual tile actions
        // accumulate here and are pushed to UndoHistory as ONE entry on release.

        private static string _batchLabel;
        private static List<Action> _batchUndos;
        private static List<Action> _batchRedos;

        /// <summary>
        /// Record one applied mod action. During a mouse gesture the action is
        /// batched; otherwise it is pushed to <see cref="UndoHistory"/> directly.
        /// </summary>
        private static void RecordAction(string label, Action undo, Action redo)
        {
            bool dragging = Input.GetMouseButton(0) || Input.GetMouseButton(1);
            if (dragging)
            {
                if (_batchUndos == null)
                {
                    _batchLabel = label;
                    _batchUndos = new List<Action>();
                    _batchRedos = new List<Action>();
                }
                _batchUndos.Add(undo);
                _batchRedos.Add(redo);
            }
            else
            {
                UndoHistory.Push(label, undo, redo);
            }
        }

        private static void FlushUndoBatch()
        {
            if (_batchUndos == null || _batchUndos.Count == 0)
            {
                _batchLabel = null;
                _batchUndos = null;
                _batchRedos = null;
                return;
            }
            var label = _batchLabel;
            var undos = _batchUndos;
            var redos = _batchRedos;
            _batchLabel = null;
            _batchUndos = null;
            _batchRedos = null;
            if (undos.Count == 1)
            {
                UndoHistory.Push(label, undos[0], redos[0]);
                return;
            }
            UndoHistory.Push(label + " x" + undos.Count,
                () =>
                {
                    for (int i = undos.Count - 1; i >= 0; i--)
                    {
                        undos[i]();
                    }
                },
                () =>
                {
                    for (int i = 0; i < redos.Count; i++)
                    {
                        redos[i]();
                    }
                });
        }

        public static void SetMode(EditorToolMode mode)
        {
            if (Mode == mode)
            {
                return;
            }
            FlushUndoBatch();
            if (Mode == EditorToolMode.LinkSwitch || Mode == EditorToolMode.LinkTeleport)
            {
                TriggerArrows.Enabled = _arrowsWereEnabled;
            }
            if (Mode == EditorToolMode.PaintElectric || Mode == EditorToolMode.EraseElectric)
            {
                FenceOverlay.Enabled = _fenceOverlayWasEnabled;
            }
            Mode = mode;
            SwitchX = SwitchY = -1;
            SwitchLayer = -1;
            if (mode == EditorToolMode.LinkSwitch || mode == EditorToolMode.LinkTeleport)
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

        public static void ArmConnectedStamp(ConnectedTileStamp stamp)
        {
            Stamp = null;
            AnimatedStamp = null;
            Composite = null;
            ConnectedStamp = stamp;
            SetMode(EditorToolMode.PaintConnectedTile);
            OnChanged();
        }

        public static void ClearBrush()
        {
            FlushUndoBatch();
            Stamp = null;
            AnimatedStamp = null;
            Composite = null;
            ConnectedStamp = null;
            SwitchX = SwitchY = -1;
            SwitchLayer = -1;
            _lastStampX = _lastStampY = int.MinValue;
            _lastAnimStampX = _lastAnimStampY = int.MinValue;
            Blocks.ClearBrush();
            SetMode(EditorToolMode.None);
            OnChanged();
        }

        /// <summary>
        /// Convert the current connected/animated/composite brush into a plain
        /// single-tile brush (keeps painting the same art without CTM logic).
        /// </summary>
        public static bool ConvertToNormalBrush()
        {
            if (ConnectedStamp != null)
            {
                // Use the fully-connected variant (mask 15), falling back to any.
                ConnectedVariant variant = null;
                List<ConnectedVariant> list;
                if (ConnectedStamp.Variants.TryGetValue(15, out list) && list.Count > 0)
                {
                    variant = list[0];
                }
                else
                {
                    foreach (var pair in ConnectedStamp.Variants)
                    {
                        if (pair.Value != null && pair.Value.Count > 0)
                        {
                            variant = pair.Value[0];
                            break;
                        }
                    }
                }
                if (variant == null)
                {
                    return false;
                }
                Stamp = new TileStamp
                {
                    Atlas = variant.Atlas,
                    X = variant.X, Y = variant.Y,
                    W = variant.W, H = variant.H,
                    Decor = ConnectedStamp.Decor,
                };
                ConnectedStamp = null;
                AnimatedStamp = null;
                Composite = null;
                SetMode(EditorToolMode.PaintTile);
                OnChanged();
                return true;
            }
            if (AnimatedStamp != null && AnimatedStamp.Frames != null &&
                AnimatedStamp.Frames.Count > 0)
            {
                var frame = AnimatedStamp.Frames[0];
                Stamp = new TileStamp
                {
                    Atlas = frame.Atlas,
                    X = frame.Rx, Y = frame.Ry,
                    W = frame.Rw, H = frame.Rh,
                    Decor = AnimatedStamp.Decor,
                };
                AnimatedStamp = null;
                Composite = null;
                SetMode(EditorToolMode.PaintTile);
                OnChanged();
                return true;
            }
            return false;
        }

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
                case EditorToolMode.LinkTeleport:
                    return SwitchX < 0
                        ? "click the START tile (e.g. stairs); you can change floors before picking the destination"
                        : $"start ({SwitchX},{SwitchY}) floor {SwitchLayer} — switch floor if needed, then click the DESTINATION tile; Esc resets";
                case EditorToolMode.PaintTile:
                    return Composite != null
                        ? $"LMB places structure \"{Composite.Name}\" {Composite.WTiles}x{Composite.HTiles} ({Composite.Parts.Count} parts){selHint}"
                        : Stamp == null
                        ? "pick a stamp in the Tilesets tab first"
                        : $"LMB stamps {Stamp.WTiles}x{Stamp.HTiles} tile(s) ({(Stamp.Decor ? "over" : "under")} characters){selHint}";
                case EditorToolMode.EraseTile:
                    return "click modded tiles to remove them";
                case EditorToolMode.PaintAnimatedTile:
                    return AnimatedStamp == null
                        ? "pick an animated tile from the building blocks first"
                        : $"LMB places animated tile \"{AnimatedStamp.Name}\" {AnimatedStamp.WTiles}x{AnimatedStamp.HTiles} ({AnimatedStamp.Frames.Count} frames @ {AnimatedStamp.Fps:0.#} FPS){selHint}";
                case EditorToolMode.EraseAnimatedTile:
                    return "click animated tiles to remove them";
                case EditorToolMode.PaintConnectedTile:
                    return ConnectedStamp == null
                        ? "pick a connected floor/wall first"
                        : $"LMB paints connected \"{ConnectedStamp.Name}\" color {ConnectedStamp.Color}; Shift+LMB locks placed tiles";
                default:
                    return selHint.TrimStart();
            }
        }

        /// <summary>Call every frame while the level editor is open.</summary>
        public static void Tick()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ClearBrush();
                return;
            }

            TickUndoHotkeys();

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
                case EditorToolMode.LinkTeleport:
                    UpdateBrushCursor(new Color(1f, 0.5f, 0.9f, 0.95f));
                    TickLinkTeleport();
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
                case EditorToolMode.PaintConnectedTile:
                    UpdateBrushCursor(new Color(0.65f, 0.45f, 1f, 0.95f));
                    TickPaintConnectedTile();
                    break;
            }

            // A finished mouse gesture closes the pending undo batch, so one
            // drag = one undo step.
            if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1))
            {
                FlushUndoBatch();
            }
        }

        /// <summary>Ctrl+Z undo, Ctrl+Y / Ctrl+Shift+Z redo (unified history).</summary>
        private static void TickUndoHotkeys()
        {
            bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            if (!ctrl || UiInput.IsTypingInInputField())
            {
                return;
            }
            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            if (Input.GetKeyDown(KeyCode.Z))
            {
                FlushUndoBatch();
                if (shift)
                {
                    UndoHistory.Redo();
                }
                else
                {
                    UndoHistory.Undo();
                }
            }
            else if (Input.GetKeyDown(KeyCode.Y))
            {
                FlushUndoBatch();
                UndoHistory.Redo();
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
            if (UiInput.IsPointerOverUi())
            {
                _lastStampX = _lastStampY = int.MinValue;
                return;
            }
            if (Input.GetMouseButton(1))
            {
                _lastStampX = _lastStampY = int.MinValue;
                EraseCurrentTileBrush();
                return;
            }
            if ((Stamp == null && Composite == null) || !Input.GetMouseButton(0))
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
            int w = Composite != null ? Composite.WTiles : Stamp.WTiles;
            int h = Composite != null ? Composite.HTiles : Stamp.HTiles;
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
            int baseLayer = CurrentEditorLayer();
            if (Composite != null)
            {
                foreach (var part in Composite.Parts)
                {
                    if (part.Animated && part.Frames != null && part.Frames.Count > 0)
                    {
                        PlaceAnimatedWithUndo(x + part.Dx, y + part.Dy, baseLayer,
                            part.Decor, part.Fps, part.Loop, part.PingPong,
                            part.Frames);
                    }
                    else if (!string.IsNullOrEmpty(part.Atlas))
                    {
                        PlaceTileWithUndo(x + part.Dx, y + part.Dy, baseLayer,
                            part.Decor, part.Atlas, part.X, part.Y, part.W, part.H);
                    }
                    else if (part.Vanilla && part.BlockId >= 0)
                    {
                        // Vanilla blocks go through the game's own instruction
                        // list; UndoHistory tracks those groups automatically.
                        Placement.PlaceBlock(part.BlockId, x + part.Dx, y + part.Dy);
                    }
                }
                return;
            }
            PlaceTileWithUndo(x, y, baseLayer, Stamp.Decor,
                Stamp.Atlas, Stamp.X, Stamp.Y, Stamp.W, Stamp.H);
        }

        /// <summary>Place a static mod tile and record it in the undo history.</summary>
        private static void PlaceTileWithUndo(int x, int y, int layer, bool decor,
            string atlas, int rx, int ry, int rw, int rh)
        {
            var prev = ModTiles.TakeAnchored(x, y, layer, decor);
            ModTiles.Place(x, y, layer, decor, atlas, rx, ry, rw, rh);
            var placed = ModTiles.GetAnchored(x, y, layer, decor);
            RecordAction("place tile",
                () =>
                {
                    ModTiles.TakeAnchored(x, y, layer, decor);
                    if (prev != null) ModTiles.Restore(prev);
                },
                () =>
                {
                    ModTiles.TakeAnchored(x, y, layer, decor);
                    if (placed != null) ModTiles.Restore(placed);
                });
        }

        /// <summary>Place an animated mod tile and record it in the undo history.</summary>
        private static void PlaceAnimatedWithUndo(int x, int y, int layer, bool decor,
            float fps, bool loop, bool pingPong, List<AnimatedModTiles.AnimFrame> frames)
        {
            var prev = AnimatedModTiles.TakeAnchored(x, y, layer, decor);
            AnimatedModTiles.Place(x, y, layer, decor, fps, loop, pingPong, frames);
            var placed = AnimatedModTiles.GetAnchored(x, y, layer, decor);
            RecordAction("place animated tile",
                () =>
                {
                    AnimatedModTiles.TakeAnchored(x, y, layer, decor);
                    if (prev != null) AnimatedModTiles.Restore(prev);
                },
                () =>
                {
                    AnimatedModTiles.TakeAnchored(x, y, layer, decor);
                    if (placed != null) AnimatedModTiles.Restore(placed);
                });
        }

        private static void TickEraseTile()
        {
            if (UiInput.IsPointerOverUi())
            {
                return;
            }
            if (!Input.GetMouseButton(0))
            {
                return;
            }
            int x, y;
            if (Placement.GetCursorTile(out x, out y))
            {
                int layer = CurrentEditorLayer();
                EraseTilesWithUndo(x, y, layer, true, true);
            }
        }

        /// <summary>Erase mod tiles at a tile and record it in the undo history.</summary>
        private static void EraseTilesWithUndo(int x, int y, int layer,
            bool staticTiles, bool animatedTiles)
        {
            var taken = staticTiles
                ? ModTiles.TakeAt(x, y, layer)
                : new List<ModTiles.Placement>();
            var takenAnim = animatedTiles
                ? AnimatedModTiles.TakeAt(x, y, layer)
                : new List<AnimatedModTiles.AnimatedPlacement>();
            if (taken.Count == 0 && takenAnim.Count == 0)
            {
                return;
            }
            RecordAction("erase tile",
                () =>
                {
                    foreach (var p in taken) ModTiles.Restore(p);
                    foreach (var p in takenAnim) AnimatedModTiles.Restore(p);
                },
                () =>
                {
                    if (taken.Count > 0) ModTiles.EraseAt(x, y, layer);
                    if (takenAnim.Count > 0) AnimatedModTiles.EraseAt(x, y, layer);
                });
        }

        private static int _lastAnimStampX = int.MinValue;
        private static int _lastAnimStampY = int.MinValue;

        private static void TickPaintAnimatedTile()
        {
            if (UiInput.IsPointerOverUi())
            {
                _lastAnimStampX = _lastAnimStampY = int.MinValue;
                return;
            }
            if (Input.GetMouseButton(1))
            {
                _lastAnimStampX = _lastAnimStampY = int.MinValue;
                EraseAnimatedBrushAtCursor();
                return;
            }
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
            int layer = CurrentEditorLayer();
            PlaceAnimatedWithUndo(x, y, layer,
                AnimatedStamp.Decor, AnimatedStamp.Fps,
                AnimatedStamp.Loop, AnimatedStamp.PingPong,
                AnimatedStamp.Frames);
        }

        private static void TickEraseAnimatedTile()
        {
            if (UiInput.IsPointerOverUi())
            {
                return;
            }
            if (!Input.GetMouseButton(0)) return;
            int x, y;
            if (Placement.GetCursorTile(out x, out y))
            {
                EraseTilesWithUndo(x, y, CurrentEditorLayer(), false, true);
            }
        }

        private static void TickPaintConnectedTile()
        {
            if (UiInput.IsPointerOverUi())
            {
                _lastStampX = _lastStampY = int.MinValue;
                return;
            }
            if (Input.GetMouseButton(1))
            {
                _lastStampX = _lastStampY = int.MinValue;
                EraseConnectedBrushAtCursor();
                return;
            }
            if (ConnectedStamp == null || !Input.GetMouseButton(0))
            {
                _lastStampX = _lastStampY = int.MinValue;
                return;
            }
            int x, y;
            if (!Placement.GetCursorTile(out x, out y)) return;
            if (x == _lastStampX && y == _lastStampY) return;
            _lastStampX = x;
            _lastStampY = y;

            bool lockThis = ConnectedStamp.LockPlaced ||
                Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            int layer = CurrentEditorLayer();
            string key = ConnectedKey(x, y, layer);
            var before = CaptureConnectedArea(x, y, layer);
            ConnectedCells[key] = new ConnectedCell
            {
                Stamp = ConnectedStamp,
                Color = ConnectedStamp.Color,
                Locked = lockThis,
                Manual = ConnectedStamp.ManualSingle || !ConnectedStamp.Auto,
                ManualMask = ConnectedStamp.ManualMask >= 0 ? ConnectedStamp.ManualMask : 15,
            };
            ResolveConnectedAt(x, y, layer);
            ResolveConnectedAt(x + 1, y, layer);
            ResolveConnectedAt(x - 1, y, layer);
            ResolveConnectedAt(x, y + 1, layer);
            ResolveConnectedAt(x, y - 1, layer);

            if (ConnectedStamp.Damaging)
            {
                ElectricFences.SetElectrified(x, y, true);
            }
            if (ConnectedStamp.CollideWithPlayers)
            {
                ModCollision.SetBlocked(x, y, layer, true);
            }
            RecordConnectedChange("connected paint",
                before, CaptureConnectedArea(x, y, layer));
        }

        private static void EraseCurrentTileBrush()
        {
            int x, y;
            if (!Placement.GetCursorTile(out x, out y))
            {
                return;
            }

            int baseLayer = CurrentEditorLayer();
            if (Composite != null)
            {
                foreach (var part in Composite.Parts)
                {
                    int px = x + part.Dx;
                    int py = y + part.Dy;
                    if (part.Animated)
                    {
                        EraseTilesWithUndo(px, py, baseLayer, false, true);
                    }
                    else if (part.Vanilla && part.BlockId >= 0)
                    {
                        Blocks.DeleteAt(part.BlockId, px, py);
                    }
                    else
                    {
                        EraseTilesWithUndo(px, py, baseLayer, true, false);
                    }
                }
                return;
            }

            if (Stamp != null)
            {
                EraseTilesWithUndo(x, y, baseLayer, true, false);
            }
        }

        private static void EraseAnimatedBrushAtCursor()
        {
            int x, y;
            if (!Placement.GetCursorTile(out x, out y))
            {
                return;
            }
            EraseTilesWithUndo(x, y, CurrentEditorLayer(), false, true);
        }

        private static void EraseConnectedBrushAtCursor()
        {
            if (ConnectedStamp == null)
            {
                return;
            }
            int x, y;
            if (!Placement.GetCursorTile(out x, out y))
            {
                return;
            }
            EraseConnectedAt(x, y, CurrentEditorLayer());
        }

        private static void EraseConnectedAt(int x, int y, int layer)
        {
            string key = ConnectedKey(x, y, layer);
            ConnectedCell cell;
            if (!ConnectedCells.TryGetValue(key, out cell))
            {
                return;
            }
            var before = CaptureConnectedArea(x, y, layer);
            ConnectedCells.Remove(key);
            if (cell.Stamp != null)
            {
                // Remove only the resolved CTM placement (anchored 1x1).
                ModTiles.TakeAnchored(x, y, layer, cell.Stamp.Decor);
            }
            else
            {
                ModTiles.EraseAt(x, y, layer);
            }
            if (cell.Stamp != null && cell.Stamp.Damaging)
            {
                ElectricFences.SetElectrified(x, y, false);
            }
            ModCollision.SetBlocked(x, y, layer, false);
            ResolveConnectedAt(x + 1, y, layer);
            ResolveConnectedAt(x - 1, y, layer);
            ResolveConnectedAt(x, y + 1, layer);
            ResolveConnectedAt(x, y - 1, layer);
            RecordConnectedChange("connected erase",
                before, CaptureConnectedArea(x, y, layer));
        }

        private static string ConnectedKey(int x, int y, int layer)
            => layer + ":" + x + ":" + y;

        // ---- connected-tile undo snapshots ----------------------------------

        private sealed class ConnectedTileSnapshot
        {
            public int X, Y, Layer;
            public ConnectedCell Cell;
            public ModTiles.Placement Floor;
            public ModTiles.Placement Decor;
            public bool Electrified;
            public bool Blocked;
        }

        private static ConnectedTileSnapshot CaptureConnectedTile(int x, int y, int layer)
        {
            ConnectedCell cell;
            ConnectedCells.TryGetValue(ConnectedKey(x, y, layer), out cell);
            return new ConnectedTileSnapshot
            {
                X = x, Y = y, Layer = layer,
                Cell = cell,
                Floor = ModTiles.GetAnchored(x, y, layer, false),
                Decor = ModTiles.GetAnchored(x, y, layer, true),
                Electrified = ElectricFences.IsElectrified(x, y),
                Blocked = ModCollision.IsBlocked(x, y, layer),
            };
        }

        private static void ApplyConnectedTileSnapshot(ConnectedTileSnapshot s)
        {
            string key = ConnectedKey(s.X, s.Y, s.Layer);
            if (s.Cell != null)
            {
                ConnectedCells[key] = s.Cell;
            }
            else
            {
                ConnectedCells.Remove(key);
            }
            ModTiles.TakeAnchored(s.X, s.Y, s.Layer, false);
            if (s.Floor != null) ModTiles.Restore(s.Floor);
            ModTiles.TakeAnchored(s.X, s.Y, s.Layer, true);
            if (s.Decor != null) ModTiles.Restore(s.Decor);
            ElectricFences.SetElectrified(s.X, s.Y, s.Electrified);
            ModCollision.SetBlocked(s.X, s.Y, s.Layer, s.Blocked);
        }

        private static List<ConnectedTileSnapshot> CaptureConnectedArea(int x, int y, int layer)
        {
            return new List<ConnectedTileSnapshot>
            {
                CaptureConnectedTile(x, y, layer),
                CaptureConnectedTile(x + 1, y, layer),
                CaptureConnectedTile(x - 1, y, layer),
                CaptureConnectedTile(x, y + 1, layer),
                CaptureConnectedTile(x, y - 1, layer),
            };
        }

        private static void RecordConnectedChange(string label,
            List<ConnectedTileSnapshot> before, List<ConnectedTileSnapshot> after)
        {
            RecordAction(label,
                () =>
                {
                    foreach (var s in before) ApplyConnectedTileSnapshot(s);
                },
                () =>
                {
                    foreach (var s in after) ApplyConnectedTileSnapshot(s);
                });
        }

        private static bool TryParseConnectedKey(string key,
            out int x, out int y, out int layer)
        {
            x = y = layer = 0;
            var parts = key.Split(':');
            return parts.Length == 3 &&
                int.TryParse(parts[0], out layer) &&
                int.TryParse(parts[1], out x) &&
                int.TryParse(parts[2], out y);
        }

        private static void SnapshotInto(int x, int y, int layer,
            Dictionary<string, ConnectedTileSnapshot> map)
        {
            string key = ConnectedKey(x, y, layer);
            if (!map.ContainsKey(key))
            {
                map[key] = CaptureConnectedTile(x, y, layer);
            }
        }

        /// <summary>
        /// Remove every painted cell of the currently armed CTM brush from the
        /// whole map (single undo step). Returns the number of cells removed.
        /// </summary>
        public static int EraseAllOfCurrentConnected()
        {
            var stamp = ConnectedStamp;
            if (stamp == null)
            {
                return 0;
            }
            var coords = new List<int>();
            foreach (var pair in ConnectedCells)
            {
                if (pair.Value.Stamp != stamp)
                {
                    continue;
                }
                int x, y, layer;
                if (TryParseConnectedKey(pair.Key, out x, out y, out layer))
                {
                    coords.Add(x);
                    coords.Add(y);
                    coords.Add(layer);
                }
            }
            if (coords.Count == 0)
            {
                return 0;
            }

            var beforeMap = new Dictionary<string, ConnectedTileSnapshot>();
            for (int i = 0; i < coords.Count; i += 3)
            {
                int x = coords[i], y = coords[i + 1], layer = coords[i + 2];
                SnapshotInto(x, y, layer, beforeMap);
                SnapshotInto(x + 1, y, layer, beforeMap);
                SnapshotInto(x - 1, y, layer, beforeMap);
                SnapshotInto(x, y + 1, layer, beforeMap);
                SnapshotInto(x, y - 1, layer, beforeMap);
            }

            for (int i = 0; i < coords.Count; i += 3)
            {
                int x = coords[i], y = coords[i + 1], layer = coords[i + 2];
                ConnectedCells.Remove(ConnectedKey(x, y, layer));
                ModTiles.TakeAnchored(x, y, layer, stamp.Decor);
                if (stamp.Damaging)
                {
                    ElectricFences.SetElectrified(x, y, false);
                }
                ModCollision.SetBlocked(x, y, layer, false);
            }
            foreach (var pair in beforeMap)
            {
                var s = pair.Value;
                ResolveConnectedAt(s.X, s.Y, s.Layer);
            }

            var after = new List<ConnectedTileSnapshot>();
            var beforeList = new List<ConnectedTileSnapshot>();
            foreach (var pair in beforeMap)
            {
                beforeList.Add(pair.Value);
                after.Add(CaptureConnectedTile(pair.Value.X, pair.Value.Y, pair.Value.Layer));
            }
            UndoHistory.Push("erase all " + (string.IsNullOrEmpty(stamp.Name) ? "CTM" : stamp.Name),
                () =>
                {
                    foreach (var s in beforeList) ApplyConnectedTileSnapshot(s);
                },
                () =>
                {
                    foreach (var s in after) ApplyConnectedTileSnapshot(s);
                });
            OnChanged();
            return coords.Count / 3;
        }

        private static bool ConnectsTo(int x, int y, int layer, ConnectedCell me)
        {
            ConnectedCell other;
            return ConnectedCells.TryGetValue(ConnectedKey(x, y, layer), out other) &&
                !other.Locked && other.Color == me.Color;
        }

        private static void ResolveConnectedAt(int x, int y, int layer)
        {
            ConnectedCell cell;
            if (!ConnectedCells.TryGetValue(ConnectedKey(x, y, layer), out cell) ||
                cell.Stamp == null)
            {
                return;
            }
            int mask = cell.Manual
                ? cell.ManualMask
                : ((ConnectsTo(x, y + 1, layer, cell) ? 1 : 0) |
                   (ConnectsTo(x + 1, y, layer, cell) ? 2 : 0) |
                   (ConnectsTo(x, y - 1, layer, cell) ? 4 : 0) |
                   (ConnectsTo(x - 1, y, layer, cell) ? 8 : 0));
            var variant = PickConnectedVariant(cell.Stamp, mask);
            if (variant == null) return;
            ModTiles.Place(x, y, layer, cell.Stamp.Decor, variant.Atlas,
                variant.X, variant.Y, variant.W, variant.H);
        }

        private static ConnectedVariant PickConnectedVariant(ConnectedTileStamp stamp, int mask)
        {
            List<ConnectedVariant> list;
            if (!stamp.Variants.TryGetValue(mask, out list) || list.Count == 0)
            {
                if (!stamp.Variants.TryGetValue(0, out list) || list.Count == 0)
                    return null;
            }
            // Weight 0 disables a variant; if every variant is disabled fall
            // back to the first so the brush still paints something.
            int total = 0;
            foreach (var v in list) total += Math.Max(v.Weight, 0);
            if (total <= 0)
            {
                return list[0];
            }
            int roll = UnityEngine.Random.Range(0, total);
            foreach (var v in list)
            {
                int w = Math.Max(v.Weight, 0);
                if (w == 0) continue;
                roll -= w;
                if (roll < 0) return v;
            }
            return list[0];
        }

        public static void ConnectedNextColor()
        {
            if (ConnectedStamp == null) return;
            SetConnectedColor(ConnectedStamp.Color >= 8 ? 1 : ConnectedStamp.Color + 1);
        }

        public static void ConnectedToggleAuto()
        {
            if (ConnectedStamp == null) return;
            SetConnectedAuto(!ConnectedStamp.Auto);
        }

        public static void ConnectedToggleLock()
        {
            if (ConnectedStamp == null) return;
            SetConnectedLock(!ConnectedStamp.LockPlaced);
        }

        public static void ConnectedNextManualMask()
        {
            if (ConnectedStamp == null) return;
            SetConnectedManualMask(ConnectedStamp.ManualMask >= 15 ? 0 : ConnectedStamp.ManualMask + 1);
        }

        public static void ConnectedToggleDamaging()
        {
            if (ConnectedStamp == null) return;
            SetConnectedDamaging(!ConnectedStamp.Damaging);
        }

        public static void ConnectedToggleCollision()
        {
            if (ConnectedStamp == null) return;
            SetConnectedCollision(!ConnectedStamp.CollideWithPlayers);
        }

        public static void ConnectedToggleDestructible()
        {
            if (ConnectedStamp == null) return;
            SetConnectedDestructible(!ConnectedStamp.Destructible);
        }

        public static void SetConnectedAuto(bool auto)
        {
            if (ConnectedStamp == null) return;
            ConnectedStamp.Auto = auto;
            ConnectedStamp.ManualSingle = !auto;
            if (!auto && ConnectedStamp.ManualMask < 0)
            {
                ConnectedStamp.ManualMask = 15;
            }
            OnChanged();
        }

        public static void SetConnectedLock(bool locked)
        {
            if (ConnectedStamp == null) return;
            ConnectedStamp.LockPlaced = locked;
            OnChanged();
        }

        public static void SetConnectedColor(int color)
        {
            if (ConnectedStamp == null) return;
            ConnectedStamp.Color = Mathf.Clamp(color, 1, 8);
            OnChanged();
        }

        public static void SetConnectedManualMask(int mask)
        {
            if (ConnectedStamp == null) return;
            ConnectedStamp.ManualMask = Mathf.Clamp(mask, 0, 15);
            ConnectedStamp.ManualSingle = true;
            ConnectedStamp.Auto = false;
            OnChanged();
        }

        public static void SetConnectedDamaging(bool damaging)
        {
            if (ConnectedStamp == null) return;
            ConnectedStamp.Damaging = damaging;
            OnChanged();
        }

        public static void SetConnectedCollision(bool collides)
        {
            if (ConnectedStamp == null) return;
            ConnectedStamp.CollideWithPlayers = collides;
            OnChanged();
        }

        public static void SetConnectedDestructible(bool destructible)
        {
            if (ConnectedStamp == null) return;
            ConnectedStamp.Destructible = destructible;
            OnChanged();
        }

        public static void SetShowConnectedColors(bool show)
        {
            ShowConnectedColors = show;
            OnChanged();
        }

        public static void SetConnectedVariantWeight(int mask, int index, int weight)
        {
            if (ConnectedStamp == null) return;
            List<ConnectedVariant> variants;
            if (!ConnectedStamp.Variants.TryGetValue(mask, out variants) ||
                index < 0 || index >= variants.Count)
            {
                return;
            }
            variants[index].Weight = Mathf.Clamp(weight, 0, 100);
            OnChanged();
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

        private static int CurrentEditorLayer()
        {
            int layer = Grid.CurrentEditorLayer;
            return layer < 0 ? 1 : layer;
        }

        private static void Paint(bool on)
        {
            if (UiInput.IsPointerOverUi())
            {
                return;
            }
            if (!Input.GetMouseButton(0))
            {
                return;
            }
            int x, y;
            if (Placement.GetCursorTile(out x, out y))
            {
                bool was = ElectricFences.IsElectrified(x, y);
                if (was == on)
                {
                    return;
                }
                ElectricFences.SetElectrified(x, y, on);
                RecordAction(on ? "electrify" : "clear electric",
                    () => ElectricFences.SetElectrified(x, y, was),
                    () => ElectricFences.SetElectrified(x, y, on));
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
            if (UiInput.IsPointerOverUi())
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
                var link = Triggers.AddLink(SwitchX, SwitchY, x, y,
                    Triggers.TriggerAction.ToggleFence);
                RecordAction("switch link",
                    () => Triggers.Remove(link),
                    () => Triggers.Restore(link));
            }
            OnChanged();
        }

        /// <summary>
        /// Two-click teleport link tool: first click marks the source tile on
        /// the current floor, the user may then switch floors, and the second
        /// click marks the destination (e.g. stairs going up or down).
        /// </summary>
        private static void TickLinkTeleport()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && SwitchX >= 0)
            {
                SwitchX = SwitchY = -1;
                SwitchLayer = -1;
                OnChanged();
                return;
            }
            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }
            if (UiInput.IsPointerOverUi())
            {
                return;
            }
            int x, y;
            if (!Placement.GetCursorTile(out x, out y))
            {
                return;
            }
            int layer = CurrentEditorLayer();
            if (SwitchX < 0)
            {
                SwitchX = x;
                SwitchY = y;
                SwitchLayer = layer;
            }
            else
            {
                var link = Triggers.AddLink(SwitchX, SwitchY, x, y,
                    Triggers.TriggerAction.Teleport, SwitchLayer, layer);
                RecordAction("teleport link",
                    () => Triggers.Remove(link),
                    () => Triggers.Restore(link));
                SwitchX = SwitchY = -1;
                SwitchLayer = -1;
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

