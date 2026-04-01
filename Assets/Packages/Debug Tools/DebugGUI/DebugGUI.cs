using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DebugTools
{
    /// <summary>
    /// Static class that contains some useful methods for drawing <see cref="GUI"/> in a <see cref="IComponentDebugGUI"/> draw call.
    /// <summary>
    public static class DebugGUI
    {
        public const float DEFAULT_SPACING = 8;
        public static float PrefixLabelWidth = 50;
        public static float HorizontalSpacing = 4;

        private static readonly MethodInfo _GUIDoTextFieldMethod = typeof(GUI).GetMethod("DoTextField", BindingFlags.Static | BindingFlags.NonPublic, null,
            new Type[] { typeof(Rect), typeof(int), typeof(GUIContent), typeof(bool), typeof(int), typeof(GUIStyle) },
            null);
        private static readonly object[] _GUIDoTextFieldParameters = new object[6];

        private static readonly GUIContent _tempContent = new GUIContent();

        public static GUIContent TempGUIContent(string label)
        {
            _tempContent.text = label;
            _tempContent.tooltip = string.Empty;
            _tempContent.image = null;
            return _tempContent;
        }

        public static Rect GetLayoutRect(string label, GUIStyle style)
        {
            return GUILayoutUtility.GetRect(TempGUIContent(label), style);
        }

        /// <summary>
        /// Draws a simple label.
        /// </summary>
        public static void Label(string label)
        {
            GUILayout.Label(label);
        }

        /// <summary>
        /// Draws a simple label.
        /// </summary>
        public static void Label(Rect rect, string label)
        {
            GUI.Label(rect, label);
        }

        /// <summary>
        /// Draws a prefix label and returns the remaining <see cref="Rect"/>.
        /// </summary>
        public static Rect PrefixLabel(string label)
        {
            return PrefixLabel(label, GUI.skin.label);
        }

        /// <summary>
        /// Draws a prefix label and returns the remaining <see cref="Rect"/>.
        /// </summary>
        public static Rect PrefixLabel(string label, GUIStyle style)
        {
            GUIContent content = TempGUIContent(label);

            Rect rect = GUILayoutUtility.GetRect(content, style);

            float fullWidth = rect.width;

            rect.width = PrefixLabelWidth;
            GUI.Label(rect, content);

            rect.x += rect.width + HorizontalSpacing;
            rect.width = fullWidth - (rect.width + HorizontalSpacing);

            return rect;
        }

        /// <summary>
        /// Draws a value label. <para/>
        /// Example: "My Integer: 100"
        /// </summary>
        public static void Label(string label, object value)
        {
            Rect rect = PrefixLabel(label);

            bool startingEnabled = GUI.enabled;

            GUI.enabled = false;

            Property(rect, value, value.GetType());

            GUI.enabled = startingEnabled;
        }

        #region Properties
        /// <summary>
        /// Draws a editable property with a prefix label. <para/>
        /// Currently only supports <see cref="int"/>, <see cref="float"/>, <see cref="string"/>, <see cref="bool"/>, 
        /// <see cref="Vector2"/>, <see cref="Vector2Int"/>, <see cref="Vector3"/> and <see cref="Vector3Int"/>.
        /// </summary>
        public static T Property<T>(string label, T value)
        {
            return (T)Property(label, value, typeof(T));
        }

        /// <summary>
        /// Draws a editable property with a prefix label. <para/>
        /// Currently only supports <see cref="int"/>, <see cref="float"/>, <see cref="string"/>, <see cref="bool"/>, 
        /// <see cref="Vector2"/>, <see cref="Vector2Int"/>, <see cref="Vector3"/> and <see cref="Vector3Int"/>.
        /// </summary>
        public static object Property(string label, object value)
        {
            return Property(label, value, value.GetType());
        }

        /// <summary>
        /// Draws a editable property with a prefix label. <para/>
        /// Currently only supports <see cref="int"/>, <see cref="float"/>, <see cref="string"/>, <see cref="bool"/>, 
        /// <see cref="Vector2"/>, <see cref="Vector2Int"/>, <see cref="Vector3"/> and <see cref="Vector3Int"/>.
        /// </summary>
        public static void Property<T>(string label, ref T value)
        {
            value = (T)Property(label, value, typeof(T));
        }

        /// <summary>
        /// Draws a editable property with a prefix label. <para/>
        /// Currently only supports <see cref="int"/>, <see cref="float"/>, <see cref="string"/>, <see cref="bool"/>, 
        /// <see cref="Vector2"/>, <see cref="Vector2Int"/>, <see cref="Vector3"/> and <see cref="Vector3Int"/>.
        /// </summary>
        public static void Property(string label, ref object value)
        {
            value = Property(label, value, value.GetType());
        }

        /// <summary>
        /// Draws a editable property with a prefix label. <para/>
        /// Currently only supports <see cref="int"/>, <see cref="float"/>, <see cref="string"/>, <see cref="bool"/>, 
        /// <see cref="Vector2"/>, <see cref="Vector2Int"/>, <see cref="Vector3"/> and <see cref="Vector3Int"/>.
        /// </summary>
        public static object Property(string label, object value, Type valueType)
        {
            Rect rect = PrefixLabel(label);

            return Property(rect, value, valueType);
        }

        /// <summary>
        /// Draws a editable property. <para/>
        /// Currently only supports <see cref="int"/>, <see cref="float"/>, <see cref="string"/>, <see cref="bool"/>, 
        /// <see cref="Vector2"/>, <see cref="Vector2Int"/>, <see cref="Vector3"/> and <see cref="Vector3Int"/>.
        /// </summary>
        public static object Property(Rect rect, object value, Type valueType)
        {
            // Int
            if (valueType == typeof(int))
            {
                return IntField(rect, (int)value);
            }
            // Float
            else if (valueType == typeof(float))
            {
                return FloatField(rect, (float)value);
            }
            // String
            else if (valueType == typeof(string))
            {
                return TextField(rect, (string)value);
            }
            // Bool
            else if (valueType == typeof(bool))
            {
                return Toggle(rect, (bool)value);
            }
            // Vector2
            else if (valueType == typeof(Vector2))
            {
                return Vector2Field(rect, (Vector2)value);
            }
            // Vector2Int
            else if (valueType == typeof(Vector2Int))
            {
                return Vector2IntField(rect, (Vector2Int)value);
            }
            // Vector3
            else if (valueType == typeof(Vector3))
            {
                return Vector3Field(rect, (Vector3)value);
            }
            // Vector3Int
            else if (valueType == typeof(Vector3Int))
            {
                return Vector3IntField(rect, (Vector3Int)value);
            }
            // Default behavior
            else
            {
                bool startingEnabled = GUI.enabled;

                GUI.enabled = false;

                TextField(rect, value == null ? "null" : value.ToString());

                GUI.enabled = startingEnabled;

                return value;
            }
        }

        #region Specific Types
        private static readonly HashSet<char> _integerChars = new HashSet<char>()
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
        };
        /// <summary>
        /// Draws a <see cref="int"/> field and returns the resulting value.
        /// </summary>
        public static int IntField(Rect rect, int value)
        {
            if (!GUI.enabled)
            {
                TextField(rect, value.ToString());

                return value;
            }

            return Mathf.RoundToInt(NumberField(rect, value, _integerChars));
        }

        private static readonly HashSet<char> _floatChars = new HashSet<char>()
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', ','
        };

        /// <summary>
        /// Draws a <see cref="float"/> field and returns the resulting value.
        /// </summary>
        public static float FloatField(Rect rect, float value)
        {
            return NumberField(rect, value, _floatChars);
        }

        #region Slider
        /// <summary>
        /// Draws a slider between a <paramref name="min"/> and <paramref name="max"/> point. Will return the resulting value.
        /// </summary>
        public static float Slider(float value, float min, float max)
        {
            return GUILayout.HorizontalSlider(value, min, max);
        }

        /// <summary>
        /// Draws a slider between a <paramref name="min"/> and <paramref name="max"/> point. Will return the resulting value.
        /// </summary>
        public static float Slider(string label, float value, float min, float max)
        {
            Rect rect = PrefixLabel(label);

            return Slider(rect, value, min, max);
        }

        /// <summary>
        /// Draws a slider between a <paramref name="min"/> and <paramref name="max"/> point. Will return the resulting value.
        /// </summary>
        public static float Slider(Rect rect, float value, float min, float max)
        {
            return GUI.HorizontalSlider(rect, value, min, max);
        }
        #endregion

        private static readonly Dictionary<int, string> _storedNumberFieldValues = new Dictionary<int, string>();
        /// <summary>
        /// Draws a <see cref="float"/> field and returns the resulting value.
        /// </summary>
        public static float NumberField(Rect rect, float value, HashSet<char> limitedChars = null)
        {
            // Def not overcomplicated nope not at all
            string oldValue = value.ToString();

            if (!GUI.enabled)
            {
                TextField(rect, oldValue);

                return value;
            }

            int controlID = GUIUtility.GetControlID(FocusType.Keyboard, rect);

            string text;

            if (_storedNumberFieldValues.ContainsKey(controlID))
            {
                text = _storedNumberFieldValues[controlID];
                _storedNumberFieldValues.Remove(controlID);
            }
            else
            {
                text = oldValue;
            }

            GUIContent content = TempGUIContent(text);

            DoTextField(rect, controlID, content);

            string newValue = content.text;

            if (GUIUtility.keyboardControl == controlID)
            {
                if (limitedChars != null)
                {
                    string limitedValue = string.Empty;

                    foreach (char c in newValue)
                    {
                        if (!limitedChars.Contains(c))
                        {
                            continue;
                        }

                        limitedValue += c;
                    }

                    newValue = limitedValue;
                }

                _storedNumberFieldValues[controlID] = newValue;

                return value;
            }

            if (newValue == oldValue || !float.TryParse(newValue, out float result))
            {
                return value;
            }
            
            return result;
        }

        /// <summary>
        /// Draws a <see cref="string"/> field and returns the resulting value.
        /// </summary>
        public static string TextField(Rect rect, string text)
        {
            if (!GUI.enabled)
            {
                GUI.Box(rect, text, GUI.skin.textField);

                return text;
            }

            return TextField(rect, text, out int _);
        }

        /// <summary>
        /// Draws a <see cref="string"/> field and returns the resulting value with the <paramref name="textEditor"/> as an out parameter.
        /// </summary>
        public static string TextField(Rect rect, string text, out TextEditor textEditor)
        {
            text = TextField(rect, text, out int controlID);

            textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), controlID);

            return text;
        }

        /// <summary>
        /// Draws a <see cref="string"/> field and returns the resulting value with the <paramref name="controlID"/> as an out parameter.
        /// </summary>
        public static string TextField(Rect rect, string text, out int controlID)
        {
            GUIContent content = TempGUIContent(text);

            controlID = GUIUtility.GetControlID(FocusType.Keyboard, rect);

            DoTextField(rect, controlID, content);

            return content.text;
        }

        /// <summary>
        /// Draws a <see cref="string"/> field with a <paramref name="controlID"/> and the resulting value is within <paramref name="content"/>.
        /// </summary>
        public static void DoTextField(Rect rect, int controlID, GUIContent content)
        {
            _GUIDoTextFieldParameters[0] = rect;
            _GUIDoTextFieldParameters[1] = controlID;
            _GUIDoTextFieldParameters[2] = content;
            _GUIDoTextFieldParameters[3] = false;
            _GUIDoTextFieldParameters[4] = -1;
            _GUIDoTextFieldParameters[5] = GUI.skin.textField;

            _GUIDoTextFieldMethod.Invoke(null, _GUIDoTextFieldParameters);
        }

        /// <summary>
        /// Draws a <see cref="bool"/> field and returns the resulting value.
        /// </summary>
        public static bool Toggle(Rect rect, bool value)
        {
            return GUI.Toggle(rect, value, GUIContent.none);
        }

        /// <summary>
        /// Fills the given <paramref name="rects"/> array with splits covering the <paramref name="coverArea"/> and adding <paramref name="spacing"/>.
        /// </summary>
        public static void SplitRects(Rect coverArea, Rect[] rects, float spacing)
        {
            int rectAmount = rects.Length;

            if (rectAmount <= 0)
            {
                return;
            }
            else if (rectAmount == 1)
            {
                rects[0] = coverArea;
                return;
            }

            float splitWidth = (float)coverArea.width / (float)rectAmount;
            float halfSpacing = spacing / 2;

            for (int i = 0; i < rectAmount; i++)
            {
                Rect rect = coverArea;

                rect.x = coverArea.x + (splitWidth * i) + (i <= 0 ? 0 : halfSpacing);

                rect.width = splitWidth;

                if (i == 0 || i == rectAmount - 1)
                {
                    rect.width -= halfSpacing;
                }
                else
                {
                    rect.width -= spacing * i;
                }

                rects[i] = rect;
            }
        }

        private static readonly Rect[] _vector2Rects = new Rect[2];
        /// <summary>
        /// Draws a <see cref="Vector2"/> field and returns the resulting value.
        /// </summary>
        public static Vector2 Vector2Field(Rect rect, Vector2 value)
        {
            SplitRects(rect, _vector2Rects, HorizontalSpacing);

            value.x = FloatField(_vector2Rects[0], value.x);
            value.y = FloatField(_vector2Rects[1], value.y);

            return value;
        }

        /// <summary>
        /// Draws a <see cref="Vector2Int"/> field and returns the resulting value.
        /// </summary>
        public static Vector2Int Vector2IntField(Rect rect, Vector2Int value)
        {
            SplitRects(rect, _vector2Rects, HorizontalSpacing);

            value.x = IntField(_vector2Rects[0], value.x);
            value.y = IntField(_vector2Rects[1], value.y);

            return value;
        }

        private static readonly Rect[] _vector3Rects = new Rect[3];
        /// <summary>
        /// Draws a <see cref="Vector3"/> field and returns the resulting value.
        /// </summary>
        public static Vector3 Vector3Field(Rect rect, Vector3 value)
        {
            SplitRects(rect, _vector3Rects, HorizontalSpacing);

            value.x = FloatField(_vector3Rects[0], value.x);
            value.y = FloatField(_vector3Rects[1], value.y);
            value.z = FloatField(_vector3Rects[2], value.z);

            return value;
        }

        /// <summary>
        /// Draws a <see cref="Vector3Int"/> field and returns the resulting value.
        /// </summary>
        public static Vector3Int Vector3IntField(Rect rect, Vector3Int value)
        {
            SplitRects(rect, _vector3Rects, HorizontalSpacing);

            value.x = IntField(_vector3Rects[0], value.x);
            value.y = IntField(_vector3Rects[1], value.y);
            value.z = IntField(_vector3Rects[2], value.z);

            return value;
        }
        #endregion
        #endregion

        /// <summary>
        /// Creates some space. Exact amount of space is the value of <see cref="DEFAULT_SPACING"/>.
        /// </summary>
        public static void Space()
        {
            Space(DEFAULT_SPACING);
        }

        /// <summary>
        /// Creates some space.
        /// </summary>
        public static void Space(float spacing)
        {
            GUILayout.Space(spacing);
        }
    }
}
