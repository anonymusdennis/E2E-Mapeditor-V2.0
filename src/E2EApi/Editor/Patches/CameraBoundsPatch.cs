using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using E2EApi.Features;
using HarmonyLib;
using UnityEngine;

namespace E2EApi.Editor.Patches
{
    /// <summary>
    /// Expands editor camera pan clamps from the hardcoded native −60…60 /
    /// −60…59 to the virtual map bounds in <see cref="MapGeometry"/>.
    /// Without this, increasing Width/Height only moves the cursor clamp while
    /// the camera cannot reach the new tiles.
    /// </summary>
    internal static class CameraBoundsPatch
    {
        internal static void EnsurePatched()
        {
            PatchRegistry.EnsurePatched(typeof(ProcessPossibleCameraMoveTranspiler));
            PatchRegistry.EnsurePatched(typeof(UpdateStateMovingCameraTranspiler));
            PatchRegistry.EnsurePatched(typeof(ClampAfterZoomInOut));
        }

        public static float CamMinX()
        {
            float minX, maxX, minY, maxY;
            MapGeometry.GetCameraLocalClamp(out minX, out maxX, out minY, out maxY);
            return minX;
        }

        public static float CamMaxX()
        {
            float minX, maxX, minY, maxY;
            MapGeometry.GetCameraLocalClamp(out minX, out maxX, out minY, out maxY);
            return maxX;
        }

        public static float CamMinY()
        {
            float minX, maxX, minY, maxY;
            MapGeometry.GetCameraLocalClamp(out minX, out maxX, out minY, out maxY);
            return minY;
        }

        public static float CamMaxY()
        {
            float minX, maxX, minY, maxY;
            MapGeometry.GetCameraLocalClamp(out minX, out maxX, out minY, out maxY);
            return maxY;
        }

        /// <summary>
        /// Replace Mathf.Clamp(v, -60, 60/59) camera limits with MapGeometry bounds.
        /// Pattern in IL: ldc.r4 -60; ldc.r4 60|59; call Mathf.Clamp.
        /// </summary>
        private static IEnumerable<CodeInstruction> RewriteCameraClamps(
            IEnumerable<CodeInstruction> instructions)
        {
            var list = new List<CodeInstruction>(instructions);
            var clamp = AccessTools.Method(typeof(Mathf), nameof(Mathf.Clamp),
                new[] { typeof(float), typeof(float), typeof(float) });
            for (int i = 0; i < list.Count - 2; i++)
            {
                if (list[i].opcode != OpCodes.Ldc_R4 ||
                    list[i + 1].opcode != OpCodes.Ldc_R4 ||
                    list[i + 2].opcode != OpCodes.Call ||
                    list[i + 2].operand as MethodInfo != clamp)
                {
                    continue;
                }
                float lo = (float)list[i].operand;
                float hi = (float)list[i + 1].operand;
                // Native camera clamps only.
                if (Mathf.Approximately(lo, -60f) && Mathf.Approximately(hi, 60f))
                {
                    list[i] = new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(CameraBoundsPatch), nameof(CamMinX)));
                    list[i + 1] = new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(CameraBoundsPatch), nameof(CamMaxX)));
                }
                else if (Mathf.Approximately(lo, -60f) && Mathf.Approximately(hi, 59f))
                {
                    list[i] = new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(CameraBoundsPatch), nameof(CamMinY)));
                    list[i + 1] = new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(CameraBoundsPatch), nameof(CamMaxY)));
                }
            }
            return list;
        }

        [HarmonyPatch(typeof(LevelEditor_Controller), "ProcessPossibleCameraMove")]
        private static class ProcessPossibleCameraMoveTranspiler
        {
            private static IEnumerable<CodeInstruction> Transpiler(
                IEnumerable<CodeInstruction> instructions)
            {
                return RewriteCameraClamps(instructions);
            }
        }

        [HarmonyPatch(typeof(LevelEditor_Controller), "UpdateState_MovingCamera")]
        private static class UpdateStateMovingCameraTranspiler
        {
            private static IEnumerable<CodeInstruction> Transpiler(
                IEnumerable<CodeInstruction> instructions)
            {
                return RewriteCameraClamps(instructions);
            }
        }

        /// <summary>
        /// Zoom-in/out paths also clamp camera to −60…60 after adjusting position.
        /// Re-clamp to virtual bounds after those updates run.
        /// </summary>
        [HarmonyPatch(typeof(LevelEditor_Controller), "ProcessPossibleCameraMove")]
        private static class ClampAfterZoomInOut
        {
            private static void Postfix(LevelEditor_Controller __instance)
            {
                ReclampCamera(__instance);
            }
        }

        internal static void ReclampCamera(LevelEditor_Controller controller)
        {
            if (controller == null || controller.m_MainCamera == null)
            {
                return;
            }
            float minX, maxX, minY, maxY;
            MapGeometry.GetCameraLocalClamp(out minX, out maxX, out minY, out maxY);
            var t = controller.m_MainCamera.transform;
            var p = t.localPosition;
            float nx = Mathf.Clamp(p.x, minX, maxX);
            float ny = Mathf.Clamp(p.y, minY, maxY);
            if (!Mathf.Approximately(nx, p.x) || !Mathf.Approximately(ny, p.y))
            {
                t.localPosition = controller.GetCorrectedPosition(
                    new Vector3(nx, ny, p.z));
            }
        }
    }
}
