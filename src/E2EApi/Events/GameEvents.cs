using System;
using HarmonyLib;

namespace E2EApi.Events
{
    /// <summary>
    /// Game lifecycle events. Patches are applied lazily the first time anyone
    /// subscribes, so an idle API costs nothing.
    /// </summary>
    public static class GameEvents
    {
        private static bool _patched;

        /// <summary>The level editor scene was entered (manager awake).</summary>
        public static event Action EditorEntered
        {
            add { EnsurePatched(); _editorEntered += value; }
            remove { _editorEntered -= value; }
        }

        /// <summary>The level editor scene was left (manager destroyed).</summary>
        public static event Action EditorExited
        {
            add { EnsurePatched(); _editorExited += value; }
            remove { _editorExited -= value; }
        }

        /// <summary>Any level (play or editor) finished its manager Awake.</summary>
        public static event Action LevelLoaded
        {
            add { EnsurePatched(); _levelLoaded += value; }
            remove { _levelLoaded -= value; }
        }

        /// <summary>Any level's manager was destroyed.</summary>
        public static event Action LevelUnloaded
        {
            add { EnsurePatched(); _levelUnloaded += value; }
            remove { _levelUnloaded -= value; }
        }

        /// <summary>
        /// The editor's play-test (preview) started: a play-mode level manager
        /// awoke while the editor session still exists in the background.
        /// </summary>
        public static event Action PlaytestStarted
        {
            add { EnsurePatched(); _playtestStarted += value; }
            remove { _playtestStarted -= value; }
        }

        /// <summary>The play-test level was torn down (returning to the editor).</summary>
        public static event Action PlaytestEnded
        {
            add { EnsurePatched(); _playtestEnded += value; }
            remove { _playtestEnded -= value; }
        }

        private static Action _editorEntered;
        private static Action _editorExited;
        private static Action _levelLoaded;
        private static Action _levelUnloaded;
        private static Action _playtestStarted;
        private static Action _playtestEnded;

        /// <summary>True while a level editor manager exists (also during play-test).</summary>
        public static bool IsInEditor => EditorLevelEditorManager.GetLevelEditorInstance() != null;

        /// <summary>
        /// True while the editor's play-test preview is running: the active
        /// level manager is a play-mode one, but the editor session persists.
        /// </summary>
        public static bool IsPlaytesting
        {
            get
            {
                var level = BaseLevelManager.GetInstance();
                return level != null && !(level is EditorLevelEditorManager) && IsInEditor;
            }
        }

        /// <summary>True when the editor is the active surface (not play-testing).</summary>
        public static bool IsEditorActive => IsInEditor && !IsPlaytesting;

        private static void EnsurePatched()
        {
            if (_patched)
            {
                return;
            }
            _patched = true;
            PatchRegistry.EnsurePatched(typeof(LifecyclePatches));
        }

        private static void Fire(Action handler, string name)
        {
            if (handler == null)
            {
                return;
            }
            try
            {
                handler();
            }
            catch (Exception e)
            {
                Log.Error($"GameEvents.{name} subscriber threw: {e}");
            }
        }

        [HarmonyPatch]
        private static class LifecyclePatches
        {
            [HarmonyPatch(typeof(BaseLevelManager), "Awake")]
            [HarmonyPostfix]
            private static void LevelAwake(BaseLevelManager __instance)
            {
                Fire(_levelLoaded, nameof(LevelLoaded));
                if (__instance is EditorLevelEditorManager)
                {
                    Fire(_editorEntered, nameof(EditorEntered));
                }
                else if (IsInEditor)
                {
                    Fire(_playtestStarted, nameof(PlaytestStarted));
                }
            }

            // NOTE: EditorLevelEditorManager overrides OnDestroy WITHOUT calling
            // base.OnDestroy(), so the base-method patch below never sees the
            // editor manager — it needs its own patch.
            [HarmonyPatch(typeof(EditorLevelEditorManager), "OnDestroy")]
            [HarmonyPostfix]
            private static void EditorDestroy()
            {
                Fire(_levelUnloaded, nameof(LevelUnloaded));
                Fire(_editorExited, nameof(EditorExited));
            }

            [HarmonyPatch(typeof(BaseLevelManager), "OnDestroy")]
            [HarmonyPostfix]
            private static void LevelDestroy(BaseLevelManager __instance)
            {
                if (__instance is EditorLevelEditorManager)
                {
                    return; // handled by EditorDestroy
                }
                Fire(_levelUnloaded, nameof(LevelUnloaded));
                if (IsInEditor)
                {
                    Fire(_playtestEnded, nameof(PlaytestEnded));
                }
            }
        }
    }
}
