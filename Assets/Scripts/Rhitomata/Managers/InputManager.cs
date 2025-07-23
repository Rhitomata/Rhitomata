using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Rhitomata {
    public class InputManager : MonoBehaviour {

        /// <summary>
        /// Checks if mouse is over UI
        /// </summary>
        /// <param name="keyInt">The key code</param>
        /// <returns>True if mouse is over UI</returns>
        public static bool IsMouseAndTouchOverUI(int keyInt) {
            if (keyInt is >= 330 or <= 322) return false;

            var touches = Input.touches;
            return touches.Any(touch => EventSystem.current.IsPointerOverGameObject(touch.fingerId)) || EventSystem.current.IsPointerOverGameObject();
        }
        
        public static bool IsMouseOverUI(int fingerId = -1, bool detectTouches = false) {
            if (EventSystem.current.IsPointerOverGameObject(fingerId))
                return true;

            if (!detectTouches) return false;
            
            var touches = Input.touches;
            return touches.Any(touch => EventSystem.current.IsPointerOverGameObject(touch.fingerId)) || EventSystem.current.IsPointerOverGameObject();
        }

        public static bool IsEditingOnInputField() {
            var obj = EventSystem.current.currentSelectedGameObject;

            if (!obj) return false;
            if (obj.GetComponent<TMP_InputField>()) return true;
            if (obj.GetComponent<InputField>()) return true;

            return false;
        }

        /// <summary>
        /// Checks if the key code is a mouse and is over UI
        /// </summary>
        /// <param name="keyInt">The key code</param>
        /// <returns>True if mouse is over UI</returns>
        public static bool IsKeyCodeOverUI(int keyInt) {
            if (keyInt is >= 330 or <= 322) return false;
            return EventSystem.current.IsPointerOverGameObject();
        }

        /// <summary>
        /// Checks if the key code is a mouse and is over UI
        /// </summary>
        /// <param name="key">The key code</param>
        /// <returns>True if mouse is over UI</returns>
        public static bool IsKeyCodeOverUI(KeyCode key) => IsKeyCodeOverUI((int)key);
    }
}