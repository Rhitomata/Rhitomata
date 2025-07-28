using UnityEngine;
using UnityEngine.EventSystems;
using static Rhitomata.Useful;

namespace Rhitomata.Timeline {
    public class LaneView : MonoBehaviour, IDragHandler, IScrollHandler {
        private TimelineView timeline => References.Instance.timeline;
        private float zoomLevel => timeline.visibleRange.length;

        public Vector2 panSensitivity;
        public float zoomSensitivity;

        // TODO: Use pointer down, save the pointer position, and when it moved, move the whole thing based on that I guess? For the middle button/panning at least
        public void OnDrag(PointerEventData eventData) {
            switch (eventData.button) {
                case PointerEventData.InputButton.Right:
                    timeline.Seek(timeline.GetTime(GetLocalPoint(transform, eventData.position).x));
                    timeline.UpdateCurrentTimeCursor();
                    break;

                case PointerEventData.InputButton.Middle:
                    var reverseModifier = Input.GetKey(KeyCode.LeftShift);
                    var moreModifier = Input.GetKey(KeyCode.LeftControl);

                    var delta = GetLocalDelta(transform, eventData);
                    delta *= moreModifier ? 4f : 1f;
                    timeline.horizontalScrollbar.OnRangeMoved((reverseModifier ? delta : -delta) * panSensitivity.x * zoomLevel);
                    timeline.verticalScrollbar.value = Mathf.Clamp01(timeline.verticalScrollbar.value + ((reverseModifier ? -delta.y : delta.y) * panSensitivity.y));
                    break;
            }
        }

        public void OnScroll(PointerEventData eventData) {
            var moreModifier = Input.GetKey(KeyCode.LeftControl);
            var lessModifier = Input.GetKey(KeyCode.LeftShift);

            var delta = eventData.scrollDelta.y * zoomLevel * zoomSensitivity;
            delta *= moreModifier ? 4 : lessModifier ? 0.25f : 1;// yes
            timeline.horizontalScrollbar.OnStartRangeMoved(new(delta, 0));
            timeline.horizontalScrollbar.OnEndRangeMoved(new(-delta, 0));

            // Laptop touchpad scrolling
            var deltaPan = eventData.scrollDelta.x * zoomLevel * panSensitivity.x * 10f;
            timeline.horizontalScrollbar.OnRangeMoved(new(deltaPan, 0));
        }
    }
}
