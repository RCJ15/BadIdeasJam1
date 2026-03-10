using UnityEngine;
using UnityEngine.InputSystem;

namespace Input
{
    /// <summary>
    /// Static class that contains the projects <see cref="UnityEngine.InputSystem.InputSettings"/> and some static versions of it's properties.
    /// <summary>
    public static class GameInputSettings
    {
        /// <summary>
        /// The projects <see cref="UnityEngine.InputSystem.InputSettings"/>.
        /// </summary>
        public static InputSettings InputSettings => InputSystem.settings;

        /// <summary>
        /// The current Update Mode for the <see cref="InputSettings"/>.
        /// </summary>
        public static InputSettings.UpdateMode UpdateMode
        {
            get => InputSettings.updateMode;
            set => InputSettings.updateMode = value;
        }

        /// <summary>
        /// The default deadzone minimum if not overwritten.
        /// </summary>
        public static float DefaultDeadzoneMin
        {
            get => InputSettings.defaultDeadzoneMin;
            set
            {
                if (value > InputSettings.defaultDeadzoneMax)
                {
                    InputSettings.defaultDeadzoneMax = value;
                }

                InputSettings.defaultDeadzoneMin = value;
            }
        }

        /// <summary>
        /// The default deadzone maximum if not overwritten.
        /// </summary>
        public static float DefaultDeadzoneMax
        {
            get => InputSettings.defaultDeadzoneMax;
            set
            {
                if (value < InputSettings.defaultDeadzoneMin)
                {
                    InputSettings.defaultDeadzoneMin = value;
                }

                InputSettings.defaultDeadzoneMax = value;
            }
        }

        /// <summary>
        /// The starting value of <see cref="DefaultDeadzoneMin"/> when the game start.
        /// </summary>
        public static float StartDefaultDeadzoneMin { get; private set; }

        /// <summary>
        /// The starting value of <see cref="DefaultDeadzoneMax"/> when the game start.
        /// </summary>
        public static float StartDefaultDeadzoneMax { get; private set; }

        [RuntimeInitializeOnLoadMethod] 
        public static void Init()
        {
            StartDefaultDeadzoneMin = DefaultDeadzoneMin;
            StartDefaultDeadzoneMax = DefaultDeadzoneMax;
        }
    }
}
