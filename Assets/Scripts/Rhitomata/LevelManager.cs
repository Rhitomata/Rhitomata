using System;
using Rhitomata;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
        state = State.Edit;
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
        Start();

        // TODO: Reset decorations as well, probably only possible when we have a proper serialization system
    }

    private void OnEnable()
    {
        switchModeAction.action.performed += OnModeSwitchPerformed;
    }

    private void OnDisable()
    {
        switchModeAction.action.performed -= OnModeSwitchPerformed;
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
}
