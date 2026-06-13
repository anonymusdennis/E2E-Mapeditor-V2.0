using E2EApi.Editor.Patches;

namespace E2EApi.Editor
{
    /// <summary>Switches that change how the vanilla level editor reacts to input.</summary>
    public static class VanillaEditor
    {
        private static bool _mouseSuppressed;

        /// <summary>
        /// While true, the vanilla editor ignores all mouse-driven editing
        /// (placing, deleting, selecting, marquee, zones …) — keyboard camera
        /// movement keeps working. Used by mod tools that own the mouse,
        /// e.g. the electricity paint brush.
        /// </summary>
        public static bool MouseSuppressed
        {
            get => _mouseSuppressed;
            set
            {
                _mouseSuppressed = value;
                if (value)
                {
                    PatchRegistry.EnsurePatched(typeof(SuppressVanillaMousePatch));
                }
            }
        }

        /// <summary>Hide or show the vanilla block brush preview object.</summary>
        public static void SetBrushVisible(bool visible)
        {
            var controller = LevelEditor_Controller.GetInstance();
            if (controller != null && controller.m_Brush != null)
            {
                controller.m_Brush.SetActive(visible);
            }
        }

        /// <summary>
        /// Show/hide every canvas of the vanilla editor UI (palette, checklist,
        /// toolbars). Used to keep the editor chrome out of play-test previews;
        /// only the Canvas components are disabled, so no editor logic runs
        /// OnDisable and everything restores cleanly.
        /// </summary>
        public static void SetEditorUiVisible(bool visible)
        {
            var ui = LevelEditor_UIController.GetInstance();
            if (ui == null)
            {
                return;
            }
            foreach (var canvas in ui.transform.root
                .GetComponentsInChildren<UnityEngine.Canvas>(true))
            {
                canvas.enabled = visible;
            }
        }
    }
}
