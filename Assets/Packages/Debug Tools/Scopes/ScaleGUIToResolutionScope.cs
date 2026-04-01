using System;
using UnityEngine;

namespace DebugTools
{
    /// <summary>
    /// A GUI Scope which scales the <see cref="GUI.matrix"/> to a given resolution
    /// </summary>
    public class ScaleGUIToResolutionScope : IDisposable
    {
        /// <summary>
        /// The resulting width after the scale.
        /// </summary>
        public float Width { get; private set; }
        /// <summary>
        /// The resulting height after the scale.
        /// </summary>
        public float Height { get; private set; }
        /// <summary>
        /// The resulting size after the scale.
        /// </summary>
        public Vector2 Size { get; private set; }
        /// <summary>
        /// A <see cref="Rect"/> that has the position (0, 0) and the size of the <see cref="Size"/> property.
        /// </summary>
        public Rect FullRect { get; private set; }
        /// <summary>
        /// The starting <see cref="Matrix4x4"/> before this scope scaled it.
        /// </summary>
        public Matrix4x4 StartMatrix { get; private set; }

        public ScaleGUIToResolutionScope(Vector2Int size) : this(size.x, size.y)
        {

        }

        public ScaleGUIToResolutionScope(Vector2 size) : this(size.x, size.y)
        {

        }

        public ScaleGUIToResolutionScope(int desiredWidth, int desiredHeight) : this((float)desiredWidth, (float)desiredHeight)
        {

        }

        public ScaleGUIToResolutionScope(float desiredWidth, float desiredHeight)
        {
            // Transform the GUI matrix to scale correctly with the size of the screen
            // Without this: The bigger the screen, the smaller the UI would be
            // So this simply fixes those scale issues
            float scaleX = Screen.width / desiredWidth;
            float scaleY = Screen.height / desiredHeight;
            float scale = (scaleX + scaleY) / 2;

            Width = Screen.width / scale;
            Height = Screen.height / scale;
            Size = new Vector2(Width, Height);
            FullRect = new Rect(Vector2.zero, Size);

            // Also save the starting matrix so we can later revert it
            StartMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1));
        }

        public void Dispose()
        {
            // Revert back to the starting matrix so no other GUI gets messed up
            GUI.matrix = StartMatrix;
        }
    }
}
