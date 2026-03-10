using System;

namespace Input
{
    /// <summary>
    /// A <see cref="Attribute"/> that contains metadata for an input in the <see cref="GameInput"/> script.
    /// </summary>
    public class InputMetadataAttribute : Attribute
    {
        /// <summary>
        /// The display name of this input.
        /// </summary>
        public string DisplayName => _displayName;
        private string _displayName;

        /// <summary>
        /// The naming format this input will use if it's a Composite input.
        /// </summary>
        public string CompositeFormat
        {
            get
            {
                if (string.IsNullOrEmpty(_compositeFormat))
                {
                    _compositeFormat = _displayName + " {0}";
                }

                return _compositeFormat;
            }
        }
        private string _compositeFormat;

        public bool CantBeUnbound = false;

        public InputMetadataAttribute()
        {

        }

        public InputMetadataAttribute(string displayName) : this()
        {
            _displayName = displayName;
        }

        public InputMetadataAttribute(string displayName, string compositeFormat) : this(displayName)
        {
            _compositeFormat = compositeFormat;
        }
    }
}
