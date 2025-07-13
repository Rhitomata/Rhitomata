using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static TheManager;

namespace Rhitomata {
    public class PlayerMovement : MonoBehaviour {
        public float speed = 15;
        public AudioSource audioSource;

        public List<Vector3> rotations = new() {
            new (0, 0, 0),
            new (0, 0, 90)
        };
        public int rotationIndex;

        public GameObject tailPrefab;
        public Transform tailParent;

        public InputActionReference tapAction;

        public bool isStarted;
        private GameObject currentTail;

        private void OnEnable() {
            tapAction.action.performed += OnTapStarted;
            tapAction.action.canceled += OnTapCancelled;
        }

        private void OnDisable() {
            tapAction.action.performed -= OnTapStarted;
            tapAction.action.canceled -= OnTapCancelled;
        }

        private void OnTapStarted(InputAction.CallbackContext obj) {
            if (ManagerInstance.CurrentState != State.Play) return;

            Debug.Log($"Tap called in phase: {obj.phase}");
            if (!isStarted) {
                audioSource.Play();
                isStarted = true;
                RotateToIndex(0);
                CreateTail();
            } else {
                Turn();
            }
        }

        private void OnTapCancelled(InputAction.CallbackContext obj) {
            Debug.Log($"Tap called in phase: {obj.phase}");
        }

        void Update() {
            if (ManagerInstance.CurrentState != State.Play) return;

            if (isStarted) {
                var velocity = speed * Time.deltaTime;
                var translation = transform.up * velocity;
                transform.localPosition += translation;

                if (currentTail != null) {
                    currentTail.transform.localPosition += translation / 2f;
                    currentTail.transform.localScale += Vector3.up * velocity;
                }
            }
        }

        public void Turn() {
            RotateToIndex((rotationIndex + 1) % rotations.Count);
            CreateTail();
        }

        private void CreateTail() {
            currentTail = Instantiate(tailPrefab, tailParent);
            currentTail.transform.localPosition = transform.localPosition;
            currentTail.transform.localEulerAngles = transform.localEulerAngles;
        }

        public void RotateToIndex(int index) {
            rotationIndex = index;

            var rotation = rotations[index];
            transform.localEulerAngles = rotation;
        }
    }
}