using Rhitomata.Data;
using UnityEngine;

namespace Rhitomata.Timeline {
    public class PointLane : TimelineLane {
        public override Keyframe CreateKeyframe(float time) {
            var modifyPoint = references.manager.project.CreateItem(time);
            references.manager.SpawnModifyPoint(modifyPoint);
            references.manager.project.AdjustAllPointFromPoint(modifyPoint);

            var keyframe = modifyPoint.keyframe as PointKeyframe;
            if (keyframe)
                return keyframe;

            // TODO: Read debug log error
            Debug.LogError("TODO: Fix spawning modify point doesn't correctly spawn the keyframe as well");
            return null;
        }

        public override void DestroyKeyframe(Keyframe item) {
            if (item is PointKeyframe keyframe) {
                keyframes.Remove(item);
                references.manager.project.RemoveItem(keyframe.modifyPoint);
                references.manager.project.AdjustAllPointFromIndex(references.manager.project.points.IndexOf(keyframe.modifyPoint) - 1);
            } else {
                base.DestroyKeyframe(item);
                Debug.LogError("A non-PointKeyframe keyframe is somehow placed on a PointLane!", gameObject);
            }
        }
    }
}