using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Rhitomata.Timeline {
    public class TimelineView : MonoBehaviour {
        [Header("Dragging")]
        public DraggableHandle peekStart;
        public DraggableHandle peekRange;
        public DraggableHandle peekEnd;
    }

    [Serializable]
    public struct Limit {
        public float min;
        public float max;

        public Limit(float min, float max) {
            this.min = min;
            this.max = max;
        }

        public static Limit none = new Limit(0, 0);
        public static Limit one = new Limit(0, 1);
    }
}