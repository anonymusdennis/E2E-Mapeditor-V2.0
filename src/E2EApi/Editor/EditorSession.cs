using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using E2EApi.Features;
using HarmonyLib;
using UnityEngine;

namespace E2EApi.Editor
{
    /// <summary>
    /// Programmatic entry into the level editor — same call the frontend
    /// "create new map" button makes. Main thread only.
    /// </summary>
    public static class EditorSession
    {
        private static readonly FieldInfo RegisteredFlowsField =
            AccessTools.Field(typeof(GlobalStart), "m_RegisteredFlows");

        /// <summary>
        /// Path to the open custom level's Level.dat (or empty when unavailable).
        /// </summary>
        public static string CurrentLevelPath
        {
            get
            {
                var globalStart = GlobalStart.GetInstance();
                string path = globalStart != null ? globalStart.m_strCustomLevelFile : null;
                if (string.IsNullOrEmpty(path))
                {
                    return "";
                }
                if (Directory.Exists(path) && !File.Exists(path))
                {
                    return Path.Combine(path, "Level.dat");
                }
                return path;
            }
        }

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
        /// the editor.
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
            // PreviewLevel saves the level, then builds the PrisonData and
            // PlaylistData that the loader needs before flipping GlobalStart
            // into the preview flow. Calling GlobalStart.PreviewEditorLevel
            // directly skips that setup and hangs the loading screen.
            // (LevelEditor_UIController has no OnPreviewButtonClicked — the
            // vanilla play-test button wires PreviewLevel on the controller.)
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
        /// Leave the level editor and return to the frontend. Used after
        /// geometry changes that require a full reload. Main thread only.
        /// </summary>
        public static bool Exit()
        {
            if (!Events.GameEvents.IsInEditor)
            {
                return false;
            }
            var globalStart = GlobalStart.GetInstance();
            if (globalStart == null)
            {
                return false;
            }
            ExtendedFloors.DiscardAliasState();
            globalStart.EndEditorLevel();
            return true;
        }

        /// <summary>
        /// Exit the editor, wait until the frontend is ready, re-enter the
        /// same level, then force-dismiss any stuck loading overlay.
        /// </summary>
        public static void ReloadAfterDelay(string path, float delaySeconds = 0.5f)
        {
            ApiRunner.StartRoutine(ReloadRoutine(path ?? "", delaySeconds));
        }

        /// <summary>
        /// Hard-reset the vanilla <see cref="LoadingFlow"/> UI and refcount.
        /// Needed when Exit+Enter races leave the loading canvas up while the
        /// editor is already interactive underneath.
        /// </summary>
        public static void ForceDismissLoadingScreen()
        {
            try
            {
                ModTiles.CancelPreload();
                var gs = GlobalStart.GetInstance();
                if (gs == null || RegisteredFlowsField == null)
                {
                    return;
                }
                var flows = RegisteredFlowsField.GetValue(gs)
                    as Dictionary<BaseFlowBehaviour.FlowType, BaseFlowBehaviour>;
                if (flows == null)
                {
                    return;
                }
                BaseFlowBehaviour flow;
                if (!flows.TryGetValue(BaseFlowBehaviour.FlowType.Loading, out flow) ||
                    flow == null)
                {
                    return;
                }
                var loading = flow as LoadingFlow;
                if (loading != null)
                {
                    loading.Reset();
                }
            }
            catch (System.Exception ex)
            {
                Log.Debug("ForceDismissLoadingScreen: " + ex.Message);
            }
        }

        private static IEnumerator ReloadRoutine(string path, float delaySeconds)
        {
            // Caller already called Exit(). Wait for the editor scene to tear down
            // and GlobalStart to return to the frontend — do NOT Enter while still
            // in EXIT_* (that stacks LoadingFlow refcounts and sticks the overlay).
            float timeout = 45f;
            float elapsed = 0f;
            while (Events.GameEvents.IsInEditor && elapsed < timeout)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < timeout)
            {
                var gs = GlobalStart.GetInstance();
                if (gs != null)
                {
                    var mode = gs.GetMode();
                    if (mode == GlobalStart.GLOBALSTART_MODE.SHOW_FRONTEND ||
                        mode == GlobalStart.GLOBALSTART_MODE.CHECK_INVITES ||
                        mode == GlobalStart.GLOBALSTART_MODE.LOADING_FLOW_DUMMY_STATE)
                    {
                        break;
                    }
                }
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            ForceDismissLoadingScreen();
            if (delaySeconds > 0f)
            {
                yield return new WaitForSeconds(delaySeconds);
            }
            else
            {
                yield return null;
            }

            Enter(path);

            elapsed = 0f;
            while (!Events.GameEvents.IsEditorActive && elapsed < timeout)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            // Enter's KILL_LOADING may have been skipped or under-counted — wipe
            // the overlay for several frames once the editor is live.
            for (int i = 0; i < 60; i++)
            {
                if (Events.GameEvents.IsEditorActive)
                {
                    ForceDismissLoadingScreen();
                }
                yield return null;
            }
            ForceDismissLoadingScreen();
            Log.Info("EditorSession reload complete — loading overlay dismissed");
        }

        /// <summary>
        /// Dismiss the "press the spacebar to start" title screen without
        /// input hardware: replicates BootFlow's WAIT_FOR_START handler with
        /// the mouse-owning Rewired player. No-op outside that boot stage.
        /// </summary>
        public static string SkipTitle()
        {
            var boot = Object.FindObjectOfType<BootFlow>();
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
