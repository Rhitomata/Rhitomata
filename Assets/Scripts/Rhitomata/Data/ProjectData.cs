using Newtonsoft.Json;

namespace Rhitomata.Data {
    public class ProjectData {
        // Information
        /// <summary>
        /// This project name can possibly mismatch with the folder name
        /// </summary>
        public string name { get; set; } = "Untitled";
        /// <summary>
        /// The creator's username
        /// </summary>
        public string author { get; set; } = "Unknown";

        /// <summary>
        /// The name of the music
        /// </summary>
        public string musicName { get; set; } = "None";
        /// <summary>
        /// The author/creator of the music
        public string musicAuthor { get; set; } = "Unknown";
        /// <summary>
        /// Relative audio path from the project
        /// </summary>
        public string musicPath { get; set; } = "audio.ogg";

        /// <summary>
        /// Absolute path of the project directory
        /// </summary>
        [JsonIgnore] public string directoryPath { get; set; }
        /// <summary>
        /// Absolute path of the project file
        /// </summary>
        [JsonIgnore] public string filePath { get; set; }

        public ProjectData() { }
        public ProjectData(string name, string author, string musicAuthor, string musicName) {
            this.name = name;
            this.author = author;
            this.musicAuthor = musicAuthor;
            this.musicName = musicName;
        }
    }
}