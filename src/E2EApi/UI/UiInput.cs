using UnityEngine.EventSystems;
using UnityEngine.UI;

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

        /// <summary>
        /// True while an InputField has keyboard focus, so tools can skip
        /// hotkeys that would fire while the user is typing.
        /// </summary>
        public static bool IsTypingInInputField()
        {
            var es = EventSystem.current;
            if (es == null || es.currentSelectedGameObject == null)
            {
                return false;
            }
            var field = es.currentSelectedGameObject.GetComponent<InputField>();
            return field != null && field.isFocused;
        }
    }
}
