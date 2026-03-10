using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input
{
    /// <summary>
    /// Updates every <see cref="InputButton"/> and allows usage of input.
    /// </summary>
    public class GameInputManager : MonoBehaviour
    {
        public const BindingFlags PROPERTIES_BINDINGS = BindingFlags.Static | BindingFlags.Public | BindingFlags.SetProperty;

        /// <summary>
        /// The main <see cref="InputActionAsset"/> that <see cref="GameInput"/> uses. <para/>
        /// May be null if this is called at the first few frames of the games start. The reason for it being null is that the asset is being loaded in an async method.
        /// </summary>
        public static InputActionAsset Asset => _asset;
        private static InputActionAsset _asset;

        /*
        /// <summary>
        /// Will be true if <see cref="Asset"/> has been loaded and is not null.
        /// </summary>
        public static bool Loaded { get; private set; } = false;

        /// <summary>
        /// An event that's fired once when the <see cref="Asset"/> has been loaded.
        /// </summary>
        public static Action OnLoadAsset { get; set; }
        */

        /// <summary>
        /// The current <see cref="ControlScheme"/> the player is using, courtesy of the <see cref="ControlSchemeManager"/>.
        /// </summary>
        public static ControlScheme CurrentControlScheme => ControlSchemeManager.CurrentControlScheme;

        private static Action _updateInputButtons;

        public static Dictionary<string, PropertyInfo> PropertyDictionary
        {
            get
            {
                if (_propertyDictionary == null)
                {
                    GenerateProperties();
                }

                return _propertyDictionary;
            }
        }
        private static Dictionary<string, PropertyInfo> _propertyDictionary = null;

        public static PropertyInfo[] Properties
        {
            get
            {
                if (_properties == null)
                {
                    GenerateProperties();
                }

                return _properties;
            }
        }
        private static PropertyInfo[] _properties = null;

        private static void GenerateProperties()
        {
            _propertyDictionary = new Dictionary<string, PropertyInfo>();
            _properties = typeof(GameInput).GetProperties(PROPERTIES_BINDINGS);

            foreach (PropertyInfo prop in _properties)
            {
                _propertyDictionary.Add(prop.Name.ToLower().Trim(), prop);
            }

            foreach (Type nestedType in typeof(GameInput).GetNestedTypes())
            {
                PropertyInfo[] nestedProperties = nestedType.GetProperties(PROPERTIES_BINDINGS);

                string prefix = nestedType.Name.ToLower().Trim() + ".";

                foreach (PropertyInfo prop in nestedProperties)
                {
                    _propertyDictionary.Add(prefix + prop.Name.ToLower().Trim(), prop);
                }

                int propertiesLength = _properties.Length;
                int nestedPropertiesLength = nestedProperties.Length;
                Array.Resize(ref _properties, propertiesLength + nestedPropertiesLength);
                Array.Copy(nestedProperties, 0, _properties, propertiesLength, nestedPropertiesLength);
            }
        }

        private static PropertyInfo _inputActionProperty = typeof(InputButton).GetProperty("InputAction", BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);

        /*
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitCreateButtons()
        {
            // Loop through every public static set field in the GameInput script
            foreach (PropertyInfo property in Properties)
            {
                if (property.GetValue(null) != null)
                {
                    continue;
                }

                CreateInputButton(property);
            }
        }
        */

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init()
        {
            /*
            // Create a temporary list which we will later discard
            List<(string, InputButton)> tempList = new List<(string, InputButton)>();
            
            // Loop through every public static set property in the GameInput script
            foreach (var pair in PropertyDictionary)
            {
                PropertyInfo property = pair.Value;

                // Ignore the property if it's not of type Button
                if (!property.PropertyType.IsSubclassOf(typeof(InputButton)) || property.PropertyType == typeof(InputButton))
                {
                    continue;
                }
                object button = property.GetValue(null);

                if (button == null)
                {
                    button = CreateInputButton(property);
                }

                tempList.Add((pair.Key, button as InputButton));
            }
            */

            _asset = InputSystem.actions;

            /*
            Loaded = true;
            OnLoadAsset?.Invoke();
            */

            foreach (var pair in PropertyDictionary)
            {
                PropertyInfo property = pair.Value;

                // Ignore the property if it's not of type Button
                if (!typeof(InputButton).IsAssignableFrom(property.PropertyType) || property.PropertyType == typeof(InputButton))
                {
                    continue;
                }

                string propertyName = pair.Key;

                // Find an input action that is named the same as the property
                string mapName;
                string actionName;

                if (!propertyName.Contains('.'))
                {
                    mapName = "default";
                    actionName = propertyName;
                }
                else
                {
                    string[] split = propertyName.Split('.');

                    mapName = split[0];
                    actionName = split[1];
                }

                // First find the action map
                InputActionMap actionMap = null;

                foreach (InputActionMap inputActionMap in Asset.actionMaps)
                {
                    if (inputActionMap.name.ToLower().Trim() != mapName)
                    {
                        continue;
                    }

                    actionMap = inputActionMap;
                    break;
                }

                // Couldn't find an input action map with the same name, so ignore this property
                if (actionMap == null)
                {
                    continue;
                }

                // Find the action in the action map
                InputAction action = null;

                foreach (InputAction inputAction in actionMap.actions)
                {
                    if (inputAction.name.ToLower().Trim() != actionName)
                    {
                        continue;
                    }

                    action = inputAction;
                    break;
                }
                
                // Couldn't find an input action with the same name, so ignore this property
                if (action == null)
                {
                    continue;
                }

                object button = property.GetValue(null);

                if (button == null)
                {
                    button = CreateInputButton(property);
                }

                // Add the Input Action to the button
                _inputActionProperty.SetValue(button, action);

                _updateInputButtons += (button as InputButton).Update;
            }

            Asset.Enable();

            // Create singleton instance
            DontDestroyOnLoad(new GameObject("Game Input", typeof(GameInputManager)));
        }

        private static object CreateInputButton(PropertyInfo property)
        {
            // Create a new input button which is the type that the property is using Activator
            object button = Activator.CreateInstance(property.PropertyType);

            // Set the value of the property to the new button we just created
            property.SetValue(null, button);

            return button;
        }

        private void Update()
        {
            // Manual update
            if (GameInputSettings.UpdateMode == InputSettings.UpdateMode.ProcessEventsManually)
            {
                InputSystem.Update();
            }

            // Update every input button
            _updateInputButtons?.Invoke();
        }

        public static void Enable()
        {
            // Enable all input maps
            foreach (InputActionMap map in Asset.actionMaps)
            {
                map.Enable();
            }
        }

        public static void Disable()
        {
            // Disable all input maps (except for Debug)
            foreach (InputActionMap map in Asset.actionMaps)
            {
                if (map.name.ToLower().Trim() == "debug")
                {
                    continue;
                }

                map.Disable();
            }
        }

        /// <summary>
        /// Calls "SaveBindingOverridesAsJson" on <see cref="Asset"/>.
        /// </summary>
        public static string SaveBindingOverridesAsJson()
        {
            return Asset.SaveBindingOverridesAsJson();
        }

        /// <summary>
        /// Calls "LoadBindingOverridesFromJson" on <see cref="Asset"/>.
        /// </summary>
        public static void LoadBindingOverridesFromJson(string json)
        {
            Asset.LoadBindingOverridesFromJson(json);
        }
    }
}
