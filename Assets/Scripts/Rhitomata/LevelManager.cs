using System;
using Rhitomata;
using SFB;
using UnityEngine;
using UnityEngine.InputSystem;

public class LevelManager : MonoBehaviour
{
    [Header("References")]
    public References references;
    [SerializeField] private InputActionReference switchModeAction;
    [SerializeField] private CanvasGroup editorUI;

    [Header("States")]
    public State state;
    public bool debug;

    private void Start()
    {
        ChangeState(State.Edit);
    }

    private void OnEnable()
    {
        switchModeAction.action.performed += OnModeSwitchPerformed;
    }

    private void OnDisable()
    {
        switchModeAction.action.performed -= OnModeSwitchPerformed;
    }

    private void Update()
    {
        if (debug)
        {
            if (Input.GetKeyDown(KeyCode.R))
                Restart();
        }
    }

    public void Restart()
    {
        references.cameraMovement.transform.localPosition = new Vector3(0, 0, -10);
        references.player.ResetAll();
        ChangeState(State.Edit);

        // TODO: Reset decorations as well, probably only possible when we have a proper serialization system
    }

    private void OnModeSwitchPerformed(InputAction.CallbackContext obj)
    {
        ChangeState(state == State.Play ? State.Edit : State.Play);
    }

    public void ChangeState(State state)
    {
        this.state = state;
        switch (state)
        {
            case State.Play:
                references.music.UnPause();
                Runtime2DTransformInteractor.TransformInteractorController.instance.UnSelectEverything();
                Runtime2DTransformInteractor.TransformInteractorController.instance.enableSelecting = false;
                editorUI.gameObject.SetActive(false);
                break;

            case State.Edit:
                references.music.Pause();
                Runtime2DTransformInteractor.TransformInteractorController.instance.enableSelecting = true;
                editorUI.gameObject.SetActive(true);
                break;
        }
    }

    #region UI

    public const string fileExtension = "rhito";// maybe change this?

    public void BrowseToSaveLevel()
    {
        var path = StandaloneFileBrowser.SaveFilePanel("Save level", "", "New Project", fileExtension);
        if (string.IsNullOrEmpty(path)) return; // Cancelled

        SaveProject(path);
    }

    public void BrowseForLevel()
    {
        var result = StandaloneFileBrowser.OpenFilePanel("Load level", "", fileExtension, false);
        if (result == null || result.Length == 0) return; // Cancelled

        var path = result[0];

        LoadProject(path);
    }

    public void BrowseForAudio()
    {
        var extesnions = new[] 
        {
            new ExtensionFilter("Sound Files", "ogg", "mp3", "wav" ),
            new ExtensionFilter("All Files", "*" ),
        };
        var result = StandaloneFileBrowser.OpenFilePanel("Load song", "", extesnions, false);
        if (result == null || result.Length == 0) return; // Cancelled

        var path = result[0];

        // Set song
    }

    #endregion UI

    #region Project

    void LoadProject(string path)
    {

    }

    void SaveProject(string path)
    {

    }

    #endregion Project
}
