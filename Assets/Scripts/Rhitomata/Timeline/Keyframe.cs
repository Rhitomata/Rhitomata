using UnityEngine;
using UnityEngine.EventSystems;
using static Rhitomata.Useful;
    
namespace Rhitomata.Timeline {
    [RequireComponent(typeof(RectTransform))]
    public class Keyframe : MonoBehaviour, IPointerClickHandler, IDragHandler, IScrollHandler, IPointerDownHandler {
        public float time {  get; private set; }
        public TimelineLane lane;

        private TimelineView timeline => References.Instance.timeline;
        private RectTransform rectTransform => transform as RectTransform;

        public void Initialize(float targetTime, int rowIndex) {
            Set(targetTime, rowIndex);
        }
        
        public void Initialize(float targetTime, float yPosition) {
            SetTime(targetTime);
            rectTransform.anchoredPosition = new Vector2(timeline.GetX(targetTime), yPosition);
        }

        public void Set(float targetTime, int rowIndex) {
            SetTime(targetTime);
            SetRowIndex(rowIndex);
        }

        public void SetTime(float targetTime) {
            time = targetTime;// Only place where time should be set

            var x = timeline.GetX(targetTime);
            SetX(x);

            // Probably not the best way to get the point?
            var modifyPoint = References.Instance.manager.project.GetModifyPointAtTime(time);
            modifyPoint.time = targetTime;
        }

        public void SetRowIndex(int rowIndex) {
            rowIndex = Mathf.Clamp(rowIndex, 0, timeline.laneHolder.childCount - 1);
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -(rowIndex * TimelineView.LANE_HEIGHT) - (TimelineView.LANE_HEIGHT / 2f));
        }

        public void SetX(float x) {
            rectTransform.anchoredPosition = new Vector2(x, rectTransform.anchoredPosition.y);
        }

        public virtual void Delete() {
           lane?.DestroyKeyframe(this);
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
            SetTime(timeline.GetTime(rectTransform.anchoredPosition.x));
        }

        public void OnScroll(PointerEventData eventData) {
            timeline.laneView.OnScroll(eventData);// Passthrough
        }

        public void OnPointerDown(PointerEventData eventData) {
            // This method is needed, even if it's empty, otherwise LaneView handles it
            // - Why does it work like that
            // - because yes
        }
    }
}
