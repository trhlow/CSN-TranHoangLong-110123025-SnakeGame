using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple BFS Pathfinding cho Snake AI - Full Version
/// </summary>
public class AIPathfinding
{
    private GridManager gridManager;
    private const int MAX_ITERATIONS = 500;

    public AIPathfinding(GridManager grid)
    {
        gridManager = grid;
    }

    /// <summary>
    /// Tìm đường từ start đến target bằng BFS
    /// </summary>
    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int target, List<Vector2Int> obstacles)
    {
        if (!IsValidPosition(start) || !IsValidPosition(target))
            return null;

        if (obstacles.Contains(target))
            return null;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(start);
        visited.Add(start);

        int iterations = 0;

        while (queue.Count > 0 && iterations < MAX_ITERATIONS)
        {
            iterations++;
            Vector2Int current = queue.Dequeue();

            if (current == target)
            {
                return ReconstructPath(cameFrom, start, current);
            }

            Vector2Int[] neighbors = {
                current + Vector2Int.up,
                current + Vector2Int.down,
                current + Vector2Int.left,
                current + Vector2Int.right
            };

            foreach (Vector2Int next in neighbors)
            {
                if (!visited.Contains(next) && IsValidPosition(next) && !obstacles.Contains(next))
                {
                    visited.Add(next);
                    cameFrom[next] = current;
                    queue.Enqueue(next);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Tìm hướng an toàn nhất
    /// </summary>
    public Vector2Int FindSafeDirection(Vector2Int currentPos, List<Vector2Int> obstacles)
    {
        Vector2Int[] directions = {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        Vector2Int bestDirection = Vector2Int.zero;
        int bestScore = -1;

        foreach (Vector2Int dir in directions)
        {
            Vector2Int nextPos = currentPos + dir;

            if (!IsValidPosition(nextPos) || obstacles.Contains(nextPos))
                continue;

            int score = EvaluateSafety(nextPos, dir, obstacles);

            if (score > bestScore)
            {
                bestScore = score;
                bestDirection = dir;
            }
        }

        return bestDirection;
    }

    private int EvaluateSafety(Vector2Int pos, Vector2Int direction, List<Vector2Int> obstacles)
    {
        int score = 0;

        // 1. Đếm ô trống xung quanh
        int freeNeighbors = CountFreeNeighbors(pos, obstacles);
        score += freeNeighbors * 10;

        // 2. Không gian phía trước
        int spaceAhead = CountSpaceAhead(pos, direction, obstacles);
        score += spaceAhead * 5;

        // 3. Khoảng cách đến tường
        int distToWall = GetDistanceToWall(pos);
        if (distToWall < 2)
        {
            score -= 20;
        }

        // 4. Tránh chỗ chật
        if (freeNeighbors < 2)
        {
            score -= 50;
        }

        return score;
    }

    private int CountFreeNeighbors(Vector2Int pos, List<Vector2Int> obstacles)
    {
        int count = 0;
        Vector2Int[] neighbors = {
            pos + Vector2Int.up,
            pos + Vector2Int.down,
            pos + Vector2Int.left,
            pos + Vector2Int.right
        };

        foreach (Vector2Int neighbor in neighbors)
        {
            if (IsValidPosition(neighbor) && !obstacles.Contains(neighbor))
            {
                count++;
            }
        }

        return count;
    }

    private int CountSpaceAhead(Vector2Int start, Vector2Int direction, List<Vector2Int> obstacles)
    {
        int count = 0;
        Vector2Int current = start;
        int maxCheck = 5;

        for (int i = 0; i < maxCheck; i++)
        {
            current += direction;

            if (!IsValidPosition(current) || obstacles.Contains(current))
                break;

            count++;
        }

        return count;
    }

    private int GetDistanceToWall(Vector2Int pos)
    {
        int minDist = Mathf.Min(
            pos.x,
            pos.y,
            gridManager.GridWidth - pos.x - 1,
            gridManager.GridHeight - pos.y - 1
        );

        return minDist;
    }

    private bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridManager.GridWidth &&
               pos.y >= 0 && pos.y < gridManager.GridHeight;
    }

    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int start, Vector2Int current)
    {
        List<Vector2Int> path = new List<Vector2Int>();

        while (cameFrom.ContainsKey(current))
        {
            path.Add(current);
            current = cameFrom[current];

            if (current == start)
                break;
        }

        path.Reverse();
        return path;
    }

    public int GetManhattanDistance(Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
    }
}