using UnityEngine.InputSystem;

namespace Input
{
    /// <summary>
    /// The <see cref="InputButton"/> class for a button that can receive and read a value of type <typeparamref name="T"/>.
    /// <summary>
    public class ValueButton<T> : InputButton where T : struct
    {
        /// <summary>
        /// The value that this <see cref="ValueButton{T}"/> will set in <see cref="Update"/>.
        /// </summary>
        public T Value { get; private set; }

        public ValueButton() : base() { }

        internal override void Update()
        {
            Value = Enabled ? InputAction.ReadValue<T>() : default;
        }

        public static implicit operator T(ValueButton<T> button) => button.Value;
    }
}

