using Rhitomata;
using Rhitomata.Timeline;
using UnityEngine;
using UnityEngine.EventSystems;
using static Useful;

[RequireComponent(typeof(RectTransform))]
public class KeyframeUI : MonoBehaviour, IPointerClickHandler, IDragHandler {
    public float time;

    private RectTransform rectTransform;

    public void Initialize(float time, int rowIndex) {
        this.time = time;
        rectTransform = GetComponent<RectTransform>();

        Set(time, rowIndex);
    }

    public void Set(float time, int rowIndex) {
        var x = References.Instance.timeline.GetX(time);
        SetX(x);
        SetRowIndex(rowIndex);
    }

    public void SetX(float x) {
        rectTransform.anchoredPosition = new Vector2(x, rectTransform.anchoredPosition.y);
    }

    public void SetRowIndex(int rowIndex) {
        rowIndex = Mathf.Clamp(rowIndex, 0, References.Instance.timeline.laneHolder.childCount - 1);
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -(rowIndex * TimelineView.laneHeight) - (TimelineView.laneHeight / 2f));
    }

    public void Delete() {
        // TODO: remove from list that hasn't been made yet
        Destroy(gameObject);
    }

    public void OnPointerClick(PointerEventData eventData) {
        switch (eventData.button) {
            case PointerEventData.InputButton.Right:
                Delete();
                break;
        }
    }

    public void OnDrag(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        var localDelta = GetLocalDelta(References.Instance.timeline.scrollingRect, eventData);
        rectTransform.anchoredPosition += new Vector2(localDelta.x, 0);
        time = References.Instance.timeline.GetTime(rectTransform.anchoredPosition.x);
    }
}
