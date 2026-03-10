using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Object = UnityEngine.Object;

/// <summary>
/// The property drawer for <see cref="ComponentCollection{T}"/>.
/// </summary>
[CustomPropertyDrawer(typeof(ComponentCollection<>))]
public class ComponentCollectionPropertyDrawer : PropertyDrawer
{
    private ReorderableList _reorderableListCache;

    private static float _elementHeight => EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

    private static GUIStyle _cacheButtonStyle = null;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Get our generic argument
        Type type = fieldInfo.FieldType.GetGenericArguments()[0];

        _reorderableListCache = SetupReorderableList(_reorderableListCache, type, property, label);

        DrawComponentCollection(_reorderableListCache, type, position, property);
    }

    public static SerializedProperty GetArrayProperty(SerializedProperty property) => property.FindPropertyRelative("components");

    public static ReorderableList SetupReorderableList(ReorderableList reorderableList, Type type, SerializedProperty property, GUIContent label, CacheComponentAttribute attribute = null)
    {
        // Create reorderable list
        if (reorderableList == null)
        {
            SerializedProperty listProp = GetArrayProperty(property);

            reorderableList = new ReorderableList(listProp.serializedObject, listProp);

            // Setup
            reorderableList.elementHeight = EditorGUIUtility.singleLineHeight;

            bool disabled = attribute != null && (attribute.AlwaysCache || attribute.DisableField);

            reorderableList.drawElementCallback = (rect, index, isActive, isFocused) => DrawElement(rect, reorderableList.serializedProperty, reorderableList.serializedProperty.GetArrayElementAtIndex(index), type, disabled);
            reorderableList.elementHeightCallback = (index) => EditorGUI.GetPropertyHeight(reorderableList.serializedProperty.GetArrayElementAtIndex(index));
            reorderableList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, $"{label} ({type.Name})");

            // Determine if we should have toolbars or not
            if (attribute != null && !attribute.HaveCacheButton())
            {
                reorderableList.displayAdd = false;
                reorderableList.displayRemove = false;
            }
            else
            {
                // Toolbar buttons visible but always disabled
                reorderableList.onCanAddCallback = (list) => false;
                reorderableList.onCanRemoveCallback = (list) => false;
            }
        }

        return reorderableList;
    }

    public static void DrawComponentCollection(ReorderableList reorderableList, Type type, Rect position, SerializedProperty property, CacheComponentAttribute attribute = null)
    {
        SerializedProperty array = GetArrayProperty(property);

        // Perform an auto cache if that is desired
        CacheMethod cacheMethod = attribute != null ? attribute.CacheMethod : CacheMethod.Default;
        bool autoCache = attribute != null && attribute.AlwaysCache;

        bool editingMultipleObj = array.serializedObject.targetObjects.Length > 1;

        if (autoCache && !editingMultipleObj)
        {
            Cache(array, type, cacheMethod);
        }

        // Determine button rect
        Rect buttonRect = position;

        buttonRect.y += position.height - _elementHeight - (array.arraySize <= 0 ? 4 : 2);
        buttonRect.height = _elementHeight;
        buttonRect.width = 58;
        buttonRect.x = position.width - buttonRect.width - 8;

        // Change GUI style of the toolbar buttons to nothing
        ReorderableList.Defaults defaults = ReorderableList.defaultBehaviours;

        GUIContent contentPlus = defaults.iconToolbarPlus;
        GUIContent contentPlusMore = defaults.iconToolbarPlusMore;
        GUIContent contentMinus = defaults.iconToolbarMinus;

        defaults.iconToolbarPlus = GUIContent.none;
        defaults.iconToolbarPlusMore = GUIContent.none;
        defaults.iconToolbarMinus = GUIContent.none;

        // Update reorderable list property in case it is lost
        reorderableList.serializedProperty = array;

        // Do list
        using (new EditorGUI.DisabledScope(editingMultipleObj))
        {
            reorderableList.DoList(position);
        }

        // Revert buttons back to their original state
        defaults.iconToolbarPlus = contentPlus;
        defaults.iconToolbarPlusMore = contentPlusMore;
        defaults.iconToolbarMinus = contentMinus;

        // Create cache button style
        if (_cacheButtonStyle == null)
        {
            _cacheButtonStyle = new GUIStyle(EditorStyles.boldLabel);

            _cacheButtonStyle.alignment = TextAnchor.MiddleCenter;

            // Change color of button on hover and active
            _cacheButtonStyle.hover.textColor = Color.white;
            _cacheButtonStyle.active.textColor = EditorStyles.linkLabel.normal.textColor;
        }

        // Cache button
        if (autoCache || (attribute != null && !attribute.HaveCacheButton()))
        {
            return;
        }

        GUIContent cacheContent = new GUIContent("Cache", "Current cache method: " + cacheMethod);

        using (new EditorGUI.DisabledScope(editingMultipleObj))
        {
            if (!GUI.Button(buttonRect, cacheContent, _cacheButtonStyle) || editingMultipleObj)
            {
                return;
            }

            Cache(array, type, cacheMethod);
        }
    }

    public static void Cache(SerializedProperty array, Type type, CacheMethod cacheMethod)
    {
        Component component = array.serializedObject.targetObject as Component;

        Cache(array, type, component, cacheMethod);
    }

    public static void Cache(SerializedProperty array, Type type, Component component, CacheMethod cacheMethod)
    {
        array.ClearArray();

        bool includeInactive = cacheMethod.HasFlag(CacheMethod.IncludeInactive);

        // Check if type is component
        if (typeof(Component).IsAssignableFrom(type))
        {
            // Use type as parameter in get the components methods
            // In children
            if (cacheMethod.HasFlag(CacheMethod.InChildren))
            {
                foreach (Component obj in component.GetComponentsInChildren(type, includeInactive))
                {
                    if (obj == component)
                    {
                        continue;
                    }

                    AddToArray(array, obj);
                }
            }
            // In parent
            else if (cacheMethod.HasFlag(CacheMethod.InParent))
            {
                foreach (Component obj in component.GetComponentsInParent(type, includeInactive))
                {
                    if (obj == component)
                    {
                        continue;
                    }

                    AddToArray(array, obj);
                }
            }
            // Regular
            else
            {
                foreach (Component obj in component.GetComponents(type))
                {
                    if (obj == component)
                    {
                        continue;
                    }

                    AddToArray(array, obj);
                }
            }
        }
        // Type is not component, so maybe it's an interface?
        else
        {
            // Get all components and filter manually

            // Disallow components that don't inherit from the type
            bool IsValidObj(Object obj) => type.IsAssignableFrom(obj.GetType());

            // In children
            if (cacheMethod.HasFlag(CacheMethod.InChildren))
            {
                foreach (Component obj in component.GetComponentsInChildren<Component>(includeInactive))
                {
                    if (obj == component)
                    {
                        continue;
                    }

                    if (!IsValidObj(obj))
                    {
                        continue;
                    }

                    AddToArray(array, obj);
                }
            }
            // In parent
            else if (cacheMethod.HasFlag(CacheMethod.InParent))
            {
                foreach (Component obj in component.GetComponentsInParent<Component>(includeInactive))
                {
                    if (obj == component)
                    {
                        continue;
                    }

                    if (!IsValidObj(obj))
                    {
                        continue;
                    }

                    AddToArray(array, obj);
                }
            }
            // Regular
            else
            {
                foreach (Component obj in component.GetComponents<Component>())
                {
                    if (obj == component)
                    {
                        continue;
                    }

                    if (!IsValidObj(obj))
                    {
                        continue;
                    }

                    AddToArray(array, obj);
                }
            }

        }
    }

    private static void AddToArray(SerializedProperty array, Component val)
    {
        int index = array.arraySize;
        array.InsertArrayElementAtIndex(index);
        array.GetArrayElementAtIndex(index).objectReferenceValue = val;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return GetComponentCollectionHeight(property);
    }

    public static float GetComponentCollectionHeight(SerializedProperty property, bool haveCacheButton = true)
    {
        SerializedProperty listProp = property.FindPropertyRelative("components");

        int length = Mathf.Max(listProp.arraySize, 1);

        float height = 28 + (_elementHeight * (float)length);

        if (haveCacheButton)
        {
            height += 20;
        }

        return height; // EditorGUIUtility.singleLineHeight; //_reorderableListCache != null ? _reorderableListCache.GetHeight() : ARRAY_ELEMENT_SIZE;
    }

    private static void DrawElement(Rect rect, SerializedProperty array, SerializedProperty prop, Type classType, bool disabled)
    {
        void DrawField() => EditorGUI.ObjectField(rect, prop, GUIContent.none);

        if (disabled || !GUI.enabled)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                DrawField();
            }
            return;
        }

        EditorGUI.BeginChangeCheck();
        Object oldValue = prop.objectReferenceValue;

        DrawField();

        // No change
        if (!EditorGUI.EndChangeCheck())
        {
            return;
        }

        Component newValue = prop.objectReferenceValue as Component;

        if (newValue == null)
        {
            return;
        }

        bool IsValidComponent(Component component) => classType.IsAssignableFrom(component.GetType());

        if (IsValidComponent(newValue))
        {
            return;
        }

        bool fixedProblem = false;

        foreach (Component component in newValue.GetComponents<Component>())
        {
            if (!IsValidComponent(component))
            {
                continue;
            }

            if (fixedProblem)
            {
                AddToArray(array, component);
            }
            else
            {
                prop.objectReferenceValue = component;
                fixedProblem = true;
            }
        }

        // No problem!
        if (fixedProblem)
        {
            return;
        }

        prop.objectReferenceValue = oldValue;

        Debug.LogWarning("Object " + newValue.name + " has no component of type: " + classType.Name + "! Reverting to old value.");
    }

}