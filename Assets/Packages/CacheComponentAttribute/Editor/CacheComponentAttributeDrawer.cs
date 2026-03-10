using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Object = UnityEngine.Object;

/// <summary>
/// The <see cref="PropertyDrawer"/> for the <see cref="CacheComponentAttribute"/>.
/// </summary>
[CustomPropertyDrawer(typeof(CacheComponentAttribute))]
public class CacheComponentAttributeDrawer : PropertyDrawer
{
    private ReorderableList _reorderableListCollectionCache;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        CacheComponentAttribute attribute = this.attribute as CacheComponentAttribute;

        Type type = fieldInfo.FieldType;

        void DrawField()
        {
            // Determine if we should draw the field or not
            if (!attribute.DrawField)
            {
                return;
            }

            // Draw the field
            using (new EditorGUI.DisabledGroupScope(attribute.DisableField))
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }

        // Failsafe
        if (type == null)
        {
            EditorGUI.HelpBox(position, "Something went wrong!", MessageType.Warning);
            return;
        }

        // Disallow non component types
        if (!type.IsSubclassOf(typeof(Component)))
        {
            // Uness our type is of type Component Collection!
            if (IsComponentCollection(type))
            {
                // Get our generic argument
                Type genericType = type.GetGenericArguments()[0];

                if (!attribute.DrawField)
                {
                    SerializedProperty array = ComponentCollectionPropertyDrawer.GetArrayProperty(property);
                    ComponentCollectionPropertyDrawer.Cache(array, genericType, attribute.CacheMethod);

                    return;
                }

                _reorderableListCollectionCache = ComponentCollectionPropertyDrawer.SetupReorderableList(_reorderableListCollectionCache, genericType, property, label, attribute);

                ComponentCollectionPropertyDrawer.DrawComponentCollection(_reorderableListCollectionCache, genericType, position, property, attribute);
                return;
            }

            EditorGUI.HelpBox(position, "You cannot put a CacheComponent attribute on a field that isn't a component!", MessageType.Warning);
            return;
        }

        // Disallow multi editing
        if (property.serializedObject.targetObjects.Length > 1)
        {
            DrawField();
            return;
        }

        // Get our modified component
        Component component = property.serializedObject.targetObject as Component;

        // Disallow any objects that aren't components
        if (component == null)
        {
            EditorGUI.HelpBox(position, "CacheComponent is being used on an object that isn't a component!", MessageType.Warning);
            return;
        }

        // Doesn't work if property already has a value, unless we always cache
        if (property.objectReferenceValue == null || attribute.AlwaysCache)
        {
            CacheMethod cacheMethod = attribute.CacheMethod;

            bool includeInactive = cacheMethod.HasFlag(CacheMethod.IncludeInactive);
            Object result = null;

            if (cacheMethod.HasFlag(CacheMethod.InChildren))
            {
                result = component.GetComponentInChildren(type, includeInactive);
            }

            if (result == null && cacheMethod.HasFlag(CacheMethod.InParent))
            {
                result = component.GetComponentInParent(type, includeInactive);
            }

            if (result == null)
            {
                result = component.GetComponent(type);
            }

            property.objectReferenceValue = result;
        }

        DrawField();
    }

    public static bool IsComponentCollection(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ComponentCollection<>);

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        CacheComponentAttribute attribute = this.attribute as CacheComponentAttribute;

        if (!attribute.DrawField)
        {
            return -EditorGUIUtility.standardVerticalSpacing;
        }

        if (IsComponentCollection(fieldInfo.FieldType))
        {
            return ComponentCollectionPropertyDrawer.GetComponentCollectionHeight(property, attribute.HaveCacheButton());
        }

        return EditorGUI.GetPropertyHeight(property);
    }
}

