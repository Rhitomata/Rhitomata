using UnityEngine;

namespace Rhitomata {
    public class ObjectSerializer : MonoBehaviour, InstanceableObject {
        public int instanceId;
        public int GetId() => instanceId;
        public void SetId(int id) => instanceId = id;
        public void OnIdRedirected(int previousId, int newId) => instanceId = newId;
    }
}