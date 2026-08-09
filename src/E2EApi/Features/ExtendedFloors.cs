using System;
using System.Reflection;
using E2EApi.Editor.Patches;
using E2EApi.Events;
using E2EApi.Persistence;
using HarmonyLib;
using Rotorz.Tile;
using UnityEngine;

namespace E2EApi.Features
{
    /// <summary>
    /// True extended physical floors beyond the vanilla six
    /// <see cref="BaseLevelManager.m_BuildingLayers"/> slots.
    /// Grows runtime arrays, aliases an extended slot onto GroundFloor for
    /// editor painting, and persists <c>[extended_floors]</c> plus sparse
    /// <c>[layer_data]</c> blobs for floors ≥ 6.
    /// </summary>
    public static class ExtendedFloors
    {
        public const int NativeCount = 6;
        public const int MaxFloors = 32;

        /// <summary>
        /// User-facing creation of floors beyond 6 is disabled (WIP).
        /// Existing maps with <c>[extended_floors]</c> can still load.
        /// </summary>
        public const bool UserCreationEnabled = false;
        private const int WorkSlot = 1; // LevelLayers.GroundFloor
        private const string SectionName = "extended_floors";
        private const string LayerDataSection = "layer_data";

        private static bool _initialised;
        private static int _activePhysicalIndex = -1;
        private static int _aliasedPhysical = -1;
        private static BaseLevelManager.LayerDataCollection _savedWorkLayer;
        private static GameObject _savedMasterLayer;
        private static bool _aliasActive;

        private static readonly FieldInfo LayerZPositionsField =
            AccessTools.Field(typeof(BaseLevelManager), "m_LayerZPositions");
        private static readonly FieldInfo LayerZSetupField =
            AccessTools.Field(typeof(BaseLevelManager), "m_bLayerZOffsetSetup");
        private static readonly FieldInfo ZoneMapField =
            AccessTools.Field(typeof(LevelEditor_ZoneManager), "m_ZoneMap");

        public static bool Enabled { get; private set; }

        /// <summary>Current <c>m_BuildingLayers.Length</c>, or <see cref="NativeCount"/>.</summary>
        public static int PhysicalCount
        {
            get
            {
                var mgr = BaseLevelManager.GetInstance();
                if (mgr != null && mgr.m_BuildingLayers != null)
                {
                    return mgr.m_BuildingLayers.Length;
                }
                return NativeCount;
            }
        }

        /// <summary>Highest valid backing-layer index (<see cref="MaxFloors"/> − 1).</summary>
        public static int MaxBackingLayer
        {
            get { return MaxFloors - 1; }
        }

        /// <summary>
        /// Editor paint target when editing a physical floor ≥ 6 (or any
        /// explicitly synced index). −1 means follow the selected virtual layer.
        /// </summary>
        public static int ActivePhysicalIndex
        {
            get { return _activePhysicalIndex; }
            private set { _activePhysicalIndex = value; }
        }

        /// <summary>Physical index currently aliased onto the GroundFloor work slot, or −1.</summary>
        public static int AliasedPhysical => _aliasedPhysical;

        public static event Action EnabledChanged;

        public static void Initialise()
        {
            if (_initialised)
            {
                return;
            }
            _initialised = true;
            ModExtras.EnsurePatched();
            ModExtras.Saving += OnSaving;
            ModExtras.Loaded += OnLoaded;
            GameEvents.EditorExited += OnEditorExited;
            GameEvents.PlaytestStarted += OnPlaytestStarted;
            ExtendedFloorPatches.EnsurePatched();
            ExtendedFloorPlayPatches.EnsurePatched();
            Log.Info("ExtendedFloors initialised");
        }

        private static void OnPlaytestStarted()
        {
            // GameLevelEditorManager.Awake fires before Level.dat finish — delay.
            ApiRunner.StartRoutine(BootstrapPlayModeRoutine());
        }

        private static System.Collections.IEnumerator BootstrapPlayModeRoutine()
        {
            yield return null;
            yield return null;
            yield return new WaitForSeconds(0.15f);
            try
            {
                BootstrapPlayMode();
            }
            catch (Exception ex)
            {
                Log.Error("ExtendedFloors.BootstrapPlayMode: " + ex);
            }
        }

        /// <summary>
        /// Re-apply grown arrays, <c>[layer_data]</c> blobs, and prison floors onto
        /// the playtest / runtime <see cref="GameLevelEditorManager"/> (editor
        /// ModExtras load is skipped during playtest).
        /// </summary>
        public static void BootstrapPlayMode()
        {
            var extras = ModExtras.Current;
            if (extras == null)
            {
                return;
            }

            // Mirror OnLoaded detection without clearing editor-owned state.
            bool want = Enabled;
            int count = Math.Max(PhysicalCount, NativeCount);
            foreach (string line in extras.Section(SectionName))
            {
                if (line == "extended=1" || line.StartsWith("extended=true"))
                {
                    want = true;
                }
                else if (line.StartsWith("count="))
                {
                    int parsed;
                    if (int.TryParse(line.Substring("count=".Length), out parsed))
                    {
                        count = Math.Max(count, parsed);
                    }
                }
            }
            foreach (string line in extras.Section("map_geometry"))
            {
                if (line == "extended=1" || line.StartsWith("extended=true"))
                {
                    want = true;
                }
            }
            try
            {
                var state = MapGeometry.Current;
                if (state != null && state.Layers != null)
                {
                    foreach (var layer in state.Layers)
                    {
                        if (layer.BackingLayer >= NativeCount)
                        {
                            want = true;
                            count = Math.Max(count, layer.BackingLayer + 1);
                        }
                    }
                }
            }
            catch { }

            foreach (string line in extras.Section(LayerDataSection))
            {
                int layerIndex;
                string payload;
                if (!TryParseLayerDataLine(line, out layerIndex, out payload))
                {
                    continue;
                }
                want = true;
                count = Math.Max(count, layerIndex + 1);
            }

            if (!want)
            {
                return;
            }

            Enable("playtest bootstrap");
            DiscardAliasState();
            EnsureCapacity(count);
            ApplyLayerDataBlobs(extras);
            EnsurePrisonFloorsUpTo(PhysicalCount);

            var mgr = BaseLevelManager.GetInstance();
            if (mgr == null || mgr.m_BuildingLayers == null)
            {
                return;
            }

            for (int i = NativeCount; i < mgr.m_BuildingLayers.Length; i++)
            {
                var layer = mgr.m_BuildingLayers[i];
                if (layer == null)
                {
                    continue;
                }
                EnsureFloorHierarchy(i, ResolveLayerType(i));
                layer.m_Changed = true;
            }

            try
            {
                mgr.UpdateTiles();
            }
            catch (Exception ex)
            {
                Log.Warn("ExtendedFloors.BootstrapPlayMode UpdateTiles: " + ex.Message);
            }

            Log.Info("ExtendedFloors: playtest bootstrap complete (physical=" + PhysicalCount + ")");
        }

        /// <summary>
        /// Grow <see cref="FloorManager"/> prison floors until
        /// <c>currentMaxFloor</c> matches <paramref name="count"/>.
        /// </summary>
        public static void EnsurePrisonFloorsUpTo(int count)
        {
            if (count < NativeCount) count = NativeCount;
            if (count > MaxFloors) count = MaxFloors;

            var fm = FloorManager.GetInstance();
            if (fm == null)
            {
                return;
            }

            GrowPrisonFloors(count);
            int guard = 0;
            while (fm.currentMaxFloor < count && guard++ < MaxFloors)
            {
                var type = ResolveLayerType(fm.currentMaxFloor);
                try
                {
                    fm.AddPrisonFloor(120, 120, ToFloorType(type));
                }
                catch (Exception ex)
                {
                    Log.Warn("ExtendedFloors.EnsurePrisonFloorsUpTo: " + ex.Message);
                    break;
                }
            }
        }

        private static void OnEditorExited()
        {
            // Manager is gone — never write _savedWorkLayer into a new instance.
            DiscardAliasState();
        }

        /// <summary>
        /// Drop alias statics without writing into <c>m_BuildingLayers</c>.
        /// Use on editor exit / before a new manager replaces the old one.
        /// </summary>
        public static void DiscardAliasState()
        {
            _savedWorkLayer = null;
            _savedMasterLayer = null;
            _aliasedPhysical = -1;
            _aliasActive = false;
            _activePhysicalIndex = -1;
        }

        /// <summary>Turn on extended-floor mode and grow runtime arrays if the editor is live.</summary>
        public static void Enable(string reason)
        {
            // Block UI/user enable while the feature is WIP. Sidecar load still
            // calls Enable("sidecar load" / "playtest bootstrap").
            if (!UserCreationEnabled &&
                reason != null &&
                reason.IndexOf("sidecar", StringComparison.OrdinalIgnoreCase) < 0 &&
                reason.IndexOf("playtest", StringComparison.OrdinalIgnoreCase) < 0 &&
                reason.IndexOf("load", StringComparison.OrdinalIgnoreCase) < 0)
            {
                Log.Warn("ExtendedFloors.Enable blocked (WIP): " + reason);
                return;
            }

            if (!Enabled)
            {
                Enabled = true;
                Log.Info("ExtendedFloors enabled: " + (reason ?? "unspecified"));
                try
                {
                    var handler = EnabledChanged;
                    if (handler != null) handler();
                }
                catch (Exception ex)
                {
                    Log.Warn("ExtendedFloors.EnabledChanged: " + ex.Message);
                }
            }

            if (ModExtras.Current != null)
            {
                ModExtras.Current.RequiresMod = true;
            }

            if (E2EApi.Events.GameEvents.IsEditorActive || BaseLevelManager.GetInstance() != null)
            {
                EnsureCapacity(Math.Max(PhysicalCount, NativeCount + 1));
            }
        }

        /// <summary>
        /// Grow <c>m_BuildingLayers</c> and related arrays to at least
        /// <paramref name="count"/> slots (clamped to <see cref="MaxFloors"/>).
        /// </summary>
        public static void EnsureCapacity(int count)
        {
            if (count < NativeCount) count = NativeCount;
            if (count > MaxFloors) count = MaxFloors;

            var mgr = BaseLevelManager.GetInstance();
            if (mgr == null)
            {
                Log.Debug("ExtendedFloors.EnsureCapacity: no BaseLevelManager yet");
                return;
            }

            GrowBuildingLayers(mgr, count);
            GrowLayerZPositions(mgr, count);
            GrowVentLayers(mgr, count);
            GrowMasterLayers(count);
            GrowZoneMaps(count);
            GrowPrisonFloors(count);

            if (LayerZSetupField != null)
            {
                LayerZSetupField.SetValue(mgr, false);
            }
        }

        /// <summary>
        /// Allocate a physical floor exclusive to <paramref name="type"/>.
        /// Prefers an unused native slot 0–5; otherwise appends at ≥ 6.
        /// </summary>
        /// <returns>Physical index, or −1 on failure.</returns>
        public static int AllocateExclusiveFloor(MapGeometry.VirtualLayerType type)
        {
            Enable("AllocateExclusiveFloor(" + type + ")");

            int unused = FindUnusedNativeSlot();
            if (unused >= 0)
            {
                Log.Info("ExtendedFloors: reusing unused native slot " + unused + " for " + type);
                EnsureFloorHierarchy(unused, type);
                TryAddPrisonFloor(type);
                return unused;
            }

            int next = PhysicalCount;
            if (next >= MaxFloors)
            {
                Log.Error("ExtendedFloors: MaxFloors (" + MaxFloors + ") reached");
                return -1;
            }

            EnsureCapacity(next + 1);
            EnsureFloorHierarchy(next, type);
            TryAddPrisonFloor(type);
            Log.Info("ExtendedFloors: allocated exclusive physical floor " + next + " (" + type + ")");
            return next;
        }

        /// <summary>
        /// Physical layer the editor should paint into: ActivePhysicalIndex when set,
        /// otherwise the selected virtual layer's backing index.
        /// </summary>
        public static int GetEditorPhysicalLayer()
        {
            return GetEditorPhysicalLayer(MapGeometry.SelectedVirtualLayerIndex);
        }

        /// <summary>
        /// Physical layer for a given virtual layer index. Honours
        /// <see cref="ActivePhysicalIndex"/> when that virtual layer is selected.
        /// </summary>
        public static int GetEditorPhysicalLayer(int virtualLayerIndex)
        {
            if (_activePhysicalIndex >= 0 &&
                virtualLayerIndex == MapGeometry.SelectedVirtualLayerIndex)
            {
                return _activePhysicalIndex;
            }
            return MapGeometry.GetBackingLayer(virtualLayerIndex);
        }

        /// <summary>
        /// Point the editor at <paramref name="physical"/>. Native slots (&lt; 6) set
        /// <c>m_CurrentLayer</c> directly; extended slots alias onto GroundFloor.
        /// </summary>
        public static void SyncEditorToPhysical(int physical)
        {
            ActivePhysicalIndex = physical;
            if (physical < 0)
            {
                ClearAlias();
                return;
            }

            if (physical < NativeCount)
            {
                ClearAlias();
                var editor = EditorLevelEditorManager.GetLevelEditorInstance();
                if (editor != null)
                {
                    editor.m_CurrentLayer = (BaseLevelManager.LevelLayers)physical;
                }
                return;
            }

            AliasNativeSlot(physical);
        }

        /// <summary>
        /// Alias extended physical <paramref name="physical"/> onto the GroundFloor
        /// work slot so vanilla PlaceTile/PlaceWall/PlaceObject write into it.
        /// Pass a native index (&lt; 6) to clear the alias.
        /// </summary>
        public static void AliasNativeSlot(int physical)
        {
            var mgr = BaseLevelManager.GetInstance();
            if (mgr == null || mgr.m_BuildingLayers == null)
            {
                return;
            }

            if (physical < NativeCount)
            {
                ClearAlias();
                if (physical >= 0 && physical < mgr.m_BuildingLayers.Length)
                {
                    mgr.m_CurrentLayer = (BaseLevelManager.LevelLayers)physical;
                    ActivePhysicalIndex = physical;
                }
                return;
            }

            if (physical < 0 || physical >= mgr.m_BuildingLayers.Length)
            {
                Log.Warn("ExtendedFloors.AliasNativeSlot: invalid physical " + physical);
                return;
            }

            if (_aliasActive && _aliasedPhysical == physical)
            {
                mgr.m_CurrentLayer = BaseLevelManager.LevelLayers.GroundFloor;
                ActivePhysicalIndex = physical;
                return;
            }

            ClearAlias();

            _savedWorkLayer = mgr.m_BuildingLayers[WorkSlot];
            mgr.m_BuildingLayers[WorkSlot] = mgr.m_BuildingLayers[physical];
            _aliasedPhysical = physical;
            _aliasActive = true;

            var highlight = LevelEditorHighLightManager.GetInstance();
            if (highlight != null && highlight.m_MasterLayers != null &&
                physical < highlight.m_MasterLayers.Length &&
                WorkSlot < highlight.m_MasterLayers.Length)
            {
                _savedMasterLayer = highlight.m_MasterLayers[WorkSlot];
                if (highlight.m_MasterLayers[physical] != null)
                {
                    highlight.m_MasterLayers[WorkSlot] = highlight.m_MasterLayers[physical];
                }
            }

            mgr.m_CurrentLayer = BaseLevelManager.LevelLayers.GroundFloor;
            ActivePhysicalIndex = physical;
            Log.Debug("ExtendedFloors: aliased physical " + physical + " onto work slot " + WorkSlot);
        }

        /// <summary>
        /// Restore the GroundFloor work slot after an extended-floor alias.
        /// Alias only swaps references, so edits already live in
        /// <c>m_BuildingLayers[P]</c>; this puts the original slot-1 collection back.
        /// </summary>
        public static void RestoreAlias()
        {
            ClearAlias();
        }

        public static void ClearAlias()
        {
            if (!_aliasActive)
            {
                _aliasedPhysical = -1;
                return;
            }

            var mgr = BaseLevelManager.GetInstance();
            if (mgr != null && mgr.m_BuildingLayers != null &&
                WorkSlot < mgr.m_BuildingLayers.Length && _savedWorkLayer != null)
            {
                // Alias assigned m_BuildingLayers[WorkSlot] = m_BuildingLayers[P],
                // so P already holds the edited arrays. Restore the original work
                // collection into slot 1; P stays in its canonical index.
                mgr.m_BuildingLayers[WorkSlot] = _savedWorkLayer;
            }

            var highlight = LevelEditorHighLightManager.GetInstance();
            if (highlight != null && highlight.m_MasterLayers != null &&
                WorkSlot < highlight.m_MasterLayers.Length && _savedMasterLayer != null)
            {
                highlight.m_MasterLayers[WorkSlot] = _savedMasterLayer;
            }

            _savedWorkLayer = null;
            _savedMasterLayer = null;
            _aliasedPhysical = -1;
            _aliasActive = false;
        }

        public static FloorManager.FLOOR_TYPE ToFloorType(MapGeometry.VirtualLayerType type)
        {
            switch (type)
            {
                case MapGeometry.VirtualLayerType.Underground:
                    return FloorManager.FLOOR_TYPE.Floor_UnderGround;
                case MapGeometry.VirtualLayerType.Vent:
                    return FloorManager.FLOOR_TYPE.Floor_Vent;
                case MapGeometry.VirtualLayerType.Roof:
                    return FloorManager.FLOOR_TYPE.Floor_Roof;
                default:
                    return FloorManager.FLOOR_TYPE.Floor_Prison;
            }
        }

        // ---- persistence ----

        private static void OnSaving(ModExtras extras)
        {
            // Put arrays back in canonical indices before reading m_BuildingLayers[P].
            RestoreAlias();

            extras.ClearSection(SectionName);
            extras.ClearSection(LayerDataSection);

            if (!Enabled && PhysicalCount <= NativeCount)
            {
                return;
            }

            var section = extras.Section(SectionName);
            section.Add("extended=1");
            section.Add("count=" + PhysicalCount);
            extras.RequiresMod = true;

            // Also stamp map_geometry for readers that only scan that section.
            var geo = extras.Section("map_geometry");
            bool hasFlag = false;
            for (int i = 0; i < geo.Count; i++)
            {
                if (geo[i].StartsWith("extended="))
                {
                    geo[i] = "extended=1";
                    hasFlag = true;
                    break;
                }
            }
            if (!hasFlag)
            {
                geo.Add("extended=1");
            }

            WriteLayerDataBlobs(extras);
        }

        private static void WriteLayerDataBlobs(ModExtras extras)
        {
            var mgr = BaseLevelManager.GetInstance();
            if (mgr == null || mgr.m_BuildingLayers == null)
            {
                return;
            }

            var section = extras.Section(LayerDataSection);
            // Persist every extended physical floor (≥ 6) that has painted content.
            // Native floors stay in vanilla Level.dat.
            for (int i = NativeCount; i < mgr.m_BuildingLayers.Length; i++)
            {
                var layer = mgr.m_BuildingLayers[i];
                if (layer == null || !LayerDataBlob.HasContent(layer))
                {
                    continue;
                }

                string payload = LayerDataBlob.Encode(layer);
                if (string.IsNullOrEmpty(payload))
                {
                    continue;
                }

                section.Add("layer=" + i + "|payload=" + payload);
                extras.RequiresMod = true;
            }

            if (section.Count > 0)
            {
                Log.Info("ExtendedFloors: wrote " + section.Count + " layer_data blob(s)");
            }
        }

        private static void OnLoaded(ModExtras extras)
        {
            ClearAlias();
            ActivePhysicalIndex = -1;

            int count = NativeCount;
            bool wantExtended = false;

            var section = extras.Section(SectionName);
            foreach (string line in section)
            {
                if (line == "extended=1" || line.StartsWith("extended=true"))
                {
                    wantExtended = true;
                }
                else if (line.StartsWith("count="))
                {
                    int parsed;
                    if (int.TryParse(line.Substring("count=".Length), out parsed))
                    {
                        count = parsed;
                    }
                }
            }

            var geo = extras.Section("map_geometry");
            foreach (string line in geo)
            {
                if (line == "extended=1" || line.StartsWith("extended=true"))
                {
                    wantExtended = true;
                }
            }

            // Virtual layers with backing ≥ 6 also imply extended mode.
            try
            {
                var state = MapGeometry.Current;
                if (state != null && state.Layers != null)
                {
                    foreach (var layer in state.Layers)
                    {
                        if (layer.BackingLayer >= NativeCount)
                        {
                            wantExtended = true;
                            if (layer.BackingLayer + 1 > count)
                            {
                                count = layer.BackingLayer + 1;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Debug("ExtendedFloors.OnLoaded geometry scan: " + ex.Message);
            }

            // layer_data blobs also imply capacity + extended mode.
            var blobs = extras.Section(LayerDataSection);
            foreach (string line in blobs)
            {
                int layerIndex;
                string payload;
                if (!TryParseLayerDataLine(line, out layerIndex, out payload))
                {
                    continue;
                }
                wantExtended = true;
                if (layerIndex + 1 > count)
                {
                    count = layerIndex + 1;
                }
            }

            if (!wantExtended)
            {
                if (Enabled)
                {
                    Enabled = false;
                    try
                    {
                        var handler = EnabledChanged;
                        if (handler != null) handler();
                    }
                    catch { }
                }
                return;
            }

            Enable("sidecar load");
            EnsureCapacity(count);
            ApplyLayerDataBlobs(extras);

            if (GameEvents.IsEditorActive)
            {
                try
                {
                    SyncEditorToPhysical(
                        MapGeometry.GetBackingLayer(MapGeometry.SelectedVirtualLayerIndex));
                    MapGeometry.ApplyLayerVisibility();
                }
                catch (Exception ex)
                {
                    Log.Debug("ExtendedFloors.OnLoaded re-sync: " + ex.Message);
                }
            }
        }

        private static void ApplyLayerDataBlobs(ModExtras extras)
        {
            var mgr = BaseLevelManager.GetInstance();
            if (mgr == null || mgr.m_BuildingLayers == null)
            {
                return;
            }

            int applied = 0;
            foreach (string line in extras.Section(LayerDataSection))
            {
                int layerIndex;
                string payload;
                if (!TryParseLayerDataLine(line, out layerIndex, out payload))
                {
                    continue;
                }
                if (layerIndex < 0 || layerIndex >= mgr.m_BuildingLayers.Length)
                {
                    Log.Warn("ExtendedFloors: layer_data index out of range: " + layerIndex);
                    continue;
                }

                var layer = mgr.m_BuildingLayers[layerIndex];
                if (layer == null)
                {
                    layer = new BaseLevelManager.LayerDataCollection();
                    layer.Init();
                    mgr.m_BuildingLayers[layerIndex] = layer;
                }

                LayerDataBlob.Decode(layer, payload);

                // Rebuild TileSystem hosts so the editor can paint after reload.
                if (layerIndex >= NativeCount)
                {
                    EnsureFloorHierarchy(layerIndex, ResolveLayerType(layerIndex));
                }

                applied++;
            }

            if (applied > 0)
            {
                Log.Info("ExtendedFloors: restored " + applied + " layer_data blob(s)");
            }
        }

        private static bool TryParseLayerDataLine(string line, out int layerIndex, out string payload)
        {
            layerIndex = -1;
            payload = null;
            if (string.IsNullOrEmpty(line) || !line.StartsWith("layer="))
            {
                return false;
            }

            // layer=<i>|payload=<base64>
            string rest = line.Substring("layer=".Length);
            int sep = rest.IndexOf("|payload=", StringComparison.Ordinal);
            if (sep < 0)
            {
                return false;
            }

            if (!int.TryParse(rest.Substring(0, sep), out layerIndex))
            {
                return false;
            }

            payload = rest.Substring(sep + "|payload=".Length);
            return !string.IsNullOrEmpty(payload);
        }

        private static MapGeometry.VirtualLayerType ResolveLayerType(int physical)
        {
            try
            {
                var registered = FloorTypeRegistry.GetType(physical);
                if (registered.HasValue)
                {
                    return registered.Value;
                }

                var state = MapGeometry.Current;
                if (state != null && state.Layers != null)
                {
                    foreach (var layer in state.Layers)
                    {
                        if (layer.BackingLayer == physical)
                        {
                            return layer.Type;
                        }
                    }
                }
            }
            catch { }
            return MapGeometry.VirtualLayerType.Ground;
        }

        // ---- capacity helpers ----

        private static void GrowBuildingLayers(BaseLevelManager mgr, int count)
        {
            var old = mgr.m_BuildingLayers;
            if (old != null && old.Length >= count)
            {
                return;
            }

            int oldLen = old != null ? old.Length : 0;
            var grown = new BaseLevelManager.LayerDataCollection[count];
            for (int i = 0; i < count; i++)
            {
                if (i < oldLen && old[i] != null)
                {
                    grown[i] = old[i];
                }
                else
                {
                    grown[i] = new BaseLevelManager.LayerDataCollection();
                    grown[i].Init();
                }
            }
            mgr.m_BuildingLayers = grown;
            Log.Debug("ExtendedFloors: m_BuildingLayers grown to " + count);
        }

        private static void GrowLayerZPositions(BaseLevelManager mgr, int count)
        {
            if (LayerZPositionsField == null) return;
            var old = LayerZPositionsField.GetValue(mgr) as float[];
            if (old != null && old.Length >= count) return;

            int oldLen = old != null ? old.Length : 0;
            var grown = new float[count];
            for (int i = 0; i < oldLen; i++)
            {
                grown[i] = old[i];
            }
            float step = -3f;
            if (oldLen >= 2)
            {
                step = old[oldLen - 1] - old[oldLen - 2];
                if (Math.Abs(step) < 0.001f) step = -3f;
            }
            for (int i = oldLen; i < count; i++)
            {
                grown[i] = (i > 0 ? grown[i - 1] : 0f) + step;
            }
            LayerZPositionsField.SetValue(mgr, grown);
        }

        private static void GrowVentLayers(BaseLevelManager mgr, int count)
        {
            if (mgr.m_VentLayers != null && mgr.m_VentLayers.Length >= count)
            {
                return;
            }
            int oldLen = mgr.m_VentLayers != null ? mgr.m_VentLayers.Length : 0;
            var grown = new bool[count];
            for (int i = 0; i < oldLen; i++)
            {
                grown[i] = mgr.m_VentLayers[i];
            }
            mgr.m_VentLayers = grown;
        }

        private static void GrowMasterLayers(int count)
        {
            var highlight = LevelEditorHighLightManager.GetInstance();
            if (highlight == null) return;

            if (highlight.m_MasterLayers == null || highlight.m_MasterLayers.Length < count)
            {
                int oldLen = highlight.m_MasterLayers != null ? highlight.m_MasterLayers.Length : 0;
                var grown = new GameObject[count];
                Transform parent = null;
                for (int i = 0; i < oldLen; i++)
                {
                    grown[i] = highlight.m_MasterLayers[i];
                    if (grown[i] != null) parent = grown[i].transform.parent;
                }
                for (int i = oldLen; i < count; i++)
                {
                    var go = new GameObject("MasterLayer_" + i);
                    if (parent != null) go.transform.parent = parent;
                    go.transform.localPosition = Vector3.zero;
                    grown[i] = go;
                }
                highlight.m_MasterLayers = grown;
            }

            if (highlight.m_FloorHighLights == null || highlight.m_FloorHighLights.Length < count)
            {
                int oldLen = highlight.m_FloorHighLights != null ? highlight.m_FloorHighLights.Length : 0;
                var grown = new LevelEditorHighLightManager.HightLightFloorTypeEnum[count];
                for (int i = 0; i < oldLen; i++)
                {
                    grown[i] = highlight.m_FloorHighLights[i];
                }
                highlight.m_FloorHighLights = grown;
            }
        }

        private static void GrowZoneMaps(int count)
        {
            var zones = LevelEditor_ZoneManager.GetInstance();
            if (zones == null || ZoneMapField == null) return;

            var old = ZoneMapField.GetValue(zones) as LevelEditor_ZoneManager.ZoneMap[];
            if (old != null && old.Length >= count) return;

            int oldLen = old != null ? old.Length : 0;
            var grown = new LevelEditor_ZoneManager.ZoneMap[count];
            for (int i = 0; i < oldLen; i++)
            {
                grown[i] = old[i];
            }
            for (int i = oldLen; i < count; i++)
            {
                var map = new LevelEditor_ZoneManager.ZoneMap();
                if (map.m_Map != null)
                {
                    for (int j = 0; j < map.m_Map.Length; j++)
                    {
                        map.m_Map[j] = -1;
                    }
                }
                grown[i] = map;
            }
            ZoneMapField.SetValue(zones, grown);
        }

        private static void GrowPrisonFloors(int count)
        {
            var fm = FloorManager.GetInstance();
            if (fm == null) return;

            if (fm.m_PrisonFloors == null || fm.m_PrisonFloors.Length < count)
            {
                int oldLen = fm.m_PrisonFloors != null ? fm.m_PrisonFloors.Length : 0;
                var grown = new FloorManager.Floor[Math.Max(count, 16)];
                for (int i = 0; i < oldLen; i++)
                {
                    grown[i] = fm.m_PrisonFloors[i];
                }
                fm.m_PrisonFloors = grown;
            }
        }

        // ---- hierarchy / allocation ----

        private static int FindUnusedNativeSlot()
        {
            var used = new bool[NativeCount];
            try
            {
                var state = MapGeometry.Current;
                if (state != null && state.Layers != null)
                {
                    foreach (var layer in state.Layers)
                    {
                        if (layer.BackingLayer >= 0 && layer.BackingLayer < NativeCount)
                        {
                            used[layer.BackingLayer] = true;
                        }
                    }
                }
            }
            catch
            {
                return -1;
            }

            // Prefer GroundFloor-ish slots first for general use.
            int[] order = { 1, 3, 0, 2, 4, 5 };
            foreach (int n in order)
            {
                if (!used[n]) return n;
            }
            return -1;
        }

        private static void EnsureFloorHierarchy(int index, MapGeometry.VirtualLayerType type)
        {
            var mgr = BaseLevelManager.GetInstance();
            if (mgr == null || mgr.m_BuildingLayers == null ||
                index < 0 || index >= mgr.m_BuildingLayers.Length)
            {
                return;
            }

            var layer = mgr.m_BuildingLayers[index];
            if (layer == null)
            {
                layer = new BaseLevelManager.LayerDataCollection();
                layer.Init();
                mgr.m_BuildingLayers[index] = layer;
            }
            else if (layer.m_TileTileIDs == null || layer.m_TileTileIDs.Length == 0)
            {
                layer.Init();
            }

            if (layer.m_Tiles != null && layer.m_Walls != null && layer.m_Objects != null)
            {
                return;
            }

            // Prefer the real GroundFloor collection while aliased (slot 1 is P).
            BaseLevelManager.LayerDataCollection template = null;
            int templateIndex = 1;
            if (_aliasActive && _savedWorkLayer != null && _savedWorkLayer.m_Tiles != null)
            {
                template = _savedWorkLayer;
                templateIndex = WorkSlot;
            }
            else
            {
                if (mgr.m_BuildingLayers.Length > 1 &&
                    mgr.m_BuildingLayers[1] != null &&
                    mgr.m_BuildingLayers[1].m_Tiles != null)
                {
                    templateIndex = 1;
                }
                else
                {
                    for (int i = 0; i < Math.Min(NativeCount, mgr.m_BuildingLayers.Length); i++)
                    {
                        if (mgr.m_BuildingLayers[i] != null &&
                            mgr.m_BuildingLayers[i].m_Tiles != null)
                        {
                            templateIndex = i;
                            break;
                        }
                    }
                }
                template = mgr.m_BuildingLayers[templateIndex];
            }

            if (template == null || template.m_Tiles == null)
            {
                Log.Warn("ExtendedFloors: no template layer to clone for index " + index);
                CreateBareHierarchy(layer, index);
                return;
            }

            var root = new GameObject("ExtendedFloor_" + index + "_" + type);
            Transform parent = template.m_Tiles.transform.parent;
            if (parent != null)
            {
                root.transform.parent = parent;
            }

            float z = EstimateLayerZ(mgr, index, templateIndex);
            root.transform.position = new Vector3(
                template.m_Tiles.transform.position.x,
                template.m_Tiles.transform.position.y,
                z);

            layer.m_Tiles = CloneLayerObject(template.m_Tiles, root.transform, "Tiles", z);
            layer.m_Walls = CloneLayerObject(template.m_Walls, root.transform, "Walls", z);
            layer.m_Objects = CloneLayerObject(template.m_Objects, root.transform, "Objects", z);
            layer.m_Decorations = CloneLayerObject(
                template.m_Decorations != null ? template.m_Decorations : template.m_Objects,
                root.transform, "Decorations", z);

            if (layer.m_Tiles != null)
            {
                layer.m_Tiles_TileSystem = layer.m_Tiles.GetComponent<TileSystem>();
            }
            if (layer.m_Walls != null)
            {
                layer.m_Walls_TileSystem = layer.m_Walls.GetComponent<TileSystem>();
            }

            // Clear cloned tile visuals so the new floor starts empty; data arrays
            // are already Init()'d empty.
            StripClonedTileChildren(layer.m_Tiles);
            StripClonedTileChildren(layer.m_Walls);
            StripClonedTileChildren(layer.m_Objects);
            StripClonedTileChildren(layer.m_Decorations);

            GrowMasterLayers(index + 1);
            var highlight = LevelEditorHighLightManager.GetInstance();
            if (highlight != null && highlight.m_MasterLayers != null &&
                index < highlight.m_MasterLayers.Length &&
                highlight.m_MasterLayers[index] != null)
            {
                root.transform.parent = highlight.m_MasterLayers[index].transform;
            }
        }

        private static void CreateBareHierarchy(BaseLevelManager.LayerDataCollection layer, int index)
        {
            var root = new GameObject("ExtendedFloor_" + index);
            layer.m_Tiles = new GameObject("Tiles");
            layer.m_Tiles.transform.parent = root.transform;
            layer.m_Walls = new GameObject("Walls");
            layer.m_Walls.transform.parent = root.transform;
            layer.m_Objects = new GameObject("Objects");
            layer.m_Objects.transform.parent = root.transform;
            layer.m_Decorations = new GameObject("Decorations");
            layer.m_Decorations.transform.parent = root.transform;
        }

        private static GameObject CloneLayerObject(GameObject template, Transform parent, string name, float z)
        {
            if (template == null)
            {
                var bare = new GameObject(name);
                bare.transform.parent = parent;
                bare.transform.localPosition = Vector3.zero;
                return bare;
            }

            var clone = UnityEngine.Object.Instantiate(template);
            clone.name = name;
            clone.transform.parent = parent;
            var pos = template.transform.position;
            clone.transform.position = new Vector3(pos.x, pos.y, z);
            clone.transform.rotation = template.transform.rotation;
            clone.transform.localScale = template.transform.localScale;
            return clone;
        }

        private static void StripClonedTileChildren(GameObject root)
        {
            if (root == null) return;
            try
            {
                // Remove instantiated tile/object visuals; keep the TileSystem host.
                for (int i = root.transform.childCount - 1; i >= 0; i--)
                {
                    var child = root.transform.GetChild(i);
                    if (child == null) continue;
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
                }
            }
            catch (Exception ex)
            {
                Log.Debug("StripClonedTileChildren: " + ex.Message);
            }
        }

        private static float EstimateLayerZ(BaseLevelManager mgr, int index, int templateIndex)
        {
            float baseZ = 0f;
            try
            {
                if (mgr.m_BuildingLayers[templateIndex].m_Tiles != null)
                {
                    baseZ = mgr.m_BuildingLayers[templateIndex].m_Tiles.transform.position.z;
                }
            }
            catch { }

            float step = -3f;
            if (LayerZPositionsField != null)
            {
                var zs = LayerZPositionsField.GetValue(mgr) as float[];
                if (zs != null && zs.Length >= 2)
                {
                    step = zs[Math.Min(templateIndex + 1, zs.Length - 1)] - zs[templateIndex];
                    if (Math.Abs(step) < 0.001f) step = -3f;
                }
            }
            return baseZ + step * (index - templateIndex);
        }

        private static void TryAddPrisonFloor(MapGeometry.VirtualLayerType type)
        {
            var fm = FloorManager.GetInstance();
            if (fm == null) return;

            try
            {
                // Only append when FloorManager has fewer floors than physical layers.
                if (fm.currentMaxFloor < PhysicalCount)
                {
                    GrowPrisonFloors(PhysicalCount);
                    fm.AddPrisonFloor(120, 120, ToFloorType(type));
                }
            }
            catch (Exception ex)
            {
                Log.Warn("ExtendedFloors.AddPrisonFloor: " + ex.Message);
            }
        }
    }
}
