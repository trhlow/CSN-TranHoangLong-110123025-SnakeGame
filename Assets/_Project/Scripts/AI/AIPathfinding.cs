using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIPathfinding
{
    private class Node
    {
        public Vector2Int position;
        public Node parent;
        public int gCost;
        public int hCost;
        public int fCost => gCost + hCost;

        public Node(Vector2Int pos)
        {
            position = pos;
            parent = null;
            gCost = 0;
            hCost = 0;
        }
    }

    private GridManager gridManager;
    private int maxIterations = 1000;

    public AIPathfinding(GridManager grid)
    {
        gridManager = grid;
    }

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int target, List<Vector2Int> obstacles)
    {
        if (!gridManager.IsValidPosition(start) || !gridManager.IsValidPosition(target))
        {
            return null;
        }

        if (obstacles.Contains(target))
        {
            return null;
        }

        List<Node> openSet = new List<Node>();
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();

        Node startNode = new Node(start);
        openSet.Add(startNode);

        int iterations = 0;

        while (openSet.Count > 0 && iterations < maxIterations)
        {
            iterations++;

            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost ||
                    (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode.position);

            if (currentNode.position == target)
            {
                return RetracePath(startNode, currentNode);
            }

            foreach (Vector2Int neighborPos in GetNeighbors(currentNode.position))
            {
                if (closedSet.Contains(neighborPos) || obstacles.Contains(neighborPos))
                {
                    continue;
                }

                int newGCost = currentNode.gCost + 1;
                Node neighborNode = openSet.FirstOrDefault(n => n.position == neighborPos);

                if (neighborNode == null)
                {
                    neighborNode = new Node(neighborPos);
                    neighborNode.gCost = newGCost;
                    neighborNode.hCost = GetManhattanDistance(neighborPos, target);
                    neighborNode.parent = currentNode;
                    openSet.Add(neighborNode);
                }
                else if (newGCost < neighborNode.gCost)
                {
                    neighborNode.gCost = newGCost;
                    neighborNode.parent = currentNode;
                }
            }
        }

        return null;
    }

    public Vector2Int FindSafeDirection(Vector2Int currentPos, List<Vector2Int> obstacles)
    {
        List<Vector2Int> directions = new List<Vector2Int>
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        directions = directions.OrderBy(x => Random.value).ToList();

        Vector2Int bestDirection = Vector2Int.zero;
        int bestScore = -1;

        foreach (Vector2Int dir in directions)
        {
            Vector2Int nextPos = currentPos + dir;

            if (!gridManager.IsValidPosition(nextPos) || obstacles.Contains(nextPos))
            {
                continue;
            }

            int score = EvaluatePosition(nextPos, obstacles);

            if (score > bestScore)
            {
                bestScore = score;
                bestDirection = dir;
            }
        }

        return bestDirection;
    }

    private int EvaluatePosition(Vector2Int pos, List<Vector2Int> obstacles)
    {
        int score = 0;

        foreach (Vector2Int neighborPos in GetNeighbors(pos))
        {
            if (gridManager.IsValidPosition(neighborPos) && !obstacles.Contains(neighborPos))
            {
                score += 10;
            }
        }

        int centerX = gridManager.GridWidth / 2;
        int centerY = gridManager.GridHeight / 2;
        Vector2Int center = new Vector2Int(centerX, centerY);
        int distanceToCenter = GetManhattanDistance(pos, center);
        score += (gridManager.GridWidth - distanceToCenter) / 2;

        int freeSpaces = CountFreeSpaces(pos, obstacles, 3);
        score += freeSpaces * 5;

        return score;
    }

    private int CountFreeSpaces(Vector2Int start, List<Vector2Int> obstacles, int maxDepth)
    {
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Queue<(Vector2Int pos, int depth)> queue = new Queue<(Vector2Int, int)>();
        queue.Enqueue((start, 0));
        visited.Add(start);

        int count = 0;

        while (queue.Count > 0)
        {
            var (currentPos, depth) = queue.Dequeue();
            count++;

            if (depth >= maxDepth)
                continue;

            foreach (Vector2Int neighborPos in GetNeighbors(currentPos))
            {
                if (visited.Contains(neighborPos) ||
                    !gridManager.IsValidPosition(neighborPos) ||
                    obstacles.Contains(neighborPos))
                {
                    continue;
                }

                visited.Add(neighborPos);
                queue.Enqueue((neighborPos, depth + 1));
            }
        }

        return count;
    }

    private List<Vector2Int> RetracePath(Node startNode, Node endNode)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Node currentNode = endNode;

        while (currentNode != startNode && currentNode != null)
        {
            path.Add(currentNode.position);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }

    private List<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        Vector2Int[] directions = {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighborPos = pos + dir;
            if (gridManager.IsValidPosition(neighborPos))
            {
                neighbors.Add(neighborPos);
            }
        }

        return neighbors;
    }

    private int GetManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}