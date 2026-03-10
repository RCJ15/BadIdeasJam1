using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
using UnityEditor.Compilation;
using System.Collections.Generic;

namespace Input.Editor
{
    /// <summary>
    /// Generates code for the <see cref="GameInput"/> script. <para/>
    /// See: <see cref="GenerateCode(InputActionAsset)"/> for further info.
    /// </summary>
    public static class GameInputCodeGeneration
    {
        private static Regex _propertyRegex = new Regex("public static .* (.*) { get; private set; }");
        private static Regex _commentedNameRegex = new Regex("/\\* (.*):.*");
        private static Regex _classNameRegex = new Regex("public static class (.*)");
        private const string REGEX_RESULT = "$1";

        private static StringBuilder _stringBuilder = new StringBuilder();

        private const string NEW_LINE = "\r\n";

        public static MonoScript GetGameInputMonoScript()
        {
            // Find the location of the Input script by looping through every single runtime script in our project
            foreach (MonoScript script in MonoImporter.GetAllRuntimeMonoScripts())
            {
                // Check if the type of the script is of type GameInput
                if (script.GetClass() != typeof(GameInput))
                {
                    // If not, then proceed to the next in the iteration
                    continue;
                }

                // We found our script so break out of the loop
                return script;
            }

            // Also print out a warning about the Input class not being found in the above clause
            Debug.LogWarning($"Mono Script for class: \"{nameof(GameInput)}\" does not exist!");

            return null;
        }

        /// <summary>
        /// Upon being called, this method will search for the <see cref="MonoScript"/> that houses the <see cref="GameInput"/> class. <para/>
        /// After finding the <see cref="MonoScript"/>, this method will then read the given <see cref="InputActionAsset"/> and insert a <see cref="InputButton"/> for every single <see cref="InputAction"/> in the <paramref name="asset"/>. <para/>
        /// Do note that this shouldn't be called anywhere outside of the <see cref="GameInputActionPostProcessor"/> and <see cref="GameInputToolbarOptions"/> scripts.
        /// </summary>
        /// <param name="asset">The <see cref="InputActionAsset"/> which will be used to generate the code for the <see cref="GameInput"/> <see cref="MonoScript"/>.</param>
        public static void GenerateCode(InputActionAsset asset)
        {
            MonoScript inputScript = GetGameInputMonoScript();

            // Return if the MonoScript asset for the Input class wasn't found
            if (inputScript == null)
            {
                return;
            }

            // Edit the script
            // We start by clearing our string builder as we don't want any previous data to interfere
            _stringBuilder.Clear();

            bool generatingCode = false;
            bool inAttribute = false;

            // Loop through every single line using the StringReader found in System.IO.
            // The reason this is used instead of string.Split('\n') is because of the fact that the line endings in some text documents can be inconsistent.
            // The StringReader is also specifically built to read from files so it should actually be used in situations like these.
            using (StringReader reader = new StringReader(inputScript.text))
            {
                string line;

                // For attribute persistance
                bool searchForAttributeName = false;
                string attributeText = "";
                List<string> pendingAttributes = new List<string>();
                string attributeName = "";
                string currentPrefix = ""; 

                Dictionary<string, List<string>> attributes = new Dictionary<string, List<string>>();

                // Odd syntax for reading line
                while ((line = reader.ReadLine()) != null)
                {
                    string trimmedLine = line.Trim();

                    // Check if we have already generated our code and if we haven't reached the "Generation End" (not case sensitive) tag.
                    // If so, then we ignore writing anymore lines and proceed to the next line.
                    // This will make it so that the old generated code in the GameInput script between the "Generation Begin" and "Generation End"
                    // tags will automatically not be written into the new file, meaning that the old code gets replaced by our new generated code.
                    // We however still need to check the old code for any attributes so that they persist to the new code that will be generated.
                    if (generatingCode && trimmedLine.ToLower() != "// generation end")
                    {
                        bool TryAddAttributes()
                        {
                            if (!string.IsNullOrEmpty(attributeName))
                            {
                                string key = currentPrefix + attributeName;

                                if (!attributes.ContainsKey(key))
                                {
                                    attributes.Add(key, new List<string>());
                                }

                                List<string> list = attributes[key];

                                foreach (string item in pendingAttributes)
                                {
                                    list.Add(item);
                                }

                                pendingAttributes.Clear();

                                return true;
                            }

                            return false;
                        }

                        if (!inAttribute)
                        {
                            Match commentedNameRegex = _commentedNameRegex.Match(trimmedLine);

                            if (commentedNameRegex.Success)
                            {
                                attributeName = commentedNameRegex.Result(REGEX_RESULT).Trim();
                            }
                            
                            if (searchForAttributeName)
                            {
                                Match match = _propertyRegex.Match(trimmedLine);

                                if (match.Success)
                                {
                                    attributeName = match.Result(REGEX_RESULT).Trim();
                                }

                                TryAddAttributes();

                                attributeName = "";
                                searchForAttributeName = false;
                            }

                            if (trimmedLine == "}")
                            {
                                currentPrefix = "";
                            }
                            else
                            {
                                Match classNameRegex = _classNameRegex.Match(trimmedLine);

                                if (classNameRegex.Success)
                                {
                                    currentPrefix = classNameRegex.Result(REGEX_RESULT).Trim() + ".";

                                    continue;
                                }
                            }
                        }
                        else
                        {
                            attributeText += NEW_LINE;
                        }

                        foreach (char c in line)
                        {
                            // Enter attribute
                            if (c == '[' && !inAttribute)
                            {
                                inAttribute = true;

                                attributeText = "";
                            }

                            if (inAttribute)
                            {
                                attributeText += c;
                            }

                            // Exit attribute
                            if (c == ']' && inAttribute)
                            {
                                inAttribute = false;
                                
                                pendingAttributes.Add(attributeText);

                                if (!TryAddAttributes())
                                {
                                    searchForAttributeName = true;
                                }
                            }
                        }

                        continue;
                    }

                    // Just add our line if it's just empty
                    if (string.IsNullOrEmpty(line))
                    {
                        _stringBuilder.AppendLine(line);
                        continue;
                    }

                    // We passed the above check, which means that our code generation is finished.
                    if (generatingCode)
                    {
                        generatingCode = false;

                        // Add our generated code to the result of our file (StringBuilder)
                        // Give it the amount of tab spaces in this line
                        // Also give it our dictionary of all of our attributes so that they persist in the new code
                        AddGeneratedCode(asset, line.Count((c) => c == '\t'), attributes);
                    }

                    // Add our line to the result of our file (StringBuilder)
                    _stringBuilder.AppendLine(line);

                    // Check if the line we just added is the "Generation Begin" (not case sensitive) tag.
                    // If so, then we call the "AddGeneratedCode" method to 
                    if (trimmedLine.ToLower() == "// generation begin")
                    {
                        // Set the generatingCode bool to true so that the next lines before the "Generation End" tag are ignored
                        generatingCode = true;
                    }
                }
            }

            // Check if the new file content is the same as before
            string newContent = _stringBuilder.ToString().Trim();

            if (inputScript.text.Trim() == newContent)
            {
                // No need to request a script compilation if the code is exactly the same as before
                return;
            }

            // Apply changes to the MonoScript by writing to the file using System.IO
            string filePath = AssetDatabase.GetAssetPath(inputScript);

            // Write to the file
            File.WriteAllText(filePath, newContent);

            // Save asset by making it dirty first (Unity otherwise won't save the file)
            EditorUtility.SetDirty(inputScript);
            AssetDatabase.SaveAssetIfDirty(inputScript);

            // Recompile scripts to apply changes instantly
            CompilationPipeline.RequestScriptCompilation();
        }

        /// <summary>
        /// Adds the auto-generated code into the static <see cref="StringBuilder"/> on this script. The <see cref="StringBuilder"/> then gets turned into the <see cref="GameInput"/> <see cref="MonoScript"/>. <br/>
        /// The code that is generated comes from the given <see cref="InputActionAsset"/>. Every <see cref="InputAction"/> in the <paramref name="asset"/> is turned into it's own <see cref="InputButton"/> (the type of the <see cref="InputButton"/> is automatically assigned appropriately).<para/>
        /// <b>EXAMPLE:</b> <br/>
        /// If an <see cref="InputAction"/> is called "My Button" and is classified as an analog button, then it's generated code will look like this: <code>public static ValueButton&lt;float&gt; MyButton { get; private set; }</code>
        /// </summary>
        /// <param name="asset">The <see cref="InputActionAsset"/> that will be read. This is what the generated code will use for generation.</param>
        /// <param name="tabSpaceAmount">A count for how many '\t' characters should be written before every single generated line of code.</param>
        /// <param name="attributes">The different attributes that will be applied to the actions.</param>
        private static void AddGeneratedCode(InputActionAsset asset, int tabSpaceAmount, Dictionary<string, List<string>> attributes)
        {
            void AddTabSpaces(int extraAmount = 0)
            {
                // Add the requested amount of tab spaces
                for (int i = 0; i < tabSpaceAmount + extraAmount; i++)
                {
                    _stringBuilder.Append('\t');
                }
            }

            // Loop through every InputActionMap in the InputActionAsset
            foreach (InputActionMap map in asset.actionMaps)
            {
                string mapName = map.name;

                bool isDefault = mapName.ToLower().Trim() == "default";
                string formattedMapName = isDefault ? null : mapName.Trim().Replace(" ", "");
                string prefix = isDefault ? "" : (formattedMapName + ".");

                if (!isDefault)
                {
                    _stringBuilder.Append(NEW_LINE);

                    AddTabSpaces();

                    _stringBuilder.Append("public static class ");
                    _stringBuilder.Append(formattedMapName);
                    _stringBuilder.Append(NEW_LINE);

                    AddTabSpaces();

                    _stringBuilder.Append("{");
                    _stringBuilder.Append(NEW_LINE);
                }

                // Loop through every InputAction in the InputActionMap
                foreach (InputAction action in map.actions)
                {
                    string actionName = action.name.Trim().Replace(" ", "");
                    string key = prefix + actionName;

                    // Add attributes if this action should have any
                    if (attributes.TryGetValue(key, out List<string> actionAttributes))
                    {
                        attributes.Remove(key);

                        foreach (string attribute in actionAttributes)
                        {
                            AddTabSpaces(isDefault ? 0 : 1);

                            _stringBuilder.Append(attribute.Trim());

                            _stringBuilder.Append(NEW_LINE);
                        }
                    }

                    AddTabSpaces(isDefault ? 0 : 1);

                    // Add the start of the line by having "public static " (the space at the end is very much there on purpose)
                    _stringBuilder.Append("public static ");

                    // Use a switch statement to check what type the InputAction is
                    switch (action.type)
                    {
                        // If it's a value type, then we need to use the ValueButton
                        case InputActionType.Value:
                        case InputActionType.PassThrough:

                            // Check what the control type of the InputAction is
                            switch (action.expectedControlType.ToLower().Trim())
                            {
                                // Analog buttons are simply a ValueButton<float>
                                case "analog":
                                    _stringBuilder.Append("ValueButton<float>");
                                    break;

                                // Vector2 has it's own class Vector2Button
                                case "vector2":
                                    _stringBuilder.Append(nameof(Vector2Button));
                                    break;

                                // If no special cases were found, just default to the nice and reliable PressButton
                                default:
                                    _stringBuilder.Append(nameof(PressButton));
                                    break;
                            }
                            break;

                        // If no special cases were found, just default to the nice and reliable PressButton
                        default:
                            _stringBuilder.Append(nameof(PressButton));
                            break;
                    }

                    // Add a space
                    _stringBuilder.Append(' ');

                    // Add the name of the InputAction and nicify it
                    _stringBuilder.Append(actionName);

                    // Add a space
                    _stringBuilder.Append(' ');

                    // Add the "{ get; private set; } at the end"
                    _stringBuilder.Append("{ get; private set; }");

                    _stringBuilder.Append(NEW_LINE);
                }
                //-- End of InputAction Loop

                if (!isDefault)
                {
                    AddTabSpaces();

                    _stringBuilder.Append("}");
                    _stringBuilder.Append(NEW_LINE);
                }
            }
            //-- End of InputActionMap loop

            bool printedAttributeStart = false;

            // Add all unused attributes to the bottom as comment fields
            foreach (var pair in attributes)
            {
                if (!printedAttributeStart)
                {
                    _stringBuilder.Append(NEW_LINE);

                    AddTabSpaces();
                    
                    _stringBuilder.Append("// Unused Attributes:");

                    _stringBuilder.Append(NEW_LINE);

                    printedAttributeStart = true;
                }

                string name = pair.Key;

                foreach (string attribute in pair.Value)
                {
                    AddTabSpaces();
                    
                    _stringBuilder.Append("/* ");
                    _stringBuilder.Append(name);
                    _stringBuilder.Append(": ");
                    _stringBuilder.Append(attribute);
                    _stringBuilder.Append("*/");
                    _stringBuilder.Append(NEW_LINE);
                }
            }
        }
        //-- End of method
    }
    //-- End of Script
}
//-- End of namespace