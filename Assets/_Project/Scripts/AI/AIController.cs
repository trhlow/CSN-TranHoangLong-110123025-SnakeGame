using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ✅ YÊU CẦU 3: AI THÔNG MINH - Tránh đâm vào thân, ưu tiên an toàn
/// </summary>
[RequireComponent(typeof(SnakeController))]
public class AIController : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float thinkDelay = 0.15f;
    [SerializeField] private int visionRange = 15;
    [SerializeField] private int lookAheadDepth = 3; // ✅ NEW: Nhìn xa bao nhiêu bước
    [SerializeField] private bool matchPlayerSpeed = true;

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

            if (matchPlayerSpeed)
            {
                SyncSpeedWithPlayer();
            }
        }
    }

    private void SyncSpeedWithPlayer()
    {
        if (GameManager.Instance == null) return;

        var playerSnake = GameManager.Instance.GetSnakeByID(1);
        if (playerSnake != null && snake != null)
        {
            float playerSpeed = playerSnake.GetMoveInterval();
            snake.SetMoveSpeed(playerSpeed);

            Debug.Log($"[AI {snake.PlayerName}] ⚙️ Synced speed with Player: {playerSpeed}s/move");
        }
    }

    private void Update()
    {
        if (snake == null || snake.IsDead)
            return;

        if (matchPlayerSpeed && Time.frameCount % 30 == 0)
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
        Vector2Int headGridPos = snake.GetHeadPosition();
        Vector2Int currentDir = snake.GetCurrentDirection();
        List<Vector2Int> obstacles = GetAllObstacles();

        if (showDebugInfo)
        {
            Debug.Log($"[AI {snake.PlayerName}] Head at: {headGridPos}, Direction:  {currentDir}");
        }

        // ✅ YÊU CẦU 3: KIỂM TRA NGUY HIỂM TRƯỚC
        if (IsInImmediateDanger(headGridPos, currentDir, obstacles))
        {
            HandleDanger(headGridPos, currentDir, obstacles);
            return;
        }

        // ✅ Follow path nếu có
        if (currentPath != null && currentPath.Count > 0)
        {
            FollowPath(headGridPos, obstacles);
            return;
        }

        // ✅ Tìm food
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

        int distance = pathfinding.GetManhattanDistance(headPos, foodPos);
        score -= distance * 2f;

        Food foodComponent = food.GetComponent<Food>();
        if (foodComponent != null)
        {
            score += foodComponent.Points * 3f;
        }

        switch (strategy)
        {
            case AIStrategy.Aggressive:
                score += 20f;
                break;

            case AIStrategy.Defensive:
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

        if (NeedRecalculatePath(food))
        {
            currentPath = pathfinding.FindPath(headPos, foodGridPos, obstacles);
            targetFood = food;

            if (showDebugInfo)
            {
                if (currentPath != null)
                    Debug.Log($"[AI] 🎯 Path found: {currentPath.Count} steps");
                else
                    Debug.LogWarning($"[AI] ❌ No path to food at {foodGridPos}");
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

        // ✅ YÊU CẦU 3: KIỂM TRA AN TOÀN TRƯỚC KHI ĐI
        if (!GridManager.Instance.IsValidPosition(nextStep) || obstacles.Contains(nextStep))
        {
            if (showDebugInfo)
                Debug.LogWarning($"[AI] ⚠️ Path blocked at {nextStep}, recalculating...");

            currentPath = null;
            targetFood = null;
            MoveSafely(headPos, obstacles);
            return;
        }

        Vector2Int direction = nextStep - headPos;

        if (showDebugInfo)
            Debug.Log($"[AI] Following path: {headPos} → {nextStep}, direction: {direction}");

        SetDirection(direction);
        currentPath.RemoveAt(0);

        if (currentPath.Count == 0)
        {
            targetFood = null;
        }
    }

    // ✅ YÊU CẦU 3: TRÁNH ĐÂM VÀO THÂN
    private void MoveSafely(Vector2Int headPos, List<Vector2Int> obstacles)
    {
        Vector2Int currentDir = snake.GetCurrentDirection();

        // ✅ Đánh giá TẤT CẢ hướng có thể
        Vector2Int[] possibleDirections = {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        float bestScore = float.MinValue;
        Vector2Int bestDir = Vector2Int.zero;

        foreach (Vector2Int dir in possibleDirections)
        {
            // ✅ Không cho phép quay đầu 180°
            if (dir + currentDir == Vector2Int.zero)
                continue;

            Vector2Int nextPos = headPos + dir;

            // ✅ Tính điểm an toàn cho hướng này
            float score = EvaluateDirectionSafety(nextPos, dir, obstacles);

            if (score > bestScore)
            {
                bestScore = score;
                bestDir = dir;
            }
        }

        if (bestDir != Vector2Int.zero)
        {
            if (showDebugInfo)
                Debug.Log($"[AI] 🛡️ Safe move:  {bestDir} (score: {bestScore: F1})");

            SetDirection(bestDir);
        }
        else
        {
            if (showDebugInfo)
                Debug.LogError($"[AI] 💀 No safe direction found!");
        }
    }

    // ✅ YÊU CẦU 3: ĐÁNH GIÁ AN TOÀN CHO MỖI HƯỚNG
    private float EvaluateDirectionSafety(Vector2Int nextPos, Vector2Int direction, List<Vector2Int> obstacles)
    {
        float score = 0f;

        // 1. Kiểm tra va chạm tường
        if (!GridManager.Instance.IsValidPosition(nextPos))
        {
            return -1000f; // ❌ Loại bỏ hoàn toàn
        }

        // 2. Kiểm tra va chạm thân
        if (obstacles.Contains(nextPos))
        {
            return -1000f; // ❌ Loại bỏ hoàn toàn
        }

        // 3. ✅ LOOK-AHEAD: Kiểm tra các bước tiếp theo
        for (int i = 1; i <= lookAheadDepth; i++)
        {
            Vector2Int futurePos = nextPos + direction * i;

            if (!GridManager.Instance.IsValidPosition(futurePos))
            {
                score -= 50f / i; // Càng xa càng ít ảnh hưởng
            }

            if (obstacles.Contains(futurePos))
            {
                score -= 100f / i;
            }
        }

        // 4. ✅ Đếm số ô trống xung quanh
        int freeNeighbors = CountFreeNeighbors(nextPos, obstacles);
        score += freeNeighbors * 20f;

        // 5. ✅ Ưu tiên hướng có nhiều không gian
        int spaceAhead = CountSpaceAhead(nextPos, direction, obstacles);
        score += spaceAhead * 10f;

        // 6. ✅ Tránh góc chết
        if (freeNeighbors < 2)
        {
            score -= 100f; // Penalize dead ends
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
            if (GridManager.Instance.IsValidPosition(neighbor) && !obstacles.Contains(neighbor))
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

            if (!GridManager.Instance.IsValidPosition(current) || obstacles.Contains(current))
                break;

            count++;
        }

        return count;
    }

    // ✅ YÊU CẦU 3: XỬ LÝ TÌNH HUỐNG NGUY HIỂM
    private void HandleDanger(Vector2Int headPos, Vector2Int currentDir, List<Vector2Int> obstacles)
    {
        if (showDebugInfo)
            Debug.LogWarning($"[AI] 🚨 DANGER!  Emergency evasion!");

        currentPath = null;
        targetFood = null;

        // ✅ Thử tất cả hướng khả dụng
        Vector2Int[] emergencyDirs = {
            new Vector2Int(-currentDir.y, currentDir.x),  // Turn left
            new Vector2Int(currentDir.y, -currentDir.x),  // Turn right
        };

        float bestScore = float.MinValue;
        Vector2Int bestDir = Vector2Int.zero;

        foreach (Vector2Int dir in emergencyDirs)
        {
            Vector2Int testPos = headPos + dir;

            if (GridManager.Instance.IsValidPosition(testPos) && !obstacles.Contains(testPos))
            {
                float score = EvaluateDirectionSafety(testPos, dir, obstacles);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestDir = dir;
                }
            }
        }

        if (bestDir != Vector2Int.zero)
        {
            if (showDebugInfo)
                Debug.Log($"[AI] ✅ Emergency escape: {bestDir}");

            SetDirection(bestDir);
        }
        else
        {
            // Last resort: bất kỳ hướng nào không đâm tường/thân
            MoveSafely(headPos, obstacles);
        }
    }

    private bool IsInImmediateDanger(Vector2Int pos, Vector2Int direction, List<Vector2Int> obstacles)
    {
        Vector2Int nextPos = pos + direction;

        if (!GridManager.Instance.IsValidPosition(nextPos))
            return true;

        if (obstacles.Contains(nextPos))
            return true;

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

            int freeNeighbors = CountFreeNeighbors(pos, obstacles);

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

        if (direction == -currentDir)
        {
            if (showDebugInfo)
                Debug.LogWarning($"[AI] ⚠️ Attempted illegal 180° turn");
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

        Gizmos.color = Color.cyan;
        for (int i = 0; i < currentPath.Count - 1; i++)
        {
            Vector3 from = GridManager.Instance.GridToWorld(currentPath[i]);
            Vector3 to = GridManager.Instance.GridToWorld(currentPath[i + 1]);
            Gizmos.DrawLine(from, to);
        }

        Gizmos.color = Color.green;
        Vector2Int headPos = snake.GetHeadPosition();
        Vector3 headWorld = GridManager.Instance.GridToWorld(headPos);
        Gizmos.DrawWireSphere(headWorld, GridManager.Instance.CellSize * 0.3f);

        if (targetFood != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetFood.transform.position, GridManager.Instance.CellSize * 0.5f);
        }
    }
}