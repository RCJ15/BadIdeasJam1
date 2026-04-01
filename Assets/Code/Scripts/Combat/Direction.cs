using UnityEngine;

public enum Direction
{
    /// <summary>
    /// North & Forward
    /// </summary>
    Up,
    /// <summary>
    /// South & Backward
    /// </summary>
    Down,
    /// <summary>
    /// West
    /// </summary>
    Left,
    /// <summary>
    /// East
    /// </summary>
    Right
}

public static class DirectionExtensions
{
    public static Direction Opposite(this Direction direction)
    {
        return direction switch
        {
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            _ => direction,
        };
    }

    public static Direction RotateClockwise(this Direction direction)
    {
        return direction switch
        {
            Direction.Up => Direction.Right,
            Direction.Right => Direction.Down,
            Direction.Down => Direction.Left,
            Direction.Left => Direction.Up,
            _ => direction,
        };
    }

    public static Direction RotateCounterClockwise(this Direction direction)
    {
        return direction switch
        {
            Direction.Up => Direction.Left,
            Direction.Left => Direction.Down,
            Direction.Down => Direction.Right,
            Direction.Right => Direction.Up,
            _ => direction,
        };
    }

    public static Vector2 ToVector2(this Direction direction)
    {
        return direction switch
        {
            Direction.Up => Vector2.up,
            Direction.Down => Vector2.down,
            Direction.Left => Vector2.left,
            Direction.Right => Vector2.right,
            _ => Vector2.zero,
        };
    }

    public static Vector3 ToVector3(this Direction direction)
    {
        return direction switch
        {
            Direction.Up => Vector3.forward,
            Direction.Down => Vector3.back,
            Direction.Left => Vector3.left,
            Direction.Right => Vector3.right,
            _ => Vector3.zero,
        };
    }

    public static Vector2Int ToVector2Int(this Direction direction)
    {
        return direction switch
        {
            Direction.Up => Vector2Int.up,
            Direction.Down => Vector2Int.down,
            Direction.Left => Vector2Int.left,
            Direction.Right => Vector2Int.right,
            _ => Vector2Int.zero,
        };
    }

    public static Vector3Int ToVector3Int(this Direction direction)
    {
        return direction switch
        {
            Direction.Up => Vector3Int.forward,
            Direction.Down => Vector3Int.back,
            Direction.Left => Vector3Int.left,
            Direction.Right => Vector3Int.right,
            _ => Vector3Int.zero,
        };
    }
}
