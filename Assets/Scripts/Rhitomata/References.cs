using Rhitomata.Timeline;
using UnityEngine;

namespace Rhitomata {
    public class References : MonoBehaviour {
        #region Singleton
        public static References Instance { get; private set; }

        private void Awake() {
            // If there is an instance, and it's not me, delete myself.
            if (Instance && Instance != this) {
                Destroy(this);
                return;
            }
            Instance = this;
        }
        #endregion Singleton

        public PlayerMovement player;
        public CameraMovement cameraMovement;
        public LevelManager manager;
        public AudioManager music;
        public TimelineView timeline;
        public Canvas canvas;
        public Transform gameHolder;
        public ProjectList projectList;
        public GameUI gameUI;
    }
}