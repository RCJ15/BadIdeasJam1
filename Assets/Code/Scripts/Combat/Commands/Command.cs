using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class Command : MonoBehaviour, ICommand
{
    protected static Board Board => Board.Instance;

    public string GUID => guid;

    public CommandType Type => type;
    public Color Color => type.Color();
    public Sprite Icon => icon;
    public bool IconFlipX => iconFlipX;
    public bool IconFlipY => iconFlipY;

    public string CommandName => commandName;
    public string FormattedCommandName
    {
        get
        {
            if (string.IsNullOrEmpty(_formattedCommandName))
            {
                _formattedCommandName = "<" + commandName.ToUpper().Replace(' ', '_') + ">";
            }

            return _formattedCommandName;
        }
    }

    public string Description => description;

    public int Energy => energy;

    public float Duration => duration;

    [SerializeField] private string guid;

    [SerializeField] private CommandType type;
    [SerializeField] private Sprite icon;
    [SerializeField] private bool iconFlipX;
    [SerializeField] private bool iconFlipY;

    [Space]
    [SerializeField] private string commandName = "name goes here";
    private string _formattedCommandName;
    [TextArea(1, 5)]
    [SerializeField] private string description = "description goes here";

    [Space]
    [SerializeField] private int energy;
    [SerializeField] private float duration;

    public abstract void Execute(Unit user);
}

public enum CommandType
{
    Movement,
    Facing,
    Attack,
    Action,
    Misc,
}

public static class CommandTypeExtensions
{
    public static Color Color(this CommandType type)
    {
        switch (type)
        {
            case CommandType.Movement:
                return CommandManager.MovementColor;

            case CommandType.Facing:
                return CommandManager.FacingColor;

            case CommandType.Attack:
                return CommandManager.AttackColor;

            case CommandType.Action:
                return CommandManager.ActionColor;

            default:
            case CommandType.Misc:
                return CommandManager.MiscColor;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Command), true)]
public class CommandEditor : Editor
{
    private static GUIStyle _style;

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();

        SerializedProperty iterator = serializedObject.GetIterator();

        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren))
        {
            string path = iterator.propertyPath;

            switch (path)
            {
                case "m_Script":
                    continue;

                case "guid":
                    if (string.IsNullOrEmpty(iterator.stringValue))
                    {
                        iterator.stringValue = GUID.Generate().ToString();
                    }

                    if (_style == null)
                    {
                        _style = new(EditorStyles.label)
                        {
                            fontStyle = FontStyle.Italic,
                            normal = { textColor = new Color(1, 1, 1, 0.35f) }
                        };
                    }

                    EditorGUILayout.LabelField("GUID: " + iterator.stringValue, _style);
                    if (GUILayout.Button("New GUID"))
                    {
                        iterator.stringValue = null;
                    }

                    EditorGUILayout.Space();
                    continue;
            }

            EditorGUILayout.PropertyField(iterator, true);

            enterChildren = false;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
