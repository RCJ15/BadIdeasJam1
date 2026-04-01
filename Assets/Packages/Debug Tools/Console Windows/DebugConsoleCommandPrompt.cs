using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace DebugTools
{
    /// <summary>
    /// The command prompt window that appears at the top of the <see cref="DebugConsole"/>.
    /// </summary>
    public class DebugConsoleCommandPrompt : DebugConsoleWindow
    {
        public const float CONSOLE_Y_POSITION = 150;

        public const string SEARCH_FIELD_CONTROL_NAME = "DEBUG Command Prompt Search Field";

        private static StringBuilder _stringBuilder = new StringBuilder();

        private static readonly BindingFlags _bindingFlags = BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Static;

        private static List<DebugCommand> _debugCommands = new List<DebugCommand>();

        private string _playerInput;
        private string _oldPlayerInput;

        private bool _lastActionWasTabInput = false;
        private int _offset;

        private bool _justOpened;
        private bool _justPressedTab;
        private int _framesAwaitingEmtpySelection;

        private List<DebugCommand> _shownCommands = new List<DebugCommand>();
        
        public DebugConsoleCommandPrompt()
        {
            // Loop through every single data type in every assembly in the entire AppDomain
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    // Get every method on the type
                    foreach (MethodInfo method in type.GetMethods(_bindingFlags))
                    {
                        // Get the Debug Command Attribute on the method
                        DebugCommandAttribute attribute = method.GetCustomAttribute<DebugCommandAttribute>();

                        // Ignore this if the attribute is null
                        if (attribute == null)
                        {
                            continue;
                        }

                        // Add the command to the command list
                        _debugCommands.Add(new DebugCommand(string.IsNullOrEmpty(attribute.DisplayName) ? method.Name : attribute.DisplayName, attribute.Description, attribute.CloseDebugConsole, method));
                    }
                }
            }

            // Sort every debug command by how they are named
            _debugCommands.Sort((a, b) => a.Name.CompareTo(b.Name));

            AddAllCommandsToShownCommands();
        }

        public void AddAllCommandsToShownCommands()
        {
            _shownCommands.Clear();

            foreach (DebugCommand command in _debugCommands)
            {
                _shownCommands.Add(command);
            }
        }

        public override void OnOpen()
        {
            _playerInput = "";

            _justOpened = true;
            _justPressedTab = false;

            _lastActionWasTabInput = false;
            _offset = 0;
        }

        public override void Draw(Rect rect, Event evt)
        {
            // Check if the up or down arrows were pressed
            bool upArrowPressed = false;
            bool downArrowPressed = false;

            if (evt.isKey && evt.type == EventType.KeyDown)
            {
                // Use any of the events so that the Text Field cannot use the events as well
                // We will later do some action with the arrow keys but not here since stuff hasn't been done yet
                if (evt.keyCode == KeyCode.UpArrow)
                {
                    upArrowPressed = true;

                    evt.Use();
                }
                else if (evt.keyCode == KeyCode.DownArrow)
                {
                    downArrowPressed = true;

                    evt.Use();
                }
            }

            if (_justPressedTab && _framesAwaitingEmtpySelection == 0)
            {
                GUIUtility.keyboardControl = 0;
            }

            // Draw the main text input field
            GUI.SetNextControlName(SEARCH_FIELD_CONTROL_NAME);

            _playerInput = DebugGUI.TextField(rect, _playerInput, out TextEditor textEditor);

            if (_justPressedTab)
            {
                _framesAwaitingEmtpySelection++;

                if (!string.IsNullOrEmpty(textEditor.SelectedText))
                {
                    textEditor.MoveTextEnd();

                    _justPressedTab = false;
                }
            }

            if (_justOpened)
            {
                // Focus on the input field control immedietely when the console is opened
                GUI.FocusControl(SEARCH_FIELD_CONTROL_NAME);

                _justOpened = false;
            }

            // Detect if our value has changed, it's empty and if the text field is focused
            bool inputChanged = _oldPlayerInput != _playerInput;
            bool isEmpty = string.IsNullOrEmpty(_playerInput);

            if (inputChanged)
            {
                _oldPlayerInput = _playerInput;
                _lastActionWasTabInput = false;
                _offset = 0;
            }

            // Determine the search query
            string searchQuery;

            if (isEmpty)
            {
                searchQuery = "";
            }
            else
            {
                int firstSpaceIndex = _playerInput.IndexOf(' ');

                searchQuery = (firstSpaceIndex < 0 ? _playerInput : _playerInput.Substring(0, firstSpaceIndex)).ToLower();
            }

            // Have placeholder text if the there is no text and it's not focused
            if (isEmpty && GUI.GetNameOfFocusedControl() != SEARCH_FIELD_CONTROL_NAME)
            {
                GUI.Label(rect, "Insert command...", GUI.skin.customStyles[3]);
            }

            // Check if the input has been changed
            if (inputChanged)
            {
                // Clear the shown commands
                _shownCommands.Clear();

                // Show all debug commands if it's empty
                if (isEmpty)
                {
                    AddAllCommandsToShownCommands();
                }
                // Show only a select few debug commands based on what the player has typed in
                else
                {
                    foreach (DebugCommand command in _debugCommands)
                    {
                        if (command.LowerName.Contains(searchQuery))
                        {
                            _shownCommands.Add(command);
                        }
                    }
                }
            }

            int shownCommandsCount = _shownCommands.Count;

            // Increase or decrease the offset whenever one of the arrows are pressed
            if (upArrowPressed)
            {
                _offset++;

                // Prevent out of range
                if (_offset >= shownCommandsCount)
                {
                    _offset = 0;
                }
            }
            else if (downArrowPressed)
            {
                _offset--;

                // Prevent out of range
                if (_offset < 0)
                {
                    _offset = shownCommandsCount - 1;
                }
            }

            if (upArrowPressed || downArrowPressed)
            {
                _lastActionWasTabInput = false;
            }

            // Draw the textbox which displays every command
            _stringBuilder.Clear();
            int commandDisplayOffset = _offset;
            string lastCommandName = null;

            for (int i = shownCommandsCount - 1 + commandDisplayOffset; i >= commandDisplayOffset; i--)
            {
                DebugCommand command = _shownCommands[i % shownCommandsCount];

                if (i == commandDisplayOffset)
                {
                    lastCommandName = command.ToString();
                }
                else
                {
                    _stringBuilder.AppendLine(command.ToString());
                }
            }

            rect.y = 0;
            rect.height = CONSOLE_Y_POSITION;

            GUI.Label(rect, shownCommandsCount == 0 ? "No Commands Match" : lastCommandName, GUI.skin.customStyles[4]);

            if (shownCommandsCount != 0)
            {
                GUI.Label(rect, _stringBuilder.ToString(), GUI.skin.customStyles[6]);
            }

            // Detect if return (enter) is pressed and if the input string is not empty
            if (!isEmpty && evt.isKey && (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter))
            {
                DebugCommand targetCommand = null;

                foreach (DebugCommand command in _shownCommands)
                {
                    if (command.LowerName != searchQuery)
                    {
                        continue;
                    }

                    targetCommand = command;
                    break;
                }

                try
                {
                    if (targetCommand != null)
                    {
                        targetCommand.Invoke(_playerInput);
                    }
                }
                finally
                {
                    _playerInput = "";
                    evt.Use();
                }
            }
            // Detect if the Tab key was pressed
            else if (shownCommandsCount != 0 && evt.isKey && evt.type == EventType.KeyDown && (evt.keyCode == KeyCode.Tab) && GUI.GetNameOfFocusedControl() == SEARCH_FIELD_CONTROL_NAME)
            {
                if (_lastActionWasTabInput)
                {
                    _offset++;

                    if (_offset >= shownCommandsCount)
                    {
                        _offset = 0;
                    }
                }

                DebugCommand command = _shownCommands[_offset];
                _lastActionWasTabInput = true;

                int firstSpaceIndex = _playerInput.IndexOf(' ');

                _playerInput = command.Name + (firstSpaceIndex > 0 ? _playerInput.Substring(firstSpaceIndex) : "");
                _oldPlayerInput = _playerInput;

                _justPressedTab = true;
                _framesAwaitingEmtpySelection = 0;

                textEditor.MoveTextEnd();

                evt.Use();
            }
        }

        /// <summary>
        /// Data for a single debug command.
        /// </summary>
        public class DebugCommand
        {
            public string Name { get; private set; }
            public string LowerName { get; private set; }
            public string Description { get; private set; }
            public bool CloseDebugConsole { get; private set; }

            public MethodInfo Method { get; private set; }

            public bool HasParameters { get; private set; }
            public int ParameterCount { get; private set; }

            public Parameter[] Parameters { get; private set; }

            public DebugCommand(string name, string description, bool closeDebugConsole, MethodInfo method)
            {
                Name = name;
                LowerName = name.ToLower();
                Description = description;
                CloseDebugConsole = closeDebugConsole;

                Method = method;

                // Cannot read parameters if there is no method to read from
                if (Method == null)
                {
                    return;
                }

                // Get every parameter on this method
                ParameterInfo[] parameters = method.GetParameters();
                ParameterCount = parameters.Length;
                HasParameters = ParameterCount > 0;

                // Method has no parameters
                if (!HasParameters)
                {
                    return;
                }

                // Create the parameters array
                Parameters = new Parameter[ParameterCount];

                for (int i = 0; i < ParameterCount; i++)
                {
                    ParameterInfo parameter = parameters[i];
                    Parameters[i] = new Parameter(parameter.Name, parameter.ParameterType);
                }
            }

            public void Invoke(string input)
            {
                // No method to invoke :(
                if (Method == null)
                {
                    return;
                }

                // Method for closing the debug console if the command demands it
                void CloseConsoleIfRequired()
                {
                    if (CloseDebugConsole)
                    {
                        DebugConsole.IsOpen = false;
                    }
                }

                // No parameters so invoke without any input at all
                if (!HasParameters)
                {
                    Method.Invoke(null, null);

                    CloseConsoleIfRequired();
                    return;
                }

                // Method has parameters, so get values from string

                // Split the string at every space as each space will be our parameter
                List<string> splits = new List<string>();

                string currentSplit = "";
                bool inString = false;

                foreach (char c in input)
                {
                    if (c == '\"')
                    {
                        inString = !inString;
                        continue;
                    }
                    else if (c == ' ' && !inString)
                    {
                        splits.Add(currentSplit);
                        currentSplit = "";
                        continue;
                    }

                    currentSplit += c;
                }

                if (!string.IsNullOrEmpty(currentSplit))
                {
                    splits.Add(currentSplit);
                }

                int splitsLength = splits.Count;

                // Keep in mind that the first split is irrelevant as it's the name of this method
                // Which is also already controlled and checked before this method is even called

                // Check if our parameter count matches the splits length
                if (ParameterCount != splitsLength - 1)
                {
                    DebugConsoleLogger.LogMessage($"Incorrect amount of parameters for command: \"{Name}\" (requires {ParameterCount} parameter{(ParameterCount != 1 ? "s" : "")}, was given {splitsLength - 1})", LogType.Error);
                    return;
                }

                try
                {
                    // Create the invoke parameters 
                    object[] invokeParamters = new object[ParameterCount];

                    bool fail = false;

                    for (int i = 0; i < ParameterCount; i++)
                    {
                        string split = splits[i + 1].Trim();

                        Parameter parameter = Parameters[i];

                        void ErrorMessage()
                        {
                            fail = true;
                            DebugConsoleLogger.LogMessage($"Parameter {i + 1} was given an invalid value (required value is of type {parameter.Type.Name})", LogType.Error);
                        }

                        // Deseralize the split into it's proper value
                        if (parameter.Type == typeof(int))
                        {
                            if (!int.TryParse(split, out int result))
                            {
                                ErrorMessage();
                                continue;
                            }

                            invokeParamters[i] = result;
                        }
                        else if (parameter.Type == typeof(float))
                        {
                            if (!float.TryParse(split, out float result))
                            {
                                ErrorMessage();
                                continue;
                            }

                            invokeParamters[i] = result;
                        }
                        else if (parameter.Type == typeof(bool))
                        {
                            if (!bool.TryParse(split, out bool result))
                            {
                                ErrorMessage();
                                continue;
                            }

                            invokeParamters[i] = result;
                        }
                        else if (parameter.Type == typeof(string))
                        {
                            invokeParamters[i] = split;
                        }
                        // Enum
                        else if (parameter.Type.IsEnum)
                        {
                            if (!Enum.TryParse(parameter.Type, split, true, out object result))
                            {
                                ErrorMessage();
                                continue;
                            }

                            invokeParamters[i] = result;
                        }
                        // Dynamic deserialization
                        else
                        {
                            invokeParamters[i] = Parameter.FromJson(split, parameter.Type);
                        }
                    }

                    // Invoke method
                    if (!fail)
                    {
                        Method.Invoke(null, invokeParamters);
                    }
                }
                // Errors are ignored
                catch (Exception)
                {
                    return;
                }

                CloseConsoleIfRequired();
            }

            private string _cachedToString;

            public override string ToString()
            {
                if (!string.IsNullOrEmpty(_cachedToString))
                {
                    return _cachedToString;
                }

                _stringBuilder.Clear();

                _stringBuilder.Append(Name);

                if (HasParameters)
                {
                    foreach (Parameter parameter in Parameters)
                    {
                        _stringBuilder.Append(' ');
                        _stringBuilder.Append(parameter.FormattedName);
                    }
                }

                if (!string.IsNullOrEmpty(Description))
                {
                    _stringBuilder.Append(" - ");

                    _stringBuilder.Append(Description);
                }

                _cachedToString = _stringBuilder.ToString();

                return _cachedToString;
            }

            /// <summary>
            /// Contains data about a single parameter
            /// </summary>
            public class Parameter
            {
                public const string FORMAT = "<{0}>";

                public string Name;
                public string FormattedName;
                public Type Type;

                public Parameter(string name, Type parameterType)
                {
                    Name = name;
                    FormattedName = string.Format(FORMAT, Name);

                    Type = parameterType;
                }

                #region Copy Pasted JSON Object Deserialization
                // This is my own copied and modified code to only allow deserialization and not serialization
                private static readonly MethodInfo _fromJsonMethod = typeof(Parameter).GetMethod("FromJsonGeneric", BindingFlags.Static | BindingFlags.NonPublic);
                private static readonly Dictionary<Type, MethodInfo> _genericFromJsonMethods = new Dictionary<Type, MethodInfo>();

                private static MethodInfo GetGenericFromJsonMethod(Type type)
                {
                    if (!_genericFromJsonMethods.ContainsKey(type))
                    {
                        _genericFromJsonMethods.Add(type, _fromJsonMethod.MakeGenericMethod(type));
                    }

                    return _genericFromJsonMethods[type];
                }

                public static object FromJson(string json, Type type)
                {
                    return GetGenericFromJsonMethod(type).Invoke(null, new object[] { json });
                }

                /// <summary>
                /// Will convert a json into a new <see cref="object"/> and return the result. <para/>
                /// This is not meant to be used on it's own but is supposed to be used with Reflections <see cref="MethodInfo.MakeGenericMethod(Type[])"/> to turn a <see cref="string"/> into a generic <see cref="object"/>. (Given it's serializable)
                /// </summary>
                private static T FromJsonGeneric<T>(string json)
                {
                    if (string.IsNullOrEmpty(json))
                    {
                        return default;
                    }

                    // Add the start and end parts of the json back. These parts were cut in ToJson<T>()
                    json = $"{ValueHolder<T>.START}{json}{ValueHolder<T>.END}";

                    // Return the json value
                    return JsonUtility.FromJson<ValueHolder<T>>(json).V;
                }

                /// <summary>
                /// Class that holds a value for Generic object serialization. In short, this is used to convert anything serializable into a <see cref="string"/>.
                /// </summary>
                [Serializable]
                public class ValueHolder<T>
                {
                    public static readonly string START = $"{{\"{nameof(V)}\":";
                    public const string END = "}";

                    public T V;

                    public ValueHolder(T obj)
                    {
                        V = obj;
                    }
                }
                #endregion
            }
        }
    }
}
