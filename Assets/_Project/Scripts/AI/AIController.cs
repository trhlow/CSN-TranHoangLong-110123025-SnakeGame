using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SnakeController))]
public class AIController : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float decisionInterval = 0.1f;
    [SerializeField] private AIStrategy strategy = AIStrategy.Balanced;

    [Header("Behavior Weights")]
    [Range(0f, 1f)][SerializeField] private float foodPriority = 0.7f;
    [Range(0f, 1f)][SerializeField] private float safetyPriority = 0.9f;
    [Range(0f, 1f)][SerializeField] private float aggressionLevel = 0.3f;

    [Header("Lookahead")]
    [SerializeField] private int pathLookahead = 5;
    [SerializeField] private int dangerLookahead = 3;

    [Header("Debug")]
    [SerializeField] private bool showDebugPath = false;
    [SerializeField] private Color pathColor = Color.cyan;

    private SnakeController snake;
    private AIPathfinding pathfinding;
    private float nextDecisionTime = 0f;
    private List<Vector2Int> currentPath;
    private GameObject targetFood;

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

        if (Time.time >= nextDecisionTime)
        {
            MakeDecision();
            nextDecisionTime = Time.time + decisionInterval;
        }
    }

    private void MakeDecision()
    {
        Vector2Int currentPos = snake.GetHeadPosition();
        Vector2Int currentDir = snake.GetCurrentDirection();

        List<Vector2Int> obstacles = GetAllObstacles();

        if (IsInDanger(currentPos, currentDir, obstacles))
        {
            Vector2Int safeDir = pathfinding.FindSafeDirection(currentPos, obstacles);
            if (safeDir != Vector2Int.zero && safeDir != -currentDir)
            {
                snake.SetDirection(safeDir);
                currentPath = null;
                return;
            }
        }

        switch (strategy)
        {
            case AIStrategy.Aggressive:
                AggressiveStrategy(currentPos, obstacles);
                break;
            case AIStrategy.Defensive:
                DefensiveStrategy(currentPos, obstacles);
                break;
            case AIStrategy.Balanced:
                BalancedStrategy(currentPos, obstacles);
                break;
        }
    }

    private void AggressiveStrategy(Vector2Int currentPos, List<Vector2Int> obstacles)
    {
        GameObject food = FindBestFood(currentPos, obstacles, true);
        if (food != null)
        {
            MoveTowardsTarget(currentPos, food, obstacles);
        }
        else
        {
            Vector2Int safeDir = pathfinding.FindSafeDirection(currentPos, obstacles);
            if (safeDir != Vector2Int.zero)
            {
                snake.SetDirection(safeDir);
            }
        }
    }

    private void DefensiveStrategy(Vector2Int currentPos, List<Vector2Int> obstacles)
    {
        GameObject food = FindBestFood(currentPos, obstacles, false);

        if (food != null)
        {
            Vector2Int foodPos = GridManager.Instance.WorldToGrid(food.transform.position);

            if (IsPathSafe(currentPos, foodPos, obstacles))
            {
                MoveTowardsTarget(currentPos, food, obstacles);
                return;
            }
        }

        Vector2Int safeDir = pathfinding.FindSafeDirection(currentPos, obstacles);
        if (safeDir != Vector2Int.zero)
        {
            snake.SetDirection(safeDir);
        }
    }

    private void BalancedStrategy(Vector2Int currentPos, List<Vector2Int> obstacles)
    {
        float dangerLevel = CalculateDangerLevel(currentPos, obstacles);

        if (dangerLevel > 0.7f)
        {
            DefensiveStrategy(currentPos, obstacles);
        }
        else if (dangerLevel < 0.3f)
        {
            AggressiveStrategy(currentPos, obstacles);
        }
        else
        {
            GameObject food = FindBestFood(currentPos, obstacles, false);
            if (food != null)
            {
                MoveTowardsTarget(currentPos, food, obstacles);
            }
            else
            {
                Vector2Int safeDir = pathfinding.FindSafeDirection(currentPos, obstacles);
                if (safeDir != Vector2Int.zero)
                {
                    snake.SetDirection(safeDir);
                }
            }
        }
    }

    private GameObject FindBestFood(Vector2Int currentPos, List<Vector2Int> obstacles, bool ignoreDistance)
    {
        if (FoodSpawner.Instance == null)
            return null;

        List<GameObject> foods = FoodSpawner.Instance.GetAllFoods();
        if (foods.Count == 0)
            return null;

        GameObject bestFood = null;
        float bestScore = float.MinValue;

        foreach (GameObject food in foods)
        {
            if (food == null) continue;

            Vector2Int foodPos = GridManager.Instance.WorldToGrid(food.transform.position);

            if (obstacles.Contains(foodPos))
                continue;

            float score = EvaluateFood(currentPos, foodPos, food, obstacles, ignoreDistance);

            if (score > bestScore)
            {
                bestScore = score;
                bestFood = food;
            }
        }

        return bestFood;
    }

    private float EvaluateFood(Vector2Int currentPos, Vector2Int foodPos, GameObject food, List<Vector2Int> obstacles, bool ignoreDistance)
    {
        float score = 0;

        int distance = GridManager.Instance.GetManhattanDistance(currentPos, foodPos);
        if (!ignoreDistance)
        {
            score -= distance * 0.5f;
        }

        Food foodComponent = food.GetComponent<Food>();
        if (foodComponent != null)
        {
            score += foodComponent.Points * 2f;
        }

        if (IsPathSafe(currentPos, foodPos, obstacles))
        {
            score += 20f;
        }
        else
        {
            score -= 30f;
        }

        return score;
    }

    private void MoveTowardsTarget(Vector2Int currentPos, GameObject target, List<Vector2Int> obstacles)
    {
        if (target == null)
            return;

        Vector2Int targetPos = GridManager.Instance.WorldToGrid(target.transform.position);

        if (currentPath != null && currentPath.Count > 0 && targetFood == target)
        {
            Vector2Int nextStep = currentPath[0];
            Vector2Int direction = nextStep - currentPos;

            if (!obstacles.Contains(nextStep) && direction != -snake.GetCurrentDirection())
            {
                snake.SetDirection(direction);
                currentPath.RemoveAt(0);
                return;
            }
            else
            {
                currentPath = null;
            }
        }

        currentPath = pathfinding.FindPath(currentPos, targetPos, obstacles);
        targetFood = target;

        if (currentPath != null && currentPath.Count > 0)
        {
            Vector2Int nextStep = currentPath[0];
            Vector2Int direction = nextStep - currentPos;

            if (direction != -snake.GetCurrentDirection())
            {
                snake.SetDirection(direction);
                currentPath.RemoveAt(0);
            }
        }
        else
        {
            Vector2Int safeDir = pathfinding.FindSafeDirection(currentPos, obstacles);
            if (safeDir != Vector2Int.zero)
            {
                snake.SetDirection(safeDir);
            }
        }
    }

    private bool IsInDanger(Vector2Int pos, Vector2Int direction, List<Vector2Int> obstacles)
    {
        Vector2Int nextPos = pos + direction;
        if (!GridManager.Instance.IsValidPosition(nextPos) || obstacles.Contains(nextPos))
        {
            return true;
        }

        for (int i = 1; i <= dangerLookahead; i++)
        {
            Vector2Int lookAhead = pos + (direction * i);
            if (!GridManager.Instance.IsValidPosition(lookAhead) || obstacles.Contains(lookAhead))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsPathSafe(Vector2Int from, Vector2Int to, List<Vector2Int> obstacles)
    {
        List<Vector2Int> path = pathfinding.FindPath(from, to, obstacles);

        if (path == null || path.Count == 0)
            return false;

        int checkSteps = Mathf.Min(path.Count, pathLookahead);
        for (int i = 0; i < checkSteps; i++)
        {
            Vector2Int pos = path[i];
            int freeNeighbors = 0;

            foreach (Vector2Int neighborPos in GridManager.Instance.GetNeighbors(pos))
            {
                if (!obstacles.Contains(neighborPos))
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

    private float CalculateDangerLevel(Vector2Int pos, List<Vector2Int> obstacles)
    {
        float danger = 0f;

        int freeNeighbors = 0;
        foreach (Vector2Int neighborPos in GridManager.Instance.GetNeighbors(pos))
        {
            if (!obstacles.Contains(neighborPos))
            {
                freeNeighbors++;
            }
        }

        danger += (4 - freeNeighbors) * 0.25f;

        int minDistToWall = Mathf.Min(
            pos.x,
            pos.y,
            GridManager.Instance.GridWidth - pos.x - 1,
            GridManager.Instance.GridHeight - pos.y - 1
        );

        if (minDistToWall < 3)
        {
            danger += 0.3f;
        }

        return Mathf.Clamp01(danger);
    }

    private List<Vector2Int> GetAllObstacles()
    {
        List<Vector2Int> obstacles = new List<Vector2Int>();

        obstacles.AddRange(snake.SegmentPositions.Skip(1));

        if (GameManager.Instance != null)
        {
            List<SnakeController> allSnakes = GameManager.Instance.GetAllSnakes();
            foreach (SnakeController other in allSnakes)
            {
                if (other == snake || other.IsDead) continue;
                obstacles.AddRange(other.SegmentPositions);
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

        Gizmos.color = pathColor;

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