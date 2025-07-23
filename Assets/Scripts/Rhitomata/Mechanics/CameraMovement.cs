using UnityEngine;

namespace Rhitomata {
    public class CameraMovement : ObjectSerializer {
        public Transform target;
        public float followSpeed = 3f;

        // TODO: Add camera shake functionality

        void Update() {
            if (!target) return;

            // Keeping the z position of the camera, otherwise, 2D objects won't be visible
            var position = target.localPosition;
            position.z = transform.localPosition.z;
            transform.localPosition = ExponentialLerp(transform.localPosition, position, followSpeed, Time.deltaTime);
        }

        public static Vector3 ExponentialLerp(Vector3 start, Vector3 end, float speed, float delta) {
            return Vector3.Lerp(start, end, 1 - Mathf.Exp(-speed * delta));
        }
    }
}