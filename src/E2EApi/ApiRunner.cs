using UnityEngine;

namespace E2EApi
{
    /// <summary>
    /// Hidden persistent MonoBehaviour that gives the API a per-frame tick
    /// (limitation enforcement, future event polling).
    /// </summary>
    internal class ApiRunner : MonoBehaviour
    {
        private static ApiRunner _instance;

        public static void Ensure()
        {
            if (_instance != null)
            {
                return;
            }
            var go = new GameObject("E2EApiRunner");
            Object.DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideAndDontSave;
            _instance = go.AddComponent<ApiRunner>();
            Log.Debug("ApiRunner created");
        }

        /// <summary>Run a coroutine on the API's persistent host object.</summary>
        public static Coroutine StartRoutine(System.Collections.IEnumerator routine)
        {
            Ensure();
            return _instance.StartCoroutine(routine);
        }

        private void Update()
        {
            MainThread.Drain();
            Editor.Limits.Enforce();
            Features.ElectricFences.Tick();
            Features.FenceOverlay.Tick();
            Features.XRay.Tick();
            Features.TriggerArrows.Tick();
            Features.ModTileOverlay.Tick();
        }
    }
}
