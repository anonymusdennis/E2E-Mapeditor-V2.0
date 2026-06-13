using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace E2EApi.Editor.Patches
{
    /// <summary>
    /// While a mod editor tool owns the mouse (<see cref="EditorTools"/>), the
    /// vanilla editor must not also react to clicks (placing blocks, opening
    /// object popups, marquee selection …). This patch skips every mouse-driven
    /// edit-state handler of <c>LevelEditor_Controller</c> but keeps the
    /// keyboard camera movement alive by calling
    /// <c>ProcessPossibleCameraMove</c> ourselves.
    /// </summary>
    [HarmonyPatch]
    internal static class SuppressVanillaMousePatch
    {
        private static readonly string[] StateMethods =
        {
            "UpdateState_NothingSelected",
            "UpdateState_SelectingObjectInLevel",
            "UpdateState_SelectedObjectInLevel",
            "UpdateState_BlockSelected",
            "UpdateState_MovingBlock",
            "UpdateState_FreeDrawing",
            "UpdateState_Marquee",
            "UpdateState_MarqueeLine",
            "UpdateState_Deleting",
            "UpdateState_Copy",
            "UpdateState_Copy_Add",
            "UpdateState_Copy_Delete",
            "UpdateState_CopySelectedObjectInLevel",
            "UpdateState_CopySelectedObjectInLevel_Edit",
            "UpdateState_Zone_WaitingToCreate",
            "UpdateState_Zone_Creating",
            "UpdateState_Zone_Editing",
            "UpdateState_Zone_Adding",
            "UpdateState_Zone_Deleting",
            "UpdateState_Zone_Selected",
        };

        private static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (string name in StateMethods)
            {
                var method = AccessTools.Method(typeof(LevelEditor_Controller), name);
                if (method != null)
                {
                    yield return method;
                }
            }
        }

        private static bool Prefix(LevelEditor_Controller __instance)
        {
            if (!VanillaEditor.MouseSuppressed)
            {
                return true;
            }
            __instance.ProcessPossibleCameraMove();
            return false;
        }
    }
}
