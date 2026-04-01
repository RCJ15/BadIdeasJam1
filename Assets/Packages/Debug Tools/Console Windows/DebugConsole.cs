using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DebugTools
{
    /// <summary>
    /// The debug console for the game. Allows runtime execution of any static method with a <see cref="DebugCommandAttribute"/> attached to it.
    /// </summary>
    public class DebugConsole : MonoBehaviour
    {
        public static bool DebugFileExists => File.Exists(Path.Combine(Application.streamingAssetsPath, "debug"));

        public static bool CanUseConsole => IN_DEVELOPER_BUILD_OR_EDITOR || DebugFileExists;

        public const bool IN_DEVELOPER_BUILD_OR_EDITOR =
#if DEBUG || UNITY_EDITOR
            true;
#else
            false;
#endif

        public const float TARGET_SCREEN_WIDTH = 1920 / 2;
        public const float TARGET_SCREEN_HEIGHT = 1080 / 2;

        public static Action OnOpen { get; set; }
        public static Action OnClose { get; set; }

        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            // Create a new GameObject with the DebugConsole script attached
            GameObject obj = new GameObject("Debug Console", typeof(DebugConsole));

            DontDestroyOnLoad(obj);

            DebugConsole debugConsole = obj.GetComponent<DebugConsole>();

            // Create debug console windows
            debugConsole._commandPrompt = new DebugConsoleCommandPrompt();
            debugConsole._logger = new DebugConsoleLogger();
            debugConsole._componentDebug = new DebugConsoleComponentDebug();
        }

        /// <summary>
        /// Adds a new log to the <see cref="DebugConsoleLogger"/> using all of the assigned information.
        /// </summary>
        public static void LogMessage(string message, LogType logType = LogType.Log)
        {
            DebugConsoleLogger.LogMessage(message, logType);
        }

        /// <summary>
        /// Adds a new log to the <see cref="DebugConsoleLogger"/> using all of the assigned information.
        /// </summary>
        public static void LogMessage(string message, string stackTrace, LogType logType = LogType.Log)
        {
            DebugConsoleLogger.LogMessage(message, stackTrace, logType);
        }

        /// <summary>
        /// Clears the <see cref="DebugConsoleLogger"/> of all logs.
        /// </summary>
        public static void ClearLog()
        {
            DebugConsoleLogger.ClearLog();
        }

        public static bool IsOpen
        {
            get => _oldIsOpen;
            set
            {
                if (_oldIsOpen == value)
                {
                    return;
                }

                _oldIsOpen = value;

                // Invoke events
                if (_oldIsOpen)
                {
                    OnOpen?.Invoke();
                }
                else
                {
                    OnClose?.Invoke();
                }
            }
        }
        private static bool _oldIsOpen = false;

        private DebugConsoleCommandPrompt _commandPrompt;
        private DebugConsoleLogger _logger;
        private DebugConsoleComponentDebug _componentDebug;

        private void Update()
        {
            // Check for input (F12 key)
            if (Keyboard.current != null && Keyboard.current.f12Key.wasPressedThisFrame && (CanUseConsole || IsOpen))
            {
                // Toggle the console on/off
                IsOpen = !IsOpen;

                // Reset console on open
                if (IsOpen)
                {
                    _commandPrompt.OnOpen();
                    _logger.OnOpen();
                    _componentDebug.OnOpen();
                }
            }
        }

        private void OnGUI()
        {
            // Console is not open so return
            // Exception is for when the Component Debug window requests to be open
            if (!IsOpen && !_componentDebug.ShouldStayOpen)
            {
                return;
            }

            // Store the current event for later usage...
            Event evt = Event.current;

            // Console is open, so start drawing the console
            using (new GUISkinScope(DebugGUISkin.Skin))
            {
                using (ScaleGUIToResolutionScope scope = new ScaleGUIToResolutionScope(TARGET_SCREEN_WIDTH, TARGET_SCREEN_HEIGHT))
                {
                    if (IsOpen)
                    {
                        // Draw command prompt
                        Rect commandPromptRect = new Rect(0, DebugConsoleCommandPrompt.CONSOLE_Y_POSITION, scope.Width, 40);

                        _commandPrompt.Draw(commandPromptRect, evt);
                    }

                    // Draw logger
                    Rect loggerRect = new Rect(0, 0, scope.Width, 150);
                    loggerRect.y = scope.Height - loggerRect.height;

                    Rect componentDebugRect = loggerRect;
                    componentDebugRect.width = 250;
                    componentDebugRect.x = scope.Width - componentDebugRect.width;

                    loggerRect.width -= componentDebugRect.width;

                    if (IsOpen)
                    {
                        _logger.Draw(loggerRect, evt);
                    }

                    // Draw component debug
                    _componentDebug.Draw(componentDebugRect, evt);
                }
            }

        }
    }
}