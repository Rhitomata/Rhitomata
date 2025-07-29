using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Rhitomata.Timeline {
    public class TimelineLane : MonoBehaviour {
        public References references;

        /// <summary>
        /// <para>The header for the lane</para>
        /// </summary>
        public RectTransform header;

        public TMP_Text headerText;

        /// <summary>
        /// <para>The parent for the keyframes, we should be able to adjust it in the hierarchy and internally on <see cref="keyframes"/></para>
        /// </summary>
        public RectTransform keyframesParent;

        /// <summary>
        /// <para>Non-moving visual lane</para>
        /// </summary>
        public RectTransform visualLane;

        private TimelineView timeline => references.timeline;

        public float centerHeight;
        public List<Keyframe> keyframes = new();

        /// <summary>
        /// <para>This is called when a keyframe is created manually when left clicked on the timeline</para>
        /// </summary>
        public virtual Keyframe CreateKeyframe(float time) => CreateKeyframe<Keyframe>(time);

        /// <summary>
        /// <para>Creates a keyframe with the given type that inherits from <see cref="Keyframe"/></para>
        /// </summary>
        public T CreateKeyframe<T>(float time) where T : Keyframe {
            var obj = Instantiate(timeline.keyframePrefab, keyframesParent);
            var keyframe = obj.AddComponent<T>();
            keyframe.lane = this;
            keyframe.SetTime(time);

            Sort(keyframe);
            
            return keyframe;
        }

        /// <summary>
        /// <para>Sorts and inserts a single keyframe by time and then reorders
        /// all corresponding GameObjects in the Unity hierarchy.</para>
        /// </summary>
        public void Sort(Keyframe key) {
            if (keyframes.Contains(key))
                keyframes.Remove(key);

            var insertIndex = keyframes.BinarySearch(key,
                Comparer<Keyframe>.Create((kf1, kf2) => kf1.time.CompareTo(kf2.time)));
            if (insertIndex < 0)
                insertIndex = ~insertIndex;

            keyframes.Insert(insertIndex, key);
            key.transform.SetSiblingIndex(insertIndex);
        }

        /// <summary>
        /// <para>Sorts the entire List&lt;Keyframe&gt; by time and then reorders
        /// all corresponding GameObjects in the Unity hierarchy.</para>
        /// </summary>
        public void SortAll() {
            keyframes = keyframes.OrderBy(kf => kf.time).ToList();
            for (var i = 0; i < keyframes.Count; i++)
                keyframes[i].transform.SetSiblingIndex(i);
        }

        /// <summary>
        /// <para>This is called when a keyframe is destroyed manually when right clicked on the timeline</para>
        /// </summary>
        public virtual void DestroyKeyframe(Keyframe item) {
            keyframes.Remove(item);
            DestroyImmediate(item);
        }

        /// <summary>
        /// <para>Returns the keyframe at the given time, or null if there is no keyframe at the given time</para>
        /// </summary>
        public Keyframe GetKeyframeAtTime(float time) {
            for (var i = 0; i < keyframes.Count; i++) {
                if (time >= keyframes[i].time) continue;

                return i == 0 ? null : keyframes[i - 1];
            }

            return keyframes.Count == 0 ? null : keyframes[^1];
        }

        /// <summary>
        /// <para>Returns the index of the keyframe at the given time, or -1 if there is no keyframe at the given time</para>
        /// </summary>
        public int GetKeyframeIndexAtTime(float time) {
            for (var i = 0; i < keyframes.Count; i++) {
                if (time >= keyframes[i].time) continue;

                return i == 0 ? -1 : i - 1;
            }

            return keyframes.Count == 0 ? -1 : keyframes.Count - 1;
        }

        protected void GetKeyframeAround<T>(float time, out T previous, out T next) where T : Keyframe {
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