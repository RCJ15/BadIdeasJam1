using System;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int GridPos { get; set; }
    public Board Board { get; set; }

    public Vector3 Pos
    {
        get => transform.position;
        set => transform.position = value;
    }

    public float PosX
    {
        get => Pos.x;
        set => Pos = new Vector3(value, Pos.y, Pos.z);
    }

    public float PosY
    {
        get => Pos.y;
        set => Pos = new Vector3(Pos.x, value, Pos.z);
    }

    public float PosZ
    {
        get => Pos.z;
        set => Pos = new Vector3(Pos.x, Pos.y, value);
    }

    public Tile[] Neighbors { get; private set; } = new Tile[4];

    /// <summary>
    /// The unit standing on this tile
    /// </summary>
    public Unit Unit { get; set; }

    public bool Occupied => Unit != null && Unit.isActiveAndEnabled;

    private static readonly float FULL_ROTATION_RADIANS = Mathf.PI * 2f;

    private Vector3 _offset;

    [Header("Sine Movement")]
    [SerializeField] private float sineIntensity;
    [SerializeField] private float sineSpeed;
    [SerializeField] private float sineOffsetIntensity = 0.5f;

    private float _sineAngle;
    private float _sineOffset;

    private void Start()
    {
        _sineOffset = (float)(GridPos.x + GridPos.y) * sineOffsetIntensity;
    }

    public void UpdatePos()
    {
        Vector3 worldPos = Board.GridToWorld(GridPos);

        Pos = worldPos + _offset;
    }

    private void LateUpdate()
    {
        _offset.y = Mathf.Sin(_sineAngle + _sineOffset) * sineIntensity;
        _sineAngle += sineSpeed * Time.deltaTime;
        _sineAngle %= FULL_ROTATION_RADIANS;

        UpdatePos();
    }

    public bool IsAdjacentTo(Tile tile) => IsAdjacentTo(tile.GridPos);

    public bool IsAdjacentTo(Vector2Int pos)
    {
        Vector2Int delta = GridPos - pos;
        return (Mathf.Abs(delta.x) == 1 && delta.y == 0) || (delta.x == 0 && Mathf.Abs(delta.y) == 1);
    }
}
