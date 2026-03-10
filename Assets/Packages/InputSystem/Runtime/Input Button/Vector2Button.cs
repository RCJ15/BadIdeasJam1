using UnityEngine;

namespace Input
{
    /// <summary>
    /// A <see cref="ValueButton{T}"/> variant that only takes in <see cref="Vector2"/> values. Allows the ability to check whenever up, down, left & right is being just pressed down, held or released.
    /// <summary>
    public class Vector2Button : ValueButton<Vector2>
    {
        /// <summary>
        /// The State of the Up button on this <see cref="ValueButton{T}"/>.
        /// </summary>
        public PressStateTracker Up;
        /// <summary>
        /// The State of the Down button on this <see cref="ValueButton{T}"/>.
        /// </summary>
        public PressStateTracker Down;
        /// <summary>
        /// The State of the Left button on this <see cref="ValueButton{T}"/>.
        /// </summary>
        public PressStateTracker Left;
        /// <summary>
        /// The State of the Right button on this <see cref="ValueButton{T}"/>.
        /// </summary>
        public PressStateTracker Right;

        public float X => Value.x;
        public float Y => Value.y;

        public Vector2Button() : base() { }

        internal override void Update()
        {
            base.Update();

            bool enabled = Enabled;

            Up.Update(enabled && Y > 0);
            Down.Update(enabled && Y < 0);

            Left.Update(enabled && X < 0);
            Right.Update(enabled && X > 0);
        }
    }
}
