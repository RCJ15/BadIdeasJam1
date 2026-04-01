using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class CommandReference
{
    public string Name => Command.CommandName;
    public string Description => Command.Description;
    public int Energy => Command.Energy;

    public Command Command
    {
        get
        {
            if (_command == null && !CommandManager.CommandDictionary.TryGetValue(guid, out _command))
            {
                Debug.LogError("Could not find command with GUID: " + guid);
            }

            return _command;
        }
    }
    private Command _command;

    [SerializeField] private string guid;

    public void Execute(Unit user) => Command.Execute(user);

    public static implicit operator Command(CommandReference reference)
    {
        return reference.Command;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(CommandReference))]
public class CommandReferenceDrawer : PropertyDrawer
{
    private CommandManager _commandManager;

    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    {
        if (_commandManager == null)
        {
            _commandManager = Resources.Load<CommandManager>(Singleton<CommandManager>.FOLDER + "/" + nameof(CommandManager));
        }

        if (_commandManager == null)
        {
            EditorGUI.HelpBox(pos, nameof(CommandManager) + " not found", MessageType.Error);
            return;
        }

        SerializedProperty guidProp = prop.FindPropertyRelative("guid");
        string guid = guidProp.stringValue;

        GUIContent dropdownLabel = new GUIContent("<Unknown>");

        int length = _commandManager.Commands.Length;
        foreach (Command command in _commandManager.Commands)
        {
            if (command == null) continue;

            if (command.GUID == guid)
            {
                dropdownLabel.text = command.CommandName;
                break;
            }
        }

        Rect dropdownPos = EditorGUI.PrefixLabel(pos, label);

        if (EditorGUI.DropdownButton(dropdownPos, dropdownLabel, FocusType.Keyboard))
        {
            GenericMenu menu = new GenericMenu();

            for (int i = 0; i < length; i++)
            {
                Command command = _commandManager.Commands[i];

                if (command == null) continue;

                menu.AddItem(new GUIContent(command.name), command.GUID == guid, () =>
                {
                    guidProp.stringValue = command.GUID;
                    prop.serializedObject.ApplyModifiedProperties();
                });
            }

            menu.ShowAsContext();
        }
    }
}
#endif