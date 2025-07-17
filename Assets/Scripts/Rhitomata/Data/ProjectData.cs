using Newtonsoft.Json;

namespace Rhitomata.Data {

    public class ProjectData {
        // Information
        public string name { get; set; } = "Untitled";
        public string author { get; set; } = "Unknown";

        public string musicAuthor { get; set; } = "Unknown";
        public string musicName { get; set; } = "None";

        public string musicFile { get; set; } = "audio.ogg";

        // Sequences
        public string[] sequenceFiles { get; set; } = {
            "easy.rseq",
            "hard.rseq",
            "insane.rseq"
        };

        [JsonIgnore] public string path { get; set; }

        public ProjectData(string name, string author, string musicAuthor, string musicName) {
            this.name = name;
            this.author = author;
            this.musicAuthor = musicAuthor;
            this.musicName = musicName;
        }
    }
}