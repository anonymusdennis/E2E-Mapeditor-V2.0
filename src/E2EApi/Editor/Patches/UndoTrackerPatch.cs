using E2EApi.Features;
using HarmonyLib;

namespace E2EApi.Editor.Patches
{
    /// <summary>
    /// Harmony Postfixes on <c>LevelEditor_Controller.UndoLast/RedoLast</c> so
    /// <see cref="UndoHistory"/> can tell the game's own toolbar undo/redo
    /// apart from brand-new edits (both grow the instruction counter).
    /// </summary>
    [HarmonyPatch]
    internal static class UndoTrackerPatch
    {
        [HarmonyPatch(typeof(LevelEditor_Controller), nameof(LevelEditor_Controller.UndoLast))]
        [HarmonyPostfix]
        private static void UndoLast_Postfix()
        {
            UndoHistory.NoteVanillaUndoCall();
        }

        [HarmonyPatch(typeof(LevelEditor_Controller), nameof(LevelEditor_Controller.RedoLast))]
        [HarmonyPostfix]
        private static void RedoLast_Postfix()
        {
            UndoHistory.NoteVanillaRedoCall();
        }
    }

    // ---- Registration -------------------------------------------------------

    internal static class UndoTrackerPatchGroup
    {
        private static bool _patched;

        internal static void EnsurePatched()
        {
            if (_patched) return;
            _patched = true;
            PatchRegistry.EnsurePatched(typeof(UndoTrackerPatch));
        }
    }
}
