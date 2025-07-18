using UnityEngine;
using UnityEngine.EventSystems;

namespace Rhitomata
{
    public class Freecam : MonoBehaviour
    {
        [SerializeField]
        private Camera targetCamera;

        public float zMin = -50f;
        public float zMax = -10f;
        public float scrollIntensity = 15f;
        public bool naturalScrolling;

        private bool _isDraggingCamera;
        private Vector3 _dragStartWorldPos;

        void Update()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel") * (naturalScrolling ? 1f : -1f);
            if (Mathf.Abs(scroll) > 0.01f)
            {
                Vector3 screenPos = Input.mousePosition;
                screenPos.z = -targetCamera.transform.position.z;
                Vector3 worldBeforeZoom = targetCamera.ScreenToWorldPoint(screenPos);

                Vector3 camPos = transform.position;
                camPos.z = Mathf.Clamp(camPos.z - scroll * scrollIntensity, zMin, zMax);
                transform.position = camPos;

                screenPos.z = -targetCamera.transform.position.z;
                Vector3 worldAfterZoom = targetCamera.ScreenToWorldPoint(screenPos);

                Vector3 offset = worldBeforeZoom - worldAfterZoom;
                transform.position += offset;
            }

            if (_isDraggingCamera)
            {
                if (Input.GetMouseButton(1))
                {
                    Vector3 screenPos = Input.mousePosition;
                    screenPos.z = -targetCamera.transform.position.z;
                    Vector3 currentWorldPos = targetCamera.ScreenToWorldPoint(screenPos);

                    Vector3 offset = _dragStartWorldPos - currentWorldPos;
                    transform.position += offset;
                }
                else
                {
                    _isDraggingCamera = false;
                }

                if (Input.GetMouseButtonUp(1))
                {
                    _isDraggingCamera = false;
                }
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (Input.GetMouseButtonDown(1))
            {
                Vector3 screenPos = Input.mousePosition;
                screenPos.z = -targetCamera.transform.position.z; // Distance from camera to z=0
                _dragStartWorldPos = targetCamera.ScreenToWorldPoint(screenPos);
                _isDraggingCamera = true;
            }
        }

        void OnDisable()
        {
            _isDraggingCamera = false;
        }
    }
}