using E2EApi.Features;
using HarmonyLib;

namespace E2EApi.Editor.Patches
{
    /// <summary>
    /// Harmony Prefix patches on <c>FloorManager.UpAFloor</c> and
    /// <c>FloorManager.DownAFloor</c>. When <see cref="FloorTypeRegistry"/> has
    /// entries the registry-ordered physical floor list drives navigation;
    /// unregistered physical floor slots (gaps in the virtual layer list) are
    /// skipped automatically.
    ///
    /// Vanilla maps (no registry) fall through to stock logic.
    ///
    /// Items 20-26 of the Engineered Multi-Floor System plan.
    /// </summary>
    [HarmonyPatch]
    internal static class FloorNavigationPatch
    {
        // ---- Item 22: UpAFloor patch ----------------------------------------

        [HarmonyPatch(typeof(FloorManager), nameof(FloorManager.UpAFloor))]
        [HarmonyPrefix]
        private static bool UpAFloor_Prefix(FloorManager __instance,
            FloorManager.Floor currentFloor, ref FloorManager.Floor __result)
        {
            if (!FloorTypeRegistry.HasEntries) return true; // vanilla passthrough

            int nextIndex = FloorTypeRegistry.FindNextFloor(
                currentFloor.m_FloorIndex, up: true, skipVents: false);

            // At boundary: return the same floor object (matches vanilla contract).
            if (nextIndex == currentFloor.m_FloorIndex)
            {
                __result = currentFloor;
                return false;
            }

            FloorManager.Floor next = __instance.FindFloorbyIndex(nextIndex);
            __result = next ?? currentFloor;
            return false;
        }

        // ---- Item 23: DownAFloor patch --------------------------------------

        [HarmonyPatch(typeof(FloorManager), nameof(FloorManager.DownAFloor))]
        [HarmonyPrefix]
        private static bool DownAFloor_Prefix(FloorManager __instance,
            FloorManager.Floor currentFloor, ref FloorManager.Floor __result)
        {
            if (!FloorTypeRegistry.HasEntries) return true; // vanilla passthrough

            int nextIndex = FloorTypeRegistry.FindNextFloor(
                currentFloor.m_FloorIndex, up: false, skipVents: false);

            // At boundary: return the same floor object (matches vanilla contract).
            if (nextIndex == currentFloor.m_FloorIndex)
            {
                __result = currentFloor;
                return false;
            }

            FloorManager.Floor next = __instance.FindFloorbyIndex(nextIndex);
            __result = next ?? currentFloor;
            return false;
        }
    }

    // ---- Registration -------------------------------------------------------

    internal static class FloorNavigationPatchGroup
    {
        private static bool _patched;

        internal static void EnsurePatched()
        {
            if (_patched) return;
            _patched = true;
            PatchRegistry.EnsurePatched(typeof(FloorNavigationPatch));
        }
    }
}
