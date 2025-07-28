using Rhitomata.Data;
using UnityEngine;

namespace Rhitomata.Timeline {
    public class TimingKeyframe : Keyframe {
        public override float time => bpmInfo.time;

        public BPMInfo bpmInfo = new();
        public ProjectData project => References.Instance.manager.project;

        public override void SetTime(float targetTime) {
            targetTime = targetTime < 0 ? 0 : targetTime;
            bpmInfo.time = targetTime;

            var bpmIndex = project.bpms.IndexOf(bpmInfo);
            
            var previousBpm = bpmIndex > 0 ? project.bpms[bpmIndex - 1] : null;
            var nextBpm = bpmIndex < project.bpms.Count - 1 ? project.bpms[bpmIndex + 1] : null;

            if (AdjustPoints(bpmIndex, previousBpm, nextBpm))
                base.SetTime(targetTime);
        }

        private bool AdjustPoints(int bpmIndex, BPMInfo previousBpm, BPMInfo nextBpm) {
            if (bpmInfo.time > nextBpm.time) {
                project.bpms.Insert(bpmIndex + 2, bpmInfo);
                project.bpms.RemoveAt(bpmIndex);
            } else if (bpmInfo.time < previousBpm.time) {
                project.bpms.Insert(bpmIndex - 2, bpmInfo);
                project.bpms.RemoveAt(bpmIndex);
            } else {
                if (Mathf.Approximately(bpmInfo.time, previousBpm.time)) return false;
                if (Mathf.Approximately(bpmInfo.time, nextBpm.time)) return false;
            }

            return true;
        }
    }
}