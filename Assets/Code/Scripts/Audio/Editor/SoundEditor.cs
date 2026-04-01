using UnityEditor;
using UnityEngine;

/// <summary>
/// The editor script for <see cref="Sound"/>.
/// </summary>
[CustomEditor(typeof(Sound))]
[CanEditMultipleObjects]
public class SoundEditor : Editor
{
    private SerializedProperty _typeProp;
    private SerializedProperty _clipsProp;
    private SerializedProperty _volumeProp;
    private SerializedProperty _pitchProp;
    private SerializedProperty _mixerGroupOverrideProp;

    private void OnEnable()
    {
        _typeProp = serializedObject.FindProperty("type");
        _clipsProp = serializedObject.FindProperty("clips");
        _volumeProp = serializedObject.FindProperty("volume");
        _pitchProp = serializedObject.FindProperty("pitch");
        _mixerGroupOverrideProp = serializedObject.FindProperty("mixerGroupOverride");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();

        EditorGUILayout.PropertyField(_typeProp);

        // Apply changes to update _typeProp for later use
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();

        // Convert _typeProp into it's actual enum value
        Sound.SoundType soundType = (Sound.SoundType)_typeProp.enumValueIndex;

        switch (soundType)
        {
            // Draw only the first clip as a property if the sound is single
            case Sound.SoundType.Single:
                // Fix not single arrays
                if (_clipsProp.arraySize != 1)
                {
                    _clipsProp.arraySize = 1;
                }

                // Draw field
                EditorGUILayout.PropertyField(_clipsProp.GetArrayElementAtIndex(0), new GUIContent("Clip"));

                break;

            // Otherwise just draw the array as is
            default:
                EditorGUILayout.PropertyField(_clipsProp);
                break;
        }

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(_volumeProp);
        EditorGUILayout.PropertyField(_pitchProp);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(_mixerGroupOverrideProp);

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledGroupScope(SoundManager.Instance == null))
        {
            if (GUILayout.Button("Play Preview"))
            {
                SoundManager.PlaySound((Sound)target);
            }
        }

        // Apply changes
        serializedObject.ApplyModifiedProperties();
    }
}