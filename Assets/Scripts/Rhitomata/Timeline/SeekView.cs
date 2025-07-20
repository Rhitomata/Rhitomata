using Rhitomata;
using Rhitomata.Timeline;
using UnityEngine;
using UnityEngine.EventSystems;
using static Useful;

public class SeekView : MonoBehaviour, IPointerDownHandler, IDragHandler {
    private TimelineView timeline => References.Instance.timeline;

    public void OnPointerDown(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Left && eventData.button != PointerEventData.InputButton.Right) return;

        timeline.cursorTime = Mathf.Clamp(timeline.GetTime(GetLocalPoint(timeline.scrollingRect, eventData.position).x), timeline.peekLimit.min, timeline.peekLimit.max);
        timeline.UpdateCurrentTimeCursor();
    }

    public void OnDrag(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Left && eventData.button != PointerEventData.InputButton.Right) return;

        timeline.cursorTime = Mathf.Clamp(timeline.GetTime(GetLocalPoint(timeline.scrollingRect, eventData.position).x), timeline.peekLimit.min, timeline.peekLimit.max);
        timeline.UpdateCurrentTimeCursor();
    }
}