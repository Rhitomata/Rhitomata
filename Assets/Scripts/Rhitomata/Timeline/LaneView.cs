using UnityEngine;
using UnityEngine.EventSystems;
using static Rhitomata.Useful;

namespace Rhitomata.Timeline {
    public class LaneView : MonoBehaviour, IPointerClickHandler, IDragHandler, IScrollHandler, IPointerDownHandler {
        private TimelineView timeline => References.Instance.timeline;
        private float zoomLevel => timeline.visibleRange.length;

        public Vector2 panSensitivity;
        public float zoomSensitivity;

        public void OnPointerClick(PointerEventData eventData) {
            if (eventData.button != PointerEventData.InputButton.Left) return;

            timeline.CreateKeyframeAtCursor();
        }

        public void OnPointerDown(PointerEventData eventData) {
            if (eventData.button != PointerEventData.InputButton.Right) return;

            timeline.cursorTime = timeline.GetTime(GetLocalPoint(timeline.scrollingRect, eventData.position).x);
            timeline.UpdateCurrentTimeCursor();
        }

        public void OnDrag(PointerEventData eventData) {
            switch (eventData.button) {
                case PointerEventData.InputButton.Right:
                    timeline.cursorTime = timeline.GetTime(GetLocalPoint(timeline.scrollingRect, eventData.position).x);
                    timeline.UpdateCurrentTimeCursor();
                    break;

                case PointerEventData.InputButton.Middle:
                    bool reverseModifier = Input.GetKey(KeyCode.LeftShift);
                    bool moreModifier = Input.GetKey(KeyCode.LeftControl);

                    var delta = GetLocalDelta(timeline.scrollingRect, eventData);
                    delta *= moreModifier ? 4f : 1f;
                    timeline.horizontalScrollbar.OnRangeMoved((reverseModifier ? delta : -delta) * panSensitivity.x * zoomLevel);
                    timeline.verticalScrollbar.value = Mathf.Clamp01(timeline.verticalScrollbar.value + ((reverseModifier ? -delta.y : delta.y) * panSensitivity.y));
                    break;
            }
        }

        public void OnScroll(PointerEventData eventData) {
            bool moreModifier = Input.GetKey(KeyCode.LeftControl);
            bool lessModifier = Input.GetKey(KeyCode.LeftShift);

            var delta = eventData.scrollDelta.y * zoomLevel * zoomSensitivity;
            delta *= moreModifier ? 4 : lessModifier ? 0.25f : 1;// yes
            timeline.horizontalScrollbar.OnStartRangeMoved(new Vector2(delta, 0));
            timeline.horizontalScrollbar.OnEndRangeMoved(new Vector2(-delta, 0));

            // Laptop touchpad scrolling (goddayum you thought about this too Sylva?)
            // - lol yeah, since scrollDelta.x exists, why not?
            var deltaPan = eventData.scrollDelta.x * zoomLevel * panSensitivity.x * 10f;
            timeline.horizontalScrollbar.OnRangeMoved(new Vector2(deltaPan, 0));
            //timeline.horizontalScrollbar.OnRangeMoved(eventData.scrollDelta);
        }
    }
}
