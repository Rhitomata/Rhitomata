using UnityEngine;
using UnityEngine.EventSystems;
using static Rhitomata.Useful;

namespace Rhitomata.Timeline {
    [RequireComponent(typeof(RectTransform))]
    public class KeyframeUI : MonoBehaviour, IPointerClickHandler, IDragHandler, IScrollHandler, IPointerDownHandler {
        public float time;

        private TimelineView timeline => References.Instance.timeline;
        private RectTransform rectTransform => transform as RectTransform;

        public void Initialize(float targetTime, int rowIndex) {
            time = targetTime;
            Set(targetTime, rowIndex);
        }

        public void Set(float targetTime, int rowIndex) {
            var x = timeline.GetX(targetTime);
            SetX(x);
            SetRowIndex(rowIndex);
        }

        public void SetX(float x) {
            rectTransform.anchoredPosition = new Vector2(x, rectTransform.anchoredPosition.y);
        }

        public void SetRowIndex(int rowIndex) {
            rowIndex = Mathf.Clamp(rowIndex, 0, timeline.laneHolder.childCount - 1);
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -(rowIndex * TimelineView.LANE_HEIGHT) - (TimelineView.LANE_HEIGHT / 2f));
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
            // Why does it work like that   
        }
    }
}
