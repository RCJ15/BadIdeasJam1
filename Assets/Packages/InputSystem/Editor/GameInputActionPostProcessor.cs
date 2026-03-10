using UnityEditor;
using UnityEngine.InputSystem;

namespace Input.Editor
{
    /// <summary>
    /// A <see cref="AssetPostprocessor"/> that will specifically and only process a single <see cref="InputActionAsset"/> with the "Game Input" Addressable Address attached. <para/>
    /// When that <see cref="InputActionAsset"/> is processed, then that most likely means that the asset was changed in some way.
    /// This means that the <see cref="GameInputCodeGeneration"/> will be activated to generate new code to the <see cref="GameInput"/> <see cref="MonoScript"/> based on the new changes to the <see cref="InputAction"/>s that were modified in the <see cref="InputActionAsset"/>.
    /// </summary>
    public class GameInputActionPostProcessor : AssetPostprocessor
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
                // Check if the asset is of type InputActionAsset
                if (typeof(InputActionAsset) != AssetDatabase.GetMainAssetTypeAtPath(assetPath))
                {
                    continue;
                }

                // Load the asset as type InputActionAsset
                InputActionAsset asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(assetPath);

                // Return if the asset is not of type InputActionAsset
                if (asset == null)
                {
                    continue;
                }

                /*
                // Look if the asset is addressable
                AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
                AddressableAssetEntry entry = settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset)));

                // Return if it's not addressable
                if (entry == null)
                {
                    continue;
                }

                // Return if the adress of the entry is incorrect
                if (entry.address != GameInputManager.GAME_INPUT_ADDRESS)
                {
                    continue;
                }
                */

                // Check if the asset is the same as the InputSystem.actions
                if (asset != InputSystem.actions)
                {
                    continue;
                }

                // Found asset!
                // This means that our target asset was changed, which probably means that the InputActions in the asset were changed
                // We now generate new code to our GameInput MonoScript to reflect these new changes to the asset, AUTOMATICALLY!!!
                GameInputCodeGeneration.GenerateCode(asset);

                return;
            }
            //-- End of Imported Assets loop
        }
        //-- End of Method
    }
    //-- End of Script
}
//-- End of Namespace
