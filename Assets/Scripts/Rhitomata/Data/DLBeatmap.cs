using System.Collections.Generic;

namespace Rhitomata.Data {
    public class DLBeatmap {
        public List<DLBeatmapBPM> bpms { get; set; } = new();
        public List<DLBeatmapTiming> timings { get; set; } = new();
    }

    public class DLBeatmapBPM {
        public float length { get; set; }
        public float offset { get; set; }
    }

    public class DLBeatmapTiming {
        public float time { get; set; }
        public float lane { get; set; }
    }
}
