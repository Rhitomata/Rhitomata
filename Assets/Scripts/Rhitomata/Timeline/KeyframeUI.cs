using Rhitomata;
using Rhitomata.Timeline;
using UnityEngine;
using UnityEngine.EventSystems;
using static Useful;

[RequireComponent(typeof(RectTransform))]
public class KeyframeUI : MonoBehaviour, IPointerClickHandler, IDragHandler, IScrollHandler, IPointerDownHandler {
    public float time;

    private TimelineView timeline => References.Instance.timeline;
    private RectTransform rectTransform;

    public void Initialize(float time, int rowIndex) {
        this.time = time;
        rectTransform = GetComponent<RectTransform>();

        Set(time, rowIndex);
    }

    public void Set(float time, int rowIndex) {
        var x = timeline.GetX(time);
        SetX(x);
        SetRowIndex(rowIndex);
    }

    public void SetX(float x) {
        rectTransform.anchoredPosition = new Vector2(x, rectTransform.anchoredPosition.y);
    }

    public void SetRowIndex(int rowIndex) {
        rowIndex = Mathf.Clamp(rowIndex, 0, timeline.laneHolder.childCount - 1);
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -(rowIndex * TimelineView.laneHeight) - (TimelineView.laneHeight / 2f));
    }

    public void Delete() {
        // TODO: Remove from a list that hasn't been made yet
        Destroy(gameObject);
    }

    public void OnPointerClick(PointerEventData eventData) {
        switch (eventData.button) {
            case PointerEventData.InputButton.Right:
                Delete();
                break;

            case PointerEventData.InputButton.Middle:
                timeline.laneView.OnDrag(eventData);// Passthrough
                break;
        }
    }

    public void OnDrag(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Left) {
            timeline.laneView.OnDrag(eventData);// Passthrough
            return;
        }

        var localDelta = GetLocalDelta(timeline.scrollingRect, eventData);
        rectTransform.anchoredPosition += new Vector2(localDelta.x, 0);
        time = timeline.GetTime(rectTransform.anchoredPosition.x);
    }

    public void OnScroll(PointerEventData eventData) {
        timeline.laneView.OnScroll(eventData);// Passthrough
    }

    public void OnPointerDown(PointerEventData eventData) {
        // This method is needed, even if its empty, otherwise LaneView handles it
    }
}
