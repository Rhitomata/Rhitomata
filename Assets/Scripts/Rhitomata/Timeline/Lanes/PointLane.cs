using Rhitomata.Data;
using UnityEngine;

namespace Rhitomata.Timeline {
    public class PointLane : TimelineLane {
        public override Keyframe CreateKeyframe(float time) {
            var keyframe = CreateKeyframe<PointKeyframe>(time);
            keyframe.modifyPoint = references.manager.project.CreateItem(time);
            references.manager.SpawnModifyPoint(keyframe.modifyPoint);
            return keyframe;
        }

        public override void DestroyKeyframe(Keyframe item) {
            if (item is PointKeyframe keyframe) {
                keyframes.Remove(item);
                references.manager.project.RemoveItem(keyframe.modifyPoint);
            } else {
                base.DestroyKeyframe(item);
                Debug.LogError("A non-PointKeyframe keyframe is somehow placed on a PointLane!", gameObject);
            }
        }
    }
}