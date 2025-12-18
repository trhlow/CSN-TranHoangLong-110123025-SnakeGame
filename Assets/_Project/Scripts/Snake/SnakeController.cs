using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SnakeController - Full version với SetMoveSpeed()
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
    [SerializeField] private float moveInterval = 1f;
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

    // Movement
    private List<Transform> segments = new List<Transform>();
    private Vector2Int direction = Vector2Int.right;
    private float moveTimer = 0f;

    // World bounds
    private float worldMinX, worldMaxX, worldMinY, worldMaxY;

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

        CalculateWorldBounds();
        SpawnInitialSnake();
    }

    private void CalculateWorldBounds()
    {
        if (GridManager.Instance != null)
        {
            int gridWidth = GridManager.Instance.GridWidth;
            int gridHeight = GridManager.Instance.GridHeight;
            float cellSize = GridManager.Instance.CellSize;
            Vector3 gridOrigin = GridManager.Instance.GridOrigin;

            worldMinX = gridOrigin.x;
            worldMaxX = gridOrigin.x + gridWidth * cellSize;
            worldMinY = gridOrigin.y;
            worldMaxY = gridOrigin.y + gridHeight * cellSize;

            if (enableDebug)
            {
                Debug.Log($"[{PlayerName}] World Bounds: X({worldMinX:F2} to {worldMaxX:F2}), Y({worldMinY:F2} to {worldMaxY:F2})");
            }
        }
        else
        {
            worldMinX = -10f;
            worldMaxX = 10f;
            worldMinY = -7.5f;
            worldMaxY = 7.5f;
            Debug.LogWarning($"[{PlayerName}] GridManager not found! Using default bounds.");
        }
    }

    private void SpawnInitialSnake()
    {
        if (headPrefab == null || bodyPrefab == null || tailPrefab == null)
        {
            Debug.LogError($"[{PlayerName}] ❌ Missing segment prefabs!");
            return;
        }

        ClearSegments();

        Vector3 spawnPos = transform.position;

        if (enableDebug)
        {
            Debug.Log($"[{PlayerName}] Spawning at world position: {spawnPos}");
        }

        SpawnHead(spawnPos);

        Vector3 currentPos = spawnPos;
        for (int i = 1; i < initialLength - 1; i++)
        {
            currentPos -= new Vector3(1, 0, 0);
            SpawnBodySegment(currentPos, i);
        }

        currentPos -= new Vector3(1, 0, 0);
        SpawnTail(currentPos);

        direction = Vector2Int.right;
        moveTimer = 0f;
        moveCount = 0;

        if (enableDebug)
            Debug.Log($"[{PlayerName}] ✅ Spawned with {segments.Count} segments");
    }

    private void SpawnHead(Vector3 position)
    {
        GameObject headObj = Instantiate(headPrefab, position, Quaternion.identity, segmentsContainer);
        headObj.name = $"{PlayerName}_Head";
        headObj.tag = "SnakeHead";

        ConfigureSegmentCollider(headObj);
        ConfigureSegmentRigidbody(headObj);

        SpriteRenderer sr = headObj.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = SnakeColor;

        segments.Add(headObj.transform);
    }

    private void SpawnBodySegment(Vector3 position, int index)
    {
        GameObject bodyObj = Instantiate(bodyPrefab, position, Quaternion.identity, segmentsContainer);
        bodyObj.name = $"{PlayerName}_Body{index}";
        bodyObj.tag = "SnakeBody";

        ConfigureSegmentCollider(bodyObj);

        SpriteRenderer sr = bodyObj.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = SnakeColor * 0.9f;

        segments.Add(bodyObj.transform);
    }

    private void SpawnTail(Vector3 position)
    {
        GameObject tailObj = Instantiate(tailPrefab, position, Quaternion.identity, segmentsContainer);
        tailObj.name = $"{PlayerName}_Tail";
        tailObj.tag = "SnakeBody";

        ConfigureSegmentCollider(tailObj);

        SpriteRenderer sr = tailObj.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = SnakeColor * 0.8f;

        segments.Add(tailObj.transform);
    }

    private void ConfigureSegmentCollider(GameObject segment)
    {
        BoxCollider2D collider = segment.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = segment.AddComponent<BoxCollider2D>();
        }
        collider.size = Vector2.one * 0.9f;
        collider.isTrigger = true;
    }

    private void ConfigureSegmentRigidbody(GameObject segment)
    {
        Rigidbody2D rb = segment.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = segment.AddComponent<Rigidbody2D>();
        }
        rb.isKinematic = true;
        rb.gravityScale = 0f;
    }

    private void ClearSegments()
    {
        foreach (Transform seg in segments)
        {
            if (seg != null)
                Destroy(seg.gameObject);
        }
        segments.Clear();
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
        if (Input.GetKeyDown(KeyUp) && direction != Vector2Int.down)
            direction = Vector2Int.up;
        else if (Input.GetKeyDown(KeyDown) && direction != Vector2Int.up)
            direction = Vector2Int.down;
        else if (Input.GetKeyDown(KeyLeft) && direction != Vector2Int.right)
            direction = Vector2Int.left;
        else if (Input.GetKeyDown(KeyRight) && direction != Vector2Int.left)
            direction = Vector2Int.right;
    }
    #endregion

    #region Movement
    public void Move()
    {
        if (!isAlive || segments.Count == 0)
            return;

        moveCount++;

        for (int i = segments.Count - 1; i > 0; i--)
        {
            segments[i].position = segments[i - 1].position;
        }

        Vector3 moveVector = new Vector3(direction.x, direction.y, 0f);
        segments[0].position += moveVector;

        Vector3 headWorldPos = segments[0].position;

        if (headWorldPos.x < worldMinX || headWorldPos.x > worldMaxX ||
            headWorldPos.y < worldMinY || headWorldPos.y > worldMaxY)
        {
            if (enableDebug)
            {
                Debug.LogError($"[{PlayerName}] 💥 Hit wall at world position {headWorldPos}");
                Debug.LogError($"[{PlayerName}] Bounds: X({worldMinX} to {worldMaxX}), Y({worldMinY} to {worldMaxY})");
            }
            Die();
            return;
        }

        if (moveCount > initialLength)
        {
            for (int i = 4; i < segments.Count; i++)
            {
                if (Vector3.Distance(segments[i].position, headWorldPos) < 0.1f)
                {
                    if (enableDebug)
                        Debug.LogError($"[{PlayerName}] 💥 Self-collision at {headWorldPos}");
                    Die();
                    return;
                }
            }
        }
    }

    public void SetDirection(Vector2Int newDirection)
    {
        if (newDirection + direction != Vector2Int.zero)
        {
            direction = newDirection;
        }
    }

    public Vector2Int GetCurrentDirection() => direction;
    #endregion

    #region Collision Detection
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isAlive)
            return;

        if (segments.Count > 0 && collision.gameObject != segments[0].gameObject)
        {
            if (collision.CompareTag("Food"))
            {
                HandleFoodCollision(collision);
            }
            else if (collision.CompareTag("SnakeBody") || collision.CompareTag("SnakeHead"))
            {
                HandleSnakeCollision(collision);
            }
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
        moveInterval = Mathf.Max(minMoveInterval, moveInterval - speedIncreasePerFood);

        OnSnakeEatFood?.Invoke(food);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterFoodEaten(PlayerID);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(PlayerID, Score);
        }

        if (FoodSpawner.Instance != null)
        {
            FoodSpawner.Instance.RemoveFood(collision.gameObject);
        }

        food.OnEaten(collision.transform.position);
    }

    private void HandleSnakeCollision(Collider2D collision)
    {
        bool isSelfCollision = false;
        foreach (Transform seg in segments)
        {
            if (seg.gameObject == collision.gameObject)
            {
                isSelfCollision = true;
                break;
            }
        }

        if (!isSelfCollision)
        {
            if (enableDebug)
                Debug.LogError($"[{PlayerName}] 💥 Collision with another snake!");
            Die();
        }
    }
    #endregion

    #region Growth
    public void Grow()
    {
        if (segments.Count < 2)
            return;

        Transform lastBody = segments[segments.Count - 2];
        Vector3 newPos = lastBody.position;

        GameObject newBodyObj = Instantiate(bodyPrefab, newPos, Quaternion.identity, segmentsContainer);
        newBodyObj.name = $"{PlayerName}_Body_Extra{segments.Count}";
        newBodyObj.tag = "SnakeBody";

        ConfigureSegmentCollider(newBodyObj);

        SpriteRenderer sr = newBodyObj.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = SnakeColor * 0.9f;

        segments.Insert(segments.Count - 1, newBodyObj.transform);

        if (enableDebug)
            Debug.Log($"[{PlayerName}] 📈 Grew to {segments.Count} segments");
    }
    #endregion

    #region Death
    public void Die()
    {
        if (!isAlive)
            return;

        isAlive = false;

        if (enableDebug)
            Debug.Log($"<color=red>💀 [{PlayerName}] Died! Final Score: {Score}</color>");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("SnakeDie");
        }

        if (CameraController.Instance != null)
        {
            CameraController.Instance.Shake(0.5f, 0.3f);
        }

        OnSnakeDied?.Invoke(this);
    }
    #endregion

    #region Utility
    public Vector2Int GetHeadPosition()
    {
        if (segments.Count == 0 || GridManager.Instance == null)
            return Vector2Int.zero;

        return GridManager.Instance.WorldToGrid(segments[0].position);
    }

    public List<Vector2Int> SegmentPositions
    {
        get
        {
            List<Vector2Int> positions = new List<Vector2Int>();

            if (GridManager.Instance == null)
                return positions;

            foreach (Transform seg in segments)
            {
                if (seg != null)
                {
                    Vector2Int gridPos = GridManager.Instance.WorldToGrid(seg.position);
                    positions.Add(gridPos);
                }
            }
            return positions;
        }
    }

    public void SetAI(bool value)
    {
        IsAIControlled = value;
    }

    public void SetSnakeColor(Color newColor)
    {
        SnakeColor = newColor;

        if (segments != null && segments.Count > 0)
        {
            for (int i = 0; i < segments.Count; i++)
            {
                SpriteRenderer sr = segments[i].GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    if (i == 0)
                        sr.color = newColor;
                    else if (i == segments.Count - 1)
                        sr.color = newColor * 0.8f;
                    else
                        sr.color = newColor * 0.9f;
                }
            }
        }
    }

    // ✅✅✅ METHOD MỚI: SET SPEED RIÊNG CHO AI ✅✅✅
    /// <summary>
    /// Set tốc độ di chuyển của snake
    /// </summary>
    public void SetMoveSpeed(float newMoveInterval)
    {
        moveInterval = Mathf.Max(0.08f, newMoveInterval);

        if (enableDebug)
        {
            Debug.Log($"[{PlayerName}] Move speed set to {moveInterval}s per move");
        }
    }
    #endregion

    #region Debug
    private void OnDrawGizmos()
    {
        if (!enableDebug || segments.Count == 0)
            return;

        Gizmos.color = Color.yellow;
        Vector3 headPos = segments[0].position;
        Vector3 dirVector = new Vector3(direction.x, direction.y, 0) * 0.5f;
        Gizmos.DrawRay(headPos, dirVector);

        Gizmos.color = SnakeColor;
        foreach (Transform seg in segments)
        {
            if (seg != null)
            {
                Gizmos.DrawWireCube(seg.position, Vector3.one * 0.9f);
            }
        }

        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Vector3 bl = new Vector3(worldMinX, worldMinY, 0);
            Vector3 br = new Vector3(worldMaxX, worldMinY, 0);
            Vector3 tl = new Vector3(worldMinX, worldMaxY, 0);
            Vector3 tr = new Vector3(worldMaxX, worldMaxY, 0);

            Gizmos.DrawLine(bl, br);
            Gizmos.DrawLine(br, tr);
            Gizmos.DrawLine(tr, tl);
            Gizmos.DrawLine(tl, bl);
        }
    }
    #endregion
}