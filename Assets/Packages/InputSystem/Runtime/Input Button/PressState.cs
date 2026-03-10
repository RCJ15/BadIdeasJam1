namespace Input
{
    /// <summary>
    /// An <see cref="Enum"/> that represents the current state of an <see cref="InputButton"/>.
    /// </summary>
    public enum PressState
    {
        /// <summary>
        /// The button is not being pressed currently.
        /// </summary>
        NotPressed,
        /// <summary>
        /// The button was just pressed this frame.
        /// </summary>
        Down,
        /// <summary>
        /// The button is currently being held down.
        /// </summary>
        Pressed,
        /// <summary>
        /// The button was just released this frame.
        /// </summary>
        Up,
    }
}
