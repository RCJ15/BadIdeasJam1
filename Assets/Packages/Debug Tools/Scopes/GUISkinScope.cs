using System;
using UnityEngine;

namespace DebugTools
{
    /// <summary>
    /// A GUI Scope that simply applies a <see cref="GUISkin"/> and then reverts it when disposed of.
    /// </summary>
    public class GUISkinScope : IDisposable
    {
        /// <summary>
        /// The starting <see cref="GUISkin"/> before this scope changed it.
        /// </summary>
        public GUISkin StartSkin { get; private set; }

        public GUISkinScope(GUISkin newSkin)
        {
            // Set the current GUI skin to the debug skin and also save the start skin so we can revert later
            StartSkin = GUI.skin;
            GUI.skin = newSkin;
        }

        public void Dispose()
        {
            // Revert back to the starting skin so no other GUI gets messed up
            GUI.skin = StartSkin;
        }
    }
}
