namespace Rhitomata.Data {
    public interface IInstanceableObject {
        int GetId();
        void SetId(int id);

        /// <summary>
        /// Called when this object is replaced by an object that was registered using overrideAnyway as true
        /// </summary>
        void OnIdRedirected(int previousId, int newId);
    }
}