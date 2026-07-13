using UnityEngine;

public static class TileUtility
{
    public static int ValueFromGridPosition(Vector2Int pos, int heightMultiplier)
    {
        return pos.x + pos.y * heightMultiplier;
    }

    public static float Distance(Tile t1, Tile t2) => Distance(t1.GridPos, t2.GridPos);
    public static float Distance(Vector2Int p1, Vector2Int p2)
    {
        return Mathf.Abs(p1.x - p2.x) + Mathf.Abs(p1.y - p2.y);
    }

    public static bool IsAdjacentTo(Tile tile, Tile other) => IsAdjacentTo(tile.GridPos, other.GridPos);
    public static bool IsAdjacentTo(Vector2Int pos, Vector2Int other)
    {
        Vector2Int delta = pos - other;
        return (Mathf.Abs(delta.x) == 1 && delta.y == 0) || (delta.x == 0 && Mathf.Abs(delta.y) == 1);
    }

    public static Direction FaceToward(Tile tile, Tile faceToward, Direction? inDirection = null) => FaceToward(tile.GridPos, faceToward.GridPos, inDirection);
    public static Direction FaceToward(Vector2Int pos, Vector2Int faceToward, Direction? inDirection = null)
    {
        Vector2 normal = (Vector2)(faceToward - pos);
        float angle = Vector2.SignedAngle(normal, Vector2.up);

        // Special corner cases
        if (inDirection.HasValue)
        {
            float absAngle = Mathf.Abs(angle);

            if (absAngle == 45f || absAngle == 135)
            {
                Direction opposite = inDirection.Value.Opposite();

                Direction y = absAngle == 45f ? Direction.Up : Direction.Down;
                Direction x = angle < 0 ? Direction.Left : Direction.Right;

                if (opposite == y)
                {
                    return x;
                }
                else if (opposite == x)
                {
                    return y;
                }
                else
                {
                    // Already looking in one of the 2 directions
                    return inDirection.Value;
                }
            }
        }

        Direction direction;

        if (angle >= -45f && angle <= 45f)
        {
            direction = Direction.Up;
        }
        else if (angle > 45f && angle <= 135)
        {
            direction = Direction.Right;
        }
        else if (angle < -45 && angle >= -135f)
        {
            direction = Direction.Left;
        }
        else
        {
            direction = Direction.Down;
        }

        return direction;
    }
}
