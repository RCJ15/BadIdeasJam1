using System;
using System.Collections;

using UnityEngine;
using UnityEngine.InputSystem;

namespace Input
{
    /// <summary>
    /// Handles setting the current <see cref="ControlScheme"/> the player is using, along with a few other device related events.
    /// </summary>
    public class ControlSchemeManager : MonoBehaviour
    {
        /// <summary>
        /// The current <see cref="ControlScheme"/> the player is using.
        /// </summary>
        public static ControlScheme CurrentControlScheme
        {
            get => _currentControlSchemeCache;

            private set
            {
                // Only update the current control scheme if it's not the scheme we already have
                if (_currentControlSchemeCache == value)
                {
                    return;
                }

                _currentControlSchemeCache = value;

                // Call event seeing as a new control scheme was selected
                OnChangeControlScheme?.Invoke(value);
            }
        }
        private static ControlScheme _currentControlSchemeCache = ControlScheme.Keyboard;

        /// <summary>
        /// Is called whenever <see cref="CurrentControlScheme"/> changes value.
        /// </summary>
        public static Action<ControlScheme> OnChangeControlScheme;

        /// <summary>
        /// Is called whenever a new <see cref="InputDevice"/> is connected (or as Unity calls it: "regained").
        /// </summary>
        public static Action<InputDevice> OnDeviceConnect;

        /// <summary>
        /// Is called whenever a new <see cref="InputDevice"/> is disconnected (or as Unity calls it: "lost").
        /// </summary>
        public static Action<InputDevice> OnDeviceDisconnect;

        private PlayerInput _playerInput;

        private void Start()
        {
            // Setup player input
            _playerInput = GetComponent<PlayerInput>();
            _playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;

            // Subscribe to events
            _playerInput.onControlsChanged += OnControlsChanged;

            // Set actions for the player input
            StartCoroutine(SetPlayerInputActions());
        }

        private void OnControlsChanged(PlayerInput playerInput)
        {
            // Convert current control scheme to ControlScheme enum
            if (!Enum.TryParse(typeof(ControlScheme), playerInput.currentControlScheme, out object result))
            {
                return;
            }

            // Set control scheme
            CurrentControlScheme = (ControlScheme)result;
        }

        private IEnumerator SetPlayerInputActions()
        {
            // Try to set the actions of the player input component forever until it eventually succeeds (async programming be like)
            while (_playerInput.actions == null)
            {
                _playerInput.actions = GameInputManager.Asset;

                yield return null;
            }
        }


        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            // Create permanent singleton instance
            DontDestroyOnLoad(new GameObject("Control Scheme Manager", typeof(PlayerInput), typeof(ControlSchemeManager)));
        }
    }
}
