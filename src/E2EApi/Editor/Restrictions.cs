using System.Collections.Generic;
using System.IO;
using HarmonyLib;

namespace E2EApi.Editor
{
    /// <summary>
    /// Master "ignore all mapping restrictions" switch. When enabled:
    /// - every block can be placed on every layer,
    /// - level validation always passes (so unfinished maps can be saved as
    ///   Finished, uploaded, and shared),
    /// - work-in-progress maps become playable from the menu,
    /// - the spawnlist unlocks (dev blocks, layers, completion) are forced on.
    ///
    /// Maps edited with the switch on remember it (sidecar flag), so their
    /// unchecked placements survive editor reloads and play-mode replay even
    /// when the vanilla support checks would drop them.
    /// </summary>
    public static class Restrictions
    {
        private static bool _ignoreAll;
        private static bool _initialised;

        private const string SectionName = "restrictions";
        private const string UncheckedLine = "uncheckedPlacements=1";

        /// <summary>
        /// True when the current map contains placements that were made with
        /// the vanilla placement checks bypassed (loaded from the sidecar, or
        /// sticky once a save happens while <see cref="IgnoreAll"/> is on).
        /// Replays then run with checks bypassed too, so those placements
        /// survive editor reloads and play mode (issues #18/#3).
        /// </summary>
        internal static bool UncheckedPlacements;

        public static bool IgnoreAll
        {
            get => _ignoreAll;
            set
            {
                _ignoreAll = value;
                if (value)
                {
                    PatchRegistry.EnsurePatched(typeof(RestrictionPatches));
                    Blocks.ShowEditorOnly = true;
                    Blocks.IgnoreLayerRestrictions = true;
                    Blocks.IgnoreCompletionState = true;
                    Log.Info("ALL mapping restrictions disabled (place anything anywhere, save/play unfinished maps)");
                }
            }
        }

        /// <summary>
        /// Install the always-on placement/delete parity patches and the
        /// sidecar persistence of the unchecked-placements flag. Called once
        /// at plugin start-up, independent of <see cref="IgnoreAll"/>.
        /// </summary>
        public static void Initialise()
        {
            if (_initialised)
            {
                return;
            }
            _initialised = true;
            PatchRegistry.EnsurePatched(typeof(ParityPatches));
            Persistence.ModExtras.Saving += OnSaving;
            Persistence.ModExtras.Loaded += OnLoaded;
        }

        private static void OnSaving(Persistence.ModExtras extras)
        {
            if (_ignoreAll)
            {
                // this editor session could place blocks without checks
                UncheckedPlacements = true;
            }
            extras.ClearSection(SectionName);
            if (UncheckedPlacements)
            {
                extras.Section(SectionName).Add(UncheckedLine);
            }
        }

        private static void OnLoaded(Persistence.ModExtras extras)
        {
            UncheckedPlacements = false;
            foreach (var line in extras.Section(SectionName))
            {
                if (line.Trim() == UncheckedLine)
                {
                    UncheckedPlacements = true;
                    break;
                }
            }
        }

        private static bool BypassChecksWanted => _ignoreAll || UncheckedPlacements;

        /// <summary>
        /// Patches that are always installed. They keep maps containing
        /// unchecked placements intact and fix vanilla delete gaps; every
        /// patch is a no-op unless the mod placed something vanilla wouldn't.
        /// </summary>
        [HarmonyPatch]
        private static class ParityPatches
        {
            // ---- replay parity (issues #18 and #3) -------------------------
            //
            // The editor honours m_IgnoreChecks when instructions run, but the
            // flag is false while a saved map replays its instruction log, so
            // unsupported placements (e.g. floor tiles with nothing beneath
            // them) silently vanish on editor reload. Force the flag during
            // replays of flagged maps and restore it afterwards.

            [HarmonyPatch(typeof(BuildingInstructionManager),
                nameof(BuildingInstructionManager.RunAllInstructions))]
            [HarmonyPrefix]
            private static void RunAllPrefix(BuildingInstructionManager __instance)
            {
                if (BypassChecksWanted)
                {
                    __instance.m_IgnoreChecks = true;
                }
            }

            [HarmonyPatch(typeof(BuildingInstructionManager),
                nameof(BuildingInstructionManager.RunAllInstructions))]
            [HarmonyPostfix]
            private static void RunAllPostfix(BuildingInstructionManager __instance)
            {
                if (BypassChecksWanted)
                {
                    __instance.m_IgnoreChecks = _ignoreAll;
                }
            }

            [HarmonyPatch(typeof(BuildingInstructionManager),
                nameof(BuildingInstructionManager.TimeSlicedRunAllInstructions))]
            [HarmonyPrefix]
            private static void TimeSlicedPrefix(BuildingInstructionManager __instance)
            {
                if (BypassChecksWanted)
                {
                    __instance.m_IgnoreChecks = true;
                }
            }

            [HarmonyPatch(typeof(BuildingInstructionManager),
                nameof(BuildingInstructionManager.TimeSlicedRunAllInstructions))]
            [HarmonyPostfix]
            private static void TimeSlicedPostfix(BuildingInstructionManager __instance)
            {
                if (BypassChecksWanted)
                {
                    __instance.m_IgnoreChecks = _ignoreAll;
                }
            }

            // ---- play-mode parity (issue #18: "stairs just broke") ---------
            //
            // GameLevelEditorManager.AddArea/AddAreaWall run the cross-floor
            // support checks unconditionally (they never read m_IgnoreChecks),
            // so tiles and walls placed with checks bypassed in the editor are
            // dropped when the map is played — stairs then sit on missing
            // floors, walls vanish, etc. After the vanilla pass, re-place the
            // cells it skipped, applying only the residual rules the editor
            // itself enforces while m_IgnoreChecks is on (rooms, occupancy,
            // limitation caps, no-blocking) so play matches the editor.

            [HarmonyPatch(typeof(GameLevelEditorManager), nameof(GameLevelEditorManager.AddArea))]
            [HarmonyPostfix]
            private static void ForceUncheckedTiles(GameLevelEditorManager __instance,
                ref BuildingInstructionManager.InstructionAreaElement obj)
            {
                if (!BypassChecksWanted || __instance == null)
                {
                    return;
                }
                var tile = BuildingBlockManager.GetBlock(obj.m_BuildingBlockID) as BuildingBlock_Tile;
                if (tile == null || tile.BlockType != BaseBuildingBlock.BuildingBlockType.Tile)
                {
                    return;
                }
                var layers = __instance.m_BuildingLayers;
                int layer = (int)__instance.m_CurrentLayer;
                if (layers == null || layer < 0 || layer >= layers.Length)
                {
                    return;
                }
                var data = layers[layer];
                if (data == null || data.m_TileTileIDs == null || data.m_TileProperties == null)
                {
                    return;
                }
                BuildingBlockManager.LimitationGroup cap = null;
                var blockManager = __instance.m_BuildingBlockManager;
                if (tile.m_LimitationGroup != -1 && blockManager != null &&
                    blockManager.m_LimitationGroups[tile.m_LimitationGroup].m_Max != 0)
                {
                    cap = blockManager.m_LimitationGroups[tile.m_LimitationGroup];
                }
                bool blockingTile = tile.m_BlockingTile;
                bool noBlockingBelow = tile.m_NoBlockingBelow;
                bool vent = __instance.m_VentLayers[layer];
                int neighbours = vent ? 9 : 1;
                var cells = new int[9];
                var random = new System.Random(obj.m_iRandomSeed);
                int x0 = obj.m_XPosition;
                int y0 = obj.m_YPosition + __instance.m_iCurrentLayerOffset_Y;
                for (int i = y0; i < y0 + obj.m_YCount; i++)
                {
                    for (int j = x0; j < x0 + obj.m_XCount; j++)
                    {
                        if (i < 0 || i > 119 || j < 0 || j > 119)
                        {
                            continue;
                        }
                        int index = i * 120 + j;
                        if ((int)(data.m_TileTileIDs[index] & BaseLevelManager.TileIDData.IDMask)
                            == obj.m_BuildingBlockID)
                        {
                            continue; // the vanilla pass already placed it
                        }
                        // neighbour ring: vanilla checks the surrounding cells
                        // too on vent layers. The "i < 117" bound is NOT a
                        // typo — it replicates the exact bound the game uses
                        // (EditorLevelEditorManager.AddArea), quirks and all,
                        // because this pass must accept precisely the set of
                        // placements the editor accepted.
                        cells[0] = index;
                        if (vent)
                        {
                            cells[1] = i > 0 ? index - 120 : -1;
                            cells[2] = i > 0 && j > 0 ? index - 121 : -1;
                            cells[3] = i > 0 && j < 119 ? index - 119 : -1;
                            cells[4] = j > 0 ? index - 1 : -1;
                            cells[5] = j < 119 ? index + 1 : -1;
                            cells[6] = i < 117 ? index + 120 : -1;
                            cells[7] = i < 117 && j > 0 ? index + 119 : -1;
                            cells[8] = i < 117 && j < 119 ? index + 121 : -1;
                        }
                        bool ok = true;
                        for (int k = 0; k < neighbours && ok; k++)
                        {
                            int cell = cells[k];
                            if (cell == -1)
                            {
                                continue;
                            }
                            var props = data.m_TileProperties[cell];
                            if ((props & BaseLevelManager.TileProperty.WallMask) != 0)
                            {
                                var wall = BuildingBlockManager.GetBlock(
                                    data.m_WallTileIDs[cell]) as BuildingBlock_Wall;
                                if (wall != null && (wall.m_AutomaticBlock ||
                                    (wall.m_FloorTileID == -1 &&
                                     (tile.m_ValidLayers & wall.m_ValidLayers) != 0)))
                                {
                                    props &= BaseLevelManager.TileProperty.InverseWallMask;
                                }
                            }
                            if (k == 0 && __instance.m_CurrentComplexAllocation == 0 &&
                                data.m_RoomPropertiesMasks[cell] != BaseLevelManager.RoomProperty.EMPTY)
                            {
                                ok = false;
                            }
                            else if (cap != null && cap.m_Max <= cap.m_CurrentTotal)
                            {
                                ok = false;
                            }
                            else if ((props & BaseLevelManager.TileProperty.WallMask) != 0)
                            {
                                ok = false;
                            }
                            else if (blockingTile && k == 0 &&
                                (props & BaseLevelManager.TileProperty.NoBlockingMask)
                                    == BaseLevelManager.TileProperty.NoBlockingMask)
                            {
                                ok = false;
                            }
                            if (ok && noBlockingBelow && layer > 1 && k == 0)
                            {
                                var below = layers[layer - 1].m_TileProperties[
                                    cell + __instance.m_DifferentLayerOffsets[layer][layer - 1]];
                                if ((below & (BaseLevelManager.TileProperty.BlockingMask |
                                    BaseLevelManager.TileProperty.TileBlockingMask)) != 0)
                                {
                                    ok = false;
                                }
                            }
                        }
                        if (ok)
                        {
                            __instance.PlaceTile(j, i, tile, random.Next());
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(GameLevelEditorManager), nameof(GameLevelEditorManager.AddAreaWall))]
            [HarmonyPostfix]
            private static void ForceUncheckedWalls(GameLevelEditorManager __instance,
                ref BuildingInstructionManager.InstructionAreaWallElement obj)
            {
                if (!BypassChecksWanted || __instance == null)
                {
                    return;
                }
                var wall = BuildingBlockManager.GetBlock(obj.m_BuildingBlockID) as BuildingBlock_Wall;
                if (wall == null || wall.BlockType != BaseBuildingBlock.BuildingBlockType.Wall)
                {
                    return;
                }
                var layers = __instance.m_BuildingLayers;
                int layer = (int)__instance.m_CurrentLayer;
                if (layers == null || layer < 0 || layer >= layers.Length)
                {
                    return;
                }
                var data = layers[layer];
                if (data == null || data.m_WallTileIDs == null || data.m_TileProperties == null)
                {
                    return;
                }
                BuildingBlock_Tile floorTile = null;
                if (wall.m_FloorTileID != -1)
                {
                    floorTile = BuildingBlockManager.GetBlock(wall.m_FloorTileID) as BuildingBlock_Tile;
                }
                BuildingBlockManager.LimitationGroup cap = null;
                var blockManager = __instance.m_BuildingBlockManager;
                if (wall.m_LimitationGroup != -1 && blockManager != null &&
                    blockManager.m_LimitationGroups[wall.m_LimitationGroup].m_Max != 0)
                {
                    cap = blockManager.m_LimitationGroups[wall.m_LimitationGroup];
                }
                var random = new System.Random(obj.m_iRandomSeed);
                int x0 = obj.m_XPosition;
                int y0 = obj.m_YPosition + __instance.m_iCurrentLayerOffset_Y;
                for (int i = y0; i < y0 + obj.m_YCount; i++)
                {
                    for (int j = x0; j < x0 + obj.m_XCount; j++)
                    {
                        if (i < 0 || i > 119 || j < 0 || j > 119)
                        {
                            continue;
                        }
                        int index = i * 120 + j;
                        if ((int)(data.m_WallTileIDs[index] & BaseLevelManager.TileIDData.IDMask)
                            == obj.m_BuildingBlockID)
                        {
                            continue; // the vanilla pass already placed it
                        }
                        var props = data.m_TileProperties[index];
                        // residual rules the editor enforces even with checks
                        // bypassed: free of no-blocking, rooms, occupants, cap
                        bool free = (props & BaseLevelManager.TileProperty.NoBlockingMask) == 0;
                        bool occupied = (props & BaseLevelManager.TileProperty.WallAndObjects) != 0;
                        if (__instance.m_CurrentComplexAllocation != 0)
                        {
                            occupied = false;
                        }
                        if (free && !occupied &&
                            (__instance.m_CurrentComplexAllocation != 0 ||
                             data.m_RoomPropertiesMasks[index] == BaseLevelManager.RoomProperty.EMPTY) &&
                            (cap == null || cap.m_Max > cap.m_CurrentTotal))
                        {
                            __instance.PlaceWall(j, i, wall, random.Next());
                            if (floorTile != null)
                            {
                                __instance.PlaceTile(j, i, floorTile, random.Next(),
                                    bMarkAsChanged: false);
                            }
                        }
                    }
                }
            }

            // ---- vent walls outside vent floors (issue #21) ----------------
            //
            // BuildingInstructionManager.DeleteAtXY refuses to delete walls
            // whose block is m_AutomaticBlock (the game auto-manages them on
            // vent layers). Placed on a normal layer via the unlocked palette
            // they become permanently undeletable. When the eraser covers such
            // a wall on a non-vent layer, add the delete the vanilla code
            // skipped. Vanilla itself never has automatic walls there, so this
            // cannot alter unmodded behaviour.

            [HarmonyPatch(typeof(BuildingInstructionManager),
                nameof(BuildingInstructionManager.DeleteAtXY))]
            [HarmonyPostfix]
            private static void DeleteAutomaticWalls(BuildingInstructionManager __instance,
                BaseLevelManager.TileProperty prop, ref bool __result)
            {
                if (__instance == null || __instance.m_Delete_ComplexRoomNumber != 0)
                {
                    return; // room deletion handles its own bookkeeping
                }
                if ((__instance.m_Delete_Allowed & BuildingInstructionManager.AllowedToDelete.Walls)
                    != BuildingInstructionManager.AllowedToDelete.Walls)
                {
                    return; // walls aren't deletable in this pass
                }
                if ((prop & BaseLevelManager.TileProperty.DeleteWallMask) != 0 ||
                    (prop & BaseLevelManager.TileProperty.WallMask) == 0)
                {
                    return; // already handled, or no wall here
                }
                var level = __instance.m_LevelManager;
                var data = __instance.m_Delete_LayerData;
                var complex = __instance.m_Delete_ComplexElement;
                if (level == null || data == null || complex == null)
                {
                    return;
                }
                int layer = (int)level.m_CurrentLayer;
                if (layer < 0 || layer >= level.m_VentLayers.Length || level.m_VentLayers[layer])
                {
                    return; // vent layers: the game auto-manages these walls
                }
                var wall = BuildingBlockManager.GetBlock(
                    data.m_WallTileIDs[__instance.m_Delete_IndexIntoLayerData]) as BuildingBlock_Wall;
                if (wall == null || !wall.m_AutomaticBlock)
                {
                    return; // vanilla already handled non-automatic walls
                }
                int instruction = __instance.AddDeleteToDeleteList(
                    __instance.m_Delete_X, __instance.m_Delete_Y,
                    BuildingInstructionManager.InstructionDeleteElement.DeleteType.Wall);
                if (instruction == -1)
                {
                    return;
                }
                data.m_TileProperties[__instance.m_Delete_IndexIntoLayerData] |=
                    BaseLevelManager.TileProperty.DeleteWallMask;
                __instance.AddToInstructionList(complex.m_ComplexInstructions, instruction,
                    BaseBuildInstruction.InstructionTypeEnum.Delete);
                __result = true;
            }
        }

        [HarmonyPatch]
        private static class RestrictionPatches
        {
            // place any block on any layer
            [HarmonyPatch(typeof(BaseBuildingBlock), nameof(BaseBuildingBlock.IsValidForLayer))]
            [HarmonyPostfix]
            private static void AnyLayer(ref bool __result)
            {
                if (_ignoreAll)
                {
                    __result = true;
                }
            }

            // allow placement even when an object/room on the floor above blocks the position
            // LevelEditorBrushController.AreWeValid() is the gate that CanBlockBePlaced() checks
            [HarmonyPatch(typeof(LevelEditorBrushController), nameof(LevelEditorBrushController.AreWeValid))]
            [HarmonyPostfix]
            private static void BrushAlwaysValid(ref bool __result)
            {
                if (_ignoreAll)
                {
                    __result = true;
                }
            }

            // keep the native m_IgnoreChecks flag in sync with the toggle so the
            // brush preview (LevelEditorBrushElement.ValidateElement) skips
            // cross-floor checks — and, crucially, stops skipping them again
            // as soon as the toggle is turned off (it is never reset by vanilla)
            [HarmonyPatch(typeof(LevelEditorBrushElement), nameof(LevelEditorBrushElement.ValidateElement))]
            [HarmonyPrefix]
            private static void BypassBrushElementChecks()
            {
                var mgr = BuildingInstructionManager.GetInstance();
                if (mgr != null && mgr.m_IgnoreChecks != _ignoreAll)
                {
                    mgr.m_IgnoreChecks = _ignoreAll;
                }
            }

            // level validation: report "no problems" so saving produces a
            // Finished version (which also enables uploading and playing)
            [HarmonyPatch(typeof(LevelDetailsManager), nameof(LevelDetailsManager.GetLevelDataValidationErrors))]
            [HarmonyPostfix]
            private static void NoValidationErrors(ref bool __result, ref List<LevelDetailsManager.ErrorData> errorList)
            {
                if (_ignoreAll)
                {
                    errorList.Clear();
                    __result = false;
                }
            }

            [HarmonyPatch(typeof(LevelDetailsManager), nameof(LevelDetailsManager.ValidateEverythingIsReachable))]
            [HarmonyPostfix]
            private static void ReachabilityPasses(ref bool __result, ref List<LevelDetailsManager.ErrorData> errorList)
            {
                if (_ignoreAll)
                {
                    errorList.Clear();
                    __result = true;
                }
            }

            [HarmonyPatch(typeof(LevelDetailsManager), nameof(LevelDetailsManager.ValidateWalkableAreas))]
            [HarmonyPostfix]
            private static void WalkablePasses(ref bool __result, ref List<LevelDetailsManager.ErrorData> errorList)
            {
                if (_ignoreAll)
                {
                    errorList.Clear();
                    __result = true;
                }
            }

            // let work-in-progress maps appear playable in the menus
            [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.IsCustomPrisonPlayable))]
            [HarmonyPostfix]
            private static void WipIsPlayable(ref bool __result)
            {
                if (_ignoreAll)
                {
                    __result = true;
                }
            }

            // when the Finished file doesn't exist, play the WIP file instead
            [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.GetCustomLevelFilePath))]
            [HarmonyPostfix]
            private static void FallBackToWipFile(SaveManager __instance, ref string __result,
                string strPrisonName, bool bWithoutFinal)
            {
                if (!_ignoreAll || bWithoutFinal || string.IsNullOrEmpty(__result))
                {
                    return;
                }
                if (!File.Exists(__result))
                {
                    string wip = __instance.GetCustomLevelFilePath(strPrisonName, bWithoutFinal: true);
                    if (!string.IsNullOrEmpty(wip) && File.Exists(wip))
                    {
                        Log.Info("playing work-in-progress level file (no Finished version exists)");
                        __result = wip;
                    }
                }
            }
        }
    }
}
