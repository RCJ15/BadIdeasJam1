using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class TooltipProvider : MonoBehaviour
{
    public bool Active
    {
        get => _active;
        set
        {
            if (_active == value) return;

            _active = value;

            if (_active)
            {
                if (isActiveAndEnabled)
                {
                    TooltipManager.AddActive(this);
                }
            }
            else
            {
                TooltipManager.RemoveActive(this);
            }
        }
    }
    private bool _active;

    public string Title => _tooltip.GetTooltipTitle();
    public string Description => _tooltip.GetTooltipDescription();

    public abstract Rect ScreenRect();

    [SerializeField] private Component tooltipComponent;
    protected ITooltip _tooltip;
    protected TooltipManager _manager;

    protected virtual void Awake()
    {
        _tooltip = tooltipComponent as ITooltip;
    }

    protected virtual void Start()
    {
        _manager = TooltipManager.Instance;
    }

    protected virtual void OnEnable()
    {
        if (_active)
        {
            TooltipManager.AddActive(this);
        }
    }

    protected virtual void OnDisable()
    {
        TooltipManager.RemoveActive(this);
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(TooltipProvider), true)]
    public class TooltipProviderEditor: Editor
    {
        private TooltipProvider _target;

        private void OnEnable()
        {
            _target = (TooltipProvider)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;

                string prop = iterator.propertyPath;

                switch (prop)
                {
                    case "m_Script":
                        continue;
                }

                bool disable;

                switch (prop)
                {
                    case nameof(tooltipComponent):
                        disable = true;
                        break;

                    default:
                        disable = false;
                        break;
                }

                if (prop == nameof(tooltipComponent))
                {
                    Component tooltipComponent = null;

                    foreach (Component component in _target.GetComponents<Component>())
                    {
                        if (component is ITooltip)
                        {
                            tooltipComponent = component;
                            break;
                        }
                    }

                    iterator.objectReferenceValue = tooltipComponent;
                }

                EditorGUI.BeginDisabledGroup(disable);
                EditorGUILayout.PropertyField(iterator, true);
                EditorGUI.EndDisabledGroup();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
