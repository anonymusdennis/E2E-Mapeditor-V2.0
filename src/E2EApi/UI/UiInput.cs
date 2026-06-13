using UnityEngine.EventSystems;

namespace E2EApi.UI
{
    /// <summary>Helpers for mod uGUI eating editor mouse input.</summary>
    public static class UiInput
    {
        /// <summary>True when the pointer is over any active uGUI raycast target.</summary>
        public static bool IsPointerOverUi()
        {
            var es = EventSystem.current;
            return es != null && es.IsPointerOverGameObject();
        }
    }
}
