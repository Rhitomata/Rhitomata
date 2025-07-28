using Rhitomata.Data;
using UnityEngine;

namespace Rhitomata.Timeline {
    public class TimingLane : TimelineLane {
        private ProjectData project => references.manager.project;
        
        public override Keyframe CreateKeyframe(float time) {
            var previousTiming = project.GetBPMIndexAtTime(time);
            var keyframe = CreateKeyframe<TimingKeyframe>(time);
            if (!keyframe) return null;
            
            keyframe.bpmInfo.time = time;
            keyframe.bpmInfo.bpm = project.bpms[previousTiming].bpm;
            keyframe.bpmInfo.divisionNumerator = project.bpms[previousTiming].divisionNumerator;
            
            project.AdjustPoints(project.GetModifyPointIndexAtTime(time));
            return keyframe;
        }

        public override void DestroyKeyframe(Keyframe item) {
            if (item is TimingKeyframe keyframe) {
                keyframes.Remove(item);
                project.bpms.Remove(keyframe.bpmInfo);
                project.AdjustPoints(project.GetModifyPointIndexAtTime(keyframe.time));
            } else {
                base.DestroyKeyframe(item);
                Debug.LogError($"A non-{nameof(TimingKeyframe)} keyframe is somehow placed on a {nameof(TimingLane)}!", gameObject);
            }
        }
        
        public TimingKeyframe CreatePredefinedKeyframe(BPMInfo bpmInfo) {
            var keyframe = CreateKeyframe<TimingKeyframe>(bpmInfo.time);
            if (!keyframe) return null;

            keyframe.bpmInfo = bpmInfo;
            return keyframe;
        }
    }
}