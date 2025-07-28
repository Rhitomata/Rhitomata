using UnityEngine;
using UnityEngine.EventSystems;
using static Rhitomata.Useful;

namespace Rhitomata.Timeline {
    public class SeekView : MonoBehaviour, IPointerDownHandler, IDragHandler {
        private TimelineView timeline => References.Instance.timeline;

        public void OnPointerDown(PointerEventData eventData) {
            if (eventData.button != PointerEventData.InputButton.Left && eventData.button != PointerEventData.InputButton.Right) return;

            timeline.Seek(Mathf.Clamp(timeline.GetTime(GetLocalPoint(transform, eventData.position).x), timeline.peekLimit.min, timeline.peekLimit.max));
            timeline.UpdateCurrentTimeCursor();
        }

        public void OnDrag(PointerEventData eventData) {
            if (eventData.button != PointerEventData.InputButton.Left && eventData.button != PointerEventData.InputButton.Right) return;

            timeline.Seek(Mathf.Clamp(timeline.GetTime(GetLocalPoint(transform, eventData.position).x), timeline.peekLimit.min, timeline.peekLimit.max));
            timeline.UpdateCurrentTimeCursor();
        }
    }
}