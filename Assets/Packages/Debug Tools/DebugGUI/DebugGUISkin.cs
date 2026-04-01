using UnityEngine;

namespace DebugTools
{
    /// <summary>
    /// Loads and holds a reference to the <see cref="GUISkin"/> for the <see cref="DebugConsole"/>.
    /// </summary>
    public static class DebugGUISkin
    {
        /// <summary>
        /// The <see cref="GUISkin"/>.
        /// </summary>
        public static GUISkin Skin { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void Init()
        {
            Skin = Resources.Load<GUISkin>("Debug Skin");
        }
    }
}
