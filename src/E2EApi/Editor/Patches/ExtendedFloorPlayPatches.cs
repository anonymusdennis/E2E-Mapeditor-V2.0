using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using E2EApi.Features;
using HarmonyLib;
using UnityEngine;

namespace E2EApi.Editor.Patches
{
    /// <summary>
    /// Playtest / runtime patches for physical floors ≥ 6: Z→layer lookup,
    /// light placer loops, stair/transition pairing, and playtest load bootstrap.
    /// </summary>
    [HarmonyPatch]
    public static class ExtendedFloorPlayPatches
    {
        private static bool _patched;
        private static readonly FieldInfo LightCurrentLayerField =
            AccessTools.Field(typeof(LevelSetup_LightPlacer), "m_CurrentLayer");
        private static readonly FieldInfo LayerZSetupField =
            AccessTools.Field(typeof(BaseLevelManager), "m_bLayerZOffsetSetup");
        private static readonly FieldInfo LayerZPositionsField =
            AccessTools.Field(typeof(BaseLevelManager), "m_LayerZPositions");

        public static void EnsurePatched()
        {
            if (_patched) return;
            _patched = true;
            PatchRegistry.EnsurePatched(typeof(ExtendedFloorPlayPatches));
        }

        // ---- Playtest load: bootstrap without wiping ModExtras.Current ----

        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LoadTheLevel))]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void LoadTheLevel_Postfix(bool __result)
        {
            if (!__result || !ExtendedFloors.Enabled)
            {
                return;
            }
            // Editor playtest: ModExtras.AfterLoadTheLevel skips; we bootstrap here.
            if (!E2EApi.Events.GameEvents.IsInEditor)
            {
                return;
            }
            ApiRunner.StartRoutine(DelayedBootstrap());
        }

        private static System.Collections.IEnumerator DelayedBootstrap()
        {
            yield return null;
            yield return new WaitForSeconds(0.1f);
            try
            {
                ExtendedFloors.BootstrapPlayMode();
            }
            catch (Exception ex)
            {
                Log.Error("ExtendedFloorPlayPatches bootstrap: " + ex);
            }
        }

        // ---- WhatLayerIsThisZIn: honour grown Z / BuildingLayers ----

        [HarmonyPatch(typeof(BaseLevelManager), nameof(BaseLevelManager.WhatLayerIsThisZIn))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        private static bool WhatLayerIsThisZIn_Prefix(
            BaseLevelManager __instance, float fZ,
            ref BaseLevelManager.LevelLayers __result)
        {
            if (!ExtendedFloors.Enabled) return true;
            if (__instance.m_BuildingLayers == null ||
                __instance.m_BuildingLayers.Length <= ExtendedFloors.NativeCount)
            {
                return true;
            }

            int len = __instance.m_BuildingLayers.Length;
            var zs = LayerZPositionsField != null
                ? LayerZPositionsField.GetValue(__instance) as float[]
                : null;
            if (zs == null || zs.Length < len)
            {
                return true;
            }

            bool setup = LayerZSetupField != null &&
                         (bool)LayerZSetupField.GetValue(__instance);
            if (!setup)
            {
                for (int i = 0; i < len; i++)
                {
                    var layer = __instance.m_BuildingLayers[i];
                    if (layer != null && layer.m_Tiles != null)
                    {
                        zs[i] = layer.m_Tiles.transform.position.z;
                    }
                }
                if (LayerZSetupField != null)
                {
                    LayerZSetupField.SetValue(__instance, true);
                }
            }

            for (int j = 0; j < len; j++)
            {
                if (fZ >= zs[j])
                {
                    __result = (BaseLevelManager.LevelLayers)j;
                    return false;
                }
            }
            __result = BaseLevelManager.LevelLayers.GroundFloor;
            return false;
        }

        // ---- Lights: InitializeMaps + ProcessingLayerLights ----

        [HarmonyPatch(typeof(LevelSetup_LightPlacer), "InitializeMaps")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        private static bool InitializeMaps_Prefix(
            LevelSetup_LightPlacer __instance, bool bZonedMap)
        {
            if (!ExtendedFloors.Enabled) return true;
            var mgr = __instance.m_LevelManager ?? BaseLevelManager.GetInstance();
            if (mgr == null || mgr.m_BuildingLayers == null ||
                mgr.m_BuildingLayers.Length <= ExtendedFloors.NativeCount)
            {
                return true;
            }

            EnsureLightGroups(__instance, mgr.m_BuildingLayers.Length);

            var mapsField = AccessTools.Field(typeof(LevelSetup_LightPlacer), "m_Maps");
            var maps = mapsField.GetValue(__instance) as System.Collections.IList;
            if (maps == null) return true;
            maps.Clear();

            var zoneMgr = __instance.m_ZoneManager ?? LevelEditor_ZoneManager.GetInstance();
            int len = mgr.m_BuildingLayers.Length;
            for (int i = 0; i < len; i++)
            {
                int[] zoneMap = null;
                if (bZonedMap && zoneMgr != null)
                {
                    var zm = zoneMgr.GetZoneMap((BaseLevelManager.LevelLayers)i);
                    if (zm != null) zoneMap = zm.m_Map;
                }

                var array2 = new LevelSetup_LightPlacer.Occupier[14400];
                var layer = mgr.m_BuildingLayers[i];
                for (int j = 0; j < 14400; j++)
                {
                    array2[j] = LevelSetup_LightPlacer.Occupier.EMPTY;
                    if (layer == null || layer.m_TileProperties == null)
                    {
                        continue;
                    }
                    var tileProperty = layer.m_TileProperties[j];
                    if (bZonedMap && zoneMap != null && zoneMap[j] != -1)
                    {
                        array2[j] |= LevelSetup_LightPlacer.Occupier.Zone;
                    }
                    if (layer.m_RoomPropertiesMasks != null &&
                        layer.m_RoomPropertiesMasks[j] != 0)
                    {
                        array2[j] |= LevelSetup_LightPlacer.Occupier.Room;
                    }
                    if ((tileProperty & BaseLevelManager.TileProperty.TileMask) == 0 ||
                        (tileProperty & BaseLevelManager.TileProperty.EnvironmentMask) == 0)
                    {
                        array2[j] |= LevelSetup_LightPlacer.Occupier.Outside;
                    }
                    if ((tileProperty & BaseLevelManager.TileProperty.WallMask) != 0)
                    {
                        array2[j] |= LevelSetup_LightPlacer.Occupier.Wall;
                    }
                }
                maps.Add(array2);
            }
            return false;
        }

        [HarmonyPatch(typeof(LevelSetup_LightPlacer), "ProcessingLayerLights")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        private static bool ProcessingLayerLights_Prefix(
            LevelSetup_LightPlacer __instance, ref bool __result)
        {
            if (!ExtendedFloors.Enabled || LightCurrentLayerField == null) return true;
            var mgr = __instance.m_LevelManager ?? BaseLevelManager.GetInstance();
            if (mgr == null || mgr.m_BuildingLayers == null ||
                mgr.m_BuildingLayers.Length <= ExtendedFloors.NativeCount)
            {
                return true;
            }

            EnsureLightGroups(__instance, mgr.m_BuildingLayers.Length);
            int cur = (int)LightCurrentLayerField.GetValue(__instance);
            __instance.FillLayerWithLights(cur);
            cur++;
            LightCurrentLayerField.SetValue(__instance, cur);
            // Vanilla: process layers 1..5 then stop when cur == 6.
            __result = cur >= mgr.m_BuildingLayers.Length;
            return false;
        }

        // ---- Transitions / stairs pairing across ≥6 ----

        [HarmonyPatch(typeof(LevelSetup_Transistion), nameof(LevelSetup_Transistion.Setup))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        private static bool TransitionSetup_Prefix(
            LevelSetup_Transistion __instance,
            ref BaseComponentSetup.SetupReturnState __result)
        {
            if (!ExtendedFloors.Enabled) return true;
            var mgr = BaseLevelManager.GetInstance();
            if (mgr == null || mgr.m_BuildingLayers == null ||
                mgr.m_BuildingLayers.Length <= ExtendedFloors.NativeCount)
            {
                return true;
            }

            int len = mgr.m_BuildingLayers.Length;
            int cells = 14400;
            for (int i = 1; i < len; i++)
            {
                var data = mgr.m_BuildingLayers[i];
                if (data == null || data.m_TileProperties == null) continue;

                for (int j = 0; j < cells; j++)
                {
                    var tileProperty = data.m_TileProperties[j];
                    int room = BaseLevelManager.GetRoomNumberFromProperty(ref data, j);
                    if (room == 0 ||
                        (tileProperty & BaseLevelManager.TileProperty.EntranceMask) !=
                        BaseLevelManager.TileProperty.EntranceMask)
                    {
                        continue;
                    }
                    var objBlock = BuildingBlockManager.GetBlock(data.m_ObjectTileIDs[j])
                        as BuildingBlock_Object;
                    if (objBlock == null ||
                        (objBlock.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.Entrance) == 0 ||
                        data.m_ObjectTileObjects[j] == null)
                    {
                        continue;
                    }
                    var go = data.m_ObjectTileObjects[j];
                    var tp = go.GetComponent<TransitionPoint>();
                    if (tp == null || tp.m_Partner != null) continue;

                    int foundLayer = -1;
                    int exitIdx = FindExitExtended(i, room, len, ref foundLayer);
                    if (exitIdx < 0 || foundLayer < 0) continue;

                    var partner = mgr.m_BuildingLayers[foundLayer].m_ObjectTileObjects[exitIdx];
                    tp.m_Partner = partner;
                    if (tp.m_AINodeLink != null && partner != null)
                    {
                        tp.m_AINodeLink.end = partner.transform;
                    }
                }
            }
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(__instance);
            }
            __result = BaseComponentSetup.SetupReturnState.Finished;
            return false;
        }

        [HarmonyPatch(typeof(LevelSetup_Transistion), nameof(LevelSetup_Transistion.FindExit))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        private static bool FindExit_Prefix(
            int iLayerIgnore, int roomNumber, ref int iFoundInLayer, ref int __result)
        {
            if (!ExtendedFloors.Enabled) return true;
            var mgr = BaseLevelManager.GetInstance();
            if (mgr == null || mgr.m_BuildingLayers == null ||
                mgr.m_BuildingLayers.Length <= ExtendedFloors.NativeCount)
            {
                return true;
            }
            __result = FindExitExtended(iLayerIgnore, roomNumber,
                mgr.m_BuildingLayers.Length, ref iFoundInLayer);
            return false;
        }

        // ---- LevelEditorTileHighlight: sample all floors ----

        [HarmonyPatch(typeof(LevelEditorTileHighlight), "UpdateLook")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> TileHighlight_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var getter = AccessTools.Method(typeof(ExtendedFloorPlayPatches),
                nameof(GetHighlightLoopBound));
            for (int i = 0; i < codes.Count - 1; i++)
            {
                if (codes[i].opcode != OpCodes.Ldc_I4_6) continue;
                var next = codes[i + 1].opcode;
                if (next != OpCodes.Blt && next != OpCodes.Blt_S &&
                    next != OpCodes.Ble && next != OpCodes.Ble_S) continue;
                codes[i] = new CodeInstruction(OpCodes.Call, getter);
                break;
            }
            return codes;
        }

        public static int GetHighlightLoopBound()
        {
            if (!ExtendedFloors.Enabled) return ExtendedFloors.NativeCount;
            var mgr = BaseLevelManager.GetInstance();
            if (mgr != null && mgr.m_BuildingLayers != null)
            {
                return mgr.m_BuildingLayers.Length;
            }
            return ExtendedFloors.PhysicalCount;
        }

        // ---- helpers ----

        private static void EnsureLightGroups(LevelSetup_LightPlacer placer, int count)
        {
            if (placer.m_LayerLightGroups == null)
            {
                placer.m_LayerLightGroups = new LevelSetup_LightPlacer.LayerLightGroup[count];
            }
            if (placer.m_LayerLightGroups.Length >= count)
            {
                return;
            }

            var old = placer.m_LayerLightGroups;
            var grown = new LevelSetup_LightPlacer.LayerLightGroup[count];
            for (int i = 0; i < count; i++)
            {
                if (i < old.Length && old[i] != null)
                {
                    grown[i] = old[i];
                }
                else if (old.Length > 1 && old[1] != null)
                {
                    // Reuse GroundFloor light definitions for extended prison floors.
                    grown[i] = old[1];
                }
                else
                {
                    grown[i] = new LevelSetup_LightPlacer.LayerLightGroup();
                }
            }
            placer.m_LayerLightGroups = grown;
        }

        private static int FindExitExtended(
            int iLayerIgnore, int roomNumber, int layerCount, ref int iFoundInLayer)
        {
            var mgr = BaseLevelManager.GetInstance();
            if (mgr == null || mgr.m_BuildingLayers == null) return -1;

            for (int i = 1; i < layerCount; i++)
            {
                if (i == iLayerIgnore) continue;
                var data = mgr.m_BuildingLayers[i];
                if (data == null || data.m_TileProperties == null) continue;

                for (int j = 0; j < 14400; j++)
                {
                    var tileProperty = data.m_TileProperties[j];
                    int room = BaseLevelManager.GetRoomNumberFromProperty(ref data, j);
                    if (room != roomNumber ||
                        (tileProperty & BaseLevelManager.TileProperty.ExitMask) !=
                        BaseLevelManager.TileProperty.ExitMask)
                    {
                        continue;
                    }
                    var objBlock = BuildingBlockManager.GetBlock(data.m_ObjectTileIDs[j])
                        as BuildingBlock_Object;
                    if (objBlock != null &&
                        (objBlock.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.Exit) != 0 &&
                        data.m_ObjectTileObjects[j] != null)
                    {
                        iFoundInLayer = i;
                        return j;
                    }
                }
            }
            return -1;
        }
    }
}
