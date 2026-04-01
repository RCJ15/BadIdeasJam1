using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Board : Singleton<Board>
{
    [CacheComponent]
    [SerializeField] private Grid grid;

    [Space]
    [SerializeField] private Vector2Int size;

    public Vector2Int OriginOffset
    {
        get
        {
            if (!_originOffset.HasValue)
            {
                _originOffset = -Vector2Int.FloorToInt((Vector2)size / 2f);
            }

            return _originOffset.Value;
        }
    }
    private Vector2Int? _originOffset;

    [Space]
    [SerializeField] private Tile tileTemplate;

    public Tile[,] Tiles => _tiles;
    private Tile[,] _tiles;

    private bool _tilesDirty;

    protected override void Awake()
    {
        base.Awake();

        _tiles = new Tile[size.x, size.y];

        int leftIndex = (int)Direction.Left;
        int rightIndex = (int)Direction.Right;
        int downIndex = (int)Direction.Down;
        int upIndex = (int)Direction.Up;

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int pos = new(x, y);

                Tile tile = Instantiate(tileTemplate, Vector3.zero, Quaternion.identity, transform);
                tile.GridPos = pos;
                tile.Board = this;

                _tiles[x, y] = tile;

                bool xOver = x > 0;
                bool yOver = y > 0;

                if (xOver)
                {
                    Tile leftNeighbor = GetTile(pos + Vector2Int.left);
                    tile.Neighbors[leftIndex] = leftNeighbor;
                    leftNeighbor.Neighbors[rightIndex] = tile;
                }

                if (yOver)
                {
                    Tile downNeighbor = GetTile(pos + Vector2Int.down);
                    tile.Neighbors[downIndex] = downNeighbor;
                    downNeighbor.Neighbors[upIndex] = tile;
                }
            }
        }

        foreach (Tile tile in _tiles)
        {
            tile.UpdatePos();
        }
    }

    private void LateUpdate()
    {
        if (!_tilesDirty)
        {
            return;
        }

        _tilesDirty = false;

        // Update tile positions
        foreach (Tile tile in _tiles)
        {
            tile.UpdatePos();
        }
    }

    public Vector2Int ClampPos(Vector2Int pos) => new(ClampX(pos.x), ClampY(pos.y));
    public int ClampX(int x) => Mathf.Clamp(x, 0, size.x - 1);
    public int ClampY(int y) => Mathf.Clamp(y, 0, size.y - 1);

    public Tile GetTile(Vector2Int pos) => GetTile(pos.x, pos.y);

    public Tile GetTile(int x, int y)
    {
        if (x < 0 || y < 0 || x >= size.x || y >= size.y) return null;

        return Tiles[x, y];
    }

    public Vector3 GridToWorld(Vector2Int pos)
    {
        pos = ClampPos(pos);
        return grid.CellToWorld((Vector3Int)(pos + OriginOffset)) + new Vector3(grid.cellSize.x, 0, grid.cellSize.y) / 2f;
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3Int cellPos = grid.WorldToCell(worldPos);

        Vector2Int pos = (Vector2Int)cellPos - OriginOffset;
        pos = ClampPos(pos);

        return pos;
    }

#if UNITY_EDITOR
    public void OnValidate()
    {
        _originOffset = null;

        Vector3 gridPos = grid.transform.localPosition;

        gridPos.x = size.x % 2 == 0 ? 0 : -grid.cellSize.x / 2f;
        gridPos.z = size.y % 2 == 0 ? 0 : -grid.cellSize.y / 2f;

        grid.transform.localPosition = gridPos;

        _tilesDirty = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3 worldPos = GridToWorld(new(x, y));
                Gizmos.DrawWireCube(worldPos, new(grid.cellSize.x, 0, grid.cellSize.z));
            }
        }
    }
#endif
}

#if UNITY_EDITOR
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

            switch(path)
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
    }
}
#endif