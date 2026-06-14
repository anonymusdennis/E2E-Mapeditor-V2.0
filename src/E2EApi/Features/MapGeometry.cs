using System;
using System.Collections.Generic;
using System.Text;
using E2EApi.Persistence;
using E2EApi.Editor;

namespace E2EApi.Features
{
    /// <summary>
    /// Sidecar-backed virtual map geometry. Vanilla remains 120x120 with six
    /// native layers; this describes the modded logical map above that.
    /// </summary>
    public static class MapGeometry
    {
        public const int FormatVersion = 1;
        public const int NativeWidth = 120;
        public const int NativeHeight = 120;
        public const int NativeLayerCount = 6;
        private const string SectionName = "map_geometry";

        public enum VirtualLayerType
        {
            Underground,
            Ground,
            Vent,
            Roof,
        }

        public class VirtualLayer
        {
            public int Id;
            public string Name;
            public VirtualLayerType Type;
            public int BackingLayer;
            /// <summary>
            /// When true the layer is visually hidden from the game view unless it is
            /// the currently selected editing layer (in which case it is temporarily shown).
            /// </summary>
            public bool Hidden;

            public VirtualLayer Clone()
            {
                return new VirtualLayer
                {
                    Id = Id,
                    Name = Name,
                    Type = Type,
                    BackingLayer = BackingLayer,
                    Hidden = Hidden,
                };
            }
        }

        public class GeometryState
        {
            public int Version = FormatVersion;
            public int Width = NativeWidth;
            public int Height = NativeHeight;
            public int OriginX;
            public int OriginY;
            public readonly List<VirtualLayer> Layers = new List<VirtualLayer>();

            public GeometryState Clone()
            {
                var clone = new GeometryState
                {
                    Version = Version,
                    Width = Width,
                    Height = Height,
                    OriginX = OriginX,
                    OriginY = OriginY,
                };
                foreach (var layer in Layers)
                {
                    clone.Layers.Add(layer.Clone());
                }
                return clone;
            }
        }

        private static bool _initialised;
        private static GeometryState _current = DefaultState();
        private static int _selectedIndex = 1;
        private static int _lastNativeLayer = -1;
        private static bool _virtualSelectionChanged;
        private static string _loadMismatchWarning;
        private static readonly List<VirtualLayer> _trashBin = new List<VirtualLayer>();

        public static event Action Changed;

        public static GeometryState Current => _current.Clone();
        public static int Width => _current.Width;
        public static int Height => _current.Height;
        public static int OriginX => _current.OriginX;
        public static int OriginY => _current.OriginY;
        public static int LayerCount => _current.Layers.Count;
        public static int SelectedVirtualLayerIndex => ClampLayerIndex(_selectedIndex);
        public static int SelectedVirtualLayerId => GetLayer(SelectedVirtualLayerIndex).Id;
        public static string CompatibilityHash => ComputeHash(_current);
        public static int Version { get; private set; }
        public static IReadOnlyList<VirtualLayer> TrashBin => _trashBin;
        public static int TrashCount => _trashBin.Count;
        public static bool IsNativeCompatible =>
            Width == NativeWidth && Height == NativeHeight && OriginX == 0 &&
            OriginY == 0 && LayerCount == NativeLayerCount;

        public static void Initialise()
        {
            if (_initialised)
            {
                return;
            }
            _initialised = true;
            ModExtras.EnsurePatched();
            ModExtras.Saving += OnSaving;
            // MapGeometry.OnLoaded must subscribe before FloorTypeRegistry.OnLoaded
            // so that the registry can read a fully-populated MapGeometry.Current.
            ModExtras.Loaded += OnLoaded;
            FloorTypeRegistry.Initialise();
            // Ensure runtime floor patches are active.
            Editor.Patches.FloorPredicatePatchGroup.EnsurePatched();
            Editor.Patches.FloorNavigationPatchGroup.EnsurePatched();
            Editor.Patches.StairsPatchGroup.EnsurePatched();
            Editor.Patches.FloorZLookupPatchGroup.EnsurePatched();
            Editor.Patches.CharacterFloorPatchGroup.EnsurePatched();
            VirtualLayerListUi.Initialise();
            Editor.Patches.MapExpansionPatchRegistrar.EnsurePatched();
        }

        public static VirtualLayer GetLayer(int index)
        {
            EnsureValid();
            index = ClampLayerIndex(index);
            return _current.Layers[index];
        }

        public static int GetBackingLayer(int virtualLayerIndex)
        {
            return GetLayer(virtualLayerIndex).BackingLayer;
        }

        public static int NativeLayerForType(VirtualLayerType type)
        {
            switch (type)
            {
                case VirtualLayerType.Underground: return 0;
                case VirtualLayerType.Vent: return 2;
                case VirtualLayerType.Roof: return 5;
                default: return 1;
            }
        }

        /// <summary>
        /// Picks the least-used native backing layer for a new virtual layer of the given type.
        /// Prefers type-appropriate layers first so that same-type virtual layers don't all
        /// collide on the same native slot.
        /// </summary>
        /// <remarks>
        /// Priority rationale:
        ///   Underground → {0, 1, 2, 3, 4, 5} : native Underground first, then overflow upward
        ///   Vent        → {2, 4, 1, 3, 0, 5} : native vent slots first, then non-vents
        ///   Roof        → {5, 3, 1, 0, 2, 4} : native Roof first, then upper floors, then lower
        ///   Ground      → {1, 3, 0, 2, 4, 5} : native GroundFloor/FirstFloor first, then rest
        /// </remarks>
        private static int SmartBackingLayer(VirtualLayerType type)
        {
            int[] preferred;
            switch (type)
            {
                case VirtualLayerType.Underground: preferred = new[] { 0, 1, 2, 3, 4, 5 }; break;
                case VirtualLayerType.Vent:        preferred = new[] { 2, 4, 1, 3, 0, 5 }; break;
                case VirtualLayerType.Roof:        preferred = new[] { 5, 3, 1, 0, 2, 4 }; break;
                default:                           preferred = new[] { 1, 3, 0, 2, 4, 5 }; break;
            }
            var usage = new int[NativeLayerCount];
            foreach (var layer in _current.Layers)
            {
                if (layer.BackingLayer >= 0 && layer.BackingLayer < NativeLayerCount)
                {
                    usage[layer.BackingLayer]++;
                }
            }
            int best = preferred[0];
            int bestUsage = int.MaxValue;
            foreach (int n in preferred)
            {
                if (usage[n] < bestUsage)
                {
                    bestUsage = usage[n];
                    best = n;
                    if (bestUsage == 0) break;
                }
            }
            return best;
        }

        public static void SelectLayer(int index)
        {
            _selectedIndex = ClampLayerIndex(index);
            _virtualSelectionChanged = true;
            SyncNativeEditorLayer();
            ApplyLayerVisibility();
            FireChanged();
        }

        public static void MoveSelected(int delta)
        {
            SelectLayer(_selectedIndex + delta);
        }

        public static void Apply(GeometryState state)
        {
            _current = Sanitize(state);
            _selectedIndex = ClampLayerIndex(_selectedIndex);
            _virtualSelectionChanged = true;
            FloorTypeRegistry.RebuildFromGeometry();
            FireChanged();
        }

        public static void ResetDefault()
        {
            Apply(DefaultState());
        }

        public static void SetBounds(int width, int height, int originX, int originY)
        {
            var state = _current.Clone();
            state.Width = width;
            state.Height = height;
            state.OriginX = originX;
            state.OriginY = originY;
            Apply(state);
        }

        public static void AddLayer(VirtualLayerType type)
        {
            var state = _current.Clone();
            int id = 0;
            foreach (var layer in state.Layers)
            {
                if (layer.Id >= id) id = layer.Id + 1;
            }
            state.Layers.Add(new VirtualLayer
            {
                Id = id,
                Name = DefaultName(type, state.Layers.Count),
                Type = type,
                BackingLayer = SmartBackingLayer(type),
            });
            Apply(state);
            SelectLayer(state.Layers.Count - 1);
        }

        public static void RemoveLayer(int index)
        {
            if (_current.Layers.Count <= 1)
            {
                return;
            }
            var state = _current.Clone();
            index = Math.Max(0, Math.Min(index, state.Layers.Count - 1));
            _trashBin.Add(state.Layers[index].Clone());
            state.Layers.RemoveAt(index);
            Apply(state);
        }

        /// <summary>Restore a previously removed layer from the trash bin.</summary>
        public static void RestoreFromTrash(int trashIndex)
        {
            if (trashIndex < 0 || trashIndex >= _trashBin.Count)
            {
                return;
            }
            var trashed = _trashBin[trashIndex].Clone();
            _trashBin.RemoveAt(trashIndex);
            var state = _current.Clone();
            int id = 0;
            foreach (var layer in state.Layers)
            {
                if (layer.Id >= id) id = layer.Id + 1;
            }
            trashed.Id = id;
            state.Layers.Add(trashed);
            Apply(state);
            SelectLayer(state.Layers.Count - 1);
        }

        /// <summary>Permanently discard all trashed layers.</summary>
        public static void ClearTrash()
        {
            if (_trashBin.Count == 0)
            {
                return;
            }
            _trashBin.Clear();
            FireChanged();
        }

        public static void SetLayerHidden(int index, bool hidden)
        {
            EnsureValid();
            var state = _current.Clone();
            index = Math.Max(0, Math.Min(index, state.Layers.Count - 1));
            state.Layers[index].Hidden = hidden;
            Apply(state);
            ApplyLayerVisibility();
        }

        public static void MoveLayer(int index, int delta)
        {
            var state = _current.Clone();
            index = Math.Max(0, Math.Min(index, state.Layers.Count - 1));
            int target = Math.Max(0, Math.Min(state.Layers.Count - 1, index + delta));
            if (target == index)
            {
                return;
            }
            var layer = state.Layers[index];
            state.Layers.RemoveAt(index);
            state.Layers.Insert(target, layer);
            Apply(state);
            SelectLayer(target);
        }

        public static void SetLayerType(int index, VirtualLayerType type)
        {
            var state = _current.Clone();
            index = Math.Max(0, Math.Min(index, state.Layers.Count - 1));
            state.Layers[index].Type = type;
            state.Layers[index].BackingLayer = SmartBackingLayer(type);
            state.Layers[index].Name = DefaultName(type, index);
            Apply(state);
        }

        public static void DuplicateLayer(int index)
        {
            var state = _current.Clone();
            index = Math.Max(0, Math.Min(index, state.Layers.Count - 1));
            var source = state.Layers[index];
            int id = 0;
            foreach (var layer in state.Layers)
            {
                if (layer.Id >= id) id = layer.Id + 1;
            }
            state.Layers.Insert(index + 1, new VirtualLayer
            {
                Id = id,
                Name = source.Name + " copy",
                Type = source.Type,
                BackingLayer = source.BackingLayer,
            });
            Apply(state);
            SelectLayer(index + 1);
        }

        /// <summary>
        /// Keep virtual/native layer selection aligned. Call once per editor frame.
        /// </summary>
        public static void SyncEditorLayers()
        {
            if (!E2EApi.Events.GameEvents.IsEditorActive)
            {
                return;
            }
            var editor = EditorLevelEditorManager.GetLevelEditorInstance();
            if (editor == null)
            {
                return;
            }
            int native = (int)editor.m_CurrentLayer;
            if (_virtualSelectionChanged)
            {
                _virtualSelectionChanged = false;
                SyncNativeEditorLayer();
                _lastNativeLayer = (int)editor.m_CurrentLayer;
                return;
            }
            if (_lastNativeLayer >= 0 && native != _lastNativeLayer)
            {
                SyncFromNativeEditorLayer(native);
            }
            else
            {
                SyncNativeEditorLayer();
            }
            _lastNativeLayer = (int)editor.m_CurrentLayer;
        }

        public static string ToJson()
        {
            EnsureValid();
            var sb = new StringBuilder();
            sb.Append("{\"version\":").Append(_current.Version)
              .Append(",\"width\":").Append(_current.Width)
              .Append(",\"height\":").Append(_current.Height)
              .Append(",\"originX\":").Append(_current.OriginX)
              .Append(",\"originY\":").Append(_current.OriginY)
              .Append(",\"selected\":").Append(SelectedVirtualLayerIndex)
              .Append(",\"hash\":\"").Append(CompatibilityHash).Append("\"")
              .Append(",\"nativeCompatible\":").Append(IsNativeCompatible ? "true" : "false")
              .Append(",\"warning\":\"").Append(JsonEscape(CompatibilityWarning)).Append("\"")
              .Append(",\"layers\":[");
            for (int i = 0; i < _current.Layers.Count; i++)
            {
                if (i > 0) sb.Append(",");
                var layer = _current.Layers[i];
                sb.Append("{\"id\":").Append(layer.Id)
                  .Append(",\"name\":\"").Append(JsonEscape(layer.Name)).Append("\"")
                  .Append(",\"type\":\"").Append(layer.Type).Append("\"")
                  .Append(",\"backingLayer\":").Append(layer.BackingLayer)
                  .Append(",\"hidden\":").Append(layer.Hidden ? "true" : "false")
                  .Append("}");
            }
            sb.Append("],\"trash\":[");
            for (int i = 0; i < _trashBin.Count; i++)
            {
                if (i > 0) sb.Append(",");
                var layer = _trashBin[i];
                sb.Append("{\"id\":").Append(layer.Id)
                  .Append(",\"name\":\"").Append(JsonEscape(layer.Name)).Append("\"")
                  .Append(",\"type\":\"").Append(layer.Type).Append("\"")
                  .Append(",\"backingLayer\":").Append(layer.BackingLayer)
                  .Append(",\"trashIndex\":").Append(i)
                  .Append("}");
            }
            sb.Append("]}");
            return sb.ToString();
        }

        public static string CompatibilityWarning
        {
            get
            {
                if (!string.IsNullOrEmpty(_loadMismatchWarning))
                {
                    return _loadMismatchWarning;
                }
                if (IsNativeCompatible)
                {
                    return "";
                }
                return "Custom geometry requires E2E Map Editor with matching Level.e2e sidecar on every multiplayer client.";
            }
        }

        public static bool HasCompatibilityIssue =>
            !string.IsNullOrEmpty(CompatibilityWarning);

        private static void OnSaving(ModExtras extras)
        {
            extras.ClearSection(SectionName);
            extras.ClearSection(SectionName + "_trash");
            if (IsDefault(_current))
            {
                extras.GeometryFeatureVersion = 0;
                extras.GeometryHash = null;
            }
            else
            {
                var section = extras.Section(SectionName);
                section.Add("version=" + FormatVersion);
                section.Add("selected=" + SelectedVirtualLayerIndex);
                section.Add("hash=" + CompatibilityHash);
                section.Add("bounds=" + _current.Width + "," + _current.Height + "," +
                    _current.OriginX + "," + _current.OriginY);
                foreach (var layer in _current.Layers)
                {
                    section.Add("layer=" + layer.Id + "|" + layer.Type + "|" +
                        layer.BackingLayer + "|" + Escape(layer.Name) + "|" +
                        (layer.Hidden ? "1" : "0"));
                }
                extras.RequiresMod = true;
                extras.GeometryFeatureVersion = FormatVersion;
                extras.GeometryHash = CompatibilityHash;
            }
            if (_trashBin.Count > 0)
            {
                var trashSection = extras.Section(SectionName + "_trash");
                foreach (var layer in _trashBin)
                {
                    trashSection.Add("layer=" + layer.Id + "|" + layer.Type + "|" +
                        layer.BackingLayer + "|" + Escape(layer.Name));
                }
            }
        }

        private static void OnLoaded(ModExtras extras)
        {
            var section = extras.Section(SectionName);
            _loadMismatchWarning = null;
            _current = section.Count == 0 ? DefaultState() : Parse(section);
            _selectedIndex = ClampLayerIndex(_selectedIndex);
            if (!string.IsNullOrEmpty(extras.GeometryHash) &&
                extras.GeometryHash != CompatibilityHash)
            {
                _loadMismatchWarning =
                    "Map geometry hash mismatch — every multiplayer client needs the same Level.e2e sidecar.";
            }
            _trashBin.Clear();
            var trashSection = extras.Section(SectionName + "_trash");
            foreach (string raw in trashSection)
            {
                if (raw.StartsWith("layer="))
                {
                    string[] p = raw.Substring("layer=".Length).Split('|');
                    if (p.Length >= 4)
                    {
                        int id, backing;
                        VirtualLayerType type;
                        if (!int.TryParse(p[0], out id)) id = _trashBin.Count;
                        type = ParseLayerType(p[1]);
                        if (!int.TryParse(p[2], out backing)) backing = NativeLayerForType(type);
                        _trashBin.Add(new VirtualLayer
                        {
                            Id = id,
                            Type = type,
                            BackingLayer = backing,
                            Name = Unescape(p[3]),
                        });
                    }
                }
            }
            _lastNativeLayer = -1;
            SyncNativeEditorLayer();
            FireChanged();
        }

        public static void SyncNativeEditorLayer()
        {
            if (!E2EApi.Events.GameEvents.IsEditorActive)
            {
                return;
            }
            var editor = EditorLevelEditorManager.GetLevelEditorInstance();
            int backing = GetBackingLayer(SelectedVirtualLayerIndex);
            if (editor != null && (int)editor.m_CurrentLayer != backing)
            {
                editor.m_CurrentLayer = (BaseLevelManager.LevelLayers)backing;
            }
        }

        public static void SyncFromNativeEditorLayer(int nativeLayer)
        {
            nativeLayer = Clamp(nativeLayer, 0, NativeLayerCount - 1);
            int best = SelectedVirtualLayerIndex;
            int bestDistance = int.MaxValue;
            for (int i = 0; i < _current.Layers.Count; i++)
            {
                if (_current.Layers[i].BackingLayer != nativeLayer)
                {
                    continue;
                }
                int distance = Math.Abs(i - _selectedIndex);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = i;
                }
            }
            if (best != _selectedIndex)
            {
                _selectedIndex = best;
                FireChanged();
            }
            SyncNativeEditorLayer();
        }

        public static bool IsWithinMapBounds(int x, int y)
        {
            return x >= OriginX && x < OriginX + Width &&
                y >= OriginY && y < OriginY + Height;
        }

        public static bool IsWithinNativeBounds(int x, int y)
        {
            return x >= 0 && x < NativeWidth && y >= 0 && y < NativeHeight;
        }

        /// <summary>
        /// Show or hide the game-level backing layer objects to match the Hidden flags.
        /// A hidden virtual layer's backing native layer is deactivated UNLESS the
        /// currently selected virtual layer also uses that same backing layer — in that
        /// case it stays visible so the player can keep editing.
        /// </summary>
        public static void ApplyLayerVisibility()
        {
            if (!E2EApi.Events.GameEvents.IsEditorActive)
            {
                return;
            }
            var manager = BaseLevelManager.GetInstance();
            if (manager == null || manager.m_BuildingLayers == null)
            {
                return;
            }

            int selectedBacking = GetBackingLayer(SelectedVirtualLayerIndex);

            // Determine visibility for each native layer slot:
            // visible if ANY non-hidden virtual layer uses it, OR if the selected
            // virtual layer (even if marked hidden) uses it (auto-unhide while editing).
            var nativeVisible = new bool[NativeLayerCount];
            for (int i = 0; i < _current.Layers.Count; i++)
            {
                var layer = _current.Layers[i];
                int backing = Clamp(layer.BackingLayer, 0, NativeLayerCount - 1);
                bool isSelected = (i == SelectedVirtualLayerIndex);
                if (!layer.Hidden || isSelected)
                {
                    nativeVisible[backing] = true;
                }
            }

            for (int n = 0; n < NativeLayerCount && n < manager.m_BuildingLayers.Length; n++)
            {
                var ld = manager.m_BuildingLayers[n];
                bool show = nativeVisible[n];
                if (ld.m_Objects != null) ld.m_Objects.SetActive(show);
                if (ld.m_Tiles != null) ld.m_Tiles.gameObject.SetActive(show);
                if (ld.m_Walls != null) ld.m_Walls.SetActive(show);
                if (ld.m_Decorations != null) ld.m_Decorations.SetActive(show);
            }
        }



        private static GeometryState Parse(List<string> lines)
        {
            var state = new GeometryState();
            string savedHash = null;
            foreach (string raw in lines)
            {
                if (raw.StartsWith("version="))
                {
                    int.TryParse(raw.Substring("version=".Length), out state.Version);
                }
                else if (raw.StartsWith("selected="))
                {
                    int.TryParse(raw.Substring("selected=".Length), out _selectedIndex);
                }
                else if (raw.StartsWith("hash="))
                {
                    savedHash = raw.Substring("hash=".Length);
                }
                else if (raw.StartsWith("bounds="))
                {
                    string[] p = raw.Substring("bounds=".Length).Split(',');
                    if (p.Length >= 4)
                    {
                        int.TryParse(p[0], out state.Width);
                        int.TryParse(p[1], out state.Height);
                        int.TryParse(p[2], out state.OriginX);
                        int.TryParse(p[3], out state.OriginY);
                    }
                }
                else if (raw.StartsWith("layer="))
                {
                    string[] p = raw.Substring("layer=".Length).Split('|');
                    if (p.Length >= 4)
                    {
                        int id, backing;
                        VirtualLayerType type;
                        if (!int.TryParse(p[0], out id)) id = state.Layers.Count;
                        type = ParseLayerType(p[1]);
                        if (!int.TryParse(p[2], out backing)) backing = NativeLayerForType(type);
                        state.Layers.Add(new VirtualLayer
                        {
                            Id = id,
                            Type = type,
                            BackingLayer = backing,
                            Name = Unescape(p[3]),
                            Hidden = p.Length >= 5 && p[4] == "1",
                        });
                    }
                }
            }
            var sanitized = Sanitize(state);
            if (!string.IsNullOrEmpty(savedHash) &&
                savedHash != ComputeHash(sanitized))
            {
                _loadMismatchWarning =
                    "Map geometry hash mismatch — update E2E Map Editor or re-save this map.";
                Log.Warn("MapGeometry: sidecar hash " + savedHash + " != computed " +
                    ComputeHash(sanitized));
            }
            return sanitized;
        }

        private static VirtualLayerType ParseLayerType(string value)
        {
            try
            {
                return (VirtualLayerType)Enum.Parse(typeof(VirtualLayerType), value);
            }
            catch
            {
                return VirtualLayerType.Ground;
            }
        }

        private static GeometryState Sanitize(GeometryState state)
        {
            if (state == null)
            {
                return DefaultState();
            }
            var clean = new GeometryState
            {
                Version = FormatVersion,
                Width = Clamp(state.Width, 1, 512),
                Height = Clamp(state.Height, 1, 512),
                OriginX = Clamp(state.OriginX, -512, 512),
                OriginY = Clamp(state.OriginY, -512, 512),
            };
            foreach (var layer in state.Layers)
            {
                if (layer == null) continue;
                var copy = layer.Clone();
                copy.BackingLayer = Clamp(copy.BackingLayer, 0, NativeLayerCount - 1);
                if (string.IsNullOrEmpty(copy.Name))
                {
                    copy.Name = DefaultName(copy.Type, clean.Layers.Count);
                }
                clean.Layers.Add(copy);
                if (clean.Layers.Count >= 256)
                {
                    break;
                }
            }
            if (clean.Layers.Count == 0)
            {
                return DefaultState();
            }
            return clean;
        }

        private static GeometryState DefaultState()
        {
            var state = new GeometryState();
            state.Layers.Add(new VirtualLayer { Id = 0, Name = "Underground", Type = VirtualLayerType.Underground, BackingLayer = 0 });
            state.Layers.Add(new VirtualLayer { Id = 1, Name = "Ground", Type = VirtualLayerType.Ground, BackingLayer = 1 });
            state.Layers.Add(new VirtualLayer { Id = 2, Name = "Ground Vent", Type = VirtualLayerType.Vent, BackingLayer = 2 });
            state.Layers.Add(new VirtualLayer { Id = 3, Name = "First Floor", Type = VirtualLayerType.Ground, BackingLayer = 3 });
            state.Layers.Add(new VirtualLayer { Id = 4, Name = "First Floor Vent", Type = VirtualLayerType.Vent, BackingLayer = 4 });
            state.Layers.Add(new VirtualLayer { Id = 5, Name = "Roof", Type = VirtualLayerType.Roof, BackingLayer = 5 });
            return state;
        }

        private static bool IsDefault(GeometryState state)
        {
            return ComputeHash(Sanitize(state)) == ComputeHash(DefaultState());
        }

        private static int ClampLayerIndex(int index)
        {
            EnsureValid();
            return Clamp(index, 0, _current.Layers.Count - 1);
        }

        private static void EnsureValid()
        {
            if (_current == null || _current.Layers.Count == 0)
            {
                _current = DefaultState();
                _selectedIndex = 1;
            }
        }

        private static string DefaultName(VirtualLayerType type, int index)
        {
            switch (type)
            {
                case VirtualLayerType.Underground: return "Underground " + index;
                case VirtualLayerType.Vent: return "Vent " + index;
                case VirtualLayerType.Roof: return "Roof " + index;
                default: return "Ground " + index;
            }
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private static string ComputeHash(GeometryState state)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + state.Width;
                hash = hash * 31 + state.Height;
                hash = hash * 31 + state.OriginX;
                hash = hash * 31 + state.OriginY;
                foreach (var layer in state.Layers)
                {
                    hash = hash * 31 + layer.Id;
                    hash = hash * 31 + (int)layer.Type;
                    hash = hash * 31 + layer.BackingLayer;
                }
                return hash.ToString("x8");
            }
        }

        private static void FireChanged()
        {
            Version++;
            var handler = Changed;
            if (handler != null)
            {
                handler();
            }
        }

        private static string Escape(string text)
        {
            return (text ?? "").Replace("\\", "\\\\").Replace("|", "\\p").Replace("\n", "\\n");
        }

        private static string Unescape(string text)
        {
            return (text ?? "").Replace("\\n", "\n").Replace("\\p", "|").Replace("\\\\", "\\");
        }

        private static string JsonEscape(string text)
        {
            return (text ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
