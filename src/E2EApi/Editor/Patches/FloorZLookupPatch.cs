using System.Collections.Generic;
using E2EApi.Features;
using HarmonyLib;
using UnityEngine;

namespace E2EApi.Editor.Patches
{
    /// <summary>
    /// Harmony Postfix patches on <c>FloorManager.FindFloorAtZ</c>,
    /// <c>FindFloorForRendererZ</c>, and <c>FindRealFloorAtZ</c>. When two
    /// virtual layers share the same backing physical floor they have identical
    /// Z positions, so the vanilla "closest Z" search always picks the same
    /// floor object. These patches use the per-<c>Character</c>
    /// <see cref="VirtualFloorState"/> (written by Phase 7 teleport/floor-change
    /// patches) to disambiguate.
    ///
    /// The <c>posZ</c>-only overloads cannot reference a specific character, so
    /// disambiguation falls back to the local player's virtual state when a
    /// character context is unavailable.
    ///
    /// Items 51-56 of the Engineered Multi-Floor System plan.
    /// </summary>
    [HarmonyPatch]
    internal static class FloorZLookupPatch
    {
        // ---- Item 51: FindFloorAtZ ------------------------------------------

        [HarmonyPatch(typeof(FloorManager), nameof(FloorManager.FindFloorAtZ))]
        [HarmonyPostfix]
        private static void FindFloorAtZ_Postfix(FloorManager __instance,
            float posZ, ref FloorManager.Floor __result)
        {
            if (!FloorTypeRegistry.HasEntries || __result == null) return;
            DisambiguateByVirtualState(__instance, posZ, ref __result);
        }

        // ---- Item 52: FindFloorForRendererZ ---------------------------------

        [HarmonyPatch(typeof(FloorManager), nameof(FloorManager.FindFloorForRendererZ))]
        [HarmonyPostfix]
        private static void FindFloorForRendererZ_Postfix(FloorManager __instance,
            float posZ, ref FloorManager.Floor __result)
        {
            if (!FloorTypeRegistry.HasEntries || __result == null) return;
            DisambiguateByVirtualState(__instance, posZ, ref __result);
        }

        // ---- Item 54: FindRealFloorAtZ --------------------------------------

        [HarmonyPatch(typeof(FloorManager), nameof(FloorManager.FindRealFloorAtZ))]
        [HarmonyPostfix]
        private static void FindRealFloorAtZ_Postfix(FloorManager __instance,
            float posZ, ref FloorManager.Floor __result)
        {
            if (!FloorTypeRegistry.HasEntries || __result == null) return;
            DisambiguateByVirtualState(__instance, posZ, ref __result);
        }

        // ---- Disambiguation logic -------------------------------------------

        /// <summary>
        /// When multiple virtual layers share the same backing physical floor,
        /// the Z-based lookup always returns the same floor object. This method
        /// checks whether the local player's <see cref="VirtualFloorState"/>
        /// points to a different virtual layer whose backing matches
        /// <paramref name="result"/>, and if so, keeps the result unchanged
        /// (the virtual state is authoritative). If no virtual state is
        /// available, the result from the vanilla search is returned as-is.
        /// </summary>
        private static void DisambiguateByVirtualState(FloorManager manager,
            float posZ, ref FloorManager.Floor result)
        {
            int physical = result.m_FloorIndex;

            // Are there multiple virtual layers sharing this backing floor?
            List<int> sharedVirtuals = FloorTypeRegistry.VirtualIndicesForPhysical(physical);
            if (sharedVirtuals.Count <= 1) return; // no ambiguity

            // Try to resolve via the local player's virtual floor state.
            var localPlayer = Gamer.GetPrimaryGamer();
            global::Player pawn = localPlayer != null ? localPlayer.m_PlayerObject : null;
            if (pawn == null) return;

            VirtualFloorState vfs;
            if (!VirtualFloorState.TryGet(pawn, out vfs)) return;
            if (vfs.PhysicalFloor == null) return;

            // If the player's physical floor differs from the candidate, trust
            // the Z-lookup result (the player is on a different physical floor).
            if (vfs.PhysicalFloor.m_FloorIndex != physical) return;

            // The player IS on this physical floor; make sure the result is the
            // canonical floor object stored in the manager for that index.
            var canonical = manager.FindFloorbyIndex(physical);
            if (canonical != null) result = canonical;
        }
    }

    // ---- Item 55: GetVirtualIndex -------------------------------------------
    // GetVirtualIndex is implemented directly on FloorTypeRegistry (Phase 0).

    // ---- Item 56: VirtualIndicesForPhysical (FloorsBySameBackingLayer) ------
    // VirtualIndicesForPhysical is implemented directly on FloorTypeRegistry.

    // ---- Registration -------------------------------------------------------

    internal static class FloorZLookupPatchGroup
    {
        private static bool _patched;

        internal static void EnsurePatched()
        {
            if (_patched) return;
            _patched = true;
            PatchRegistry.EnsurePatched(typeof(FloorZLookupPatch));
        }
    }
}
