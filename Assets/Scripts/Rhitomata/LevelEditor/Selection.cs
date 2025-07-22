using System.Collections.Generic;
using Riten.Native.Cursors;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rhitomata {
    public class Selection : MonoBehaviour {
        public bool enableSelecting = true;
        private ISelectable _currentHovered;

        private bool _isDragging = false;
        private Vector3 _dragStartWorldPoint;
        private Plane _dragPlane;
        private readonly Dictionary<ISelectable, Vector3> _dragStartPositions = new();
        private bool _wasOverUI;

        private void Update() {
            if (!enableSelecting) return;

            var isOverUI = EventSystem.current.IsPointerOverGameObject();
            if (_wasOverUI != isOverUI) {
                if (_currentHovered != null) {
                    _currentHovered?.OnExit();
                    _currentHovered = null;
                    UpdateCursor();
                }

                _wasOverUI = isOverUI;
            }

            if (!Camera.main) return;

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!isOverUI) {
                ISelectable hovered = null;

                if (Physics.Raycast(ray, out var hit))
                    hovered = hit.collider.GetComponent<ISelectable>();

                if (hovered != _currentHovered) {
                    _currentHovered?.OnExit();
                    hovered?.OnEnter();
                    _currentHovered = hovered;
                    UpdateCursor();
                }

                if (Input.GetMouseButtonDown(0)) {
                    if (hovered != null) {
                        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                            if (SelectedObjects.Contains(hovered))
                                RemoveSelection(hovered);
                            else
                                AddSelection(hovered);
                        } else {
                            SelectSingle(hovered);
                        }

                        if (SelectedObjects.Count > 0) {
                            var selected = SelectedObjectTransform();
                            _dragPlane = new Plane(Vector3.back, selected.position);

                            if (_dragPlane.Raycast(ray, out var enter)) {
                                _dragStartWorldPoint = ray.GetPoint(enter);
                                _dragStartPositions.Clear();

                                foreach (var obj in SelectedObjects) {
                                    if (obj is MonoBehaviour mb) {
                                        _dragStartPositions[obj] = mb.transform.position;
                                    }
                                }

                                _isDragging = true;
                            }
                        }
                    } else {
                        Clear();
                    }
                    UpdateCursor();
                }
            }

            // REMINDER TODO: If someone deletes an object while we're dragging, it will bug out
            if (Input.GetMouseButtonUp(0)) {
                _isDragging = false;
                _dragStartPositions.Clear();
                
                UpdateCursor();
            }

            if (_isDragging) {
                if (_dragPlane.Raycast(ray, out var enter)) {
                    var currentWorldPoint = ray.GetPoint(enter);
                    var delta = currentWorldPoint - _dragStartWorldPoint;

                    foreach (var kvp in _dragStartPositions) {
                        var obj = kvp.Key;
                        var startPos = kvp.Value;

                        if (obj is MonoBehaviour mb) {
                            mb.transform.position = startPos + delta;
                        }
                    }
                    
                    UpdateCursor();
                }
            }
        }

        public void UpdateCursor() {
            if (_isDragging) {
                NativeCursor.SetCursor(NTCursors.ClosedHand);
            } else {
                if (_currentHovered != null) {
                    NativeCursor.SetCursor(SelectedObjects.Contains(_currentHovered)
                        ? NTCursors.OpenHand
                        : NTCursors.Crosshair);
                } else {
                    NativeCursor.ResetCursor();
                }
            }
        }

        private Transform SelectedObjectTransform() {
            if (SelectedObject is MonoBehaviour mb)
                return mb.transform;
            return null;
        }

        public static List<ISelectable> SelectedObjects = new();
        public static ISelectable SelectedObject => SelectedObjects.Count > 0 ? SelectedObjects[0] : null;

        public static void Clear() {
            var removable = new List<ISelectable>();
            foreach (var obj in SelectedObjects) {
                if (obj.OnDeselect())
                    removable.Add(obj);
            }

            foreach (var obj in removable)
                SelectedObjects.Remove(obj);
        }

        public static bool SelectSingle(ISelectable selectable) {
            if (SelectedObjects.Contains(selectable)) return false;
            if (selectable.OnSelect()) {
                Clear();
                SelectedObjects.Add(selectable);
                return true;
            }

            return false;
        }

        public static bool AddSelection(ISelectable selectable) {
            if (SelectedObjects.Contains(selectable)) return false;

            if (selectable.OnSelect()) {
                SelectedObjects.Add(selectable);
                return true;
            }

            return false;
        }

        public static bool RemoveSelection(ISelectable selectable) {
            if (!SelectedObjects.Contains(selectable)) return false;

            if (selectable.OnDeselect()) {
                SelectedObjects.Remove(selectable);
                return true;
            }

            return false;
        }
    }
}