using UnityEngine;

namespace Rhitomata {
    public class Tail : MonoBehaviour {
        public void AdjustStretch(Vector3 start, Vector3 end) {
            Vector3 direction = end - start;
            float length = direction.magnitude;
            transform.localPosition = (start + end) * 0.5f;
            if (direction != Vector3.zero) {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.localRotation = Quaternion.Euler(0f, 0f, angle - 90f);
            }
            transform.localScale = new Vector3(1f, length + 1f, 1f);
        }
    }
}