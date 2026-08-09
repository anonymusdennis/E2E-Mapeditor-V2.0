using System.Collections.Generic;
using System.Reflection.Emit;
using E2EApi.Features;
using HarmonyLib;

namespace E2EApi.Editor.Patches
{
    /// <summary>
    /// Harmony patches that make vanilla editor/runtime code tolerate
    /// <see cref="ExtendedFloors"/> physical layer counts beyond 6, and that
    /// navigate floors by true physical index when extended mode is on.
    ///
    /// Editor painting for physical ≥ 6 uses
    /// <see cref="ExtendedFloors.AliasNativeSlot"/> (GroundFloor work-slot alias).
    /// While aliased, WIP instruction recording is skipped so Level.dat GroundFloor
    /// is not polluted — extended content lives in <c>[layer_data]</c> blobs.
    /// </summary>
    [HarmonyPatch]
    public static class ExtendedFloorPatches
    {
        private static bool _patched;

        public static void EnsurePatched()
        {
            if (_patched) return;
            _patched = true;
            PatchRegistry.EnsurePatched(typeof(ExtendedFloorPatches));
        }

        // ---- IsEverythingSetUp: validate Length instead of hardcoded 6 ----

        [HarmonyPatch(typeof(BaseLevelManager), nameof(BaseLevelManager.IsEverythingSetUp))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        private static bool Base_IsEverythingSetUp_Prefix(BaseLevelManager __instance, ref bool __result)
        {
            if (!ExtendedFloors.Enabled) return true;
            __result = ValidateBuildingLayers(__instance, requireTileSystems: false);
            return false;
        }

        [HarmonyPatch(typeof(EditorLevelEditorManager), nameof(EditorLevelEditorManager.IsEverythingSetUp))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        private static bool Editor_IsEverythingSetUp_Prefix(EditorLevelEditorManager __instance, ref bool __result)
        {
            if (!ExtendedFloors.Enabled) return true;
            __result = ValidateBuildingLayers(__instance, requireTileSystems: false);
            return false;
        }

        [HarmonyPatch(typeof(GameLevelEditorManager), nameof(GameLevelEditorManager.IsEverythingSetUp))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        private static bool Game_IsEverythingSetUp_Prefix(GameLevelEditorManager __instance, ref bool __result)
        {
            if (!ExtendedFloors.Enabled) return true;
            __result = ValidateBuildingLayers(__instance, requireTileSystems: true);
            try { __instance.m_bIsEveryThingSetUp = __result; } catch { }
            return false;
        }

        [HarmonyPatch(typeof(LevelEditorHighLightManager), nameof(LevelEditorHighLightManager.IsSetupCorrectly))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        private static bool Highlight_IsSetupCorrectly_Prefix(
            LevelEditorHighLightManager __instance, ref bool __result)
        {
            if (!ExtendedFloors.Enabled) return true;

            if (__instance.m_MasterLayers == null ||
                __instance.m_MasterLayers.Length < ExtendedFloors.NativeCount)
            {
                __result = false;
                return false;
            }
            for (int i = __instance.m_MasterLayers.Length - 1; i >= 0; i--)
            {
                if (__instance.m_MasterLayers[i] == null)
                {
                    __result = false;
                    return false;
                }
            }
            __result = __instance.m_HighlightPrefab != null &&
                       __instance.m_LevelBase != null &&
                       __instance.m_LowerLevelPrefab != null;
            return false;
        }

        // ---- Restore alias BEFORE Level.dat validation / serialize ----

        [HarmonyPatch(typeof(LevelDetailsManager), nameof(LevelDetailsManager.StoreTheLevel))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        private static void StoreTheLevel_Prefix()
        {
            if (ExtendedFloors.Enabled)
            {
                ExtendedFloors.RestoreAlias();
            }
        }

        // ---- Keep alias in sync before editor placement ----

        [HarmonyPatch(typeof(EditorLevelEditorManager), "PlaceTile")]
        [HarmonyPrefix]
        private static void PlaceTile_Prefix()
        {
            EnsureAliasForActivePhysical();
        }

        [HarmonyPatch(typeof(EditorLevelEditorManager), "PlaceWall")]
        [HarmonyPrefix]
        private static void PlaceWall_Prefix()
        {
            EnsureAliasForActivePhysical();
        }

        [HarmonyPatch(typeof(EditorLevelEditorManager), "PlaceObject")]
        [HarmonyPrefix]
        private static void PlaceObject_Prefix()
        {
            EnsureAliasForActivePhysical();
        }

        [HarmonyPatch(typeof(EditorLevelEditorManagerV2), "PlaceObject")]
        [HarmonyPrefix]
        private static void PlaceObjectV2_Prefix()
        {
            EnsureAliasForActivePhysical();
        }

        // ---- Skip WIP instruction recording while aliased onto extended floors ----

        [HarmonyPatch(typeof(BuildingInstructionManager), nameof(BuildingInstructionManager.AddBlockOnce))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        private static bool AddBlockOnce_Prefix(
            BuildingInstructionManager __instance,
            int iID, sbyte X, sbyte Y, int seed, bool bDontRun, bool bCheckLimit,
            ref bool __result)
        {
            if (!IsPaintingExtendedAlias()) return true;

            EnsureAliasForActivePhysical();
            var block = BuildingBlockManager.GetBlock(iID);
            if (block == null)
            {
                __result = false;
                return false;
            }

            var mgr = __instance.m_LevelManager ?? BaseLevelManager.GetInstance();
            switch (block.BlockType)
            {
                case BaseBuildingBlock.BuildingBlockType.Wall:
                {
                    var obj = new BuildingInstructionManager.InstructionOnceWallElement();
                    obj.m_BuildingBlockID = iID;
                    obj.m_iRandomSeed = seed;
                    obj.m_XPosition = X;
                    obj.m_YPosition = Y;
                    if (mgr != null && !bDontRun)
                    {
                        mgr.AddSingleWall(ref obj);
                    }
                    __result = true;
                    return false;
                }
                case BaseBuildingBlock.BuildingBlockType.Object:
                    if (bCheckLimit)
                    {
                        var bbm = BuildingBlockManager.GetInstance();
                        var objBlock = bbm != null
                            ? bbm.GetBuildingBlock(iID) as BuildingBlock_Object
                            : null;
                        if (objBlock != null && objBlock.m_LimitationGroup != -1 &&
                            objBlock.m_LimitationCount > 0)
                        {
                            var group = bbm.GetTheLimitationGroup(objBlock.m_LimitationGroup);
                            if (group != null && group.m_Max != 0 &&
                                group.m_CurrentTotal + objBlock.m_LimitationCount > group.m_Max)
                            {
                                __result = false;
                                return false;
                            }
                        }
                    }
                    goto case BaseBuildingBlock.BuildingBlockType.Tile;
                case BaseBuildingBlock.BuildingBlockType.Tile:
                case BaseBuildingBlock.BuildingBlockType.Decoration:
                {
                    var obj = new BuildingInstructionManager.InstructionOnceElement();
                    obj.m_BuildingBlockID = iID;
                    obj.m_iRandomSeed = seed;
                    obj.m_XPosition = X;
                    obj.m_YPosition = Y;
                    if (mgr != null && !bDontRun)
                    {
                        mgr.AddSingle(ref obj);
                    }
                    __result = true;
                    return false;
                }
                default:
                    // Rooms/complexes: let vanilla run (rare on extended floors).
                    return true;
            }
        }

        [HarmonyPatch(typeof(BuildingInstructionManager), nameof(BuildingInstructionManager.AddBlockArea))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        private static bool AddBlockArea_Prefix(
            BuildingInstructionManager __instance,
            int iID, sbyte X, sbyte Y, sbyte Width, sbyte Height, int seed, bool bDontRun,
            ref bool __result)
        {
            if (!IsPaintingExtendedAlias()) return true;

            EnsureAliasForActivePhysical();
            var block = iID == -1 ? null : BuildingBlockManager.GetBlock(iID);
            if (iID != -1 && block == null)
            {
                __result = false;
                return false;
            }

            var mgr = __instance.m_LevelManager ?? BaseLevelManager.GetInstance();
            if (block != null && block.BlockType == BaseBuildingBlock.BuildingBlockType.Wall)
            {
                var obj = new BuildingInstructionManager.InstructionAreaWallElement();
                obj.m_BuildingBlockID = iID;
                obj.m_iRandomSeed = seed;
                obj.m_XPosition = X;
                obj.m_YPosition = Y;
                obj.m_XCount = Width;
                obj.m_YCount = Height;
                if (mgr != null && !bDontRun)
                {
                    mgr.AddAreaWall(ref obj);
                }
                __result = true;
                return false;
            }

            var area = new BuildingInstructionManager.InstructionAreaElement();
            area.m_BuildingBlockID = iID;
            area.m_iRandomSeed = seed;
            area.m_XPosition = X;
            area.m_YPosition = Y;
            area.m_XCount = Width;
            area.m_YCount = Height;
            if (mgr != null && !bDontRun)
            {
                mgr.AddArea(ref area);
            }
            __result = true;
            return false;
        }

        // ---- Layer validity / defaults: treat ≥6 as GroundFloor ----

        [HarmonyPatch(typeof(BaseBuildingBlock), nameof(BaseBuildingBlock.IsValidForLayer))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        private static bool IsValidForLayer_Prefix(
            BaseBuildingBlock __instance,
            BaseLevelManager.LevelLayers layer,
            ref bool __result)
        {
            if (!ExtendedFloors.Enabled) return true;
            int layerIndex = (int)layer;
            if (layerIndex < ExtendedFloors.NativeCount) return true;

            // Extended physical cast into LevelLayers — validate as GroundFloor.
            int num = 3 << ((int)BaseLevelManager.LevelLayers.GroundFloor * 2);
            __result = (__instance.m_ValidLayers & num) != 0;
            return false;
        }

        [HarmonyPatch(typeof(BuildingBlockManager), nameof(BuildingBlockManager.GetDefaultLayerBlock))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        private static bool GetDefaultLayerBlock_Prefix(
            BaseLevelManager.LevelLayers layer,
            BaseLevelManager.LayersEnvironment environment,
            ref int __result)
        {
            if (!ExtendedFloors.Enabled) return true;
            if ((int)layer < ExtendedFloors.NativeCount) return true;
            __result = BuildingBlockManager.GetDefaultLayerBlock(
                BaseLevelManager.LevelLayers.GroundFloor, environment);
            return false;
        }

        // ---- Delete: mutate aliased P, record nothing + disable undo ----

        [HarmonyPatch(typeof(BuildingInstructionManager), nameof(BuildingInstructionManager.DeleteArea))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        private static bool DeleteArea_Prefix(
            BuildingInstructionManager __instance,
            int X, int Y, int iWidth, int iHeight,
            ref bool __result)
        {
            if (!IsPaintingExtendedAlias()) return true;
            // Shift = custom-tiles-only — let ModifierDelete own that path.
            if (ModifierDelete.ShiftHeld) return true;

            EnsureAliasForActivePhysical();
            var mgr = __instance.m_LevelManager ?? BaseLevelManager.GetInstance();
            if (mgr == null || mgr.m_BuildingLayers == null)
            {
                __result = false;
                return false;
            }
            if (X < 0 || Y < 0 || X + iWidth > 120 || Y + iHeight > 120)
            {
                __result = false;
                return false;
            }

            int layerIndex = (int)mgr.m_CurrentLayer;
            if (layerIndex < 0 || layerIndex >= mgr.m_BuildingLayers.Length)
            {
                __result = false;
                return false;
            }

            var layer = mgr.m_BuildingLayers[layerIndex];
            if (layer == null || layer.m_TileProperties == null)
            {
                __result = false;
                return false;
            }

            bool any = false;
            for (int y = Y; y < Y + iHeight; y++)
            {
                for (int x = X; x < X + iWidth; x++)
                {
                    int idx = y * 120 + x;
                    var prop = layer.m_TileProperties[idx];
                    if ((prop & BaseLevelManager.TileProperty.TileMask) != 0)
                    {
                        any |= ApplyDeleteNoRecord(mgr, x, y,
                            BuildingInstructionManager.InstructionDeleteElement.DeleteType.Tile);
                    }
                    if ((prop & BaseLevelManager.TileProperty.WallMask) != 0)
                    {
                        any |= ApplyDeleteNoRecord(mgr, x, y,
                            BuildingInstructionManager.InstructionDeleteElement.DeleteType.Wall);
                    }
                    if ((prop & (BaseLevelManager.TileProperty.ObjectMask |
                                 BaseLevelManager.TileProperty.DecorationMask)) != 0)
                    {
                        any |= ApplyDeleteNoRecord(mgr, x, y,
                            BuildingInstructionManager.InstructionDeleteElement.DeleteType.Object);
                    }
                }
            }

            if (any)
            {
                try { mgr.UpdateTiles(); }
                catch { }
            }
            __result = any;
            return false;
        }

        [HarmonyPatch(typeof(BuildingInstructionManager), nameof(BuildingInstructionManager.CanUndo))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        private static bool CanUndo_Prefix(ref bool __result)
        {
            if (!IsPaintingExtendedAlias()) return true;
            __result = false;
            return false;
        }

        [HarmonyPatch(typeof(BuildingInstructionManager), nameof(BuildingInstructionManager.CanRedo))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        private static bool CanRedo_Prefix(ref bool __result)
        {
            if (!IsPaintingExtendedAlias()) return true;
            __result = false;
            return false;
        }

        [HarmonyPatch(typeof(BuildingInstructionManager), nameof(BuildingInstructionManager.Undo))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        private static bool Undo_Prefix()
        {
            return !IsPaintingExtendedAlias();
        }

        [HarmonyPatch(typeof(BuildingInstructionManager), nameof(BuildingInstructionManager.Redo))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        private static bool Redo_Prefix()
        {
            return !IsPaintingExtendedAlias();
        }

        // ---- Zones ----

        [HarmonyPatch(typeof(LevelEditor_ZoneManager), nameof(LevelEditor_ZoneManager.GetZoneMap))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        private static bool GetZoneMap_Prefix(
            LevelEditor_ZoneManager __instance,
            BaseLevelManager.LevelLayers layer,
            ref LevelEditor_ZoneManager.ZoneMap __result)
        {
            if (!ExtendedFloors.Enabled) return true;

            ExtendedFloors.EnsureCapacity(ExtendedFloors.PhysicalCount);
            var maps = AccessTools.Field(typeof(LevelEditor_ZoneManager), "m_ZoneMap")
                .GetValue(__instance) as LevelEditor_ZoneManager.ZoneMap[];
            int i = (int)layer;
            if (maps == null || i < 0 || i >= maps.Length)
            {
                __result = null;
                return false;
            }
            __result = maps[i];
            return false;
        }

        [HarmonyPatch(typeof(LevelEditor_ZoneManager), nameof(LevelEditor_ZoneManager.GetFreeZone))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        private static void GetFreeZone_Prefix(ref BaseLevelManager.LevelLayers layer)
        {
            if (!ExtendedFloors.Enabled) return;
            // While painting an extended floor, stamp zones onto the physical index.
            if (ExtendedFloors.AliasedPhysical >= ExtendedFloors.NativeCount &&
                layer == BaseLevelManager.LevelLayers.GroundFloor)
            {
                layer = (BaseLevelManager.LevelLayers)ExtendedFloors.AliasedPhysical;
            }
        }

        [HarmonyPatch(typeof(LevelEditor_ZoneManager), nameof(LevelEditor_ZoneManager.GetFreeZone))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> GetFreeZone_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            // Vanilla rejects (int)layer >= 6 — widen to zone-map length.
            var codes = new List<CodeInstruction>(instructions);
            var getter = AccessTools.Method(typeof(ExtendedFloorPatches),
                nameof(GetMaxZoneLayerIndex));
            for (int i = 0; i < codes.Count - 1; i++)
            {
                if (codes[i].opcode != OpCodes.Ldc_I4_6)
                {
                    continue;
                }
                var next = codes[i + 1].opcode;
                if (next != OpCodes.Bge && next != OpCodes.Bge_S &&
                    next != OpCodes.Bgt && next != OpCodes.Bgt_S)
                {
                    continue;
                }
                codes[i] = new CodeInstruction(OpCodes.Ldarg_0);
                codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, getter));
                break;
            }
            return codes;
        }

        public static int GetMaxZoneLayerIndex(LevelEditor_ZoneManager zones)
        {
            if (!ExtendedFloors.Enabled)
            {
                return ExtendedFloors.NativeCount;
            }
            ExtendedFloors.EnsureCapacity(ExtendedFloors.PhysicalCount);
            var maps = AccessTools.Field(typeof(LevelEditor_ZoneManager), "m_ZoneMap")
                .GetValue(zones) as LevelEditor_ZoneManager.ZoneMap[];
            return maps != null ? maps.Length : ExtendedFloors.PhysicalCount;
        }

        // ---- UpdateTiles: widen i < 6 → m_BuildingLayers.Length ----

        [HarmonyPatch(typeof(EditorLevelEditorManager), nameof(EditorLevelEditorManager.UpdateTiles))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UpdateTiles_Editor_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            return RewriteUpdateTilesLoopBound(instructions);
        }

        [HarmonyPatch(typeof(EditorLevelEditorManagerV2), nameof(EditorLevelEditorManagerV2.UpdateTiles))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UpdateTiles_EditorV2_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            return RewriteUpdateTilesLoopBound(instructions);
        }

        [HarmonyPatch(typeof(GameLevelEditorManager), nameof(GameLevelEditorManager.UpdateTiles))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UpdateTiles_Game_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            return RewriteUpdateTilesLoopBound(instructions);
        }

        /// <summary>
        /// Used by UpdateTiles transpilers — must stay public for Harmony call.
        /// </summary>
        public static int GetUpdateTilesLoopBound(BaseLevelManager mgr)
        {
            if (mgr != null && mgr.m_BuildingLayers != null &&
                (ExtendedFloors.Enabled || mgr.m_BuildingLayers.Length > ExtendedFloors.NativeCount))
            {
                return mgr.m_BuildingLayers.Length;
            }
            return ExtendedFloors.NativeCount;
        }

        // ---- Floor navigation: defer to registry when present ----

        [HarmonyPatch(typeof(FloorManager), nameof(FloorManager.UpAFloor))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        private static bool UpAFloor_Prefix(FloorManager __instance,
            FloorManager.Floor currentFloor, ref FloorManager.Floor __result)
        {
            if (!ExtendedFloors.Enabled || currentFloor == null) return true;
            // Let FloorNavigationPatch own registry-ordered nav (vents/gaps).
            if (FloorTypeRegistry.HasEntries) return true;

            int max = __instance.currentMaxFloor > 0
                ? __instance.currentMaxFloor
                : ExtendedFloors.PhysicalCount;
            int next = currentFloor.m_FloorIndex + 1;
            if (next < max)
            {
                var floor = __instance.FindFloorbyIndex(next);
                __result = floor ?? currentFloor;
            }
            else
            {
                __result = currentFloor;
            }
            return false;
        }

        [HarmonyPatch(typeof(FloorManager), nameof(FloorManager.DownAFloor))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        private static bool DownAFloor_Prefix(FloorManager __instance,
            FloorManager.Floor currentFloor, ref FloorManager.Floor __result)
        {
            if (!ExtendedFloors.Enabled || currentFloor == null) return true;
            if (FloorTypeRegistry.HasEntries) return true;

            int prev = currentFloor.m_FloorIndex - 1;
            if (prev >= 0)
            {
                var floor = __instance.FindFloorbyIndex(prev);
                __result = floor ?? currentFloor;
            }
            else
            {
                __result = currentFloor;
            }
            return false;
        }

        // ---- helpers ----

        private static IEnumerable<CodeInstruction> RewriteUpdateTilesLoopBound(
            IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var getter = AccessTools.Method(typeof(ExtendedFloorPatches),
                nameof(GetUpdateTilesLoopBound));
            for (int i = 0; i < codes.Count - 1; i++)
            {
                if (codes[i].opcode != OpCodes.Ldc_I4_6)
                {
                    continue;
                }
                var next = codes[i + 1].opcode;
                if (next != OpCodes.Blt && next != OpCodes.Blt_S &&
                    next != OpCodes.Ble && next != OpCodes.Ble_S &&
                    next != OpCodes.Clt)
                {
                    continue;
                }
                codes[i] = new CodeInstruction(OpCodes.Ldarg_0);
                codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, getter));
                break;
            }
            return codes;
        }

        private static bool ApplyDeleteNoRecord(
            BaseLevelManager mgr, int x, int y,
            BuildingInstructionManager.InstructionDeleteElement.DeleteType type)
        {
            var el = new BuildingInstructionManager.InstructionDeleteElement();
            el.m_XPosition = (sbyte)x;
            el.m_YPosition = (sbyte)y;
            el.m_DeleteType = type;
            try
            {
                mgr.AddDelete(ref el, bStorePrevious: false);
                return true;
            }
            catch (System.Exception ex)
            {
                Log.Debug("ExtendedFloor delete: " + ex.Message);
                return false;
            }
        }

        private static bool IsPaintingExtendedAlias()
        {
            return ExtendedFloors.Enabled &&
                   ExtendedFloors.AliasedPhysical >= ExtendedFloors.NativeCount;
        }

        private static void EnsureAliasForActivePhysical()
        {
            if (!ExtendedFloors.Enabled) return;
            int phys = ExtendedFloors.GetEditorPhysicalLayer();
            if (phys >= ExtendedFloors.NativeCount)
            {
                // Always reaffirm — SyncNativeEditorLayer (or cutaway) may have
                // overwritten m_CurrentLayer even when AliasedPhysical already matches.
                ExtendedFloors.AliasNativeSlot(phys);
            }
        }

        private static bool ValidateBuildingLayers(BaseLevelManager mgr, bool requireTileSystems)
        {
            if (mgr == null) return false;
            if (mgr.m_BuildingBlockManager == null) return false;
            if (mgr.m_BuildingGroupManager == null) return false;
            if (mgr.m_ZoneManager == null) return false;
            if (mgr.m_BuildingLayers == null || mgr.m_BuildingLayers.Length == 0) return false;

            bool result = true;
            int len = mgr.m_BuildingLayers.Length;
            for (int i = 0; i < len; i++)
            {
                var layer = mgr.m_BuildingLayers[i];
                if (layer == null)
                {
                    result = false;
                    continue;
                }
                if (layer.m_Objects == null) result = false;
                if (layer.m_Tiles == null) result = false;
                if (layer.m_Walls == null) result = false;
                if (layer.m_Decorations == null)
                {
                    if (layer.m_Objects == null)
                    {
                        result = false;
                    }
                    else
                    {
                        layer.m_Decorations = layer.m_Objects;
                    }
                }
                if (requireTileSystems)
                {
                    if (layer.m_Tiles_TileSystem == null) result = false;
                    if (layer.m_Walls_TileSystem == null) result = false;
                }
            }
            return result;
        }
    }
}
