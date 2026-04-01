using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DebugTools
{
    /// <summary>
    /// A static class that contains some basic debug commands
    /// <summary>
    public class GeneralDebugCommands
    {
#if UNITY_EDITOR
        [DebugCommand("Has the same effect as pressing the pause button in the Unity Editor")]
        public static void ForceEditorPause()
        {
            Debug.Log("Forced Editor Pause");

            EditorApplication.isPaused = true;
        }
#endif

        [DebugCommand("Clears the log located below the command prompt", CloseDebugConsole = false)]
        public static void ClearLog()
        {
            DebugConsole.ClearLog();
        }

        [DebugCommand("Sets Time.timeScale to the given value")]
        public static void SetTimeScale(float value)
        {
            Time.timeScale = value;
        }

        [DebugCommand("Goes to the given scene")]
        public static void GotoScene(int sceneIndex)
        {
            SceneManager.LoadScene(sceneIndex);
        }

        [DebugCommand("Loads the given scene additively")]
        public static void LoadScene(int sceneIndex)
        {
            SceneManager.LoadScene(sceneIndex, LoadSceneMode.Additive);
        }


        [DebugCommand("Unloads the given scene")]
        public static void UnloadScene(int sceneIndex)
        {
            SceneManager.UnloadSceneAsync(sceneIndex);
        }
    }
}
