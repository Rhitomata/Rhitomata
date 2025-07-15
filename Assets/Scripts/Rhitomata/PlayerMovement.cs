using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rhitomata
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("References")]
        public References references;

        [SerializeField] private GameObject tailPrefab;
        [SerializeField] private Transform tailParent;

        [SerializeField] private KeyCode[] tapKeys = {
            KeyCode.Mouse0,
            KeyCode.UpArrow,
            KeyCode.Space
        };

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

        private int inputQueue;
        private GameObject currentTail;
        private readonly List<GameObject> tails = new();

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

        public void InputUpdate() {
            if (inputQueue > 0) {
                if (references.manager.state != State.Play) return;

                if (!isStarted) {
                    references.music.Play();
                    isStarted = true;
                    RotateToIndex(0);
                    CreateTail();
                }
                else {
                    Turn();
                }
            }
        }

        public void UpdateInputQueue() {
            if (Application.isMobilePlatform) {
                foreach (var touch in Input.touches) {
                    if (touch.phase == TouchPhase.Began)
                        if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                            inputQueue++;
                }
            }
            else {
                foreach (var key in tapKeys) {
                    if (Input.GetKeyDown(key))
                        if (!InputManager.IsMouseOverUI((int)key))
                            inputQueue++;
                }
            }
        }

        public void ResetInputQueue() => inputQueue = 0;

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

        void ClearTails()
        {
            currentTail = null;
            foreach (var tail in tails)
            {
                Destroy(tail.gameObject);
            }
            tails.Clear();
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
            ClearTails();
            rotationIndex = 0;
            transform.localPosition = Vector3.zero;
            references.music.Stop();
        }
    }
}