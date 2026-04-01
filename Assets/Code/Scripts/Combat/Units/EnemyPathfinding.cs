using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implementation of A* pathfinding.
/// </summary>
[SingletonMode(true)]
public class EnemyPathfinding : Singleton<EnemyPathfinding>
{
    private static readonly Dictionary<Tile, TileScores> _scores = new();
    private static readonly List<Tile> _open = new();
    private static readonly HashSet<Vector2Int> _closed = new();
    private static readonly Dictionary<Vector2Int, Tile> _parentMap = new();

    // Potential settings

    public static List<Tile> FindPath(Vector2Int start, Vector2Int end)
    {
        List<Tile> result = new();
        FindPath(start, end, result);
        return result;
    }

    public static List<Tile> FindPath(Tile start, Tile end)
    {
        List<Tile> result = new();
        FindPath(start, end, result);
        return result;
    }

    public static void FindPath(Vector2Int start, Vector2Int end, List<Tile> list)
    {
        Board board = Board.Instance;

        if (board == null)
        {
            list.Clear();
            return;
        }

        FindPath(board.GetTile(start), board.GetTile(end), list);
    }

    public static void FindPath(Tile start, Tile end, List<Tile> list)
    {
        list.Clear();

        if (start == null) return;
        if (end == null) return;

        bool endOccupied = end.Occupied;

        _open.Clear();
        _scores.Clear();
        _closed.Clear();
        _parentMap.Clear();

        _open.Add(start);
        _scores.Add(start, new(start, 0, Distance(start, end)));

        int count = 1;

        while (count > 0)
        {
            // Get the tile with the lowest F cost
            Tile current = null;
            int index = -1;
            TileScores currentScores = new TileScores(null, Mathf.Infinity, Mathf.Infinity);

            for (int i = 0; i < count; i++)
            {
                Tile tile = _open[i];
                TileScores score = _scores[tile];

                if (score.F < currentScores.F)
                {
                    current = tile;
                    currentScores = score;
                    index = i;
                }
            }

            // Reached end???
            bool reachedEnd;

            if (endOccupied)
            {
                // Reaching a neighbor is enough if the end space is occupied
                reachedEnd = false;

                for (int i = 0; i < 4; i++)
                {
                    Tile endNeighbor = end.Neighbors[i];

                    if (endNeighbor == null) continue;

                    if (current == endNeighbor)
                    {
                        reachedEnd = true;
                        break;
                    }
                }
            }
            else
            {
                reachedEnd = current == end;
            }

            if (reachedEnd)
            {
                ConstructPath(_parentMap, current, list);
                return;
            }

            _open.RemoveAt(index);
            count--;

            _closed.Add(current.GridPos);

            for (int i = 0; i < 4; i++)
            {
                Tile neighbor = current.Neighbors[i];

                if (neighbor == null || _closed.Contains(neighbor.GridPos) || neighbor.Occupied) continue;

                float tentativeG = currentScores.G + 1;

                if (_scores.TryGetValue(neighbor, out TileScores score) && tentativeG >= score.G)
                {
                    continue;
                }

                score.Tile = neighbor;
                score.G = tentativeG;
                score.H = Distance(neighbor, end);

                _scores[neighbor] = score;

                // Set the current node as the parent of the neighbor
                _parentMap[neighbor.GridPos] = current;

                if (!_open.Contains(neighbor))
                {
                    _open.Add(neighbor);
                    count++;
                }
            }
        }

        // Couldn't find path :(
    }

    private static List<Tile> ConstructPath(Dictionary<Vector2Int, Tile> parentMap, Tile current, List<Tile> output)
    {
        output.Add(current);

        while (parentMap.ContainsKey(current.GridPos))
        {
            current = parentMap[current.GridPos];

            output.Add(current);
        }

        output.Reverse();

        return output;
    }

    public static float Distance(Tile t1, Tile t2)
    {
        return Distance(t1.GridPos, t2.GridPos);
    }
    public static float Distance(Vector2Int p1, Vector2Int p2)
    {
        return Mathf.Abs(p1.x - p2.x) + Mathf.Abs(p1.y - p2.y);
    }

    internal static List<Tile> FindPath(Tile tile1, object tile2)
    {
        throw new NotImplementedException();
    }

    public struct TileScores
    {
        public Tile Tile;

        /// <summary>
        /// G is how many spaces it takes to reach this tile
        /// </summary>
        public float G;
        /// <summary>
        /// H is how far away the space is from the destination
        /// </summary>
        public float H;
        /// <summary>
        /// F is the total cost of both G and H
        /// </summary>
        public float F => G + H;

        public TileScores(Tile tile, float g, float h)
        {
            Tile = tile;
            G = g;
            H = h;
        }
    }
}
