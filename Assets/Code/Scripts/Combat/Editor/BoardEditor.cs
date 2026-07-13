using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Board))]
public class BoardEditor : Editor
{
    private Board _board;

    private void OnEnable()
    {
        _board = (Board)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();
        SerializedProperty prop = serializedObject.GetIterator();

        bool enterChildren = true;

        while (prop.NextVisible(enterChildren))
        {
            string path = prop.propertyPath;

            enterChildren = false;

            switch (path)
            {
                case "m_Script":
                    continue;
            }

            EditorGUILayout.PropertyField(prop, true);

            switch (path)
            {
                case "size":
                    Grid grid = _board.GetComponentInChildren<Grid>(true);

                    Undo.RecordObject(grid, "Change Cell Size/Gap");

                    EditorGUI.BeginChangeCheck();

                    grid.cellSize = EditorGUILayout.Vector3Field("Cell Size", grid.cellSize);
                    grid.cellGap = EditorGUILayout.Vector3Field("Cell Gap", grid.cellGap);

                    if (EditorGUI.EndChangeCheck())
                    {
                        _board.OnValidate();
                    }
                    break;
            }
        }

        serializedObject.ApplyModifiedProperties();

        if (!Application.isPlaying)
        {
            return;
        }

        EditorGUILayout.Space();

        // Draw minimap of the board
        _board.OnDebugGUI();
        /*
        Vector2Int size = _board.Size;
        float cellSize = 20f;

        EditorGUI.BeginDisabledGroup(true);

        Rect overallRect = EditorGUILayout.GetControlRect(GUILayout.Width(cellSize * (float)size.x), GUILayout.Height(cellSize * (float)size.y));
        EditorGUI.DrawRect(overallRect, new(0, 0, 0, 0.1f));

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Rect rect = new(new Vector2(overallRect.xMin, overallRect.yMax) + new Vector2((float)x * cellSize, (float)(y + 1) * -cellSize), cellSize * Vector2.one);
                EditorGUI.Toggle(rect, _board.Tiles[x, y].Occupied);
            }
        }

        EditorGUI.EndDisabledGroup();
        */
    }
}
