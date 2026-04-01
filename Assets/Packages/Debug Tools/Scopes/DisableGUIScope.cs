using System;
using UnityEngine;

namespace DebugTools
{
    /// <summary>
    /// A GUI Scope that simply sets the value of <see cref="GUI.enabled"/> and then reverts it when disposed of.
    /// <summary>
    public class DisableGUIScope : IDisposable
    {
        /// <summary>
        /// The value of <see cref="GUI.enabled"/> was when this scope was created.
        /// </summary>
        public bool StartingEnabledState { get; private set; }

        public DisableGUIScope(bool disable)
        {
            // Set the enabled state to if we are disabled or not and also save the starting enabled state so we can revert later
            StartingEnabledState = GUI.enabled;
            GUI.enabled = !disable;
        }

        public void Dispose()
        {
            // Revert back to the starting enabled state so no other GUI gets messed up
            GUI.enabled = StartingEnabledState;
        }
    }
}
