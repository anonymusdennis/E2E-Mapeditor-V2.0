using E2EApi.Features;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace E2EApi.Editor.Patches
{
    /// <summary>
    /// Writes <see cref="VirtualFloorState"/> whenever the local player
    /// teleports (via <c>Player.Teleport</c>) to a new floor.  This gives
    /// <see cref="FloorZLookupPatch"/> accurate virtual-layer context so
    /// Z-position disambiguation works when two virtual layers share the same
    /// backing physical floor.
    ///
    /// Items 34-40 of the Engineered Multi-Floor System plan.
    /// </summary>
    [HarmonyPatch]
    internal static class CharacterTeleportPatch
    {
        internal static MethodInfo TargetMethod()
        {
            // global::Player.Teleport(Vector3 position, FloorManager.Floor newFloor)
            var m = typeof(global::Player).GetMethod("Teleport",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { typeof(Vector3), typeof(FloorManager.Floor) },
                null);
            if (m == null)
                Log.Warn("CharacterTeleportPatch: Player.Teleport(Vector3, Floor) not found " +
                         "— VirtualFloorState will not update on teleport");
            return m;
        }

        // __instance = global::Player (IS-A Character)
        // __result   = bool return value of Teleport
        // __1        = second positional parameter: FloorManager.Floor
        static void Postfix(global::Player __instance, bool __result, FloorManager.Floor __1)
        {
            if (!__result || !FloorTypeRegistry.HasEntries || __1 == null) return;
            int vi = FloorTypeRegistry.GetVirtualIndex(__1);
            if (vi >= 0)
                VirtualFloorState.Set(__instance, vi, __1);
        }
    }

    // ---- Items 41-44: floor-change tracking via CurrentFloor setter ----------
    // Stair and vent transitions do not necessarily go through Teleport.  We
    // try to patch the property setter (or SetFloor method) that assigns a
    // character's physical floor so that VirtualFloorState stays current even
    // for AI characters and non-teleport floor changes.

    [HarmonyPatch]
    internal static class CharacterFloorChangePatch
    {
        internal static MethodInfo TargetMethod()
        {
            // Try property setter first (generated as set_CurrentFloor by C# compiler)
            var m = typeof(Character).GetMethod("set_CurrentFloor",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (m != null) return m;

            // Fallback: some decompilers expose it as SetFloor or SetCurrentFloor
            m = typeof(Character).GetMethod("SetFloor",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null, new[] { typeof(FloorManager.Floor) }, null);
            if (m != null) return m;

            m = typeof(Character).GetMethod("SetCurrentFloor",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null, new[] { typeof(FloorManager.Floor) }, null);
            if (m != null) return m;

            Log.Warn("CharacterFloorChangePatch: no floor-setter found on Character " +
                     "— VirtualFloorState may be stale after stair/vent transitions");
            return null;
        }

        // __0 = first positional parameter: FloorManager.Floor value
        static void Postfix(Character __instance, FloorManager.Floor __0)
        {
            if (!FloorTypeRegistry.HasEntries || __0 == null) return;
            int vi = FloorTypeRegistry.GetVirtualIndex(__0);
            if (vi >= 0)
                VirtualFloorState.Set(__instance, vi, __0);
        }
    }

    // ---- Registration -------------------------------------------------------

    internal static class CharacterFloorPatchGroup
    {
        private static bool _patched;

        /// <summary>
        /// Apply character floor-tracking patches.  Safe to call multiple times.
        /// Called from <see cref="Features.MapGeometry.Initialise"/> so the patches
        /// are always active when virtual layers are in use.
        /// </summary>
        internal static void EnsurePatched()
        {
            if (_patched) return;
            _patched = true;
            PatchRegistry.EnsurePatched(typeof(CharacterTeleportPatch));
            PatchRegistry.EnsurePatched(typeof(CharacterFloorChangePatch));
        }
    }
}
