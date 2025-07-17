using Rhitomata;
using SFB;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Rhitomata.Editor")]
public class LevelManager : MonoBehaviour {
    [Header("References")]
    public References references;
    [SerializeField] private KeyCode switchStateKey = KeyCode.Tab;
    [SerializeField] private CanvasGroup editorUI;
    [SerializeField] private ProjectList projectList;

    [Header("Project")]
    public ProjectInfo project = new();

    [Header("States")]
    public State state;
    public bool debug;

    private float desyncThreshold = 0.3f;
    private float time;

    private void Start() {
        // TODO: We'll make a Window class for showing and hiding these properly
        projectList.gameObject.SetActive(false);
        ChangeState(State.Edit);
    }

    private void Update() {
        if (references.manager.state == State.Play) {
            if (!references.music.isPlaying) references.music.Play();
            time += Time.deltaTime;

            // Syncs music player when it gets too off-sync
            // I don't know if it's better for the music player to resync or for the time to resync
            // TODO: Maybe cache clip length for better performance?
            var musicPlayer = references.music;
            if (musicPlayer.clip != null && (musicPlayer.time > time + desyncThreshold || musicPlayer.time < time - desyncThreshold)) musicPlayer.time = Mathf.Clamp(time, 0, musicPlayer.clip.length);
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

    public void ChangeState(State state) {
        this.state = state;
        switch (state) {
            case State.Play:
                references.transformController.DehoverAll();
                // TODO: Save selection and restore it when going back to edit mode
                references.transformController.DeselectAll();
                references.transformController.enableSelecting = false;
                Runtime2DTransformInteractor.TransformInteractorController.instance.DeselectAll();
                Runtime2DTransformInteractor.TransformInteractorController.instance.enableSelecting = false;
                editorUI.gameObject.SetActive(false);
                break;

            case State.Edit:
                references.transformController.enableSelecting = true;
                Runtime2DTransformInteractor.TransformInteractorController.instance.enableSelecting = true;
                editorUI.gameObject.SetActive(true);
                break;
        }
    }

    #region UI

    public const string fileExtension = "rhito";// maybe change this?

    public void BrowseToSaveProject() {
        var extensions = new[]
        {
            new ExtensionFilter("Rhitomata Project", fileExtension),
            new ExtensionFilter("All Files", "*"),
        };
        var path = StandaloneFileBrowser.SaveFilePanel("Save project", ProjectList.ProjectsDir, "New Project", extensions);
        if (string.IsNullOrEmpty(path)) return; // Cancelled

        SaveProject(path);
    }

    public void BrowseForProject() {
        var extensions = new[]
        {
            new ExtensionFilter("Rhitomata Project", fileExtension),
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
    public void CreateProject(ProjectInfo projectInfo) {
        File.WriteAllText(projectInfo.path, RhitomataSerializer.Serialize(projectInfo));
        LoadProject(projectInfo.path);
    }

    void LoadProject(string path) {

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

        project.audioPath = GetRelativePath(path, project.path);
        references.music.clip = myClip;
        Debug.Log("Loaded song!");
    }

    string GetRelativePath(string filespec, string folder) {
        var pathUri = new Uri(filespec);
        // Folders must end in a slash
        if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString())) {
            folder += Path.DirectorySeparatorChar;
        }

        var folderUri = new Uri(folder);
        return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar)).Replace('\\', '/');
    }
}