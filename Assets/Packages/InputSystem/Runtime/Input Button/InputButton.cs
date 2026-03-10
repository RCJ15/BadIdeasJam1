using UnityEngine.InputSystem;

namespace Input
{
    /// <summary>
    /// The base class for any Input that <see cref="GameInputManager"/> will activate.
    /// </summary>
    public abstract class InputButton
    {
        public InputButton()
        {
        }

        /// <summary>
        /// The <see cref="UnityEngine.InputSystem.InputAction"/> that is attached to this <see cref="InputButton"/>.
        /// </summary>
        public InputAction InputAction { get; private set; }

        /// <summary>
        /// Enables the <see cref="UnityEngine.InputSystem.InputAction"/> that is attached to this <see cref="InputButton"/>.
        /// </summary>
        public void Enable() => InputAction.Enable();

        /// <summary>
        /// Disables the <see cref="UnityEngine.InputSystem.InputAction"/> that is attached to this <see cref="InputButton"/>.
        /// </summary>
        public void Disable() => InputAction.Disable();

        /// <summary>
        /// Wether or not the <see cref="InputAction"/> is enabled or disabled.
        /// </summary>
        public bool Enabled => InputAction != null && InputAction.enabled;

        /// <summary>
        /// Called every frame. This should never be called outside of the <see cref="GameInputManager"/> script.
        /// </summary>
        internal abstract void Update();
    }
}
