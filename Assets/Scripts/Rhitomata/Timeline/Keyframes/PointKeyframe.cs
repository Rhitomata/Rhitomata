using Rhitomata.Data;
using UnityEngine;

namespace Rhitomata.Timeline {
    public class PointKeyframe : Keyframe {
        public ModifyPoint modifyPoint;

        public override void SetTime(float targetTime) {
            if (modifyPoint == null) return;
            modifyPoint.time = targetTime;
        }
    }
}