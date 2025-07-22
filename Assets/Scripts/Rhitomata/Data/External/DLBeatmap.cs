using System.Collections.Generic;

namespace Rhitomata.Data {
    /// <summary>
    /// A custom format developed to read a converted osu! map by a custom program
    /// </summary>
    public class DLBeatmap {
        public List<DLBeatmapBpm> bpms { get; set; } = new();
        public List<DLBeatmapTiming> timings { get; set; } = new();
    }

    public class DLBeatmapBpm {
        public float length { get; set; }
        public float offset { get; set; }
    }

    public class DLBeatmapTiming {
        public float time { get; set; }
        public float lane { get; set; }
    }
}
