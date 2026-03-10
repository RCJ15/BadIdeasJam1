using System.Diagnostics;
using UnityEditor;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

namespace Input.Editor
{
    /// <summary>
    /// Adds some options to the top toolbar in Unity. All options are found under "Tools/Game Input".
    /// </summary>
    public static class GameInputToolbarOptions
    {
        private static readonly Stopwatch _stopwatch = new Stopwatch();

        public const string PATH_START = "Tools/Game Input/";

        [MenuItem(PATH_START + "Ping Input Action Asset", priority = 1013)]
        public static void PingInputActionAsset()
        {
            EditorGUIUtility.PingObject(InputSystem.actions);
        }

        [MenuItem(PATH_START + "Open Input Action Asset", priority = 1014)]
        public static void OpenInputActionAsset()
        {
            AssetDatabase.OpenAsset(InputSystem.actions);
        }

        [MenuItem(PATH_START + "Generate Game Input Code", priority = 1025, secondaryPriority = 1015)]
        public static void GenerateGameInputCode()
        {
            Debug.Log("Generating code for Game Input asset...");

            _stopwatch.Start();

            GameInputCodeGeneration.GenerateCode(InputSystem.actions);

            Debug.Log("Done! Time taken: " + _stopwatch.ElapsedMilliseconds + " ms");

            _stopwatch.Stop();
        }

        [MenuItem(PATH_START + "Ping Game Input Script", priority = 1026, secondaryPriority = 1016)]
        public static void PingGameInputScript()
        {
            MonoScript script = GameInputCodeGeneration.GetGameInputMonoScript();

            if (script == null)
            {
                return;
            }

            EditorGUIUtility.PingObject(script);
        }

        [MenuItem(PATH_START + "Open Game Input Script", priority = 1027, secondaryPriority = 1017)]
        public static void OpenGameInputScript()
        {
            MonoScript script = GameInputCodeGeneration.GetGameInputMonoScript();

            if (script == null)
            {
                return;
            }

            AssetDatabase.OpenAsset(script);
        }
    }
}
