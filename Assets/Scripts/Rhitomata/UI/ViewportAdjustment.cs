using UnityEngine;

namespace Rhitomata.UI {
    public class ViewportAdjustment : MonoBehaviour {
        public Camera targetCamera;

        public RectTransform freeSpaceRect;
        public Canvas canvas;

        private void LateUpdate() {
            if (!targetCamera || !freeSpaceRect || !canvas)
                return;

            var worldCorners = new Vector3[4];
            freeSpaceRect.GetWorldCorners(worldCorners);

            Vector3 bottomLeft = RectTransformUtility.WorldToScreenPoint(null, worldCorners[0]);
            Vector3 topRight = RectTransformUtility.WorldToScreenPoint(null, worldCorners[2]);

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            var newCameraRect = new Rect {
                x = bottomLeft.x / screenWidth,
                y = bottomLeft.y / screenHeight,
                width = (topRight.x - bottomLeft.x) / screenWidth,
                height = (topRight.y - bottomLeft.y) / screenHeight
            };

            targetCamera.rect = newCameraRect;
        }
    }
}