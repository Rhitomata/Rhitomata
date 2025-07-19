using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Rhitomata.Timeline
{
    public class TimelineView : MonoBehaviour, IPointerDownHandler, IPointerMoveHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IEndDragHandler
    {
        [Header("Info")] 
        public float seekTime;
        public AudioSource source;
        public float negativeTimeTest = -1f;
        
        [Header("Dragging")]
        public TimelineSection section = TimelineSection.None;
        public Limit peekLimit = new Limit();
        
        [Header("Bounds")]
        public RectTransform timelineViewBounds;
        public RectTransform horizontalScrollBounds;

        [Header("Scrollbar")]
        public Scrollbar verticalScrollbar;
        public RectTransform horizontalPeekArea;
        public RectTransform horizontalPeekStart;
        public RectTransform horizontalPeekEnd;
        public RectTransform horizontalCurrentTime;

        // Input stuff
        private bool _isDragging = false;
        private PointerEventData _lastDragEventData;
        private Vector2 _dragStart, _dragEnd;
        private Vector2 _localPositionStart;
        private IList _draggedItems;

        private void Start()
        {
            peekLimit.min = -5f;
            peekLimit.max = source.clip.length + 5f;
        }

        #region Dragging
        public void OnDrag(PointerEventData eventData)
        {
            _isDragging = true;
            if (section == TimelineSection.None) return;

            _lastDragEventData = eventData;
            if (section == TimelineSection.CurrentTime)
            {
                
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (section != TimelineSection.None)
            {
                Debug.Log($"Drag Mode Update: {section} -> {TimelineSection.None}");
                section = TimelineSection.None;
            }
            _isDragging = false;
        }
        #endregion

        #region Pointers
        public void OnPointerDown(PointerEventData eventData)
        {
            if (section != TimelineSection.None) return;
            
            _isDragging = false;
            _lastDragEventData = eventData;
            var previousMode = section;

            var list = new Dictionary<RectTransform, TimelineSection>()
            {
                { horizontalCurrentTime, TimelineSection.CurrentTime },
                { horizontalPeekArea, TimelineSection.PeekRange },
                { horizontalPeekStart, TimelineSection.PeekStart },
                { horizontalPeekEnd, TimelineSection.PeekEnd }
            };

            foreach (var item in list)
            {
                if (!item.Key.ContainsPointer(eventData)) continue;
                
                section = item.Value;
                item.Key.ToLocalPos(eventData, out _dragStart);
                _localPositionStart = item.Key.anchoredPosition;
                break;
            }

            if (section == TimelineSection.CurrentTime)
            {
                source.time = negativeTimeTest;
            }
            
            if (previousMode != section)
                Debug.Log($"Drag Mode Update: {previousMode} -> {section}");
        }

        public void OnPointerMove(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (section != TimelineSection.None)
            {
                Debug.Log($"Drag Mode Update: {section} -> {TimelineSection.None}");
                section = TimelineSection.None;
            }
        }

        // Dunno if I'll ever do anything with hover interaction
        public void OnPointerEnter(PointerEventData eventData)
        {
        }

        public void OnPointerExit(PointerEventData eventData)
        {
        }
        #endregion
    }

    public enum TimelineSection
    {
        None = 0,

        CurrentTime = 2,
        PeekRange = 4,
        PeekStart = 6,
        PeekEnd = 8,

        TimelineDrag = 1,
        Timeline = 3,
        Select = 5,
        ItemDrag = 7,
    }

    [System.Serializable]
    public struct Limit
    {
        public float min;
        public float max;
        
        public Limit(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public static Limit none = new Limit(0, 0);
        public static Limit one = new Limit(0, 1);
    }
}