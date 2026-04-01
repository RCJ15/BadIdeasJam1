namespace DebugTools
{
    /// <summary>
    /// Interface that can be put onto any <see cref="UnityEngine.MonoBehaviour"/> to start drawing Debug <see cref="UnityEngine.GUI"/> in the <see cref="DebugConsole"/>.
    /// </summary>
    public interface IComponentDebugGUI
    {
        /// <summary>
        /// Override this to determine if this Debug GUI is available or not. <br/>
        /// By default this always returns true.
        /// </summary>
        public bool DebugGUIAvailable()
        {
            return true;
        }
        /// <summary>
        /// The name of this Debug GUI in the Component Debug window.
        /// </summary>
        public string DebugGUIName()
        {
            return null;
        }
        /// <summary>
        /// Main method for drawing Debug GUI. Use <see cref="UnityEngine.GUILayout"/> primarily.
        /// </summary>
        public void OnDebugGUI();
    }
}
