using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

    // Potential settings go here

    public static async Task<List<Tile>> FindPath(Vector2Int start, Vector2Int end, CancellationToken token)
    {
        List<Tile> result = new();
        await FindPath(start, end, result, token);
        return result;
    }

    public static async Task<List<Tile>> FindPath(Tile start, Tile end, CancellationToken token)
    {
        List<Tile> result = new();
        await FindPath(start, end, result, token);
        return result;
    }

    public static async Task FindPath(Vector2Int start, Vector2Int end, List<Tile> list, CancellationToken token)
    {
        Board board = Board.Instance;

        if (board == null)
        {
            list.Clear();
            return;
        }

        await FindPath(board.GetTile(start), board.GetTile(end), list, token);
    }

    public static async Task FindPath(Tile start, Tile end, List<Tile> list, CancellationToken token)
    {
        list.Clear();

        if (start == null) return;
        if (end == null) return;

        bool endOccupied = end.Occupied;

        await Awaitable.BackgroundThreadAsync();

        Tile closest = start;
        float closestH = TileUtility.Distance(start, end);

        _open.Clear();
        _scores.Clear();
        _closed.Clear();
        _parentMap.Clear();

        _open.Add(start);
        _scores.Add(start, new(start, 0, TileUtility.Distance(start, end)));

        int count = 1;

        while (count > 0)
        {
            if (token.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }

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

            // Set closest tile
            if (currentScores.H < closestH)
            {
                closestH = currentScores.H;
                closest = current;
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
                await ConstructPath(_parentMap, current, list, token);
                return;
            }

            _open.RemoveAt(index);
            count--;

            _closed.Add(current.GridPos);

            // Go through all valid neighbors around the current tile and label them as next to be searched
            for (int i = 0; i < 4; i++)
            {
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }

                Tile neighbor = current.Neighbors[i];

                await Awaitable.MainThreadAsync();

                try
                {
                    if (neighbor == null || _closed.Contains(neighbor.GridPos) || neighbor.Occupied)
                    {
                        continue;
                    }
                }
                finally
                {
                    await Awaitable.BackgroundThreadAsync();
                }

                float tentativeG = currentScores.G + 1;

                if (_scores.TryGetValue(neighbor, out TileScores score) && tentativeG >= score.G)
                {
                    continue;
                }

                score.Tile = neighbor;
                score.G = tentativeG;
                score.H = TileUtility.Distance(neighbor, end);

                _scores[neighbor] = score;

                // Set the current node as the parent of the neighbor
                // As in the neighbor should follow back to this tile
                _parentMap[neighbor.GridPos] = current;

                if (!_open.Contains(neighbor))
                {
                    _open.Add(neighbor);
                    count++;
                }
            }
        }

        // Couldn't find direct line to goal so take the next best
        if (closest != null)
        {
            await ConstructPath(_parentMap, closest, list, token);
        }
    }

    private static async Task ConstructPath(Dictionary<Vector2Int, Tile> parentMap, Tile current, List<Tile> output, CancellationToken token)
    {
        await Awaitable.BackgroundThreadAsync();

        output.Add(current);

        while (parentMap.ContainsKey(current.GridPos))
        {
            if (token.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }

            current = parentMap[current.GridPos];

            output.Add(current);
        }

        output.Reverse();
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
