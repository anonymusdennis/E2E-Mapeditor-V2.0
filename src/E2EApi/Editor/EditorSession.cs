namespace E2EApi.Editor
{
    /// <summary>
    /// Programmatic entry into the level editor — same call the frontend
    /// "create new map" button makes. Main thread only.
    /// </summary>
    public static class EditorSession
    {
        /// <summary>
        /// Start an editor session from the main menu. Empty path = new map.
        /// Returns false when the game isn't in a state to do it.
        /// </summary>
        public static bool Enter(string levelFile = "")
        {
            var globalStart = GlobalStart.GetInstance();
            if (globalStart == null)
            {
                return false;
            }
            globalStart.EnterLevelEditor(levelFile ?? "");
            return true;
        }

        /// <summary>
        /// Play-test the map currently open in the editor. Goes through the
        /// editor controller's own preview flow (save + playlist setup), the
        /// same path as the vanilla play-test button. Returns false outside
        /// an editor session.
        /// </summary>
        public static bool PlayTest()
        {
            if (!Events.GameEvents.IsInEditor)
            {
                return false;
            }
            var controller = LevelEditor_Controller.GetInstance();
            if (controller == null)
            {
                return false;
            }
            // If the prison-checker dialog is on screen, go through its own
            // preview handler: it hides the dialog (restoring input/timescale)
            // before starting the preview. A modal dialog left open stalls
            // the loading-screen fade forever.
            var dialog = UnityEngine.Object.FindObjectOfType<LevelEditor_PrisonCheckerDialog>();
            if (dialog != null && dialog.gameObject.activeInHierarchy)
            {
                dialog.OnPreviewButtonClicked();
                return true;
            }
            // PreviewLevel saves the level, then builds the PrisonData and
            // PlaylistData that the loader needs before flipping GlobalStart
            // into the preview flow. Calling GlobalStart.PreviewEditorLevel
            // directly skips that setup and hangs the loading screen.
            controller.PreviewLevel();
            return true;
        }

        /// <summary>
        /// Save the map currently open in the editor (same as the vanilla
        /// save flow). When the prison checker finds zero errors this also
        /// writes Level_Finished.dat, which triggers the vanilla-fallback
        /// processing for maps with mod content. Main thread only.
        /// </summary>
        public static string Save()
        {
            if (!Events.GameEvents.IsInEditor)
            {
                return "not in the editor";
            }
            var controller = LevelEditor_Controller.GetInstance();
            if (controller == null)
            {
                return "no editor controller";
            }
            string result = "pending (snapshot in flight)";
            controller.SaveTheLevel(bForceNew: false,
                r => { result = r.ToString(); });
            return "save: " + result;
        }

        /// <summary>
        /// Dismiss the "press the spacebar to start" title screen without
        /// input hardware: replicates BootFlow's WAIT_FOR_START handler with
        /// the mouse-owning Rewired player. No-op outside that boot stage.
        /// </summary>
        public static string SkipTitle()
        {
            var boot = UnityEngine.Object.FindObjectOfType<BootFlow>();
            if (boot == null)
            {
                return "no BootFlow (already past boot?)";
            }
            if (boot.m_BootFlowMode != BootFlow.BOOTFLOW_MODE.WAIT_FOR_START)
            {
                return "boot mode is " + boot.m_BootFlowMode + ", not WAIT_FOR_START";
            }

            Rewired.Player player = null;
            var players = Rewired.ReInput.players.Players;
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i] != null && players[i].controllers.hasMouse)
                {
                    player = players[i];
                    break;
                }
            }
            if (player == null && players.Count > 0)
            {
                player = players[0];
            }
            if (player == null)
            {
                return "no rewired players";
            }

            boot.m_BootControllerIndex = player.id;
            if (!Platform.GetInstance().EndDiscovery(player.id, bIsPrimary: true))
            {
                boot.m_BootFlowMode = BootFlow.BOOTFLOW_MODE.WAIT_FOR_SIGNIN;
            }
            else
            {
                boot.m_BootFlowMode = BootFlow.BOOTFLOW_MODE.POST_SIGNIN;
            }
            return "ok (player " + player.id + ")";
        }
    }
}
