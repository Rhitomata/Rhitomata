using Rhitomata.Data;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rhitomata {
    public class PlayerMovement : ObjectSerializer {
        [Header("References")]
        private References references => References.Instance;
        public bool isInterpolating = true;
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

        [Header("Approach Rates")]
        public int minIndicatorRange;
        public int maxIndicatorRange;
        public float indicatorAppearTime;

        private int _inputQueue;
        private GameObject _currentTail;
        private readonly List<GameObject> _tails = new();

        private int _pointIndex = -1;
        private int _interpolatedIndex = -1;
        private Tail _startTail;
        private float time => references.timeline.cursorTime;
        private double _audioStartDspTime;

        private void Awake() {
            _startTail = CreateAdjustableTail(transform.position, rotations[0]);
            _audioStartDspTime = AudioSettings.dspTime;
        }

        public void InvalidateTailCache(int fromIndex) {
            if (fromIndex <= _pointIndex)
                _interpolatedIndex = Mathf.Clamp(fromIndex - 1, -1, references.manager.project.points.Count - 1);
        }

        void Update() {
            if (!isInterpolating)
                return;

            var dspTimeSinceStart = AudioSettings.dspTime - _audioStartDspTime;
            if (references.manager.state == State.Play) {
                references.timeline.Seek(references.music.time, true);
            }

            _pointIndex = references.manager.project.GetModifyPointIndexAtTime(time);
            while (_interpolatedIndex != _pointIndex) {
                if (_interpolatedIndex < _pointIndex) {// If this tail is BEFORE the player's position
                    // Move forward
                    var nextPoint = references.manager.project.points[_interpolatedIndex + 1];
                    if (_interpolatedIndex == -1) {
                        _startTail.gameObject.SetActive(true);
                        _startTail.AdjustStretch(Vector3.zero, nextPoint.position);
                    } else {
                        var currentPoint = references.manager.project.points[_interpolatedIndex];
                        currentPoint.tail.AdjustStretch(currentPoint.position, nextPoint.position);
                        currentPoint.tail.gameObject.SetActive(true);
                        currentPoint.hasPassed = true;
                    }
                    references.manager.Judge(JudgementType.Perfect, 0f);
                    _interpolatedIndex++;
                } else {// If this tail is AFTER the player's position
                    if (_interpolatedIndex >= references.manager.project.points.Count) {
                        references.manager.RemoveJudgement(JudgementType.Perfect);
                        _interpolatedIndex--;
                        // TODO: The point is deleted
                    } else {
                        // Move backwards
                        var currentPoint = references.manager.project.points[_interpolatedIndex];
                        currentPoint.tail.gameObject.SetActive(false);
                        currentPoint.hasPassed = false;
                        _interpolatedIndex--;
                        if (references.manager.state == State.Play && references.music.isPlaying)
                            Debug.LogWarning("The player somehow went back in points while playmode on index " + _interpolatedIndex);
                        references.manager.RemoveJudgement(JudgementType.Perfect);
                    }
                }
            }

            transform.localPosition = references.manager.project.GetPositionForTime(time);
            if (_interpolatedIndex > -1) {
                references.manager.project.points[_interpolatedIndex].tail.AdjustStretch(references.manager.project.points[_interpolatedIndex].position, transform.localPosition);
                references.manager.project.points[_interpolatedIndex].tail.gameObject.SetActive(true);
            } else {
                if (time > 0) {
                    _startTail.AdjustStretch(Vector3.zero, transform.localPosition);
                    _startTail.gameObject.SetActive(true);
                } else {
                    _startTail.gameObject.SetActive(false);
                }
            }

            var start = Mathf.Max(0, _pointIndex - minIndicatorRange);
            var end = Mathf.Min(references.manager.project.points.Count - 1, _pointIndex + maxIndicatorRange);

            for (var i = start; i <= end; i++) {
                var point = references.manager.project.points[i];
                point.indicator.progress = Mathf.Clamp((time - (point.time - indicatorAppearTime)) / indicatorAppearTime, 0f, 1f);
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
                            _inputQueue++;
                }
            } else {
                foreach (var key in tapKeys) {
                    if (Input.GetKeyDown(key))
                        if (!InputManager.IsKeyCodeOverUI((int)key))
                            _inputQueue++;
                }
            }
        }

        /// <summary>
        /// Not sure if we should fix this script to use a while loop to process
        /// all the inputs in one frame or just one queue each frame
        /// </summary>
        public void InputUpdate() {
            if (_inputQueue > 0) {
                if (!isStarted) {
                    references.music.Play();
                    isStarted = true;
                    RotateToIndex(0);
                    CreateTail();
                } else {
                    Turn();
                }
                _inputQueue--;
            }
        }

        /// <summary>
        /// Not sure why we need a function for this, might be removed soon
        /// </summary>
        public void ResetInputQueue() => _inputQueue = 0;

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
            _currentTail = Instantiate(tailPrefab, tailParent);
            _currentTail.transform.localPosition = transform.localPosition;
            _currentTail.transform.localEulerAngles = transform.localEulerAngles;
            _tails.Add(_currentTail);
            return _currentTail;
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
            _tails.Add(tail.gameObject);
            return tail;
        }

        /// <summary>
        /// Clear the tail list and destroys them
        /// </summary>
        void ClearTails() {
            _currentTail = null;
            foreach (var tail in _tails)
                Destroy(tail);
            _tails.Clear();
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