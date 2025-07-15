using Runtime2DTransformInteractor;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rhitomata
{
    /// <summary>
    /// Having new input system mixed with old input system is very confusing actually
    /// </summary>
    public class InputManager : MonoBehaviour {
        public TransformInteractorController transformController;
        private bool wasHovering;

        private void Update() {
            bool isOverUI = EventSystem.current.IsPointerOverGameObject();
            if (wasHovering == isOverUI) {
                if (isOverUI) {
                    transformController.DehoverAll();
                    TransformInteractorController.instance.SetDefaultMouseCursor();
                }
                wasHovering = !isOverUI;
            }
        }

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

        /// <summary>
        /// Checks if mouse is over UI
        /// </summary>
        /// <param name="keyInt">The key code</param>
        /// <returns>True if mouse is over UI</returns>
        public static bool IsMouseOverUI(int keyInt) {
            if (keyInt is >= 330 or <= 322) return false;
            return EventSystem.current.IsPointerOverGameObject();
        }
    }
}