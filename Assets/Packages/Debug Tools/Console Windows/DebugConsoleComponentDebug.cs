using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DebugTools
{
    /// <summary>
    /// The component debug window that appears in the bottom right corner of the <see cref="DebugConsole"/>.
    /// </summary>
    public class DebugConsoleComponentDebug : DebugConsoleWindow
    {
        public const string SEARCH_FIELD_CONTROL_NAME = "DEBUG Component Debug Search Field";

        private static readonly Dictionary<Type, List<InterfaceReference>> _availableComponentGUI = new Dictionary<Type, List<InterfaceReference>>();

        private InterfaceReference _selectedInterface;
        private Type _selectedType;

        private string _searchQuery;
        private Vector2 _scrollPosition;

        public bool ShouldStayOpen { get; private set; } = false;

        public override void OnOpen()
        {
            Refresh();

            if (_selectedInterface == null && _selectedType != null && !_availableComponentGUI.ContainsKey(_selectedType))
            {
                _selectedType = null;
            }

            _searchQuery = null;
        }

        public override void Draw(Rect rect, Event evt)
        {
            GUI.Box(rect, GUIContent.none);

            GUILayout.BeginArea(rect);

            // Draw which object is currently selected as a Label
            string labelText;
            bool nothingSelected = false;

            if (_selectedInterface != null)
            {
                labelText = "<b>Selected:</b> " + _selectedInterface.Name;
            }
            else if (_selectedType != null)
            {
                labelText = "<b>Select Object</b>";
            }
            else
            {
                labelText = "<b>Select</b>";
                nothingSelected = true;
            }

            Rect labelRect = GUILayoutUtility.GetRect(rect.width, 20);

            labelRect.x += 5;
            labelRect.width -= 10;

            Rect lockRect = labelRect;
            lockRect.width = 20;

            labelRect.width -= lockRect.width;
            lockRect.x += labelRect.width + 5;

            // Use the correct custom GUI Style
            GUI.Label(labelRect, labelText, GUI.skin.customStyles[7]);

            ShouldStayOpen = GUI.Toggle(lockRect, ShouldStayOpen, GUIContent.none);

            // Disable the next GUI if we have nothing selected
            if (nothingSelected)
            {
                GUI.enabled = false;
            }

            GUILayout.BeginHorizontal();

            // This button will go back a step in our selection
            if (GUILayout.Button("◀", GUILayout.Width(20), GUILayout.Height(20)) && GUI.enabled)
            {
                if (_selectedInterface == null)
                {
                    _selectedType = null;
                }
                else
                {
                    _selectedInterface = null;

                    if (_availableComponentGUI.TryGetValue(_selectedType, out var list) && list.Count <= 1)
                    {
                        _selectedType = null;
                    }
                }
            }

            // Disable next GUI if we have some things not selected
            GUI.enabled = _selectedType == null || _selectedInterface == null;

            // Search field for filtering our search
            GUI.SetNextControlName(SEARCH_FIELD_CONTROL_NAME);
            _searchQuery = GUILayout.TextField(_searchQuery, GUILayout.Height(20));

            // If we have no search query, then display some special text
            if (string.IsNullOrEmpty(_searchQuery) && GUI.GetNameOfFocusedControl() != SEARCH_FIELD_CONTROL_NAME)
            {
                GUI.Label(GUILayoutUtility.GetLastRect(), "Search Filter...", GUI.skin.customStyles[3]);
            }

            // Reenable GUI if it has been disabled before
            if (!GUI.enabled)
            {
                GUI.enabled = true;
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(4);

            // Draw a scroll view
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(rect.width));

            // If we have no type selected, then give GUI to select one
            if (_selectedType == null)
            {
                foreach (var pair in _availableComponentGUI)
                {
                    string key = pair.Key.Name;

                    // Match search query 
                    if (!string.IsNullOrEmpty(_searchQuery) && !key.ToLower().Contains(_searchQuery))
                    {
                        continue;
                    }

                    int listCount = pair.Value.Count;

                    // Button for selecting this type
                    string buttonText = $"▶ {key}";

                    if (listCount > 1)
                    {
                        buttonText += $" ({listCount})";
                    }

                    if (GUILayout.Button(buttonText))
                    {
                        _selectedType = pair.Key;

                        if (listCount <= 1)
                        {
                            _selectedInterface = pair.Value[0];
                        }
                    }

                    if (_selectedType != null)
                    {
                        break;
                    }
                }
            }
            // If we have no Component Debug GUI interface selected then give GUI to select one
            else if (_selectedInterface == null)
            {
                if (!_availableComponentGUI.ContainsKey(_selectedType))
                {
                    _selectedType = null;
                }
                else
                {
                    foreach (InterfaceReference reference in _availableComponentGUI[_selectedType])
                    {
                        string key = reference.Name;

                        // Match search query 
                        if (!string.IsNullOrEmpty(_searchQuery) && !key.ToLower().Contains(_searchQuery))
                        {
                            continue;
                        }

                        if (GUILayout.Button($"▶ {key}"))
                        {
                            _selectedInterface = reference;
                        }

                        if (_selectedInterface != null)
                        {
                            break;
                        }
                    }
                }
            }
            // If we have a Component Debug GUI interface selected then just show it!
            else
            {
                try
                {
                    _selectedInterface.Interface.OnDebugGUI();
                }
                catch (Exception)
                {
                    // End
                    GUILayout.EndScrollView();

                    GUILayout.EndArea();

                    // Then throw
                    throw;
                }
            }

            // End
            GUILayout.EndScrollView();

            GUILayout.EndArea();
        }

        public void Refresh()
        {
            _availableComponentGUI.Clear();

            foreach (MonoBehaviour obj in Object.FindObjectsOfType<MonoBehaviour>())
            {
                if (obj is not IComponentDebugGUI)
                {
                    continue;
                }

                IComponentDebugGUI componentDebugGUI = obj as IComponentDebugGUI;

                if (!componentDebugGUI.DebugGUIAvailable())
                {
                    continue;
                }

                Type type = obj.GetType();

                if (!_availableComponentGUI.ContainsKey(type))
                {
                    _availableComponentGUI.Add(type, new List<InterfaceReference>());
                }

                _availableComponentGUI[type].Add(new InterfaceReference(obj, componentDebugGUI));
            }

            // Set name
            foreach (var pair in _availableComponentGUI)
            {
                int count = pair.Value.Count;

                // Failsafe
                if (count <= 0)
                {
                    continue;
                }

                if (count <= 1)
                {
                    pair.Value[0].SetName(false);
                }
                else
                {
                    foreach (InterfaceReference reference in pair.Value)
                    {
                        reference.SetName(true);
                    }
                }
            }
        }

        public class InterfaceReference
        {
            public MonoBehaviour Script { get; private set; }
            public IComponentDebugGUI Interface { get; private set; }
            public string Name { get; private set; }

            public InterfaceReference(MonoBehaviour script, IComponentDebugGUI @interface)
            {
                Script = script;
                Interface = @interface;
            }

            public void SetName(bool addSuffixIfNeeded)
            {
                Name = Interface.DebugGUIName();

                if (string.IsNullOrEmpty(Name))
                {
                    Name = Script.name.Trim();

                    if (addSuffixIfNeeded && Name == Script.GetType().Name)
                    {
                        Name += " (Object)";
                    }
                }
            }
        }
    }
}
