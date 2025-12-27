using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SnakeController))]
public class AIController : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float thinkDelay = 0.15f;
    [SerializeField] private int visionRange = 15;
    [SerializeField] private bool matchPlayerSpeed = true; // ✅ NEW: Tự động đồng bộ tốc độ với Player

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
            
            // ✅ FIX: Đồng bộ tốc độ với Player nếu bật
            if (matchPlayerSpeed)
            {
                SyncSpeedWithPlayer();
            }
        }
    }

    /// <summary>
    /// ✅ NEW: Đồng bộ tốc độ với Player
    /// </summary>
    private void SyncSpeedWithPlayer()
    {
        if (GameManager.Instance == null) return;

        // Tìm Player Snake (ID = 1)
        var playerSnake = GameManager.Instance.GetSnakeByID(1);
        if (playerSnake != null && snake != null)
        {
            // Copy tốc độ từ Player
            float playerSpeed = playerSnake.GetMoveInterval();
            snake.SetMoveSpeed(playerSpeed);
            
            Debug.Log($"[AI {snake.PlayerName}] ⚙️ Synced speed with Player: {playerSpeed}s/move");
        }
    }

    private void Update()
    {
        if (snake == null || snake.IsDead)
            return;

        // ✅ FIX: Liên tục sync tốc độ với Player (khi Player ăn food tăng tốc)
        if (matchPlayerSpeed && Time.frameCount % 30 == 0) // Check mỗi 30 frames
        {
            SyncSpeedWithPlayer();
        }

        if (Time.time - lastThinkTime >= thinkDelay)
        {
            Think();
            lastThinkTime = Time.time;
        }
    }

    private void Think()
    {
        // ✅ FIX: Get head position in GRID coordinates
        Vector2Int headGridPos = snake.GetHeadPosition();
        Vector2Int currentDir = snake.GetCurrentDirection();
        List<Vector2Int> obstacles = GetAllObstacles();

        if (showDebugInfo)
        {
            Debug.Log($"[AI {snake.PlayerName}] Head at grid: {headGridPos}, Direction: {currentDir}");
        }

        // ✅ Priority 1: Immediate danger check
        if (IsInImmediateDanger(headGridPos, currentDir, obstacles))
        {
            HandleDanger(headGridPos, currentDir, obstacles);
            return;
        }

        // ✅ Priority 2: Follow path to food
        if (currentPath != null && currentPath.Count > 0)
        {
            FollowPath(headGridPos, obstacles);
            return;
        }

        // ✅ Priority 3: Find and pursue food
        FindAndMoveToFood(headGridPos, obstacles);
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
            PlanPathToFood(headPos, bestFood, obstacles);
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

            Vector2Int foodGridPos = GridManager.Instance.WorldToGrid(food.transform.position);
            int distance = pathfinding.GetManhattanDistance(headPos, foodGridPos);

            if (distance > visionRange)
                continue;

            float score = EvaluateFood(headPos, foodGridPos, food, obstacles);

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

        // Distance penalty
        int distance = pathfinding.GetManhattanDistance(headPos, foodPos);
        score -= distance * 2f;

        // Food value bonus
        Food foodComponent = food.GetComponent<Food>();
        if (foodComponent != null)
        {
            score += foodComponent.Points * 3f;
        }

        // Strategy-specific evaluation
        switch (strategy)
        {
            case AIStrategy.Aggressive:
                // Prefer closer food, ignore safety
                score += 20f;
                break;

            case AIStrategy.Defensive:
                // Only pursue if path is safe
                List<Vector2Int> path = pathfinding.FindPath(headPos, foodPos, obstacles);
                if (path == null || !IsPathSafe(path, obstacles))
                {
                    score -= 1000f;
                }
                else
                {
                    score += 100f;
                }
                break;

            case AIStrategy.Balanced:
                // Balance between safety and opportunity
                path = pathfinding.FindPath(headPos, foodPos, obstacles);
                if (path != null)
                {
                    if (IsPathSafe(path, obstacles))
                    {
                        score += 50f;
                    }
                    else
                    {
                        score -= 30f;
                    }
                }
                break;
        }

        return score;
    }

    private void PlanPathToFood(Vector2Int headPos, GameObject food, List<Vector2Int> obstacles)
    {
        Vector2Int foodGridPos = GridManager.Instance.WorldToGrid(food.transform.position);

        // ✅ Only recalculate if needed
        if (NeedRecalculatePath(food))
        {
            currentPath = pathfinding.FindPath(headPos, foodGridPos, obstacles);
            targetFood = food;

            if (showDebugInfo)
            {
                if (currentPath != null)
                    Debug.Log($"[AI {snake.PlayerName}] 🎯 Path found to food: {currentPath.Count} steps");
                else
                    Debug.LogWarning($"[AI {snake.PlayerName}] ❌ No path to food at {foodGridPos}");
            }
        }

        FollowPath(headPos, obstacles);
    }

    private void FollowPath(Vector2Int headPos, List<Vector2Int> obstacles)
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            MoveSafely(headPos, obstacles);
            return;
        }

        Vector2Int nextStep = currentPath[0];

        // ✅ Validate next step
        if (!GridManager.Instance.IsValidPosition(nextStep) || obstacles.Contains(nextStep))
        {
            if (showDebugInfo)
                Debug.LogWarning($"[AI {snake.PlayerName}] ⚠️ Path blocked at {nextStep}, recalculating...");

            currentPath = null;
            targetFood = null;
            MoveSafely(headPos, obstacles);
            return;
        }

        // ✅ Calculate direction IN GRID SPACE
        Vector2Int direction = nextStep - headPos;

        if (showDebugInfo)
            Debug.Log($"[AI {snake.PlayerName}] Following path: {headPos} → {nextStep}, direction: {direction}");

        SetDirection(direction);
        currentPath.RemoveAt(0);

        // ✅ Clear path if reached food
        if (currentPath.Count == 0)
        {
            targetFood = null;
        }
    }

    private void MoveSafely(Vector2Int headPos, List<Vector2Int> obstacles)
    {
        Vector2Int safeDir = pathfinding.FindSafeDirection(headPos, obstacles);

        if (safeDir != Vector2Int.zero)
        {
            if (showDebugInfo)
                Debug.Log($"[AI {snake.PlayerName}] 🛡️ Safe move: {safeDir}");

            SetDirection(safeDir);
        }
        else
        {
            if (showDebugInfo)
                Debug.LogError($"[AI {snake.PlayerName}] 💀 No safe direction found!");
        }
    }

    private void HandleDanger(Vector2Int headPos, Vector2Int currentDir, List<Vector2Int> obstacles)
    {
        if (showDebugInfo)
            Debug.LogWarning($"[AI {snake.PlayerName}] 🚨 DANGER! Emergency evasion!");

        currentPath = null;
        targetFood = null;

        // ✅ Try to find escape direction
        Vector2Int[] emergencyDirs = {
            new Vector2Int(-currentDir.y, currentDir.x),  // Turn left
            new Vector2Int(currentDir.y, -currentDir.x),  // Turn right
            -currentDir  // Turn back (last resort)
        };

        foreach (Vector2Int dir in emergencyDirs)
        {
            Vector2Int testPos = headPos + dir;

            if (GridManager.Instance.IsValidPosition(testPos) && !obstacles.Contains(testPos))
            {
                // ✅ Check if this direction has space
                int spaceAhead = CountSpaceAhead(testPos, dir, obstacles);

                if (spaceAhead > 0)
                {
                    if (showDebugInfo)
                        Debug.Log($"[AI {snake.PlayerName}] ✅ Emergency escape: {dir} ({spaceAhead} spaces)");

                    SetDirection(dir);
                    return;
                }
            }
        }

        // ✅ Last resort: any valid direction
        Vector2Int lastResort = pathfinding.FindSafeDirection(headPos, obstacles);
        if (lastResort != Vector2Int.zero)
        {
            SetDirection(lastResort);
        }
    }

    private bool IsInImmediateDanger(Vector2Int pos, Vector2Int direction, List<Vector2Int> obstacles)
    {
        Vector2Int nextPos = pos + direction;

        // ✅ Check wall collision
        if (!GridManager.Instance.IsValidPosition(nextPos))
            return true;

        // ✅ Check obstacle collision
        if (obstacles.Contains(nextPos))
            return true;

        return false;
    }

    private bool IsPathSafe(List<Vector2Int> path, List<Vector2Int> obstacles)
    {
        if (path == null || path.Count == 0)
            return false;

        // ✅ Check first 3 steps for dead ends
        int checkLength = Mathf.Min(path.Count, 3);

        for (int i = 0; i < checkLength; i++)
        {
            Vector2Int pos = path[i];

            // Count free neighbors
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

            // ✅ Need at least 2 exits to avoid dead end
            if (freeNeighbors < 2)
            {
                return false;
            }
        }

        return true;
    }

    private int CountSpaceAhead(Vector2Int start, Vector2Int direction, List<Vector2Int> obstacles)
    {
        int count = 0;
        Vector2Int current = start;
        int maxCheck = 5;

        for (int i = 0; i < maxCheck; i++)
        {
            current += direction;

            if (!GridManager.Instance.IsValidPosition(current) || obstacles.Contains(current))
                break;

            count++;
        }

        return count;
    }

    private bool NeedRecalculatePath(GameObject food)
    {
        if (currentPath == null || currentPath.Count == 0)
            return true;

        if (targetFood != food)
            return true;

        // ✅ Recalculate if food moved (edge case)
        if (targetFood != null)
        {
            Vector2Int foodGridPos = GridManager.Instance.WorldToGrid(food.transform.position);
            Vector2Int pathEndPos = currentPath.Count > 0 ? currentPath[currentPath.Count - 1] : Vector2Int.zero;

            if (foodGridPos != pathEndPos)
                return true;
        }

        return false;
    }

    private void SetDirection(Vector2Int direction)
    {
        Vector2Int currentDir = snake.GetCurrentDirection();

        // ✅ Prevent 180° turn
        if (direction == -currentDir)
        {
            if (showDebugInfo)
                Debug.LogWarning($"[AI {snake.PlayerName}] ⚠️ Attempted illegal 180° turn");
            return;
        }

        if (direction != Vector2Int.zero)
        {
            snake.SetDirection(direction);
        }
    }

    private List<Vector2Int> GetAllObstacles()
    {
        List<Vector2Int> obstacles = new List<Vector2Int>();

        // ✅ Add own body (skip head)
        if (snake != null && !snake.IsDead)
        {
            List<Vector2Int> segments = snake.SegmentPositions;

            for (int i = 1; i < segments.Count; i++)
            {
                obstacles.Add(segments[i]);
            }
        }

        // ✅ Add other snakes
        if (GameManager.Instance != null)
        {
            List<SnakeController> allSnakes = GameManager.Instance.GetAllSnakes();

            foreach (SnakeController other in allSnakes)
            {
                if (other == snake || other.IsDead)
                    continue;

                List<Vector2Int> otherSegments = other.SegmentPositions;
                obstacles.AddRange(otherSegments);
            }
        }

        return obstacles;
    }

    private void OnDrawGizmos()
    {
        if (!showDebugPath || currentPath == null || currentPath.Count == 0)
            return;

        if (GridManager.Instance == null || snake == null)
            return;

        // Draw path
        Gizmos.color = Color.cyan;
        for (int i = 0; i < currentPath.Count - 1; i++)
        {
            Vector3 from = GridManager.Instance.GridToWorld(currentPath[i]);
            Vector3 to = GridManager.Instance.GridToWorld(currentPath[i + 1]);
            Gizmos.DrawLine(from, to);
        }

        // Draw current head position
        Gizmos.color = Color.green;
        Vector2Int headPos = snake.GetHeadPosition();
        Vector3 headWorld = GridManager.Instance.GridToWorld(headPos);
        Gizmos.DrawWireSphere(headWorld, GridManager.Instance.CellSize * 0.3f);

        // Draw target food
        if (targetFood != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetFood.transform.position, GridManager.Instance.CellSize * 0.5f);
        }
    }
}