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
    /// </summary>
    public static class Restrictions
    {
        private static bool _ignoreAll;

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

            // set the native m_IgnoreChecks flag so the brush preview
            // (LevelEditorBrushElement.ValidateElement) skips cross-floor checks
            [HarmonyPatch(typeof(LevelEditorBrushElement), nameof(LevelEditorBrushElement.ValidateElement))]
            [HarmonyPrefix]
            private static void BypassBrushElementChecks()
            {
                if (!_ignoreAll)
                {
                    return;
                }
                var mgr = BuildingInstructionManager.GetInstance();
                if (mgr != null)
                {
                    mgr.m_IgnoreChecks = true;
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
