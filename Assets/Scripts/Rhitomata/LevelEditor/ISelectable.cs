namespace Rhitomata
{
    public interface ISelectable
    {
        /// <summary>
        /// A callback that is called when there's a request to select this object
        /// </summary>
        /// <returns>Returns true if the object can be selected, false if it shouldn't be</returns>
        public bool OnSelect();

        /// <summary>
        /// A callback that is called when there's a request to deselect this object
        /// </summary>
        /// <returns>Returns true if the object can be deselected, false if it cannot</returns>
        public bool OnDeselect();

        public void OnEnter();
        public void OnExit();

        public bool IsSelected();
    }
}