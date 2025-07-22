using System.IO;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Rhitomata.Data;
using SFB;
using static Rhitomata.Useful;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Rhitomata.Editor")]
namespace Rhitomata {
    public class LevelManager : InstanceManager<ObjectSerializer> {
        [Header("References")]
        public References references;
        [SerializeField] private KeyCode switchStateKey = KeyCode.Tab;
        [SerializeField] private CanvasGroup editorUI;

        [Header("Data")]
        public ProjectData project = new();

        [Header("States")]
        public State state;
        public bool debug;

        [Header("Prefabs & Environments")] 
        public Transform game;
        public GameObject spritePrefab;

        private float desyncThreshold = 0.3f;// TODO: Make customizable from UI maybe?
        public float time;

        private void Start() {
            ChangeState(State.Edit);
        }

        private void Update() {
            if (references.manager.state == State.Play) {
                if (!references.music.isPlaying) references.music.Play();
                time += Time.deltaTime;

                var musicPlayer = references.music;
                if (musicPlayer.clip && (musicPlayer.time > time + desyncThreshold || musicPlayer.time < time - desyncThreshold)) musicPlayer.time = Mathf.Clamp(time, 0, musicPlayer.clip.length);
            } else {
                if (references.music.isPlaying) references.music.Stop();
            }

            if (debug) {
                if (Input.GetKeyDown(KeyCode.R))
                    Restart();
            }

            if (Input.GetKeyDown(switchStateKey))
                ChangeState(state == State.Play ? State.Edit : State.Play);
        }

        public void Restart() {
            time = 0;
            references.cameraMovement.transform.localPosition = new Vector3(0, 0, -10);
            references.player.ResetAll();
            ChangeState(State.Edit);

            // TODO: Reset decorations as well, probably only possible when we have a proper serialization system
        }

        public void ChangeState(State newState) {
            state = newState;
            switch (newState) {
                case State.Play:
                    editorUI.gameObject.SetActive(false);
                    break;

                case State.Edit:
                    editorUI.gameObject.SetActive(true);
                    break;
            }
        }

        #region Objects
        public SpriteObject SpawnSpriteObject(Sprite sprite) {
            var spriteObject = Instantiate(spritePrefab, game).GetComponent<SpriteObject>();
            spriteObject.transform.position = Vector3.zero;
            spriteObject.Initialize(sprite, lastInstanceId + 1);
            return spriteObject;
        }
        #endregion

        #region UI

        private const string FILE_EXTENSION = "rhito";// TODO: Use the file extensions below instead, once we have the storage/structure figured out

        /// <summary>
        /// This file format is just a renamed .zip and contains all the files needed to be able to play a level.
        /// </summary>
        private const string FILE_EXTENSION_EDIT = "rhitomata";

        /// <summary>
        /// This file format includes compressed data to play a level.
        /// </summary>
        private const string FILE_EXTENSION_PLAY = "rhitoplay";

        public void BrowseToSaveProject() {
            var extensions = new[]
            {
                new ExtensionFilter("Rhitomata Project", FILE_EXTENSION),
                new ExtensionFilter("All Files", "*"),
            };
            var path = StandaloneFileBrowser.SaveFilePanel("Save project", ProjectList.projectsDir, "New Project", extensions);
            if (string.IsNullOrEmpty(path)) return; // Cancelled

            SaveProject(path);
        }

        public void BrowseForProject() {
            var extensions = new[]
            {
                new ExtensionFilter("Rhitomata Project", FILE_EXTENSION),
                new ExtensionFilter("All Files", "*"),
            };
            var result = StandaloneFileBrowser.OpenFilePanel("Load project", ProjectList.projectsDir, extensions, false);
            if (result == null || result.Length == 0) return; // Cancelled

            var path = result[0];

            LoadProject(path);
        }

        public void BrowseForSong() {
            var extensions = new[]
            {
                new ExtensionFilter("Sound Files", "ogg", "mp3", "wav"),
                new ExtensionFilter("All Files", "*"),
            };
            var result = StandaloneFileBrowser.OpenFilePanel("Import song", "", extensions, false);
            if (result == null || result.Length == 0) return; // Cancelled

            var path = result[0];

            StartCoroutine(ImportSong(path));
        }

        public void BrowseForBeatmap() {
            var extensions = new[]
            {
                new ExtensionFilter("Text Files", "txt"),
                new ExtensionFilter("All Files", "*"),
            };
            var result = StandaloneFileBrowser.OpenFilePanel("Import beatmap", "", extensions, false);
            if (result == null || result.Length == 0) return; // Cancelled

            var path = result[0];

            // Set beatmap
        }

        public void Exit() {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
            Application.Quit();
        }
        #endregion UI

        #region Project
        public void CreateProject(ProjectData data) {
            Storage.CheckDirectory(data.directoryPath);
            Storage.WriteAllText(data.directoryPath.Combine("project.json"), RhitomataSerializer.Serialize(data));
            LoadProject(data.directoryPath);
        }

        public void LoadProject(string directoryPath) {
            // TODO: Load objects, music, etc. Use UniTask instead of Coroutines because I like async
            var content = Storage.ReadAllText(directoryPath.Combine("project.json"));
            if (string.IsNullOrWhiteSpace(content)) {
                Debug.LogWarning("The project file has no data, cancel loading the project.");
                return;
            }

            try {
                var projectData = RhitomataSerializer.Deserialize<ProjectData>(content);
                if (projectData == null) {
                    Debug.LogWarning("The project failed to load while reading the data, unspecified error.");
                    return;
                }

                project = projectData;
                print($"Loaded project {project.name} by {project.author}");

                // TODO: Load data in these orders: song, beatmap, timeline items & indicators, assets, objects
            } catch (System.Exception exception) {
                Debug.LogException(exception);
            }
        }

        public void SaveProject(string directoryPath) {

        }
        #endregion Project

        /// <summary>
        /// This does not copy the song to the proper path
        /// </summary>
        /// <param name="path">Absolute path of the song</param>
        private IEnumerator ImportSong(string path) {
            if (project == null) {
                Debug.LogWarning("There's no project open yet, but a song is trying to be imported anyway!");
                yield break;
            }
            if (string.IsNullOrEmpty(path)) {
                Debug.LogWarning("An error occurred while importing the song: Empty path");
                yield break;
            }

            using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.UNKNOWN);

            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success) {
                Debug.LogWarning($"An error occurred while importing the song: {www.error}");
                yield break;
            }

            var myClip = DownloadHandlerAudioClip.GetContent(www);
            if (myClip == null) {
                Debug.LogWarning($"An error occurred while importing the song: Unknown file type?");
                yield break;
            }

            project.musicPath = GetRelativePath(path, project.directoryPath);
            references.music.clip = myClip;
            Debug.Log("Loaded song!");
        }
    }
    
    public enum State {
        Play = 0,
        Edit = 1,
    }

    public enum PlaymodeState {
        Playing = 0,
        Paused = 1,
        Stopped = 2
    }
}