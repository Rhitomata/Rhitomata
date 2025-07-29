using UnityEngine;
using UnityEngine.EventSystems;
using static Rhitomata.Useful;
    
namespace Rhitomata.Timeline {
    [RequireComponent(typeof(RectTransform))]
    public class Keyframe : MonoBehaviour, IPointerClickHandler, IDragHandler, IScrollHandler {
        public virtual float time {  get; private set; }
        public TimelineLane lane;

        private TimelineView timeline => References.Instance.timeline;
        private RectTransform rectTransform => transform as RectTransform;

        public virtual void SetTime(float targetTime) {
            time = targetTime;

            var x = timeline.GetX(targetTime);
            SetX(x);
            lane.Sort(this);
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

            var localDelta = GetLocalDelta(timeline.laneView.transform, eventData);
            rectTransform.anchoredPosition += new Vector2(localDelta.x, 0);
            SetTime(timeline.GetTime(rectTransform.anchoredPosition.x));
        }

        public void OnScroll(PointerEventData eventData) {
            timeline.laneView.OnScroll(eventData);
        }
    }
}
