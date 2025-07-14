using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rhitomata
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("References")]
        public References references;

        [SerializeField] private GameObject tailPrefab;
        [SerializeField] private Transform tailParent;

        [SerializeField] private InputActionReference tapAction;

        [Header("Properties")]
        public float speed = 15;
        public List<Vector3> rotations = new() {
            new (0, 0, 0),
            new (0, 0, 90)
        };
        public bool debug;

        [Header("States")]
        public int rotationIndex;
        public bool isStarted;

        private GameObject currentTail;
        private readonly List<GameObject> tails = new();

        private void OnEnable()
        {
            tapAction.action.performed += OnTapStarted;
            tapAction.action.canceled += OnTapCancelled;
        }

        private void OnDisable()
        {
            tapAction.action.performed -= OnTapStarted;
            tapAction.action.canceled -= OnTapCancelled;
        }

        private void OnTapStarted(InputAction.CallbackContext obj)
        {
            if (references.manager.state != State.Play) return;

#if UNITY_EDITOR
            if (debug)
                Debug.Log($"Tap called in phase: {obj.phase}");
#endif
            if (!isStarted)
            {
                references.music.Play();
                isStarted = true;
                RotateToIndex(0);
                CreateTail();
            }
            else
            {
                Turn();
            }
        }

        private void OnTapCancelled(InputAction.CallbackContext obj)
        {
#if UNITY_EDITOR
            if (debug)
                Debug.Log($"Tap called in phase: {obj.phase}");
#endif
        }

        void Update()
        {
            if (references.manager.state != State.Play) return;

            if (isStarted)
            {
                var velocity = speed * Time.deltaTime;
                var translation = transform.up * velocity;
                transform.localPosition += translation;

                if (currentTail != null)
                {
                    currentTail.transform.localPosition += translation / 2f;
                    currentTail.transform.localScale += Vector3.up * velocity;
                }
            }
        }

        public void Turn()
        {
            RotateToIndex((rotationIndex + 1) % rotations.Count);
            CreateTail();
        }

        private void CreateTail()
        {
            currentTail = Instantiate(tailPrefab, tailParent);
            currentTail.transform.localPosition = transform.localPosition;
            currentTail.transform.localEulerAngles = transform.localEulerAngles;
            tails.Add(currentTail);
        }

        public void RotateToIndex(int index)
        {
            rotationIndex = index;

            var rotation = rotations[index];
            transform.localEulerAngles = rotation;
        }

        public void ResetAll()
        {
            isStarted = false;
            currentTail = null;
            rotationIndex = 0;
            transform.localPosition = Vector3.zero;
            references.music.Stop();
        }
    }
}