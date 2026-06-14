using E2EApi.Features;
using HarmonyLib;
using UnityEngine;

namespace E2EApi.Editor.Patches
{
    /// <summary>
    /// Harmony Postfix patch on <c>StaticLadder.Start()</c>. After the vanilla
    /// initialisation runs, recalculates <c>m_DownwardTransition</c> and
    /// <c>m_NumFloorTransitions</c> using the registry-aware physical floor
    /// ordering from <see cref="FloorTypeRegistry"/> instead of raw
    /// physical-floor-index comparison.
    ///
    /// Problem addressed (item 29): when the mod's virtual layer list in
    /// MapGeometry contains gaps (physical floors not assigned to any virtual
    /// layer), the physical-index delta for NumFloorTransitions is still
    /// correct for Z-movement purposes but the direction check now uses the
    /// sorted registered-physical-position order so it works for non-standard
    /// backing-layer assignments too.
    ///
    /// Items 27-33 of the Engineered Multi-Floor System plan.
    /// </summary>
    [HarmonyPatch(typeof(StaticLadder), "Start")]
    internal static class StairsPatch
    {
        [HarmonyPostfix]
        private static void Start_Postfix(StaticLadder __instance)
        {
            if (!FloorTypeRegistry.HasEntries) return;

            int startFloor = __instance.m_TileFloor;
            int endFloor   = __instance.m_EndTileFloor;

            // Registry position within the sorted registered-physical list.
            // This position always reflects physical Z-ordering (ascending index
            // = ascending Z toward roof), so comparing positions gives the same
            // direction as comparing raw physical indices — except it also works
            // when only a subset of physical floors are registered.
            int startPos = FloorTypeRegistry.GetRegisteredPhysicalPosition(startFloor);
            int endPos   = FloorTypeRegistry.GetRegisteredPhysicalPosition(endFloor);

            if (startPos < 0 || endPos < 0)
            {
                // One or both floors not in registry — fall back to vanilla logic.
                return;
            }

            // "Downward" means going from a higher physical position to a lower one
            // (i.e. from roof direction toward underground direction).
            bool downward = startPos > endPos;
            if (downward != __instance.m_DownwardTransition)
            {
                __instance.m_DownwardTransition = downward;
                Log.Debug($"StairsPatch: corrected direction for ladder at "
                    + $"({__instance.TileRow},{__instance.TileColumn}) "
                    + $"floor {startFloor}(pos {startPos})→{endFloor}(pos {endPos}) "
                    + $"downward={downward}");
            }

            // NumFloorTransitions drives the Z displacement in DoTransition:
            //   Z += -m_FloorOffset * numFloors  (m_FloorOffset = -3)
            // We keep this as the raw physical-floor-index distance so the
            // player lands precisely on the target floor's Z position.
            __instance.m_NumFloorTransitions = Mathf.Abs(startFloor - endFloor);
        }
    }

    // ---- Registration -------------------------------------------------------

    internal static class StairsPatchGroup
    {
        private static bool _patched;

        internal static void EnsurePatched()
        {
            if (_patched) return;
            _patched = true;
            PatchRegistry.EnsurePatched(typeof(StairsPatch));
        }
    }
}
