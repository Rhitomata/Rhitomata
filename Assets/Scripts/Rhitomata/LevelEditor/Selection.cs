using System.Collections.Generic;
using UnityEngine;

namespace Rhitomata
{
    public class Selection : MonoBehaviour
    {
        public bool enableSelecting = true;
        private ISelectable currentHovered;

        private bool isDragging = false;
        private Vector3 dragStartWorldPoint;
        private Plane dragPlane;
        private Dictionary<ISelectable, Vector3> dragStartPositions = new();

        void Update()
        {
            if (!enableSelecting) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            ISelectable hovered = null;

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                hovered = hit.collider.GetComponent<ISelectable>();
            }

            if (hovered != currentHovered)
            {
                if (currentHovered != null)
                    currentHovered.OnExit();

                if (hovered != null)
                    hovered.OnEnter();

                currentHovered = hovered;
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (hovered != null)
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        if (SelectedObjects.Contains(hovered))
                            RemoveSelection(hovered);
                        else
                            AddSelection(hovered);
                    }
                    else
                    {
                        SelectSingle(hovered);
                    }

                    if (SelectedObjects.Count > 0)
                    {
                        var transform = SelectedObjectTransform();
                        dragPlane = new Plane(Vector3.back, transform.position);

                        if (dragPlane.Raycast(ray, out float enter))
                        {
                            dragStartWorldPoint = ray.GetPoint(enter);
                            dragStartPositions.Clear();

                            foreach (var obj in SelectedObjects)
                            {
                                if (obj is MonoBehaviour mb)
                                {
                                    dragStartPositions[obj] = mb.transform.position;
                                }
                            }

                            isDragging = true;
                        }
                    }

                }
                else
                {
                    Clear(); 
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                dragStartPositions.Clear();
            }

            if (isDragging)
            {
                if (dragPlane.Raycast(ray, out float enter))
                {
                    Vector3 currentWorldPoint = ray.GetPoint(enter);
                    Vector3 delta = currentWorldPoint - dragStartWorldPoint;

                    foreach (var kvp in dragStartPositions)
                    {
                        ISelectable obj = kvp.Key;
                        Vector3 startPos = kvp.Value;

                        if (obj is MonoBehaviour mb)
                        {
                            mb.transform.position = startPos + delta;
                        }
                    }
                }
            }
        }

        private Transform SelectedObjectTransform()
        {
            if (SelectedObject is MonoBehaviour mb)
                return mb.transform;
            return null;
        }

        public static List<ISelectable> SelectedObjects = new();
        public static ISelectable SelectedObject => SelectedObjects.Count > 0 ? SelectedObjects[0] : null;

        public static void Clear()
        {
            var removable = new List<ISelectable>();
            foreach (var obj in SelectedObjects)
            {
                if (obj.OnDeselect())
                    removable.Add(obj);
            }

            foreach (var obj in removable)
                SelectedObjects.Remove(obj);
        }

        public static bool SelectSingle(ISelectable selectable)
        {
            if (SelectedObjects.Contains(selectable)) return false;
            if (selectable.OnSelect())
            {
                Clear();
                SelectedObjects.Add(selectable);
                return true;
            }

            return false;
        }

        public static bool AddSelection(ISelectable selectable)
        {
            if (SelectedObjects.Contains(selectable)) return false;

            if (selectable.OnSelect())
            {
                SelectedObjects.Add(selectable);
                return true;
            }

            return false;
        }

        public static bool RemoveSelection(ISelectable selectable)
        {
            if (!SelectedObjects.Contains(selectable)) return false;

            if (selectable.OnDeselect())
            {
                SelectedObjects.Remove(selectable);
                return true;
            }

            return false;
        }
    }
}