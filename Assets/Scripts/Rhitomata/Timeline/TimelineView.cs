using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Rhitomata.Timeline {
    public class TimelineView : MonoBehaviour {
        [Header("Scrollbar")]
        public Scrollbar verticalScrollbar;
        public RectTransform horizontalPeekArea;
        public RectTransform horizontalPeekStart;
        public RectTransform horizontalPeekEnd;
        public RectTransform horizontalCurrentTime;
    }
}