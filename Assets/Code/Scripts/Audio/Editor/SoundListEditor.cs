using UnityEditor;
using UnityEngine;

/// <summary>
/// The editor script for <see cref="SoundList"/>.
/// </summary>
[CustomEditor(typeof(SoundList))]
public class SoundListEditor : Editor
{
    private SerializedProperty _soundsProp;

    private void OnEnable()
    {
        _soundsProp = serializedObject.FindProperty("sounds");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();

        // Draw the sounds property as a disabled array
        EditorGUI.BeginDisabledGroup(true);

        EditorGUILayout.PropertyField(_soundsProp);

        EditorGUI.EndDisabledGroup();

        // Draw button for caching every single sound scriptable object
        if (GUILayout.Button("Cache Sounds"))
        {
            CacheSounds(serializedObject);
        }

        // Apply changes
        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// Will automatically search and cache every single <see cref="Sound"/> in the entire project into the <see cref="SoundList.Sounds"/> array.<para/>
    /// Do note that <see cref="SerializedObject.ApplyModifiedProperties"/> should be called after using this method, otherwise no data will be saved.
    /// </summary>
    public static void CacheSounds(SerializedObject serializedObject)
    {
        SerializedProperty soundsProp = serializedObject.FindProperty("sounds");

        soundsProp.ClearArray();

        // Loop through every single Sound in the entire project
        foreach (string guid in AssetDatabase.FindAssets($"t: {nameof(Sound)}"))
        {
            // Load sound asset
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            Sound sound = AssetDatabase.LoadAssetAtPath<Sound>(assetPath);

            // Create a new array element and assign it's value to the sound asset
            int index = soundsProp.arraySize;
            soundsProp.InsertArrayElementAtIndex(index);

            soundsProp.GetArrayElementAtIndex(index).objectReferenceValue = sound;
        }
    }
}