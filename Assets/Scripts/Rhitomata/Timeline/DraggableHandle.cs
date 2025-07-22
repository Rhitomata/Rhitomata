using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Rhitomata.Useful;

namespace Rhitomata.Timeline {
    public class DraggableHandle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
        public Image image;
        public bool changeBasedOnMaterial = true;
        public Color hoveredColor = Color.gray;
        public Color pressedColor = Color.white;
        public Color normalColor = Color.black;

        private RectTransform _rectTransform;
        public RectTransform rectTransform => _rectTransform ? _rectTransform : transform as RectTransform;
        public DraggableEvent onStartDrag;
        public DraggableEvent onDrag;
        public DraggableEvent onMoved;
        public DraggableEvent onEndDrag;
        public DraggableDeltaEvent onDragDelta;

        private bool _isHovered;
        private bool _isPressed;
        private bool _isDragging;

        private void Start() {
            if (!changeBasedOnMaterial) return;

            normalColor = image.material.color;
            image.material = null;
            image.color = normalColor;
        }

        public void OnPointerEnter(PointerEventData eventData) {
            _isHovered = true;
            UpdateColor(eventData);
        }

        public void OnPointerExit(PointerEventData eventData) {
            _isHovered = false;
            UpdateColor(eventData);
        }

        public void OnPointerDown(PointerEventData eventData) {
            _isPressed = true;
            UpdateColor(eventData);
        }

        public void OnPointerUp(PointerEventData eventData) {
            _isPressed = false;
            UpdateColor(eventData);
            FinishDrag(eventData);
        }

        public void OnPointerMove(PointerEventData eventData) {
            if (_isPressed) {
                onMoved?.Invoke(eventData);
            }
        }

        public void OnBeginDrag(PointerEventData eventData) {
            _isDragging = true;
            onStartDrag?.Invoke(eventData);
        }

        public void OnEndDrag(PointerEventData eventData) => FinishDrag(eventData);
        public void OnDrag(PointerEventData eventData) {
            _isDragging = true;
            onDrag?.Invoke(eventData);
            onDragDelta?.Invoke(GetLocalDelta(transform, eventData));
        }

        public void FinishDrag(PointerEventData eventData) {
            if (!_isDragging) return;
            _isDragging = false;

            onEndDrag?.Invoke(eventData);
        }

        private void UpdateColor(PointerEventData eventData) {
            if (eventData.dragging && eventData.pointerDrag != gameObject) {
                image.color = normalColor;
                return;
            }

            if (_isPressed) {
                image.color = pressedColor;
            } else if (_isHovered) {
                image.color = hoveredColor;
            } else {
                image.color = normalColor;
            }
        }

        [System.Serializable]
        public class DraggableEvent : UnityEvent<PointerEventData> { }

        [System.Serializable]
        public class DraggableDeltaEvent : UnityEvent<Vector2> { }
    }
}