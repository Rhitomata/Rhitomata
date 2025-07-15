using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime2DTransformInteractor
{
    public class Rotator : MonoBehaviour
    {
        public SpriteBounds spriteBounds;
        public LineRenderer lineRenderer;
        
        private Vector2 lastMousePosition;
        private Vector2 rotationPoint;

        private void OnMouseEnter()
        {
            if (!TransformInteractorController.instance.enableSelecting || !canDrag) return;
            
            TransformInteractorController.instance.SetRotatorMouseCursor();
        }

        private void OnMouseExit()
        {
            TransformInteractorController.instance.SetDefaultMouseCursor();
        }

        private void OnMouseOver()
        {
            if (!TransformInteractorController.instance.enableSelecting || !canDrag) return;

            TransformInteractorController.instance.SetRotatorMouseCursor();
        }

        private void OnDisable()
        {
            TransformInteractorController.instance.SetDefaultMouseCursor();
        }

        private void OnMouseDown()
        {
            if (!TransformInteractorController.instance.enableSelecting || TransformInteractorController.isOverUI)
            {
                canDrag = false;
                return;
            }
            canDrag = true;

            Vector2 mousePixelsCoordinates = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            lastMousePosition = new Vector3(mousePixelsCoordinates.x, mousePixelsCoordinates.y, transform.position.z);

            rotationPoint = (spriteBounds.topLeftCorner.transform.position + spriteBounds.bottomRightCorner.transform.position) / 2;
        }

        private bool canDrag;
        private void OnMouseDrag()
        {
            if (!TransformInteractorController.instance.enableSelecting || !canDrag) return;
            
            TransformInteractorController.instance.SetRotatorMouseCursor();
            
            Vector2 mousePixelsCoordinates = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 newPosition = new Vector3(mousePixelsCoordinates.x, mousePixelsCoordinates.y, transform.position.z);

            float angle = Vector2.SignedAngle((lastMousePosition - new Vector2(rotationPoint.x, rotationPoint.y)),
                newPosition - new Vector2(rotationPoint.x, rotationPoint.y));

            RotateObjects(angle);

            lastMousePosition = newPosition;
        }

        private void RotateObjects(float angle)
        {
            spriteBounds.transform.RotateAround(rotationPoint, Vector3.forward, angle);

            spriteBounds.interactor.AdaptTransform();
        }
    }
}
