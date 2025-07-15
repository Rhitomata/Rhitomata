using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Runtime2DTransformInteractor
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class TransformInteractor : MonoBehaviour
    {
        // Prefabs
        private GameObject spriteBoundsPrefab;

        [HideInInspector]
        public Interactor interactor;

        // Variables
        [HideInInspector]
        public bool selected;
        private Vector2 lastMousePosition;

        private void Start()
        {
            spriteBoundsPrefab = TransformInteractorController.instance.boundingRectanglePrefab;
            selected = false;
        }

        private void Update()
        {
            if (TransformInteractorController.instance.adjustSizeToZoom)
            {
                if (interactor)
                {
                    ChangeSizeOnZoom();
                }
            }
        }

        public void ResetBoundingRectangle()
        {
            interactor.Setup(interactor.targetGameObject);
        }

        private void ChangeSizeOnZoom()
        {
            float ratio = TransformInteractorController.instance.mainCamera.orthographicSize / Screen.height;

            Vector2 cornerScale = new Vector2(
                ratio * TransformInteractorController.instance.defaultCornerWidth,
                ratio * TransformInteractorController.instance.defaultCornerHeight);
            Vector2 rotationScale = new Vector2(
                ratio * TransformInteractorController.instance.defaultRotationWidth,
                ratio * TransformInteractorController.instance.defaultRotationHeight);
            float lineWidth = ratio * TransformInteractorController.instance.defaultLineWidth;

            interactor.spriteBounds.topLeftCorner.transform.localScale = cornerScale;

            interactor.spriteBounds.bottomLeftCorner.transform.localScale = cornerScale;

            interactor.spriteBounds.topRightCorner.transform.localScale = cornerScale;

            interactor.spriteBounds.bottomRightCorner.transform.localScale = cornerScale;

            interactor.spriteBounds.topLine.lineRenderer.startWidth = lineWidth;
            interactor.spriteBounds.topLine.lineRenderer.endWidth = lineWidth;

            interactor.spriteBounds.bottomLine.lineRenderer.startWidth = lineWidth;
            interactor.spriteBounds.bottomLine.lineRenderer.endWidth = lineWidth;

            interactor.spriteBounds.leftLine.lineRenderer.startWidth = lineWidth;
            interactor.spriteBounds.leftLine.lineRenderer.endWidth = lineWidth;

            interactor.spriteBounds.rightLine.lineRenderer.startWidth = lineWidth;
            interactor.spriteBounds.rightLine.lineRenderer.endWidth = lineWidth;

            interactor.spriteBounds.rotator.transform.localScale = rotationScale;
            interactor.spriteBounds.rotator.transform.localPosition = new Vector2(0,
                ratio * (interactor.spriteBounds.topLine.transform.localPosition.y
                + TransformInteractorController.instance.defaultRotationLineLength * 100
                + TransformInteractorController.instance.defaultRotationHeight + 10));

            interactor.spriteBounds.rotator.lineRenderer.startWidth = lineWidth;
            interactor.spriteBounds.rotator.lineRenderer.endWidth = lineWidth;
            interactor.spriteBounds.rotator.lineRenderer.transform.localScale =
                new Vector2(1, ratio * TransformInteractorController.instance.defaultRotationLineLength * 100);
        }

        private void CreateInteractor()
        {
            if (interactor != null) return;

            if (!TransformInteractorController.instance.hoveredElements.Contains(this))
                TransformInteractorController.instance.hoveredElements.Add(this);
            interactor = Instantiate(spriteBoundsPrefab).GetComponent<Interactor>();
            interactor.Setup(gameObject);
        }

        private void OnMouseEnter()
        {
            if (!TransformInteractorController.instance.enableSelecting || TransformInteractorController.isOverUI) return;

            TransformInteractorController.instance.SetMoveMouseCursor();
            if (!TransformInteractorController.instance.hoveredElements.Contains(this))
                TransformInteractorController.instance.hoveredElements.Add(this);
            if (!selected)
            {
                CreateInteractor();
            }
        }

        private void OnMouseExit()
        {
            if (!TransformInteractorController.instance.enableSelecting) return;

            if (TransformInteractorController.instance.hoveredElements.Contains(this))
                TransformInteractorController.instance.hoveredElements.Remove(this);
            TransformInteractorController.instance.SetDefaultMouseCursor();
            if (!selected)
            {
                // Cast a ray to check if one of the bounding rectangle elements is hit
                Collider2D colliderHit = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                if (!colliderHit || !interactor ||
                    (colliderHit.gameObject != interactor.spriteBounds.leftLine.gameObject
                    && colliderHit.gameObject != interactor.spriteBounds.rightLine.gameObject
                    && colliderHit.gameObject != interactor.spriteBounds.topLine.gameObject
                    && colliderHit.gameObject != interactor.spriteBounds.bottomLine.gameObject
                    && colliderHit.gameObject != interactor.spriteBounds.topLeftCorner.gameObject
                    && colliderHit.gameObject != interactor.spriteBounds.topRightCorner.gameObject
                    && colliderHit.gameObject != interactor.spriteBounds.bottomLeftCorner.gameObject
                    && colliderHit.gameObject != interactor.spriteBounds.bottomRightCorner.gameObject)
                    ) {
                    Deselect();
                }
            }
        }

        private void OnMouseOver()
        {
            if (!TransformInteractorController.instance.enableSelecting || TransformInteractorController.isOverUI) return;

            CreateInteractor();
            TransformInteractorController.instance.SetMoveMouseCursor();
        }

        private void OnMouseUp()
        {
            if (!TransformInteractorController.instance.enableSelecting) return;

            if (!selected)
            {
                Select();
            }
        }

        private void OnMouseDown()
        {
            if (!TransformInteractorController.instance.enableSelecting || TransformInteractorController.isOverUI)
            {
                canDrag = false;
                return;
            }
            canDrag = true;

            lastMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        private bool canDrag = false;
        private void OnMouseDrag()
        {
            if (!TransformInteractorController.instance.enableSelecting || !canDrag) return;

            if (!selected)
            {
                Select();
            }

            if (selected)
            {
                Vector2 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                TransformInteractorController.instance.MoveSelectedObjects(newPosition - lastMousePosition);
                lastMousePosition = newPosition;
            }
        }

        public void Select()
        {
            if (!TransformInteractorController.instance.enableSelecting) return;

            if (TransformInteractorController.instance.allowMultiSelection)
            {
                bool unselect = true;
                foreach (KeyCode key in TransformInteractorController.instance.multiSelectionKeys)
                {
                    if (Input.GetKey(key))
                    {
                        unselect = false;
                    }
                }

                if (unselect)
                {
                    TransformInteractorController.instance.DeselectAll();
                }
            }
            else
            {
                TransformInteractorController.instance.DeselectAll();
            }
            selected = true;
            TransformInteractorController.instance.selectedElements.Add(this);
            TransformInteractorController.instance.SetMoveMouseCursor();
        }

        public void Deselect()
        {
            selected = false;
            canDrag = false;
            if (interactor)
                Destroy(interactor.gameObject);
            if (TransformInteractorController.instance.selectedElements.Contains(this))
                TransformInteractorController.instance.selectedElements.Remove(this);
            if (TransformInteractorController.instance.hoveredElements.Contains(this))
                TransformInteractorController.instance.hoveredElements.Remove(this);
        }
    }
}
