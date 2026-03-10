using UnityEditor;

/// <summary>
/// Looks for whenever a <see cref="Sound"/> is created and will thusly call <see cref="SoundListEditor.CacheSounds(SerializedObject)"/> if one is detected.
/// <summary>
public class SoundListPostProcessor : AssetPostprocessor
{
    /// <summary>
    /// Method that is called internally by Unity when any asset is processed.
    /// </summary>
    /// <param name="importedAssets">An array of file paths for every new asset imported by Unity. If a file is in this array, then that means that the file was either changed or newly added to the project.</param>
    /// <param name="deletedAssets">An array of file paths for every deleted asset.</param>
    /// <param name="movedAssets">An array of file paths for every asset that were moved to a new location. Note that these file paths are the new and current locations of these files as they have already been moved. For old locations use <paramref name="movedFromAssetPaths"/>.</param>
    /// <param name="movedFromAssetPaths">An array of file paths for every asset that were moved to a new location. Note that these file paths are the old and not the current locations of the files.</param>
    /// <param name="didDomainReload">Will be true if the scripts were recompiled already (AKA the domain being reloaded).</param>
    public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
    {
        // Loop through every imported asset. These assets are either newly added to the project or were changed.
        foreach (string assetPath in importedAssets)
        {
            if (SoundListEditorInstance.CheckAssetAndCacheSounds(assetPath))
            {
                return;
            }
        }
    }
}