using System.IO;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Rhitomata.Data;
using Rhitomata.Timeline;
using Rhitomata.UI;
using SFB;
using UnityEngine.Events;
using static Rhitomata.Useful;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Rhitomata.Editor")]
namespace Rhitomata {
    public class LevelManager : InstanceManager<ObjectSerializer> {
        [Header("References")]
        public References references;
        [SerializeField] private KeyCode switchStateKey = KeyCode.Tab;
        [SerializeField] private CanvasGroup editorUI;
        public PointLane pointLane;
        public AudioClip defaultMusic;
        public Freecam freecam;
        public CameraMovement cameraMovement;

        [Header("Data")]
        public ProjectData project = new();

        private string _directory;
        public string directory => project != null ? project.directoryPath : _directory;

        [Header("States")]
        public State state;
        public bool debug;

        [Header("Prefabs & Environments")] 
        public Transform game;

        public Transform indicatorsParent;
        public GameObject spritePrefab;
        public GameObject indicatorPrefab;
        
        [Header("Judgement")]
        public Color perfectColor = Color.green;
        public float perfectThreshold = 0.08f;
        public Color goodColor = Color.blue;
        public float goodThreshold = 0.16f;
        public Color missColor = Color.red;

        [Header("Scores")]
        public int perfectJudgement;
        public int goodJudgement;
        public int missJudgement;
        public int maxCombo;
        public int combo;
        public float score;

        private float _animationTimer;
        private float _visibleScore;
        private Vector2 _initialLabelScale;

        private float desyncThreshold = 0.3f;// TODO: Make customizable from UI maybe?
        private CancellationTokenSource _projectLoadCts;
        
        [Header("Scores")]
        public UnityEvent onProjectLoaded;

        private void Start() {
            ChangeState(State.Edit);
        }

        private void Update() {
            if (debug) {
                if (Input.GetKeyDown(KeyCode.R))
                    Restart();
            }

            if (Input.GetKeyDown(switchStateKey))
                ChangeState(state == State.Play ? State.Edit : State.Play);
        }

        public void Restart() {
            ChangeState(State.Edit);
            references.cameraMovement.transform.localPosition = new Vector3(0, 0, -10);
            references.player.ResetAll();

            // TODO: Reset decorations as well, probably only possible when we have a proper serialization system
        }

        public void ChangeState(State newState) {
            state = newState;
            switch (newState) {
                case State.Play:
                    freecam.enabled = false;
                    cameraMovement.enabled = true;
                    if (!references.music.isPlaying) references.music.Play();
                    // editorUI.gameObject.SetActive(false);
                    break;

                case State.Edit:
                    freecam.enabled = true;
                    cameraMovement.enabled = false;
                    if (references.music.isPlaying) references.music.Stop();
                    // editorUI.gameObject.SetActive(true);
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

        public ModifyPoint SpawnModifyPoint(ModifyPoint point) {
            if (point.isInstantiated) return point;
            
            point.indicator = Instantiate(indicatorPrefab, indicatorsParent).GetComponent<Indicator>();
            point.indicator.transform.localPosition = point.position;
            point.indicator.transform.localEulerAngles = point.eulerAngles;
            var keyframe = references.timeline.CreatePredefinedKeyframe<PointKeyframe, PointLane>(point.time, pointLane);
            keyframe.modifyPoint = point;
            point.keyframe = keyframe;
            point.tail = references.player.CreateAdjustableTail(point.position, point.eulerAngles);
            point.isInstantiated = true;
            return point;
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
            _ = ImportSongAsync(path);
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

                projectData.directoryPath = directoryPath;
                projectData.filePath = directoryPath.Combine("project.json");

                _directory = directoryPath;
                project = projectData;

                LoadProject();
            } catch (System.Exception exception) {
                Debug.LogException(exception);
            }
        }
        
        /// <summary>
        /// Set <see cref="directory"/> and <see cref="project"/> first before using this function to load asynchonously
        /// </summary>
        private async void LoadProject() {
            _projectLoadCts = new CancellationTokenSource();
            var token = _projectLoadCts.Token;
            
            // TODO: Make a custom loading screen for projects
            var cancelButton = new MessageBoxButton("Cancel", () => _projectLoadCts.Cancel(), close: true);
            var messageBox = MessageBox.Show(
                "Loading project...",
                "Loading",
                null,
                new[] { cancelButton }
            );

            try {
                if (!string.IsNullOrWhiteSpace(project.musicPath) && Storage.FileExists(directory.Combine(project.musicPath))) {
                    await ImportSongAsync(directory.Combine(project.musicPath), token);
                } else {
                    references.music.clip = defaultMusic;
                }
                
                token.ThrowIfCancellationRequested();

                messageBox.message = "Adjusting all points";
                await UniTask.WaitForEndOfFrame(token);
                project.AdjustAllPointFromIndex(0); // Recalculate all points after the first one

                messageBox.message = $"Spawning {project.points.Count} points";
                await UniTask.WaitForEndOfFrame(token);
                await RandomIterator.LoopAsync(2, 20, project.points.Count, i => {
                    var point = project.points[i];
                    SpawnModifyPoint(point);
                    
                    token.ThrowIfCancellationRequested();
                }, () => UniTask.WaitForEndOfFrame(token));

                messageBox.message = $"Attaching additional keyframes...";
                await UniTask.WaitForEndOfFrame(token);
                // TODO: Load data in these orders: beatmap, timeline items & indicators, assets, objects
                messageBox.Close();
            } catch (System.OperationCanceledException) {
                MessageBox.ShowError("Project loading was cancelled!");
                messageBox.Close();
            } catch (System.Exception ex) {
                MessageBox.ShowError("An error encountered while loading the project! " + ex.Message);
                Debug.LogException(ex);
                messageBox.Close();
            } finally {
                _projectLoadCts?.Dispose();
                _projectLoadCts = null;
            }
        }

        public void SaveProject(string directoryPath) {
            
        }
        #endregion Project
        
        #region Import
        private async UniTask ImportSongAsync(string path, CancellationToken token = default) {
            if (project == null) {
                Debug.LogWarning("There's no project open yet, but a song is trying to be imported anyway!");
                return;
            }

            if (string.IsNullOrEmpty(path)) {
                Debug.LogWarning("An error occurred while importing the song: Empty path");
                return;
            }

            using var www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.UNKNOWN);
            await www.SendWebRequest().WithCancellation(token);
            token.ThrowIfCancellationRequested();

            if (www.result != UnityWebRequest.Result.Success) {
                Debug.LogWarning($"An error occurred while importing the song: {www.error}");
                return;
            }

            var myClip = DownloadHandlerAudioClip.GetContent(www);
            if (!myClip) {
                Debug.LogWarning("An error occurred while importing the song: Unknown file type?");
                return;
            }

            project.musicPath = GetRelativePath(path, project.directoryPath);
            references.music.clip = myClip;
            Debug.Log("Loaded song!");
        }
        #endregion
        
        #region Judgement
	    public void Judge(JudgementType type, float offset) {
		    switch (type) {
			    case JudgementType.Perfect:
				    perfectJudgement++;
                    combo++;
				    references.gameUI.judgementLabel.color = perfectColor;
                    break;
			    case JudgementType.Good:
				    goodJudgement++;
				    combo++;
                    references.gameUI.judgementLabel.color = goodColor;
                    break;
			    case JudgementType.Miss:
				    missJudgement++;
                    references.gameUI.judgementLabel.color = missColor;
				    combo = 0;
                    break;
		    }

            references.gameUI.judgementLabel.transform.localScale = Vector3.one * 1.2f;
            if (combo > maxCombo)
			    maxCombo = combo;

            var accuracy = (perfectJudgement + (goodJudgement * 0.65f)) / (float)project.points.Count;
            var comboPercentage = FloatDivide(maxCombo, project.points.Count);
            score = (900000 * accuracy) + (100000 * comboPercentage);
            references.gameUI.comboLabel.text = $"{combo}x/{project.points.Count}";
		    references.gameUI.comboLabel.transform.localScale = Vector3.one * 1.2f;
            _animationTimer = 1f;
        }

        public void RemoveJudgement(JudgementType type) {
            switch (type) {
                case JudgementType.Perfect:
                    perfectJudgement--;
                    combo--;
                    break;
                case JudgementType.Good:
                    goodJudgement--;
                    combo--;
                    break;
                case JudgementType.Miss:
                    missJudgement--;
                    break;
            }

		    if (perfectJudgement < 0) perfectJudgement = 0;
            if (goodJudgement < 0) goodJudgement = 0;
            if (missJudgement < 0) missJudgement = 0;
            if (combo < 0) combo = 0;

            var accuracy = (perfectJudgement + (goodJudgement * 0.65f)) / (float)project.points.Count;
            var comboPercentage = FloatDivide(maxCombo, project.points.Count);
            score = (900000 * accuracy) + (100000 * comboPercentage);
            references.gameUI.comboLabel.text = $"{combo}x/{project.points.Count}";
            references.gameUI.comboLabel.transform.localScale = Vector3.one * 1.2f;

		    _visibleScore = score;
        }

	    public void ResetJudgement() {
		    perfectJudgement = 0;
		    goodJudgement = 0;
		    missJudgement = 0;
            combo = 0;
		    maxCombo = 0;
		    score = 0;
            references.gameUI.comboLabel.text = $"{combo}x/{project.points.Count}";
		    _animationTimer = 1f;
        }
        #endregion

	    public float FloatDivide(float n, float d) => n / d;
    }

    public enum JudgementType {
	    Perfect,
	    Good,
	    Miss
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