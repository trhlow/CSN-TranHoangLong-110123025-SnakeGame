using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SnakeController - FIXED: Grid-aligned movement
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class SnakeController : MonoBehaviour
{
    #region Properties
    [Header("Identity")]
    public int PlayerID { get; private set; }
    public string PlayerName { get; private set; }
    public Color SnakeColor { get; private set; }
    public bool IsAIControlled { get; private set; }

    [Header("Segment Prefabs")]
    [SerializeField] private GameObject headPrefab;
    [SerializeField] private GameObject bodyPrefab;
    [SerializeField] private GameObject tailPrefab;
    [SerializeField] private Transform segmentsContainer;

    [Header("Movement Settings")]
    [SerializeField] private float moveInterval = 0.2f;
    [SerializeField] private int initialLength = 5;
    [SerializeField] private float speedIncreasePerFood = 0.002f;
    [SerializeField] private float minMoveInterval = 0.08f;

    [Header("Input Keys")]
    public KeyCode KeyUp = KeyCode.W;
    public KeyCode KeyDown = KeyCode.S;
    public KeyCode KeyLeft = KeyCode.A;
    public KeyCode KeyRight = KeyCode.D;

    [Header("Debug")]
    [SerializeField] private bool enableDebug = false;

    // Game state
    public int Score { get; private set; } = 0;
    public bool IsDead => !isAlive;
    private bool isAlive = true;
    private int moveCount = 0;

    // Movement - ✅ LƯU Ý: Dùng GRID COORDINATES
    private List<Vector2Int> segmentGridPositions = new List<Vector2Int>();
    private List<Transform> segmentTransforms = new List<Transform>();
    private Vector2Int direction = Vector2Int.right;
    private Vector2Int nextDirection = Vector2Int.right; // Buffer input
    private float moveTimer = 0f;

    // Events
    public event System.Action<SnakeController> OnSnakeDied;
    public event System.Action<Food> OnSnakeEatFood;
    #endregion

    #region Initialization
    public void Setup(int playerID, Color color, bool isAI, string playerName = "Player",
        KeyCode kUp = KeyCode.W, KeyCode kDown = KeyCode.S, KeyCode kLeft = KeyCode.A, KeyCode kRight = KeyCode.D)
    {
        PlayerID = playerID;
        SnakeColor = color;
        IsAIControlled = isAI;
        PlayerName = playerName;

        KeyUp = kUp;
        KeyDown = kDown;
        KeyLeft = kLeft;
        KeyRight = kRight;

        Score = 0;
        isAlive = true;
        direction = Vector2Int.right;
        nextDirection = Vector2Int.right;

        SpawnInitialSnake();
    }

    private void SpawnInitialSnake()
    {
        if (headPrefab == null || bodyPrefab == null || tailPrefab == null)
        {
            Debug.LogError($"[{PlayerName}] ❌ Missing segment prefabs!");
            return;
        }

        if (GridManager.Instance == null)
        {
            Debug.LogError($"[{PlayerName}] ❌ GridManager not found!");
            return;
        }

        ClearSegments();

        // ✅ FIX: Snap spawn position to grid
        Vector2Int spawnGridPos = GridManager.Instance.WorldToGrid(transform.position);

        if (enableDebug)
            Debug.Log($"[{PlayerName}] Spawning at grid: {spawnGridPos}");

        // Spawn head
        segmentGridPositions.Add(spawnGridPos);
        SpawnSegmentVisual(spawnGridPos, SegmentType.Head);

        // Spawn body
        Vector2Int currentGridPos = spawnGridPos;
        for (int i = 1; i < initialLength - 1; i++)
        {
            currentGridPos += Vector2Int.left; // Move left in grid space
            segmentGridPositions.Add(currentGridPos);
            SpawnSegmentVisual(currentGridPos, SegmentType.Body);
        }

        // Spawn tail
        currentGridPos += Vector2Int.left;
        segmentGridPositions.Add(currentGridPos);
        SpawnSegmentVisual(currentGridPos, SegmentType.Tail);

        direction = Vector2Int.right;
        nextDirection = Vector2Int.right;
        moveTimer = 0f;
        moveCount = 0;

        if (enableDebug)
            Debug.Log($"[{PlayerName}] ✅ Spawned {segmentGridPositions.Count} segments");
    }

    private enum SegmentType { Head, Body, Tail }

    private void SpawnSegmentVisual(Vector2Int gridPos, SegmentType type)
    {
        GameObject prefab = type switch
        {
            SegmentType.Head => headPrefab,
            SegmentType.Tail => tailPrefab,
            _ => bodyPrefab
        };

        // ✅ Convert grid to world position
        Vector3 worldPos = GridManager.Instance.GridToWorld(gridPos);

        GameObject segmentObj = Instantiate(prefab, worldPos, Quaternion.identity, segmentsContainer);
        segmentObj.name = $"{PlayerName}_{type}{segmentTransforms.Count}";

        // Set tag
        segmentObj.tag = type == SegmentType.Head ? "SnakeHead" : "SnakeBody";

        // Setup collider
        CircleCollider2D collider = segmentObj.GetComponent<CircleCollider2D>();
        if (collider == null)
            collider = segmentObj.AddComponent<CircleCollider2D>();

        collider.isTrigger = true;
        collider.radius = GridManager.Instance.CellSize * 0.4f; // 80% of cell

        // Setup collision handler
        SegmentCollisionHandler handler = segmentObj.AddComponent<SegmentCollisionHandler>();
        handler.snake = this;

        // Set color
        SpriteRenderer sr = segmentObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = type switch
            {
                SegmentType.Head => SnakeColor,
                SegmentType.Tail => SnakeColor * 0.8f,
                _ => SnakeColor * 0.9f
            };
        }

        segmentTransforms.Add(segmentObj.transform);

        // ✅ Occupy grid cell
        GridManager.Instance.OccupyCell(gridPos, segmentObj);
    }

    private void ClearSegments()
    {
        // Free grid cells
        foreach (Vector2Int gridPos in segmentGridPositions)
        {
            GridManager.Instance.FreeCell(gridPos);
        }
        segmentGridPositions.Clear();

        // Destroy visual segments
        foreach (Transform seg in segmentTransforms)
        {
            if (seg != null)
                Destroy(seg.gameObject);
        }
        segmentTransforms.Clear();
    }
    #endregion

    #region Update Loop
    private void Update()
    {
        if (!isAlive || GameManager.Instance == null || !GameManager.Instance.IsPlaying)
            return;

        if (!IsAIControlled)
        {
            HandlePlayerInput();
        }

        moveTimer += Time.deltaTime;
        if (moveTimer >= moveInterval)
        {
            Move();
            moveTimer = 0f;
        }
    }

    private void HandlePlayerInput()
    {
        // ✅ Buffer input để tránh miss input giữa các frame
        if (Input.GetKeyDown(KeyUp) && direction != Vector2Int.down)
            nextDirection = Vector2Int.up;
        else if (Input.GetKeyDown(KeyDown) && direction != Vector2Int.up)
            nextDirection = Vector2Int.down;
        else if (Input.GetKeyDown(KeyLeft) && direction != Vector2Int.right)
            nextDirection = Vector2Int.left;
        else if (Input.GetKeyDown(KeyRight) && direction != Vector2Int.left)
            nextDirection = Vector2Int.right;
    }
    #endregion

    #region Movement
    public void Move()
    {
        if (!isAlive || segmentGridPositions.Count == 0)
            return;

        moveCount++;

        // ✅ Apply buffered direction
        direction = nextDirection;

        // ✅ Calculate new head position IN GRID SPACE
        Vector2Int oldHeadPos = segmentGridPositions[0];
        Vector2Int newHeadPos = oldHeadPos + direction;

        // ✅ Check collision BEFORE moving
        if (!GridManager.Instance.IsValidPosition(newHeadPos))
        {
            if (enableDebug)
                Debug.LogError($"[{PlayerName}] 💥 Hit wall at grid {newHeadPos}");
            Die();
            return;
        }

        // ✅ Check self-collision (skip first 4 segments for grace period)
        if (moveCount > initialLength)
        {
            for (int i = 1; i < segmentGridPositions.Count; i++)
            {
                if (segmentGridPositions[i] == newHeadPos)
                {
                    if (enableDebug)
                        Debug.LogError($"[{PlayerName}] 💥 Self-collision at grid {newHeadPos}");
                    Die();
                    return;
                }
            }
        }

        // ✅ Move segments in GRID SPACE
        Vector2Int tailGridPos = segmentGridPositions[segmentGridPositions.Count - 1];

        for (int i = segmentGridPositions.Count - 1; i > 0; i--)
        {
            segmentGridPositions[i] = segmentGridPositions[i - 1];
        }

        segmentGridPositions[0] = newHeadPos;

        // ✅ Update visual positions
        for (int i = 0; i < segmentTransforms.Count; i++)
        {
            Vector3 worldPos = GridManager.Instance.GridToWorld(segmentGridPositions[i]);
            segmentTransforms[i].position = worldPos;
        }

        // ✅ Update grid occupancy
        GridManager.Instance.FreeCell(tailGridPos);
        GridManager.Instance.OccupyCell(newHeadPos, segmentTransforms[0].gameObject);
    }

    public void SetDirection(Vector2Int newDirection)
    {
        // ✅ Prevent 180° turn
        if (newDirection + direction != Vector2Int.zero && newDirection != Vector2Int.zero)
        {
            nextDirection = newDirection;
        }
    }

    public Vector2Int GetCurrentDirection() => direction;
    #endregion

    #region Collision Detection
    // ✅ Được gọi từ SegmentCollisionHandler
    public void OnSegmentTriggerEnter(Collider2D collision, GameObject segment)
    {
        if (!isAlive)
            return;

        // ✅ Only head can eat food
        if (segment == segmentTransforms[0].gameObject && collision.CompareTag("Food"))
        {
            HandleFoodCollision(collision);
        }
        else if (collision.CompareTag("SnakeBody") || collision.CompareTag("SnakeHead"))
        {
            HandleSnakeCollision(collision);
        }
    }

    private void HandleFoodCollision(Collider2D collision)
    {
        Food food = collision.GetComponent<Food>();
        if (food == null)
            return;

        if (enableDebug)
            Debug.Log($"<color=green>🍎 [{PlayerName}] Ate {food.Rarity} food (+{food.Points}pts)</color>");

        Score += food.Points;
        Grow();

        // ✅ Speed up
        moveInterval = Mathf.Max(minMoveInterval, moveInterval - speedIncreasePerFood);

        OnSnakeEatFood?.Invoke(food);

        if (GameManager.Instance != null)
            GameManager.Instance.RegisterFoodEaten(PlayerID);

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateScore(PlayerID, Score);

        if (FoodSpawner.Instance != null)
            FoodSpawner.Instance.RemoveFood(collision.gameObject);

        food.OnEaten(collision.transform.position);
    }

    private void HandleSnakeCollision(Collider2D collision)
    {
        // ✅ Check if it's NOT our own segment
        bool isOwnSegment = false;
        foreach (Transform seg in segmentTransforms)
        {
            if (seg != null && seg.gameObject == collision.gameObject)
            {
                isOwnSegment = true;
                break;
            }
        }

        if (!isOwnSegment)
        {
            if (enableDebug)
                Debug.LogError($"[{PlayerName}] 💥 Hit another snake!");
            Die();
        }
    }
    #endregion

    #region Growth
    public void Grow()
    {
        if (segmentGridPositions.Count < 2)
            return;

        // ✅ Insert new segment before tail
        int tailIndex = segmentGridPositions.Count - 1;
        Vector2Int newSegmentGridPos = segmentGridPositions[tailIndex];

        segmentGridPositions.Insert(tailIndex, newSegmentGridPos);

        // Spawn visual
        Vector3 worldPos = GridManager.Instance.GridToWorld(newSegmentGridPos);
        GameObject bodyObj = Instantiate(bodyPrefab, worldPos, Quaternion.identity, segmentsContainer);
        bodyObj.name = $"{PlayerName}_Body_Extra{segmentTransforms.Count}";
        bodyObj.tag = "SnakeBody";

        CircleCollider2D collider = bodyObj.GetComponent<CircleCollider2D>();
        if (collider == null)
            collider = bodyObj.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = GridManager.Instance.CellSize * 0.4f;

        SegmentCollisionHandler handler = bodyObj.AddComponent<SegmentCollisionHandler>();
        handler.snake = this;

        SpriteRenderer sr = bodyObj.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = SnakeColor * 0.9f;

        segmentTransforms.Insert(tailIndex, bodyObj.transform);

        if (enableDebug)
            Debug.Log($"[{PlayerName}] 📈 Grew to {segmentGridPositions.Count} segments");
    }
    #endregion

    #region Death & Utility
    public void Die()
    {
        if (!isAlive)
            return;

        isAlive = false;

        if (enableDebug)
            Debug.Log($"<color=red>💀 [{PlayerName}] Died! Final Score: {Score}</color>");

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("SnakeDie");

        if (CameraController.Instance != null)
            CameraController.Instance.Shake(0.5f, 0.3f);

        OnSnakeDied?.Invoke(this);
    }

    public Vector2Int GetHeadPosition()
    {
        return segmentGridPositions.Count > 0 ? segmentGridPositions[0] : Vector2Int.zero;
    }

    public List<Vector2Int> SegmentPositions
    {
        get { return new List<Vector2Int>(segmentGridPositions); }
    }

    public void SetAI(bool value)
    {
        IsAIControlled = value;
    }

    public void SetMoveSpeed(float newMoveInterval)
    {
        moveInterval = Mathf.Max(minMoveInterval, newMoveInterval);
    }

    public void SetSnakeColor(Color newColor)
    {
        SnakeColor = newColor;
        for (int i = 0; i < segmentTransforms.Count; i++)
        {
            SpriteRenderer sr = segmentTransforms[i].GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = i == 0 ? newColor :
                           i == segmentTransforms.Count - 1 ? newColor * 0.8f :
                           newColor * 0.9f;
            }
        }
    }
    #endregion

    #region Debug
    private void OnDrawGizmos()
    {
        if (!enableDebug || segmentGridPositions.Count == 0)
            return;

        // Draw snake path
        Gizmos.color = SnakeColor;
        foreach (Vector2Int gridPos in segmentGridPositions)
        {
            Vector3 worldPos = GridManager.Instance.GridToWorld(gridPos);
            Gizmos.DrawWireCube(worldPos, Vector3.one * GridManager.Instance.CellSize * 0.9f);
        }

        // Draw direction
        Gizmos.color = Color.yellow;
        Vector3 headWorld = GridManager.Instance.GridToWorld(segmentGridPositions[0]);
        Vector3 dirVector = new Vector3(direction.x, direction.y, 0) * GridManager.Instance.CellSize;
        Gizmos.DrawRay(headWorld, dirVector);
    }
    #endregion
}

// ✅ NEW: Collision handler component
public class SegmentCollisionHandler : MonoBehaviour
{
    [HideInInspector] public SnakeController snake;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (snake != null)
        {
            snake.OnSegmentTriggerEnter(collision, gameObject);
        }
    }
}