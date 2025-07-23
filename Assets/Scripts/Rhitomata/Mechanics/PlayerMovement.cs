using Rhitomata.Data;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rhitomata {
    public class PlayerMovement : ObjectSerializer {
        [Header("References")]
        private References references => References.Instance;
        private ProjectData project => references.manager.project;

        [SerializeField] private GameObject tailPrefab;
        [SerializeField] private Transform tailParent;

        [SerializeField]
        private KeyCode[] tapKeys = {
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

        private int pointIndex = -1;
        private int _interpolatedIndex = -1;
        private Tail startTail;

        private void Awake() {
            startTail = CreateAdjustableTail(transform.position, rotations[0]);
        }

        void Update() {
            /*if (references.manager.state != State.Play) return;

            UpdateInputQueue();
            InputUpdate();

            if (isStarted) {
                var velocity = speed * Time.deltaTime;
                var translation = transform.up * velocity;
                transform.localPosition += translation;

                if (currentTail != null) {
                    currentTail.transform.localPosition += translation / 2f;
                    currentTail.transform.localScale += Vector3.up * velocity;
                }
            }*/

            print(references.manager.time);

            pointIndex = project.GetModifyPointIndexAtTime(references.manager.time);
            while (pointIndex != _interpolatedIndex) {
                if (_interpolatedIndex < pointIndex) {
                    // Move forward
                    var nextPoint = project.points[_interpolatedIndex + 1];
                    if (_interpolatedIndex == -1) {
                        startTail.gameObject.SetActive(true);
                        startTail.AdjustStretch(Vector3.zero, nextPoint.position);
                    } else {
                        var currentPoint = project.points[_interpolatedIndex];
                        currentPoint.tail.AdjustStretch(currentPoint.position, nextPoint.position);
                        currentPoint.tail.gameObject.SetActive(true);
                        currentPoint.hasPassed = true;
                    }
                    //references.manager.Judge(JudgementType.Perfect, 0f);
                    _interpolatedIndex++;
                } else {
                    if (_interpolatedIndex >= project.points.Count) {
                        //references.manager.RemoveJudgement(JudgementType.Perfect);
                        _interpolatedIndex--;
                        // TODO: The point is deleted
                    } else {
                        // Move backwards
                        var currentPoint = project.points[_interpolatedIndex];
                        currentPoint.tail.gameObject.SetActive(false);
                        currentPoint.hasPassed = false;
                        _interpolatedIndex--;
                        if (references.manager.state == State.Play && references.music.time != 0)
                            Debug.LogWarning($"The player somehow went back in points during play mode! Index: {_interpolatedIndex}");

                        //references.manager.RemoveJudgement(JudgementType.Perfect);
                    }
                }
            }
        }

        /// <summary>
        /// Actually processes input and puts it in the queue, otherwise we lose some inputs
        /// </summary>
        public void UpdateInputQueue() {
            if (Application.isMobilePlatform) {
                foreach (var touch in Input.touches) {
                    if (touch.phase == TouchPhase.Began)
                        if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                            inputQueue++;
                }
            } else {
                foreach (var key in tapKeys) {
                    if (Input.GetKeyDown(key))
                        if (!InputManager.IsKeyCodeOverUI((int)key))
                            inputQueue++;
                }
            }
        }

        /// <summary>
        /// Not sure if we should fix this script to use a while loop to process
        /// all the inputs in one frame or just one queue each frame
        /// </summary>
        public void InputUpdate() {
            if (inputQueue > 0) {
                if (!isStarted) {
                    references.music.Play();
                    isStarted = true;
                    RotateToIndex(0);
                    CreateTail();
                } else {
                    Turn();
                }
                inputQueue--;
            }
        }

        /// <summary>
        /// Not sure why we need a function for this, might be removed soon
        /// </summary>
        public void ResetInputQueue() => inputQueue = 0;

        /// <summary>
        /// Rotates the player and creates a tail along with it
        /// </summary>
        public void Turn() {
            RotateToIndex((rotationIndex + 1) % rotations.Count);
            CreateTail();
        }

        /// <summary>
        /// Creates a tail with the same position and rotation as the player,
        /// this also registers the tail on the tail list
        /// </summary>
        /// <returns>The produced tail</returns>
        private GameObject CreateTail() {
            currentTail = Instantiate(tailPrefab, tailParent);
            currentTail.transform.localPosition = transform.localPosition;
            currentTail.transform.localEulerAngles = transform.localEulerAngles;
            tails.Add(currentTail);
            return currentTail;
        }
        
        /// <summary>
        /// Creates a tail with the same position and rotation as the player,
        /// this also registers the tail on the tail list
        /// </summary>
        /// <returns>The produced tail</returns>
        public Tail CreateAdjustableTail(Vector3 position, Vector3 eulerAngles) {
            var tail = Instantiate(tailPrefab, tailParent).AddComponent<Tail>();
            tail.transform.localPosition = transform.localPosition;
            tail.transform.localEulerAngles = transform.localEulerAngles;
            tails.Add(tail.gameObject);
            return tail;
        }

        /// <summary>
        /// Clear the tail list and destroys them
        /// </summary>
        void ClearTails() {
            currentTail = null;
            foreach (var tail in tails)
                Destroy(tail);
            tails.Clear();
        }

        /// <summary>
        /// Rotate to angles defined in <see cref="rotations"/>
        /// </summary>
        /// <param name="index">The <see cref="rotations"/> index</param>
        public void RotateToIndex(int index) {
            rotationIndex = index;

            var rotation = rotations[index];
            transform.localEulerAngles = rotation;
        }

        /// <summary>
        /// Resets the position, tail, rotation, and other player states
        /// </summary>
        public void ResetAll() {
            isStarted = false;
            ClearTails();
            rotationIndex = 0;
            transform.localPosition = Vector3.zero;
            references.music.Stop();
        }
    }
}