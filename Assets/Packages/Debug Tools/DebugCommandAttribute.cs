using System;

namespace DebugTools
{
    /// <summary>
    /// Attach this attribute to any static method and that method will automatically become a usable command in the <see cref="DebugConsole"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class DebugCommandAttribute : Attribute
    {
        /// <summary>
        /// The name of the command as shown in the <see cref="DebugConsole"/>. If left empty, then the name of the method this attribute is applied to will be used instead.
        /// </summary>
        public string DisplayName;

        /// <summary>
        /// The description of this command that will display in the <see cref="DebugConsole"/>.
        /// </summary>
        public string Description => _description;
        private string _description;

        /// <summary>
        /// If the <see cref="DebugConsole"/> should close when this command is executed. Is true by default.
        /// </summary>
        public bool CloseDebugConsole = true;

        public DebugCommandAttribute()
        {

        }

        public DebugCommandAttribute(string description)
        {
            _description = description;
        }
    }
}
