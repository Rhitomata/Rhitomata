using System.Collections.Generic;
using UnityEngine;

namespace Rhitomata.Timeline {
    public class TimelineLane : MonoBehaviour {
        public References references;

        private TimelineView timeline => references.timeline;
        
        public float centerHeight;
        public List<Keyframe> keyframes = new();

        /// <summary>
        /// This is called when a keyframe is created manually when left clicked on the timeline
        /// </summary>
        public virtual Keyframe CreateKeyframe(float time) => CreateKeyframe<Keyframe>(time);
        public T CreateKeyframe<T>(float time) where T : Keyframe {
            var obj = Instantiate(timeline.keyframePrefab, timeline.scrollingRect);
            var keyframe = obj.AddComponent<T>();
            keyframe.Initialize(time, centerHeight);
            
            var index = GetKeyframeIndexAtTime(time);
            keyframes.Insert(index + 1, keyframe);

            return keyframe;
        }

        /// <summary>
        /// This is called when a keyframe is destroyed manually when right clicked on the timeline
        /// </summary>
        public virtual void DestroyKeyframe(Keyframe item) {
            keyframes.Remove(item);
            DestroyImmediate(item);
        }

        public Keyframe GetKeyframeAtTime(float time) {
            for (var i = 0; i < keyframes.Count; i++) {
                if (time >= keyframes[i].time) continue;
                
                return i == 0 ? null : keyframes[i - 1];
            }
            return keyframes.Count == 0 ? null : keyframes[^1];
        }
        
        public int GetKeyframeIndexAtTime(float time) {
            for (var i = 0; i < keyframes.Count; i++) {
                if (time >= keyframes[i].time) continue;
                
                return i == 0 ? -1 : i - 1;
            }
            return keyframes.Count == 0 ? -1 : keyframes.Count - 1;
        }
        
        protected void GetKeyframeAround<T>(float time, out T previous, out T next) where T : Keyframe
        {
            previous = null;
            next = null;

            if (keyframes == null || keyframes.Count == 0)
                return;

            var index = GetKeyframeIndexAtTime(time);
            if (index >= 0 && index < keyframes.Count)
                previous = keyframes[index] as T;

            if (index + 1 < keyframes.Count)
                next = keyframes[index + 1] as T;
        }
        // TODO: Add functions to call when time is changed and interpolate or interpret the keyframes
        
        
        // Just in case we want to use a faster search algorithm
        // public Keyframe GetKeyframeAtTimeBinarySearch(float time)
        // {
        //     if (keyframes.Count == 0) return null;
        //
        //     var left = 0;
        //     var right = keyframes.Count - 1;
        //
        //     while (left <= right)
        //     {
        //         var mid = (left + right) / 2;
        //         var midTime = keyframes[mid].time;
        //
        //         if (time < midTime)
        //         {
        //             right = mid - 1;
        //         }
        //         else
        //         {
        //             left = mid + 1;
        //         }
        //     }
        //
        //     // After the loop, right points to the last keyframe with time <= given time
        //     return right >= 0 ? keyframes[right] : null;
        // }
        
        // public int GetKeyframeIndexAtTime(float time)
        // {
        //     if (keyframes.Count == 0) return -1;
        //
        //     int left = 0;
        //     int right = keyframes.Count - 1;
        //
        //     while (left <= right)
        //     {
        //         int mid = (left + right) / 2;
        //         float midTime = keyframes[mid].time;
        //
        //         if (time < midTime)
        //         {
        //             right = mid - 1;
        //         }
        //         else
        //         {
        //             left = mid + 1;
        //         }
        //     }
        //
        //     // After the loop, 'right' is the last index where keyframes[right].time <= time
        //     return right >= 0 ? right : -1;
        // }
    }
}