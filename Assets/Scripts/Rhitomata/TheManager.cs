using Rhitomata;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TheManager : MonoBehaviour
{
    #region Singleton

    public static TheManager ManagerInstance;

    private void Start()
    {
        if (ManagerInstance != null && ManagerInstance != this)
        {
            Destroy(this);
            return;
        }

        ManagerInstance = this;
        Initialize();
    }

    #endregion Singleton

    public bool debug;

    public PlayerMovement player;

    [SerializeField] private InputActionReference switchModeAction;

    [SerializeField] private CanvasGroup editorUI;

    public enum State
    {
        Play,
        Edit,
    }

    private State _state;
    public State CurrentState
    {
        get
        {
            return _state;
        }
        set
        {
            _state = value;

            switch (CurrentState)
            {
                case State.Play:
                    player.audioSource.UnPause();
                    Runtime2DTransformInteractor.TransformInteractorController.instance.UnSelectEverything();
                    Runtime2DTransformInteractor.TransformInteractorController.instance.enableSelecting = false;
                    editorUI.gameObject.SetActive(false);
                    break;

                case State.Edit:
                    player.audioSource.Pause();
                    Runtime2DTransformInteractor.TransformInteractorController.instance.enableSelecting = true;
                    editorUI.gameObject.SetActive(true);
                    break;
            }
        }
    }

    private void Initialize()
    {
        CurrentState = State.Edit;
    }

    private void Update()
    {
        if (debug)
        {
            if (Input.GetKeyDown(KeyCode.R)) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
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
        CurrentState = CurrentState == State.Play ? State.Edit : State.Play;
    }
}
