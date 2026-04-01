using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DebugTools
{
    /// <summary>
    /// The base class for every single seperate window in the <see cref="DebugConsole"/>.
    /// </summary>
    public abstract class DebugConsoleWindow
    {
        public abstract void OnOpen();

        public abstract void Draw(Rect rect, Event evt);
    }
}
