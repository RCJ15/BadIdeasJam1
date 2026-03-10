using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SubAssetsWindow : EditorWindow
{
    [SerializeField] private Object parent;

    [SerializeField] private string renameParamter;

    [MenuItem("Window/Sub Asset Window")]
    private static void Open()
    {
        SubAssetsWindow window = GetWindow<SubAssetsWindow>();

        window.titleContent = new GUIContent("Sub Assets Window");
    }

    public void OnGUI()
    {
        parent = EditorGUILayout.ObjectField(parent, typeof(Object), false);

        EditorGUILayout.Space();

        bool disable = parent == null || Selection.objects.Length <= 0 || AssetDatabase.IsSubAsset(parent);

        using (new EditorGUI.DisabledScope(disable))
        {
            if (GUILayout.Button("Add selected assets as sub assets"))
            {
                List<Object> allNewObjects = new List<Object>();

                foreach (Object obj in Selection.objects)
                {
                    Object newObj = Instantiate(obj);
                    newObj.name = obj.name;

                    AssetDatabase.AddObjectToAsset(newObj, parent);

                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(obj));

                    allNewObjects.Add(newObj);
                }

                Selection.objects = allNewObjects.ToArray();

                Reload();
            }

            if (GUILayout.Button("Remove selected sub assets"))
            {
                foreach (Object obj in Selection.objects)
                {
                    if (!AssetDatabase.IsSubAsset(obj))
                    {
                        continue;
                    }

                    AssetDatabase.RemoveObjectFromAsset(obj);

                    string newPath = AssetDatabase.GetAssetPath(parent);

                    newPath = newPath.Substring(0, newPath.LastIndexOf('/') + 1);

                    string ext;
                    switch (obj)
                    {
                        case Material:
                            ext = ".mat";
                            break;

                        case Cubemap:
                            ext = ".Cubemap";
                            break;

                        case GUISkin:
                            ext = ".GUISkin";
                            break;

                        case AnimationClip:
                            ext = ".anim";
                            break;

                        default:
                            ext = ".asset";
                            break;
                    }

                    newPath += obj.name + ext;

                    AssetDatabase.CreateAsset(obj, newPath);
                }

                Reload();
            }
        }

        EditorGUILayout.Space();

        renameParamter = EditorGUILayout.TextField(renameParamter);

        using (new EditorGUI.DisabledScope(disable))
        {
            if (GUILayout.Button("Rename selected sub assets"))
            {
                foreach (Object obj in Selection.objects)
                {
                    if (!AssetDatabase.IsSubAsset(obj))
                    {
                        continue;
                    }

                    obj.name = renameParamter;
                }

                Reload();
            }
        }
    }

    private void Reload()
    {
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(parent), ImportAssetOptions.ForceUpdate);
    }
}