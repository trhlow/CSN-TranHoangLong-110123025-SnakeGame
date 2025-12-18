using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SnakeController))]
public class AIController : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float thinkDelay = 0.2f; // Tốc độ suy nghĩ (có thể khác với move speed)
    [SerializeField] private int visionRange = 15;

    [Header("Strategy")]
    [SerializeField] private AIStrategy strategy = AIStrategy.Balanced;

    [Header("Debug")]
    [SerializeField] private bool showDebugPath = false;
    [SerializeField] private bool showDebugInfo = false;

    private SnakeController snake;
    private AIPathfinding pathfinding;

    private float lastThinkTime = 0f;
    private List<Vector2Int> currentPath = null;
    private GameObject targetFood = null;

    public enum AIStrategy
    {
        Aggressive,
        Defensive,
        Balanced
    }

    private void Awake()
    {
        snake = GetComponent<SnakeController>();
        pathfinding = new AIPathfinding(GridManager.Instance);
    }

    private void Start()
    {
        if (snake != null)
        {
            snake.SetAI(true);
        }
    }

    private void Update()
    {
        if (snake == null || snake.IsDead)
            return;

        if (Time.time - lastThinkTime >= thinkDelay)
        {
            Think();
            lastThinkTime = Time.time;
        }
    }

    private void Think()
    {
        Vector2Int headPos = GetHeadGridPosition();
        Vector2Int currentDir = snake.GetCurrentDirection();
        List<Vector2Int> obstacles = GetAllObstacles();

        // Kiểm tra nguy hiểm trước mắt
        if (IsInDanger(headPos, currentDir, obstacles))
        {
            HandleDanger(headPos, obstacles);
            return;
        }

        // Tìm food và di chuyển
        FindAndMoveToFood(headPos, obstacles);
    }

    private void FindAndMoveToFood(Vector2Int headPos, List<Vector2Int> obstacles)
    {
        if (FoodSpawner.Instance == null)
        {
            MoveSafely(headPos, obstacles);
            return;
        }

        List<GameObject> allFoods = FoodSpawner.Instance.GetAllFoods();
        if (allFoods.Count == 0)
        {
            MoveSafely(headPos, obstacles);
            return;
        }

        GameObject bestFood = FindBestFood(headPos, allFoods, obstacles);

        if (bestFood != null)
        {
            MoveTowardsFood(headPos, bestFood, obstacles);
        }
        else
        {
            MoveSafely(headPos, obstacles);
        }
    }

    private GameObject FindBestFood(Vector2Int headPos, List<GameObject> foods, List<Vector2Int> obstacles)
    {
        GameObject bestFood = null;
        float bestScore = float.MinValue;

        foreach (GameObject food in foods)
        {
            if (food == null)
                continue;

            Vector2Int foodPos = GridManager.Instance.WorldToGrid(food.transform.position);
            int distance = pathfinding.GetManhattanDistance(headPos, foodPos);

            if (distance > visionRange)
                continue;

            float score = EvaluateFood(headPos, foodPos, food, obstacles);

            if (score > bestScore)
            {
                bestScore = score;
                bestFood = food;
            }
        }

        return bestFood;
    }

    private float EvaluateFood(Vector2Int headPos, Vector2Int foodPos, GameObject food, List<Vector2Int> obstacles)
    {
        float score = 0;

        int distance = pathfinding.GetManhattanDistance(headPos, foodPos);
        score -= distance;

        Food foodComponent = food.GetComponent<Food>();
        if (foodComponent != null)
        {
            score += foodComponent.Points * 2f;
        }

        switch (strategy)
        {
            case AIStrategy.Aggressive:
                break;

            case AIStrategy.Defensive:
                List<Vector2Int> path = pathfinding.FindPath(headPos, foodPos, obstacles);
                if (path == null || !IsPathSafe(path, obstacles))
                {
                    score -= 1000f;
                }
                else
                {
                    score += 50f;
                }
                break;

            case AIStrategy.Balanced:
                path = pathfinding.FindPath(headPos, foodPos, obstacles);
                if (path != null)
                {
                    if (IsPathSafe(path, obstacles))
                    {
                        score += 30f;
                    }
                    else
                    {
                        score -= 20f;
                    }
                }
                break;
        }

        return score;
    }

    private void MoveTowardsFood(Vector2Int headPos, GameObject food, List<Vector2Int> obstacles)
    {
        Vector2Int foodPos = GridManager.Instance.WorldToGrid(food.transform.position);

        if (NeedRecalculatePath(food))
        {
            currentPath = pathfinding.FindPath(headPos, foodPos, obstacles);
            targetFood = food;

            if (showDebugInfo && currentPath == null)
            {
                Debug.Log("[AI] Không tìm thấy đường đến food");
            }
        }

        if (currentPath != null && currentPath.Count > 0)
        {
            Vector2Int nextStep = currentPath[0];

            if (!obstacles.Contains(nextStep) && GridManager.Instance.IsValidPosition(nextStep))
            {
                Vector2Int direction = nextStep - headPos;
                SetDirection(direction);
                currentPath.RemoveAt(0);
            }
            else
            {
                currentPath = null;
                MoveSafely(headPos, obstacles);
            }
        }
        else
        {
            MoveSafely(headPos, obstacles);
        }
    }

    private void MoveSafely(Vector2Int headPos, List<Vector2Int> obstacles)
    {
        Vector2Int safeDir = pathfinding.FindSafeDirection(headPos, obstacles);

        if (safeDir != Vector2Int.zero)
        {
            SetDirection(safeDir);
        }
    }

    private void HandleDanger(Vector2Int headPos, List<Vector2Int> obstacles)
    {
        if (showDebugInfo)
        {
            Debug.LogWarning("[AI] Nguy hiểm! Tìm hướng thoát...");
        }

        currentPath = null;
        targetFood = null;

        Vector2Int safeDir = pathfinding.FindSafeDirection(headPos, obstacles);

        if (safeDir != Vector2Int.zero)
        {
            SetDirection(safeDir);
        }
    }

    private bool IsInDanger(Vector2Int pos, Vector2Int direction, List<Vector2Int> obstacles)
    {
        Vector2Int nextPos = pos + direction;

        if (!GridManager.Instance.IsValidPosition(nextPos) || obstacles.Contains(nextPos))
        {
            return true;
        }

        return false;
    }

    private bool IsPathSafe(List<Vector2Int> path, List<Vector2Int> obstacles)
    {
        if (path == null || path.Count == 0)
            return false;

        int checkLength = Mathf.Min(path.Count, 3);

        for (int i = 0; i < checkLength; i++)
        {
            Vector2Int pos = path[i];

            int freeNeighbors = 0;
            Vector2Int[] neighbors = {
                pos + Vector2Int.up,
                pos + Vector2Int.down,
                pos + Vector2Int.left,
                pos + Vector2Int.right
            };

            foreach (Vector2Int neighbor in neighbors)
            {
                if (GridManager.Instance.IsValidPosition(neighbor) && !obstacles.Contains(neighbor))
                {
                    freeNeighbors++;
                }
            }

            if (freeNeighbors < 2)
            {
                return false;
            }
        }

        return true;
    }

    private bool NeedRecalculatePath(GameObject food)
    {
        if (currentPath == null || currentPath.Count == 0)
            return true;

        if (targetFood != food)
            return true;

        return false;
    }

    private void SetDirection(Vector2Int direction)
    {
        Vector2Int currentDir = snake.GetCurrentDirection();

        if (direction == -currentDir)
            return;

        snake.SetDirection(direction);
    }

    private Vector2Int GetHeadGridPosition()
    {
        return GridManager.Instance.WorldToGrid(snake.transform.position);
    }

    private List<Vector2Int> GetAllObstacles()
    {
        List<Vector2Int> obstacles = new List<Vector2Int>();

        if (snake != null && snake.SegmentPositions != null)
        {
            var segments = snake.SegmentPositions;
            for (int i = 1; i < segments.Count; i++)
            {
                obstacles.Add(segments[i]);
            }
        }

        if (GameManager.Instance != null)
        {
            List<SnakeController> allSnakes = GameManager.Instance.GetAllSnakes();
            foreach (SnakeController other in allSnakes)
            {
                if (other == snake || other.IsDead)
                    continue;

                var otherSegments = other.SegmentPositions;
                if (otherSegments != null)
                {
                    obstacles.AddRange(otherSegments);
                }
            }
        }

        return obstacles;
    }

    private void OnDrawGizmos()
    {
        if (!showDebugPath || currentPath == null || currentPath.Count == 0)
            return;

        if (GridManager.Instance == null)
            return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < currentPath.Count - 1; i++)
        {
            Vector3 from = GridManager.Instance.GridToWorld(currentPath[i]);
            Vector3 to = GridManager.Instance.GridToWorld(currentPath[i + 1]);
            Gizmos.DrawLine(from, to);
        }

        if (targetFood != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetFood.transform.position, 0.3f);
        }
    }
}