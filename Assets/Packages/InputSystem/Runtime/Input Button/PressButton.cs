using System;
using UnityEngine.InputSystem;

namespace Input
{
    /// <summary>
    /// The <see cref="InputButton"/> class for a basic button that can be pressed down and released.
    /// </summary>
    public class PressButton : InputButton
    {
        public PressButton() : base() { }

        /// <summary>
        /// True if the button was just pressed this frame.
        /// </summary>
        public bool Down => _stateTracker.Down;
        /// <summary>
        /// Always true whilst this button is pressed down.
        /// </summary>
        public bool Pressed => _stateTracker.Pressed;
        /// <summary>
        /// True if the button was just released this frame.
        /// </summary>
        public bool Up => _stateTracker.Up;

        /// <summary>
        /// Called whenever <see cref="Down"/> becomes true.
        /// </summary>
        public Action OnDown { get => _stateTracker.OnDown; set => _stateTracker.OnDown = value; }
        /// <summary>
        /// Called whilst <see cref="Pressed"/> is true (functions as an update function basically).
        /// </summary>
        public Action OnPressed { get => _stateTracker.OnPressed; set => _stateTracker.OnPressed = value; }
        /// <summary>
        /// Called whenever <see cref="Up"/> becomes true.
        /// </summary>
        public Action OnUp { get => _stateTracker.OnUp; set => _stateTracker.OnUp = value; }

        /// <summary>
        /// The current state of the button.
        /// </summary>
        public PressState State => _stateTracker.State;
        /// <summary>
        /// Called whenever <see cref="State"/> changes value.
        /// </summary>
        public Action<PressState> OnStateChange { get => _stateTracker.OnStateChange; set => _stateTracker.OnStateChange = value; }

        private PressStateTracker _stateTracker;

        internal override void Update()
        {
            // Set Pressed
            _stateTracker.Update(Enabled && InputAction.IsPressed());
        }

        public static implicit operator bool(PressButton button) => button._stateTracker;
    }
}
