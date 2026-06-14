using E2EApi.Features;
using HarmonyLib;

namespace E2EApi.Editor.Patches
{
    /// <summary>
    /// Harmony Postfix patches on <c>FloorManager.Floor</c> predicate methods
    /// (IsVent, IsUnderGround, IsPrisonFloor, etc.). When
    /// <see cref="FloorTypeRegistry"/> has entries the registered virtual type
    /// overrides the native <c>FLOOR_TYPE</c>; vanilla maps fall through
    /// unchanged.
    ///
    /// Items 11-19 of the Engineered Multi-Floor System plan.
    /// </summary>
    [HarmonyPatch]
    internal static class FloorPredicatePatch
    {
        // ---- Item 11: IsVent ------------------------------------------------

        [HarmonyPatch(typeof(FloorManager.Floor), nameof(FloorManager.Floor.IsVent))]
        [HarmonyPostfix]
        private static void IsVent_Postfix(FloorManager.Floor __instance, ref bool __result)
        {
            if (!FloorTypeRegistry.HasEntries) return;
            var t = FloorTypeRegistry.GetType(__instance.m_FloorIndex);
            if (!t.HasValue) return;
            __result = t.Value == MapGeometry.VirtualLayerType.Vent;
        }

        // ---- Item 12: IsUnderGround -----------------------------------------

        [HarmonyPatch(typeof(FloorManager.Floor), nameof(FloorManager.Floor.IsUnderGround))]
        [HarmonyPostfix]
        private static void IsUnderGround_Postfix(FloorManager.Floor __instance, ref bool __result)
        {
            if (!FloorTypeRegistry.HasEntries) return;
            var t = FloorTypeRegistry.GetType(__instance.m_FloorIndex);
            if (!t.HasValue) return;
            __result = t.Value == MapGeometry.VirtualLayerType.Underground;
        }

        // ---- Item 13: IsPrisonFloor -----------------------------------------

        [HarmonyPatch(typeof(FloorManager.Floor), nameof(FloorManager.Floor.IsPrisonFloor))]
        [HarmonyPostfix]
        private static void IsPrisonFloor_Postfix(FloorManager.Floor __instance, ref bool __result)
        {
            if (!FloorTypeRegistry.HasEntries) return;
            var t = FloorTypeRegistry.GetType(__instance.m_FloorIndex);
            if (!t.HasValue) return;
            __result = t.Value == MapGeometry.VirtualLayerType.Ground;
        }

        // ---- Item 14: IsPrisonFloorOrRoof -----------------------------------

        [HarmonyPatch(typeof(FloorManager.Floor), nameof(FloorManager.Floor.IsPrisonFloorOrRoof))]
        [HarmonyPostfix]
        private static void IsPrisonFloorOrRoof_Postfix(FloorManager.Floor __instance, ref bool __result)
        {
            if (!FloorTypeRegistry.HasEntries) return;
            var t = FloorTypeRegistry.GetType(__instance.m_FloorIndex);
            if (!t.HasValue) return;
            __result = t.Value == MapGeometry.VirtualLayerType.Ground ||
                       t.Value == MapGeometry.VirtualLayerType.Roof;
        }

        // ---- Item 15: IsAboveVent -------------------------------------------

        [HarmonyPatch(typeof(FloorManager.Floor), nameof(FloorManager.Floor.IsAboveVent))]
        [HarmonyPostfix]
        private static void IsAboveVent_Postfix(FloorManager.Floor __instance, ref bool __result)
        {
            if (!FloorTypeRegistry.HasEntries) return;
            int belowIndex = __instance.m_FloorIndex - 1;
            if (belowIndex < 0) { __result = false; return; }
            var t = FloorTypeRegistry.GetType(belowIndex);
            if (!t.HasValue) return;
            __result = t.Value == MapGeometry.VirtualLayerType.Vent;
        }

        // ---- Item 16: IsAboveUnderGround ------------------------------------

        [HarmonyPatch(typeof(FloorManager.Floor), nameof(FloorManager.Floor.IsAboveUnderGround))]
        [HarmonyPostfix]
        private static void IsAboveUnderGround_Postfix(FloorManager.Floor __instance, ref bool __result)
        {
            if (!FloorTypeRegistry.HasEntries) return;
            int belowIndex = __instance.m_FloorIndex - 1;
            if (belowIndex < 0) { __result = false; return; }
            var t = FloorTypeRegistry.GetType(belowIndex);
            if (!t.HasValue) return;
            __result = t.Value == MapGeometry.VirtualLayerType.Underground;
        }

        // ---- Item 17: IsTheGroundFloor --------------------------------------

        [HarmonyPatch(typeof(FloorManager.Floor), nameof(FloorManager.Floor.IsTheGroundFloor))]
        [HarmonyPostfix]
        private static void IsTheGroundFloor_Postfix(FloorManager.Floor __instance, ref bool __result)
        {
            if (!FloorTypeRegistry.HasEntries) return;

            // The "ground floor" is the first registered Ground-type floor in
            // ascending physical-floor-index order.
            var manager = FloorManager.GetInstance();
            if (manager == null) return;

            for (int i = 0; i < manager.currentMaxFloor; i++)
            {
                var floor = manager.m_PrisonFloors[i];
                if (floor == null) continue;
                var t = FloorTypeRegistry.GetType(floor.m_FloorIndex);
                if (t.HasValue && t.Value == MapGeometry.VirtualLayerType.Ground)
                {
                    __result = floor.m_FloorIndex == __instance.m_FloorIndex;
                    return;
                }
                // If no registry entry, fall back to native Prison type
                if (!t.HasValue && floor.m_FloorType == FloorManager.FLOOR_TYPE.Floor_Prison)
                {
                    __result = floor.m_FloorIndex == __instance.m_FloorIndex;
                    return;
                }
            }
            __result = false;
        }
    }

    // ---- Item 18-19: registration group ------------------------------------

    internal static class FloorPredicatePatchGroup
    {
        private static bool _patched;

        /// <summary>
        /// Apply all floor-predicate patches. Safe to call multiple times; patches
        /// are registered only once. Intended to be called lazily the first time
        /// <see cref="FloorTypeRegistry"/> has entries (e.g. on map load).
        /// </summary>
        internal static void EnsurePatched()
        {
            if (_patched) return;
            _patched = true;
            PatchRegistry.EnsurePatched(typeof(FloorPredicatePatch));
        }
    }
}
