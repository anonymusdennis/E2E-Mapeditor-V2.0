using System;
using System.Collections.Generic;
using System.Text;
using E2EApi.Persistence;

namespace E2EApi.Features
{
    /// <summary>
    /// Runtime mapping from physical <c>FloorManager.Floor.m_FloorIndex</c> to
    /// <c>MapGeometry.VirtualLayerType</c>. Rebuilt from <see cref="MapGeometry"/>
    /// whenever the virtual layer list changes (editor-side preview) and
    /// persisted to the <c>Level.e2e</c> sidecar under <c>[floor_type_map]</c>
    /// for runtime-only sessions where the editor is not active.
    ///
    /// Vanilla maps (no sidecar) always return <c>null</c> from
    /// <see cref="GetType"/>, so all convenience wrappers fall back to the
    /// native <c>FLOOR_TYPE</c> comparisons.
    /// </summary>
    public static class FloorTypeRegistry
    {
        private const string SectionName = "floor_type_map";

        // Physical floor index (m_FloorIndex, 0-5) → registered VirtualLayerType.
        // When two virtual layers share the same backing layer the last writer wins
        // in the type map; the full virtual ordering is tracked in _orderedVirtual.
        private static readonly Dictionary<int, MapGeometry.VirtualLayerType> _byPhysicalIndex =
            new Dictionary<int, MapGeometry.VirtualLayerType>();

        // Sorted (ascending physical-floor-index) list of registered physical floors.
        private static readonly List<int> _registeredPhysical = new List<int>();

        // Virtual layer ordering: virtualIndex → physical floor index (m_FloorIndex / BackingLayer).
        private static readonly List<int> _virtualToPhysical = new List<int>();

        // Physical floor index → list of virtual layer indices that map to it (for shared-backing disambiguation).
        private static readonly Dictionary<int, List<int>> _physicalToVirtual =
            new Dictionary<int, List<int>>();

        private static bool _initialised;

        // ---- Initialisation -------------------------------------------------

        /// <summary>
        /// Subscribe to sidecar events. Must be called after
        /// <see cref="MapGeometry.Initialise"/> so that MapGeometry populates
        /// first when <see cref="ModExtras.Loaded"/> fires.
        /// </summary>
        public static void Initialise()
        {
            if (_initialised) return;
            _initialised = true;
            ModExtras.Loaded += OnLoaded;
            ModExtras.Saving += OnSaving;
        }

        private static void OnLoaded(ModExtras extras)
        {
            // MapGeometry.OnLoaded has already run (it subscribed first in
            // MapGeometry.Initialise), so MapGeometry.Current is up-to-date.
            Clear();

            // Primary source: rebuild from the authoritative MapGeometry state.
            RebuildFromGeometry();

            // Secondary: if the sidecar carries an explicit [floor_type_map]
            // section (written for runtime-only environments), apply it on top.
            var section = extras.Section(SectionName);
            foreach (string line in section)
            {
                int floorIndex;
                MapGeometry.VirtualLayerType type;
                if (TryParseLine(line, out floorIndex, out type))
                {
                    _byPhysicalIndex[floorIndex] = type;
                }
            }
            RebuildRegisteredPhysical();
            Log.Debug($"FloorTypeRegistry loaded: {_byPhysicalIndex.Count} physical floors registered");
        }

        private static void OnSaving(ModExtras extras)
        {
            extras.ClearSection(SectionName);
            if (_virtualToPhysical.Count == 0) return;
            var section = extras.Section(SectionName);
            for (int vi = 0; vi < _virtualToPhysical.Count; vi++)
            {
                int physicalIndex = _virtualToPhysical[vi];
                MapGeometry.VirtualLayerType type;
                if (_byPhysicalIndex.TryGetValue(physicalIndex, out type))
                {
                    section.Add("floorIndex=" + physicalIndex + ",type=" + type);
                }
            }
        }

        // ---- Public API -----------------------------------------------------

        public static bool HasEntries => _byPhysicalIndex.Count > 0;

        /// <summary>
        /// Returns the registered virtual type for a physical floor index, or
        /// <c>null</c> when no override is registered (vanilla map).
        /// </summary>
        public static MapGeometry.VirtualLayerType? GetType(int floorIndex)
        {
            MapGeometry.VirtualLayerType t;
            if (_byPhysicalIndex.TryGetValue(floorIndex, out t)) return t;
            return null;
        }

        // ---- Convenience type wrappers (fall back to native FLOOR_TYPE) ------

        public static bool IsVent(FloorManager.Floor floor)
        {
            if (floor == null) return false;
            var t = GetType(floor.m_FloorIndex);
            if (t.HasValue) return t.Value == MapGeometry.VirtualLayerType.Vent;
            return floor.IsVent();
        }

        public static bool IsUnderground(FloorManager.Floor floor)
        {
            if (floor == null) return false;
            var t = GetType(floor.m_FloorIndex);
            if (t.HasValue) return t.Value == MapGeometry.VirtualLayerType.Underground;
            return floor.IsUnderGround();
        }

        public static bool IsGround(FloorManager.Floor floor)
        {
            if (floor == null) return false;
            var t = GetType(floor.m_FloorIndex);
            if (t.HasValue) return t.Value == MapGeometry.VirtualLayerType.Ground;
            return floor.IsPrisonFloor();
        }

        public static bool IsRoof(FloorManager.Floor floor)
        {
            if (floor == null) return false;
            var t = GetType(floor.m_FloorIndex);
            if (t.HasValue) return t.Value == MapGeometry.VirtualLayerType.Roof;
            return floor.m_FloorType == FloorManager.FLOOR_TYPE.Floor_Roof;
        }

        // ---- Navigation helpers ---------------------------------------------

        /// <summary>
        /// Finds the adjacent physical floor index in the given direction, following
        /// the registered physical ordering (ascending indices). Floors not in the
        /// registry are skipped; vent layers are optionally skipped too.
        /// Returns <paramref name="currentPhysicalIndex"/> when at a boundary.
        /// </summary>
        public static int FindNextFloor(int currentPhysicalIndex, bool up, bool skipVents = false)
        {
            if (_registeredPhysical.Count == 0) return currentPhysicalIndex;

            int pos = _registeredPhysical.IndexOf(currentPhysicalIndex);
            if (pos < 0) return currentPhysicalIndex;

            int step = up ? 1 : -1;
            for (int i = pos + step; up ? i < _registeredPhysical.Count : i >= 0; i += step)
            {
                int next = _registeredPhysical[i];
                if (skipVents)
                {
                    MapGeometry.VirtualLayerType t;
                    if (_byPhysicalIndex.TryGetValue(next, out t) &&
                        t == MapGeometry.VirtualLayerType.Vent)
                        continue;
                }
                return next;
            }
            return currentPhysicalIndex; // at boundary
        }

        /// <summary>Finds the next physical floor that is Ground or Roof, skipping vents.</summary>
        public static int FindNextGroundOrRoof(int currentPhysicalIndex, bool up)
        {
            return FindNextFloor(currentPhysicalIndex, up, skipVents: true);
        }

        /// <summary>Finds the next physical floor of a specific virtual type in the given direction.</summary>
        public static int FindNextOfType(int currentPhysicalIndex, bool up,
            MapGeometry.VirtualLayerType targetType)
        {
            if (_registeredPhysical.Count == 0) return currentPhysicalIndex;

            int pos = _registeredPhysical.IndexOf(currentPhysicalIndex);
            if (pos < 0) return currentPhysicalIndex;

            int step = up ? 1 : -1;
            for (int i = pos + step; up ? i < _registeredPhysical.Count : i >= 0; i += step)
            {
                int next = _registeredPhysical[i];
                MapGeometry.VirtualLayerType t;
                if (_byPhysicalIndex.TryGetValue(next, out t) && t == targetType)
                    return next;
            }
            return currentPhysicalIndex;
        }

        // ---- Virtual-layer index helpers (Phase 7 disambiguation) -----------

        /// <summary>
        /// Returns the virtual layer index (position in the virtual layer list)
        /// for the given physical floor, or -1 if not found.
        /// When multiple virtual layers share the same backing physical floor,
        /// returns the first (lowest) virtual index.
        /// </summary>
        public static int GetVirtualIndex(FloorManager.Floor floor)
        {
            if (floor == null) return -1;
            List<int> virtuals;
            if (_physicalToVirtual.TryGetValue(floor.m_FloorIndex, out virtuals) &&
                virtuals.Count > 0)
                return virtuals[0];
            return -1;
        }

        /// <summary>
        /// Returns the position (0-based) of <paramref name="physicalIndex"/> within
        /// the sorted registered-physical-floor list. This ordering always reflects
        /// the physical Z-position order (ascending floor index = ascending Z toward
        /// the roof). Returns -1 when the floor is not registered.
        ///
        /// Use this for stair direction comparisons: a higher position means the floor
        /// is physically higher (closer to the roof), regardless of virtual layer order.
        /// </summary>
        public static int GetRegisteredPhysicalPosition(int physicalIndex)
        {
            return _registeredPhysical.IndexOf(physicalIndex);
        }

        /// <summary>
        /// Returns all virtual layer indices that share the same backing physical floor.
        /// Used by Z-position disambiguation when two virtual layers have the same Z.
        /// </summary>
        public static List<int> VirtualIndicesForPhysical(int physicalIndex)
        {
            List<int> virtuals;
            if (_physicalToVirtual.TryGetValue(physicalIndex, out virtuals))
                return new List<int>(virtuals);
            return new List<int>();
        }

        // ---- Rebuild from MapGeometry ----------------------------------------

        /// <summary>
        /// Rebuild the registry from the current <see cref="MapGeometry"/> virtual
        /// layer list. Called by <see cref="MapGeometry.Apply"/> for editor-side
        /// preview and by <see cref="OnLoaded"/> after sidecar parse.
        /// </summary>
        public static void RebuildFromGeometry()
        {
            _byPhysicalIndex.Clear();
            _virtualToPhysical.Clear();
            _physicalToVirtual.Clear();
            _registeredPhysical.Clear();

            var state = MapGeometry.Current;
            if (state == null) return;

            for (int vi = 0; vi < state.Layers.Count; vi++)
            {
                var layer = state.Layers[vi];
                int physical = layer.BackingLayer; // 0-5, equals m_FloorIndex

                // Last virtual layer that maps to a given physical slot wins for type.
                _byPhysicalIndex[physical] = layer.Type;

                _virtualToPhysical.Add(physical);

                List<int> virtuals;
                if (!_physicalToVirtual.TryGetValue(physical, out virtuals))
                {
                    virtuals = new List<int>();
                    _physicalToVirtual[physical] = virtuals;
                }
                virtuals.Add(vi);
            }
            RebuildRegisteredPhysical();
        }

        public static void Clear()
        {
            _byPhysicalIndex.Clear();
            _registeredPhysical.Clear();
            _virtualToPhysical.Clear();
            _physicalToVirtual.Clear();
        }

        // ---- Debug ----------------------------------------------------------

        /// <summary>
        /// Dumps the current registry state as a human-readable string for the
        /// <c>e2e.floor_registry</c> ApiRunner command.
        /// </summary>
        public static string Dump()
        {
            if (_byPhysicalIndex.Count == 0)
                return "FloorTypeRegistry: empty (vanilla map / no sidecar)";

            var sb = new StringBuilder();
            sb.AppendLine("FloorTypeRegistry:");
            sb.AppendLine("  Physical floor index → VirtualLayerType:");
            foreach (int pi in _registeredPhysical)
            {
                MapGeometry.VirtualLayerType t;
                _byPhysicalIndex.TryGetValue(pi, out t);
                List<int> virtuals;
                _physicalToVirtual.TryGetValue(pi, out virtuals);
                string vis = virtuals != null ? string.Join(",", virtuals) : "-";
                sb.AppendLine($"    floorIndex={pi} type={t} virtualIndices=[{vis}]");
            }
            sb.AppendLine($"  Virtual order ({_virtualToPhysical.Count} layers):");
            for (int vi = 0; vi < _virtualToPhysical.Count; vi++)
            {
                int pi = _virtualToPhysical[vi];
                MapGeometry.VirtualLayerType t;
                _byPhysicalIndex.TryGetValue(pi, out t);
                sb.AppendLine($"    vi={vi} → physicalFloor={pi} type={t}");
            }
            return sb.ToString();
        }

        // ---- Private helpers ------------------------------------------------

        private static void RebuildRegisteredPhysical()
        {
            _registeredPhysical.Clear();
            _registeredPhysical.AddRange(_byPhysicalIndex.Keys);
            _registeredPhysical.Sort();
        }

        private static bool TryParseLine(string line, out int floorIndex,
            out MapGeometry.VirtualLayerType type)
        {
            floorIndex = -1;
            type = MapGeometry.VirtualLayerType.Ground;
            bool gotIndex = false, gotType = false;
            foreach (string part in line.Split(','))
            {
                if (part.StartsWith("floorIndex="))
                {
                    gotIndex = int.TryParse(part.Substring("floorIndex=".Length),
                        out floorIndex);
                }
                else if (part.StartsWith("type="))
                {
                    string typeName = part.Substring("type=".Length);
                    try
                    {
                        type = (MapGeometry.VirtualLayerType)Enum.Parse(
                            typeof(MapGeometry.VirtualLayerType), typeName,
                            ignoreCase: true);
                        gotType = true;
                    }
                    catch { }
                }
            }
            return gotIndex && gotType;
        }
    }
}
