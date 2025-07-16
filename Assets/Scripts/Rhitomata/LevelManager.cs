using System.IO;
using Rhitomata;
using SFB;
using UnityEngine;

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

    private void Start() {
        // We'll make a Window class for showing and hiding these properly
        projectList.gameObject.SetActive(false);
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
                references.music.UnPause();
                Runtime2DTransformInteractor.TransformInteractorController.instance.DeselectAll();
                Runtime2DTransformInteractor.TransformInteractorController.instance.enableSelecting = false;
                editorUI.gameObject.SetActive(false);
                break;

            case State.Edit:
                references.transformController.enableSelecting = true;
                references.music.Pause();
                Runtime2DTransformInteractor.TransformInteractorController.instance.enableSelecting = true;
                editorUI.gameObject.SetActive(true);
                break;
        }
    }

    #region UI

    public const string fileExtension = "rhito";// maybe change this?

    public void BrowseToSaveProject() {
        var extesnions = new[]
        {
            new ExtensionFilter("Rhitomata Project", fileExtension),
            new ExtensionFilter("All Files", "*"),
        };
        var path = StandaloneFileBrowser.SaveFilePanel("Save project", ProjectList.ProjectsDir, "New Project", extesnions);
        if (string.IsNullOrEmpty(path)) return; // Cancelled

        SaveProject(path);
    }

    public void BrowseForProject() {
        var extesnions = new[]
        {
            new ExtensionFilter("Rhitomata Project", fileExtension),
            new ExtensionFilter("All Files", "*"),
        };
        var result = StandaloneFileBrowser.OpenFilePanel("Load project", ProjectList.ProjectsDir, extesnions, false);
        if (result == null || result.Length == 0) return; // Cancelled

        var path = result[0];

        LoadProject(path);
    }

    public void BrowseForSong() {
        var extesnions = new[]
        {
            new ExtensionFilter("Sound Files", "ogg", "mp3", "wav"),
            new ExtensionFilter("All Files", "*"),
        };
        var result = StandaloneFileBrowser.OpenFilePanel("Import song", "", extesnions, false);
        if (result == null || result.Length == 0) return; // Cancelled

        var path = result[0];

        // Set song
    }

    public void BrowseForBeatmap() {
        var extesnions = new[]
        {
            new ExtensionFilter("Text Files", "txt"),
            new ExtensionFilter("All Files", "*"),
        };
        var result = StandaloneFileBrowser.OpenFilePanel("Import beatmap", "", extesnions, false);
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
    }

    void LoadProject(string path) {

    }

    void SaveProject(string path) {

    }
    #endregion Project
}