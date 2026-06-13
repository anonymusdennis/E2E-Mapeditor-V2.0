using System.Globalization;
using System.Text;
using UnityEngine;

namespace E2EApi
{
    /// <summary>Introspection helpers for debugging overlays and the editor camera.</summary>
    public static class Diagnostics
    {
        /// <summary>JSON snapshot of overlay/camera state (used by the web UI's /api/debug).</summary>
        public static string OverlayDebugJson()
        {
            var inv = CultureInfo.InvariantCulture;
            var sb = new StringBuilder();
            sb.Append("{");
            var tile = Editor.Grid.TileToWorld(60, 60);
            sb.Append("\"tile60\":").Append(tile != null
                ? "[" + tile.Value.x.ToString("0.##", inv) + "," + tile.Value.y.ToString("0.##", inv) +
                  "," + tile.Value.z.ToString("0.##", inv) + "]"
                : "null");
            var controller = LevelEditor_Controller.GetInstance();
            if (controller != null && controller.m_MainCamera != null)
            {
                var cam = controller.m_MainCamera;
                sb.Append(",\"cam\":[")
                  .Append(cam.transform.position.x.ToString("0.##", inv)).Append(",")
                  .Append(cam.transform.position.y.ToString("0.##", inv)).Append(",")
                  .Append(cam.transform.position.z.ToString("0.##", inv)).Append("]");
                sb.Append(",\"ortho\":").Append(cam.orthographicSize.ToString("0.##", inv));
                sb.Append(",\"edgePan\":").Append(
                    controller.m_fCameraPanEdgeDistance.ToString("0.###", inv));
                sb.Append(",\"brushVisible\":").Append(
                    controller.m_Brush != null && controller.m_Brush.activeSelf ? "true" : "false");
                sb.Append(",\"currentBlock\":").Append(controller.m_CurrentBlock);
            }
            int fenceMarks = 0;
            foreach (var renderer in Object.FindObjectsOfType<SpriteRenderer>())
            {
                if (renderer != null && renderer.gameObject.name == "E2E_FenceMark")
                {
                    fenceMarks++;
                    if (fenceMarks == 1)
                    {
                        var p = renderer.transform.position;
                        sb.Append(",\"firstMark\":[")
                          .Append(p.x.ToString("0.##", inv)).Append(",")
                          .Append(p.y.ToString("0.##", inv)).Append(",")
                          .Append(p.z.ToString("0.##", inv)).Append("]");
                    }
                }
            }
            sb.Append(",\"fenceMarks\":").Append(fenceMarks);
            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>JSON snapshot of the GlobalStart loading state machine.</summary>
        public static string LoadStateJson()
        {
            var gs = GlobalStart.GetInstance();
            if (gs == null)
            {
                return "{\"mode\":\"no GlobalStart\"}";
            }
            var sb = new StringBuilder();
            sb.Append("{\"mode\":\"").Append(gs.CurrentGlobalStartMode).Append("\"");
            sb.Append(",\"preview\":").Append(gs.m_PreviewEditorLevel ? "true" : "false");
            sb.Append(",\"custom\":").Append(gs.m_CustomLevel ? "true" : "false");
            sb.Append(",\"levelLoaded\":").Append(gs.m_bLevelLoaded ? "true" : "false");
            sb.Append(",\"levelFailed\":").Append(gs.m_bLevelFailedToLoad ? "true" : "false");
            sb.Append(",\"frontEndLoaded\":").Append(gs.m_bFrontEndLoaded ? "true" : "false");
            sb.Append(",\"hudLoaded\":").Append(gs.m_bHUDMenuLoaded ? "true" : "false");
            sb.Append(",\"igmLoaded\":").Append(gs.m_bInGameMenusLoaded ? "true" : "false");
            sb.Append(",\"gc\":").Append(gs.m_bGarbageCollected ? "true" : "false");
            sb.Append(",\"loadError\":").Append(gs.m_bLoadError ? "true" : "false");
            sb.Append(",\"customDataBytes\":").Append(
                gs.m_CustomLevelData != null ? gs.m_CustomLevelData.Count : -1);
            sb.Append(",\"sceneName\":\"").Append(gs.m_CurrentLevelSceneName ?? "").Append("\"");
            sb.Append(",\"timeScale\":").Append(UnityEngine.Time.timeScale.ToString(
                System.Globalization.CultureInfo.InvariantCulture));
            var checker = UnityEngine.Object.FindObjectOfType<LevelEditor_PrisonCheckerDialog>();
            sb.Append(",\"checkerDialog\":").Append(
                checker != null && checker.gameObject.activeInHierarchy ? "true" : "false");
            sb.Append("}");
            return sb.ToString();
        }
    }
}
