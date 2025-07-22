using Rhitomata.Data;
using UnityEngine;

namespace Rhitomata {
    public class ObjectSerializer : MonoBehaviour, IInstanceableObject {
        public int instanceId;
        public int GetId() => instanceId;
        public void SetId(int id) => instanceId = id;
        public void OnIdRedirected(int previousId, int newId) => instanceId = newId;
        
        public virtual void OnDeserialized(string data) { }
        public virtual string OnSerialized() => "";
    }

    public class ObjectSerializer<T> : ObjectSerializer {
        public override void OnDeserialized(string data) => Deserialize(RhitomataSerializer.Deserialize<T>(data));
        public override string OnSerialized() => RhitomataSerializer.Serialize(Serialize());

        protected virtual void Deserialize(T data) { }
        protected virtual T Serialize() => default;
    }
}