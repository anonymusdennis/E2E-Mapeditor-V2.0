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
            Stamp = null;
            AnimatedStamp = null;
            Composite = null;
            ConnectedStamp = null;
            SwitchX = SwitchY = -1;
            _lastStampX = _lastStampY = int.MinValue;
            _lastAnimStampX = _lastAnimStampY = int.MinValue;
            Blocks.ClearBrush();
            SetMode(EditorToolMode.None);
            OnChanged();
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
                case EditorToolMode.PaintConnectedTile:
                    UpdateBrushCursor(new Color(0.65f, 0.45f, 1f, 0.95f));
                    TickPaintConnectedTile();
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
                        AnimatedModTiles.Place(x + part.Dx, y + part.Dy, baseLayer,
                            part.Decor, part.Fps, part.Loop, part.PingPong,
                            part.Frames);
                    }
                    else if (!string.IsNullOrEmpty(part.Atlas))
                    {
                        ModTiles.Place(x + part.Dx, y + part.Dy, baseLayer,
                            part.Decor, part.Atlas, part.X, part.Y, part.W, part.H);
                    }
                    else if (part.Vanilla && part.BlockId >= 0)
                    {
                        Placement.PlaceBlock(part.BlockId, x + part.Dx, y + part.Dy);
                    }
                }
                return;
            }
            ModTiles.Place(x, y, baseLayer, Stamp.Decor,
                Stamp.Atlas, Stamp.X, Stamp.Y, Stamp.W, Stamp.H);
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
                ModTiles.EraseAt(x, y, layer);
                AnimatedModTiles.EraseAt(x, y, layer);
            }
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
            AnimatedModTiles.Place(x, y, layer,
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
                AnimatedModTiles.EraseAt(x, y, CurrentEditorLayer());
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
                        AnimatedModTiles.EraseAt(px, py, baseLayer);
                    }
                    else if (part.Vanilla && part.BlockId >= 0)
                    {
                        Blocks.DeleteAt(part.BlockId, px, py);
                    }
                    else
                    {
                        ModTiles.EraseAt(px, py, baseLayer);
                    }
                }
                return;
            }

            if (Stamp != null)
            {
                ModTiles.EraseAt(x, y, baseLayer);
            }
        }

        private static void EraseAnimatedBrushAtCursor()
        {
            int x, y;
            if (!Placement.GetCursorTile(out x, out y))
            {
                return;
            }
            AnimatedModTiles.EraseAt(x, y, CurrentEditorLayer());
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
            ConnectedCells.Remove(key);
            ModTiles.EraseAt(x, y, layer);
            if (cell.Stamp != null && cell.Stamp.Damaging)
            {
                ElectricFences.SetElectrified(x, y, false);
            }
            ResolveConnectedAt(x + 1, y, layer);
            ResolveConnectedAt(x - 1, y, layer);
            ResolveConnectedAt(x, y + 1, layer);
            ResolveConnectedAt(x, y - 1, layer);
        }

        private static string ConnectedKey(int x, int y, int layer)
            => layer + ":" + x + ":" + y;

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
            int total = 0;
            foreach (var v in list) total += Math.Max(v.Weight, 1);
            int roll = UnityEngine.Random.Range(0, Math.Max(total, 1));
            foreach (var v in list)
            {
                roll -= Math.Max(v.Weight, 1);
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
            variants[index].Weight = Mathf.Clamp(weight, 1, 100);
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

