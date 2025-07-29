using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rhitomata.Timeline {
    public class SeekView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
        private RectTransform _rt;
        private RectTransform rt => _rt = _rt ? _rt : transform as RectTransform;
        
        private TimelineView timeline => References.Instance.timeline;
        private bool _isDragging;

        private void Awake() {
            _rt = transform as RectTransform;
        }

        private void Update() {
            if (!_isDragging) return;

            if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1)) {
                _isDragging = false;
                return;
            }
            
            AdjustCursorToMousePosition();
        }

        public void OnPointerDown(PointerEventData eventData) {
            if (eventData.button != PointerEventData.InputButton.Left && eventData.button != PointerEventData.InputButton.Right) return;

            _isDragging = true;
            AdjustCursorToMousePosition();
        }

        public void OnPointerUp(PointerEventData eventData) {
            _isDragging = false;
        }

        private void AdjustCursorToMousePosition() {
            float time = GetLocalMousePosition().x / rt.rect.width * timeline.visibleRange.length + timeline.visibleRange.min;
            timeline.Seek(Mathf.Clamp(time, timeline.peekLimit.min, timeline.peekLimit.max));
            timeline.UpdateCurrentTimeCursor();
        }
        
        private Vector2 GetLocalMousePosition() {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, Input.mousePosition, null, out var mousePosRelative);
            mousePosRelative.y = rt.rect.height - mousePosRelative.y; // Invert down - top to top - down
            return mousePosRelative;
        }
    }
}