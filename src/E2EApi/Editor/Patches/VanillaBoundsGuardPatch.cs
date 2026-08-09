using E2EApi.Features;
using HarmonyLib;

namespace E2EApi.Editor.Patches
{
    /// <summary>
    /// Blocks vanilla Level.dat writes outside the native 120×120 grid.
    /// Expanded map bounds are for mod/sidecar content; writing OOB into the
    /// native arrays crashes with IndexOutOfRange (Y*120+X).
    /// </summary>
    [HarmonyPatch(typeof(BuildingInstructionManager), nameof(BuildingInstructionManager.AddBlockOnce))]
    internal static class VanillaBoundsGuardPatch
    {
        private static bool Prefix(sbyte X, sbyte Y, ref bool __result)
        {
            if (MapGeometry.IsWithinNativeBounds(X, Y))
            {
                return true;
            }
            Log.Debug("vanilla place blocked outside native grid: " + X + "," + Y);
            __result = false;
            return false;
        }
    }

    internal static class VanillaBoundsGuardPatchRegistrar
    {
        internal static void EnsurePatched()
        {
            PatchRegistry.EnsurePatched(typeof(VanillaBoundsGuardPatch));
        }
    }
}
