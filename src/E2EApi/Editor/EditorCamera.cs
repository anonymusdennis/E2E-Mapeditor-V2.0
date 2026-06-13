using UnityEngine;

namespace E2EApi.Editor
{
    /// <summary>Editor camera helpers: extended zoom range, jump-to-tile, edge-pan lock.</summary>
    public static class EditorCamera
    {
        private static LevelEditor_Controller _extendedFor;
        private static LevelEditor_Controller _lockAppliedTo;
        private static float _savedEdgeDistance = -1f;
        private static bool _lockPan;

        /// <summary>
        /// When true, the camera no longer auto-pans while the mouse approaches
        /// the screen edges; only the map-move keys (WASD/arrows) move it.
        /// Works by zeroing <c>m_fCameraPanEdgeDistance</c> on the live controller.
        /// </summary>
        public static bool LockPan
        {
            get => _lockPan;
            set
            {
                _lockPan = value;
                ApplyLock();
            }
        }

        /// <summary>Re-applies the pan lock; call when a new editor session starts.</summary>
        public static void ApplyLock()
        {
            var controller = LevelEditor_Controller.GetInstance();
            if (controller == null)
            {
                _lockAppliedTo = null;
                return;
            }
            if (_lockPan)
            {
                if (_lockAppliedTo != controller)
                {
                    _savedEdgeDistance = controller.m_fCameraPanEdgeDistance;
                    _lockAppliedTo = controller;
                }
                controller.m_fCameraPanEdgeDistance = 0f;
            }
            else if (_lockAppliedTo == controller && _savedEdgeDistance >= 0f)
            {
                controller.m_fCameraPanEdgeDistance = _savedEdgeDistance;
                _lockAppliedTo = null;
            }
        }

        /// <summary>
        /// Vanilla rewrites edge-pan distance every frame; call each tick while locked.
        /// </summary>
        public static void MaintainLock()
        {
            if (!_lockPan)
            {
                return;
            }
            var controller = LevelEditor_Controller.GetInstance();
            if (controller == null)
            {
                return;
            }
            if (_lockAppliedTo != controller)
            {
                _savedEdgeDistance = controller.m_fCameraPanEdgeDistance;
                _lockAppliedTo = controller;
            }
            if (controller.m_fCameraPanEdgeDistance != 0f)
            {
                controller.m_fCameraPanEdgeDistance = 0f;
            }
        }

        /// <summary>
        /// Adds extra zoom-in steps below the vanilla minimum (each step halves
        /// the orthographic size). Safe to call repeatedly; runs once per editor
        /// session.
        /// </summary>
        public static void ExtendZoom(int extraSteps = 2)
        {
            var controller = LevelEditor_Controller.GetInstance();
            if (controller == null || controller == _extendedFor || extraSteps <= 0)
            {
                return;
            }
            var levels = controller.m_ZoomLevels;
            if (levels == null || levels.Length == 0)
            {
                return;
            }
            var extended = new float[levels.Length + extraSteps];
            float smallest = levels[0];
            // index 0 = most zoomed in; prepend halved orthographic sizes
            for (int i = 0; i < extraSteps; i++)
            {
                extended[i] = smallest / Mathf.Pow(2f, extraSteps - i);
            }
            for (int i = 0; i < levels.Length; i++)
            {
                extended[extraSteps + i] = levels[i];
            }
            controller.m_ZoomLevels = extended;
            controller.m_CurrentZoomLevel += extraSteps;
            _extendedFor = controller;
            Log.Info($"editor zoom extended by {extraSteps} steps (min ortho {extended[0]:0.##})");
        }

        /// <summary>Centre the editor camera on a tile.</summary>
        public static bool JumpTo(int x, int y)
        {
            var controller = LevelEditor_Controller.GetInstance();
            if (controller == null || controller.m_MainCamera == null)
            {
                return false;
            }
            var cam = controller.m_MainCamera.transform;
            var target = new Vector3(x, y, cam.localPosition.z);
            cam.localPosition = controller.GetCorrectedPosition(target);
            return true;
        }
    }
}
