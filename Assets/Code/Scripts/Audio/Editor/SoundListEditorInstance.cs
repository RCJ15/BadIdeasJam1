using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Basically just contains the editor version of <see cref="SoundList.Instance"/>.
/// <summary>
public class SoundListEditorInstance : MonoBehaviour
{
    /// <summary>
    /// Uses <see cref="AddressablesEditorUtility.LoadAssetUsingAddress"/> to load and cache the static <see cref="SoundList"/> instance for use within the editor.
    /// </summary>
    public static SoundList Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<SoundList>(nameof(SoundList));
            }

            return _instance;
        }
    }
    private static SoundList _instance;

    /// <summary>
    /// Will check an asset file path if it's of type <see cref="Sound"/> and will therefore call <see cref="SoundListEditor.CacheSounds(SerializedObject)"/> if the asset path is of correct type.
    /// </summary>
    /// <param name="assetPath">The file path of the asset.</param>
    /// <returns>If <see cref="SoundListEditor.CacheSounds(SerializedObject)"/> was called or not.</returns>
    public static bool CheckAssetAndCacheSounds(string assetPath)
    {
        // We are simply interested in the type of file that was modified, so we use GetMainAssetTypeAtPath instead of loading the asset
        Type type = AssetDatabase.GetMainAssetTypeAtPath(assetPath);

        // We found a file that was of type Sound
        if (type == typeof(Sound))
        {
            using (SerializedObject serializedObject = new SerializedObject(Instance))
            {
                // Cache every single Sound in the project and return out of this method
                SoundListEditor.CacheSounds(serializedObject);

                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }

            return true;
        }

        return false;
    }
}