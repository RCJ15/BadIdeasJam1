using System;

namespace Input
{
    /// <summary>
    /// A container class that tracks the <see cref="PressState"/> of a <see cref="PressButton"/>. A buttons state can either be: Just pressed down, currently being pressed or just released. Can be used for other <see cref="InputButton"/> types, such as <see cref="Vector2Button"/>.
    /// </summary>
    public struct PressStateTracker
    {
        /// <summary>
        /// True if the button was just pressed this frame.
        /// </summary>
        public bool Down { get; private set; }
        /// <summary>
        /// Always true whilst this button is pressed down.
        /// </summary>
        public bool Pressed { get; private set; }
        /// <summary>
        /// True if the button was just released this frame.
        /// </summary>
        public bool Up { get; private set; }

        private bool _oldPressed;

        /// <summary>
        /// Called whenever <see cref="Down"/> becomes true.
        /// </summary>
        public Action OnDown { get; set; }
        /// <summary>
        /// Called whilst <see cref="Pressed"/> is true (functions as an update function basically).
        /// </summary>
        public Action OnPressed { get; set; }
        /// <summary>
        /// Called whenever <see cref="Up"/> becomes true.
        /// </summary>
        public Action OnUp { get; set; }

        /// <summary>
        /// The current state of the button.
        /// </summary>
        public PressState State { get; private set; }

        private PressState _oldState;

        /// <summary>
        /// Called whenever <see cref="State"/> changes value.
        /// </summary>
        public Action<PressState> OnStateChange { get; set; }

        /// <summary>
        /// Updates the booleans.
        /// </summary>
        public void Update(bool pressed)
        {
            // Reset Down and Up if they are currently true
            if (Down)
            {
                Down = false;
            }
            if (Up)
            {
                Up = false;
            }

            // Set Pressed
            Pressed = pressed;

            // Set the state
            State = Pressed ? PressState.Pressed : PressState.NotPressed;

            if (Pressed)
            {
                OnPressed?.Invoke();
            }

            // Compare our current Pressed with our old Pressed
            if (Pressed != _oldPressed)
            {
                // Pressed has changed, meaning that we should set Down and Up
                if (Pressed)
                {
                    Down = true;

                    OnDown?.Invoke();

                    State = PressState.Down;
                }
                else
                {
                    Up = true;

                    OnUp?.Invoke();

                    State = PressState.Up;
                }
            }

            // Compare our current State with our old State
            if (State != _oldState)
            {
                OnStateChange?.Invoke(State);
            }

            // Set old variables
            _oldPressed = Pressed;
            _oldState = State;
        }

        public static implicit operator bool(PressStateTracker state) => state.Down;
    }
}
