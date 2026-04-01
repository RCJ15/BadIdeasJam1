using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[SingletonMode(true)]
public class CommandManager : Singleton<CommandManager>
{
    public static Color MovementColor => Instance.movementColor;
    public static Color FacingColor => Instance.facingColor;
    public static Color AttackColor => Instance.attackColor;
    public static Color ActionColor => Instance.actionColor;
    public static Color MiscColor => Instance.miscColor;

    public static readonly Dictionary<string, Command> CommandDictionary = new();
    public Command[] Commands => commands;
    [SerializeField] private Command[] commands;

    [Space]
    [SerializeField] private Color movementColor = Color.white;
    [SerializeField] private Color facingColor = Color.white;
    [SerializeField] private Color attackColor = Color.white;
    [SerializeField] private Color actionColor = Color.white;
    [SerializeField] private Color miscColor = Color.white;

    protected override void Awake()
    {
        base.Awake();

        foreach (Command command in commands)
        {
            if (command == null) continue;

            CommandDictionary.Add(command.GUID, command);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CommandManager))]
public class CommandManagerEditor : Editor
{
    private CommandManager _target;

    private void OnEnable()
    {
        _target = (CommandManager)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SerializedProperty commands = serializedObject.FindProperty("commands");

        commands.ClearArray();

        foreach (Command command in _target.GetComponentsInChildren<Command>())
        {
            int i = commands.arraySize;
            commands.InsertArrayElementAtIndex(i);
            commands.GetArrayElementAtIndex(i).objectReferenceValue = command;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif