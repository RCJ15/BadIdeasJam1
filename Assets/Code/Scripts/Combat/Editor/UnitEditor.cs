using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Unit), true)]
public class UnitEditor : Editor
{
    private Unit _target;

    private void OnEnable()
    {
        _target = (Unit)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUI.BeginDisabledGroup(!Application.isPlaying);
        EditorGUILayout.LabelField("Control Unit", EditorStyles.boldLabel);

        Vector2 cellSize = new(40, 20);

        Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(cellSize.x * 3f), GUILayout.Height(cellSize.y * 3f));

        EditorGUI.DrawRect(rect, new(0, 0, 0, 0.1f));

        void Move(Vector2Int offset) => _target.MoveToTile(_target.GridPos + offset, true);

        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                Rect cellRect = new(rect.min + new Vector2((float)x * cellSize.x, (float)y * cellSize.y), cellSize);

                if (y == 1)
                {
                    if (x == 0)
                    {
                        if (GUI.Button(cellRect, "Left"))
                        {
                            Move(Vector2Int.left);
                        }
                    }
                    else if (x == 2)
                    {
                        if (GUI.Button(cellRect, "Right"))
                        {
                            Move(Vector2Int.right);
                        }
                    }
                }
                else if (x == 1)
                {
                    if (y == 0)
                    {
                        if (GUI.Button(cellRect, "Up"))
                        {
                            Move(Vector2Int.up);
                        }
                    }
                    else if (y == 2)
                    {
                        if (GUI.Button(cellRect, "Down"))
                        {
                            Move(Vector2Int.down);
                        }
                    }
                }

            }
        }

        EditorGUI.EndDisabledGroup();
    }
}
