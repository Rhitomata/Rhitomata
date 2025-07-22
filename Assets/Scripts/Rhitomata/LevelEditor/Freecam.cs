using UnityEngine;
using UnityEngine.EventSystems;

namespace Rhitomata {
    public class Freecam : MonoBehaviour {
        [SerializeField]
        private Camera targetCamera;

        public float zMin = -50f;
        public float zMax = -10f;
        public float scrollIntensity = 15f;
        public bool naturalScrolling;
        public bool scrollToCursor = true;

        private bool _isDraggingCamera;
        private Vector3 _dragStartWorldPos;

        private void Update() {
            if (_isDraggingCamera) {
                if (Input.GetMouseButton(1) || Input.GetMouseButton(2)) {
                    var screenPos = Input.mousePosition;
                    screenPos.z = -targetCamera.transform.position.z;
                    var currentWorldPos = targetCamera.ScreenToWorldPoint(screenPos);

                    var offset = _dragStartWorldPos - currentWorldPos;
                    transform.position += offset;
                } else {
                    _isDraggingCamera = false;
                }

                if (Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2)) {
                    _isDraggingCamera = false;
                }
            }

            if (EventSystem.current && EventSystem.current.IsPointerOverGameObject())
                return;

            if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) {
                var screenPos = Input.mousePosition;
                screenPos.z = -targetCamera.transform.position.z; // Distance from camera to z=0
                _dragStartWorldPos = targetCamera.ScreenToWorldPoint(screenPos);
                _isDraggingCamera = true;
            }
            
            var scroll = Input.GetAxis("Mouse ScrollWheel") * (naturalScrolling ? 1f : -1f);
            if (!(Mathf.Abs(scroll) > 0.01f)) return;
            
            if (scrollToCursor) {
                var screenPos = Input.mousePosition;
                screenPos.z = -targetCamera.transform.position.z;
                var worldBeforeZoom = targetCamera.ScreenToWorldPoint(screenPos);

                var camPos = transform.position;
                camPos.z = Mathf.Clamp(camPos.z - scroll * scrollIntensity, zMin, zMax);
                transform.position = camPos;

                screenPos.z = -targetCamera.transform.position.z;
                var worldAfterZoom = targetCamera.ScreenToWorldPoint(screenPos);

                var offset = worldBeforeZoom - worldAfterZoom;
                transform.position += offset;
            } else {
                var position = transform.position;
                position.z = Mathf.Clamp(position.z - scroll, zMin, zMax);
                transform.position = position;
            }
        }

        void OnDisable() {   
            _isDraggingCamera = false;
        }
    }
}