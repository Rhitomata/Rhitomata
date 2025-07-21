using System.Collections;
using System.Collections.Generic;
using System.IO;
using Rhitomata.Data;
using SFB;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static Useful;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Rhitomata.Editor")]
namespace Rhitomata
{
    public class LevelManager : MonoBehaviour {
        [Header("References")]
        public References references;
        [SerializeField] private KeyCode switchStateKey = KeyCode.Tab;
        [SerializeField] private CanvasGroup editorUI;
        [SerializeField] private ProjectList projectList;

        [Header("Project")]
        public ProjectData project = new();

        [Header("States")]
        public State state;
        public bool debug;

        private float desyncThreshold = 0.3f;// TODO: Make customizable from UI maybe?
        public float time;

        private void Start() {
            // TODO: We'll make a Window class for showing and hiding these properly
            projectList.gameObject.SetActive(false);
            ChangeState(State.Edit);
            DeleteSpritesUI();
        }

        private void Update() {
            if (references.manager.state == State.Play) {
                if (!references.music.isPlaying) references.music.Play();
                time += Time.deltaTime;

                // Syncs music player when it gets too off-sync
                // I don't know if it's better for the music player to resync or for the time to resync
                // TODO: Maybe cache clip length for better performance?
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

        #region UI

        private const string FILE_EXTENSION = "rhito";// maybe change this?

        public void BrowseToSaveProject() {
            var extensions = new[]
            {
                new ExtensionFilter("Rhitomata Project", FILE_EXTENSION),
                new ExtensionFilter("All Files", "*"),
            };
            var path = StandaloneFileBrowser.SaveFilePanel("Save project", ProjectList.ProjectsDir, "New Project", extensions);
            if (string.IsNullOrEmpty(path)) return; // Cancelled

            SaveProject(path);
        }

        public void BrowseForProject() {
            var extensions = new[]
            {
                new ExtensionFilter("Rhitomata Project", FILE_EXTENSION),
                new ExtensionFilter("All Files", "*"),
            };
            var result = StandaloneFileBrowser.OpenFilePanel("Load project", ProjectList.ProjectsDir, extensions, false);
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

        public void ShowProjectList() {
            // We'll make a Window class for showing and hiding these properly
            projectList.gameObject.SetActive(true);
        }

        public void Exit() {
            Application.Quit();
        }
        #endregion UI

        #region Project
        public void CreateProject(ProjectData data) {
            File.WriteAllText(data.directoryPath, RhitomataSerializer.Serialize(data));
            LoadProject(data.directoryPath);
        }

        void LoadProject(string directoryPath)
        {
            var content = File.ReadAllText(directoryPath);
            if (string.IsNullOrWhiteSpace(content))
            {
                Debug.LogWarning("The project file has no data, cancel loading the project");
                return;
            }

            try
            {
                var projectData = RhitomataSerializer.Deserialize<ProjectData>(content);
                if (projectData == null)
                {
                    Debug.LogWarning("The project failed to load while reading the data, unspecified error.");
                    return;
                }

                project = projectData;
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        void SaveProject(string path) {

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

            if (!string.IsNullOrEmpty(project.directoryPath))// HACK
                project.musicPath = GetRelativePath(path, project.directoryPath);
            references.music.clip = myClip;
            Debug.Log("Loaded song!");
        }

        #region Sprites

        public List<SpriteUI> sprites = new();

        public void BrowseForSprite() {
            var extensions = new[]
            {
                new ExtensionFilter("Image Files", "png", "jpg", "jpeg"),
                new ExtensionFilter("All Files", "*"),
            };
            var result = StandaloneFileBrowser.OpenFilePanel("Import sprites", ProjectList.ProjectsDir, extensions, true);
            if (result == null || result.Length == 0) return; // Cancelled

            foreach (var path in result) {
                CreateSpriteObject(path);
            }
        }

        public void DeleteSpritesUI() {
            foreach (Transform t in references.spriteUIHolder)
            {
                var spriteUI = t.GetComponent<SpriteUI>();
                // Free up the memory used by the texture and sprites
                var sprite = spriteUI.GetSprite();
                if (sprite)
                {
                    Destroy(spriteUI.GetSprite().texture);
                    Destroy(spriteUI.GetSprite());
                }

                Destroy(t.gameObject);
            }
        }

        public void CreateSpriteObject(string texturePath) {
            var sprite = CreateSpriteFromPath(texturePath);
            CreateSpriteObject(sprite, Path.GetFileNameWithoutExtension(texturePath));
        }

        public void CreateSpriteObject(Sprite sprite, string spriteName) {
            var spriteUI = Instantiate(references.spriteUIPrefab, references.spriteUIHolder).GetComponent<SpriteUI>();
            spriteUI.Initialize(sprite, spriteName);
            sprites.Add(spriteUI);
        }

        private static Sprite CreateSpriteFromPath(string path) {
            // TODO: Add a place to manage these creations and destroy the sprites and textures if necessary
            var fileData = File.ReadAllBytes(path);
            var texture = new Texture2D(2, 2);
            if (texture.LoadImage(fileData)) {
                var rect = new Rect(0, 0, texture.width, texture.height);
                var pivot = new Vector2(0.5f, 0.5f);
                var sprite = Sprite.Create(texture, rect, pivot);
                return sprite;
            }

            Debug.LogError("Failed to create sprite!");
            return null;
        }

        #endregion Sprites
    }
}