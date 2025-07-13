using UnityEngine;

namespace Rhitomata {
    public class CameraMovement : MonoBehaviour {
        public Transform target;
        public float followSpeed = 3f;

        void Update() {
            if (!target) return;

            var position = target.localPosition;
            position.z = transform.localPosition.z;
            transform.localPosition = ExponentialLerp(transform.localPosition, position, followSpeed, Time.deltaTime);
        }

        /// <summary>
        /// A modified lerp function optimized for unpredictable movement. 
        /// Should be moving this to a new class but I'm putting this here for now
        /// </summary>
        public static Vector3 ExponentialLerp(Vector3 start, Vector3 end, float speed, float delta) {
            return Vector3.Lerp(start, end, 1 - Mathf.Exp(-speed * delta));
        }
    }
}