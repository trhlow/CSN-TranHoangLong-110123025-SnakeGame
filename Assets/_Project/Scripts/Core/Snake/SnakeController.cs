using System;
using System.Collections.Generic;
using UnityEngine;

public class SnakeController : MonoBehaviour
{
    [Header("Identity")]
    public int PlayerID { get; private set; }
    public string PlayerName { get; private set; }
    public Color SnakeColor { get; private set; }
    public bool IsAIControlled { get; private set; }

    [Header("Segment Prefabs")]
    [SerializeField] public GameObject headPrefab;
    [SerializeField] public GameObject bodyPrefab;
    [SerializeField] public GameObject tailPrefab;
    [SerializeField] public Transform segmentsContainer;

    [Header("Settings")]
    [SerializeField] private float moveInterval = 0.15f;
    [SerializeField] private int initialLength = 5;

    [Header("Input")]
    public KeyCode KeyUp = KeyCode.W;
    public KeyCode KeyDown = KeyCode.S;
    public KeyCode KeyLeft = KeyCode.A;
    public KeyCode KeyRight = KeyCode.D;

    [Header("Debug - XEM Ở ĐÂY")]
    [SerializeField] private bool enableDebug = true;

    public int Score { get; private set; } = 0;
    public bool IsDead => !isAlive;
    private bool isAlive = true;
    private int moveCount = 0;

    private List<Transform> segments = new List<Transform>();
    private Vector2Int direction = Vector2Int.right;
    private float moveTimer = 0f;

    private int playMinX = -100;
    private int playMaxX = 100;
    private int playMinY = -100;
    private int playMaxY = 100;
    private bool playAreaSet = false;

    public Vector2Int CurrentFoodPos { get; set; }

    public event System.Action<SnakeController> OnSnakeDied;
    public event System.Action OnSnakeEatFood;

    public void SetPlayArea(int minX, int maxX, int minY, int maxY)
    {
        playMinX = minX;
        playMaxX = maxX;
        playMinY = minY;
        playMaxY = maxY;
        playAreaSet = true;
        Debug.Log($"<color=cyan>[{PlayerName}] Play Area SET: X({minX} → {maxX}), Y({minY} → {maxY})</color>");
    }

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

        if (!playAreaSet)
        {
            Debug.LogWarning($"<color=yellow>[{PlayerName}] PlayArea chưa được set!</color>");
        }

        ResetSnake();
    }

    public void ResetSnake()
    {
        if (headPrefab == null || bodyPrefab == null || tailPrefab == null)
        {
            Debug.LogError($"[{PlayerName}] PREFABS MISSING!");
            return;
        }

        int startX = (playMinX + playMaxX) / 2;
        int startY = (playMinY + playMaxY) / 2;
        Vector3 spawnPos = new Vector3(startX, startY, 0f);

        Debug.Log($"<color=lime>[{PlayerName}] Spawn tại: {spawnPos}</color>");

        foreach (Transform seg in segments)
        {
            if (seg != null)
                Destroy(seg.gameObject);
        }
        segments.Clear();

        var headObj = Instantiate(headPrefab, spawnPos, Quaternion.identity, segmentsContainer);
        headObj.name = $"{PlayerName}_Head";

        // ✅ THÊM COLLIDER2D CHO HEAD
        var headCollider = headObj.GetComponent<BoxCollider2D>();
        if (headCollider == null)
        {
            headCollider = headObj.AddComponent<BoxCollider2D>();
            Debug.Log($"<color=yellow>[{PlayerName}] Đã thêm BoxCollider2D cho Head</color>");
        }
        headCollider.isTrigger = true; // ✅ QUAN TRỌNG!

        SpriteRenderer headSR = headObj.GetComponent<SpriteRenderer>();
        if (headSR) headSR.color = SnakeColor;
        segments.Add(headObj.transform);

        Vector3 curPos = spawnPos;
        for (int i = 1; i < initialLength - 1; i++)
        {
            curPos -= new Vector3(1, 0, 0);
            var bodyObj = Instantiate(bodyPrefab, curPos, Quaternion.identity, segmentsContainer);
            bodyObj.name = $"{PlayerName}_Body{i}";
            SpriteRenderer bodySR = bodyObj.GetComponent<SpriteRenderer>();
            if (bodySR) bodySR.color = SnakeColor * 0.9f;
            segments.Add(bodyObj.transform);
        }

        curPos -= new Vector3(1, 0, 0);
        var tailObj = Instantiate(tailPrefab, curPos, Quaternion.identity, segmentsContainer);
        tailObj.name = $"{PlayerName}_Tail";
        SpriteRenderer tailSR = tailObj.GetComponent<SpriteRenderer>();
        if (tailSR) tailSR.color = SnakeColor * 0.8f;
        segments.Add(tailObj.transform);

        direction = Vector2Int.right;
        moveTimer = 0f;
        isAlive = true;
        moveCount = 0;

        Debug.Log($"<color=lime>[{PlayerName}] Spawn hoàn tất! Segments: {segments.Count}</color>");
    }

    private void Update()
    {
        if (!isAlive) return;

        if (!IsAIControlled)
            HandlePlayerInput();

        moveTimer += Time.deltaTime;
        if (moveTimer >= moveInterval)
        {
            Move();
            moveTimer = 0f;
        }
    }

    private void HandlePlayerInput()
    {
        if (Input.GetKeyDown(KeyUp) && direction != Vector2Int.down) direction = Vector2Int.up;
        if (Input.GetKeyDown(KeyDown) && direction != Vector2Int.up) direction = Vector2Int.down;
        if (Input.GetKeyDown(KeyLeft) && direction != Vector2Int.right) direction = Vector2Int.left;
        if (Input.GetKeyDown(KeyRight) && direction != Vector2Int.left) direction = Vector2Int.right;
    }

    public void Move()
    {
        if (!isAlive || segments.Count == 0) return;

        moveCount++;

        for (int i = segments.Count - 1; i > 0; i--)
            segments[i].position = segments[i - 1].position;

        Vector3 moveVect = new Vector3(direction.x, direction.y, 0f);
        segments[0].position += moveVect;

        Vector2Int headGridPos = GetHeadPosition();

        // ✅ DEBUG: In ra vị trí head và food
        if (enableDebug)
        {
            Debug.Log($"<color=cyan>[{PlayerName}] Head: {headGridPos} | Food: {CurrentFoodPos}</color>");
        }

        // Kiểm tra va chạm tường
        if (headGridPos.x < playMinX || headGridPos.x > playMaxX ||
            headGridPos.y < playMinY || headGridPos.y > playMaxY)
        {
            Debug.LogError($"<color=red>[{PlayerName}] ĐÂM VÀO TƯỜNG tại {headGridPos}!</color>");
            Die();
            return;
        }

        // Kiểm tra tự cắn
        if (moveCount > initialLength)
        {
            for (int i = 1; i < segments.Count; i++)
            {
                Vector2Int segmentPos = new Vector2Int(
                    Mathf.RoundToInt(segments[i].position.x),
                    Mathf.RoundToInt(segments[i].position.y)
                );
                if (segmentPos == headGridPos)
                {
                    Debug.LogError($"<color=red>[{PlayerName}] TỰ CẮN tại {headGridPos}!</color>");
                    Die();
                    return;
                }
            }
        }

        // ✅ KIỂM TRA ĂN FOOD
        if (headGridPos == CurrentFoodPos)
        {
            Debug.Log($"<color=yellow>🍎 [{PlayerName}] ĐÃ ĂN FOOD tại {headGridPos}!</color>");
            Grow();
            OnSnakeEatFood?.Invoke();
        }
    }

    // ✅ THÊM HÀM OnTriggerEnter2D ĐỂ DETECT VA CHẠM VỚI FOOD
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isAlive) return;

        if (collision.CompareTag("Food"))
        {
            Debug.Log($"<color=green>🍎 [{PlayerName}] TRIGGER với FOOD!</color>");

            // Lấy vị trí food
            Vector2Int foodPos = new Vector2Int(
                Mathf.RoundToInt(collision.transform.position.x),
                Mathf.RoundToInt(collision.transform.position.y)
            );

            // Cập nhật CurrentFoodPos
            CurrentFoodPos = foodPos;

            // Grow và invoke event
            Grow();
            OnSnakeEatFood?.Invoke();

            // Xóa food (GameManager sẽ spawn lại)
            Destroy(collision.gameObject);
        }
    }

    public void Grow()
    {
        if (segments.Count < 2) return;

        Transform lastBody = segments[segments.Count - 2];
        Vector3 newPos = lastBody.position;
        var newBodyObj = Instantiate(bodyPrefab, newPos, Quaternion.identity, segmentsContainer);
        newBodyObj.name = $"{PlayerName}_Body_Extra{Score}";
        newBodyObj.GetComponent<SpriteRenderer>().color = SnakeColor * 0.9f;

        segments.Insert(segments.Count - 1, newBodyObj.transform);
        Score++;

        Debug.Log($"<color=lime>[{PlayerName}] GROW! Score: {Score}</color>");
    }

    public Vector2Int GetHeadPosition()
    {
        if (segments.Count == 0) return Vector2Int.zero;
        Vector3 pos = segments[0].position;
        return new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
    }

    public bool IsOccupied(Vector2Int pos)
    {
        foreach (var s in segments)
        {
            Vector3 sp = s.position;
            var g = new Vector2Int(Mathf.RoundToInt(sp.x), Mathf.RoundToInt(sp.y));
            if (g == pos) return true;
        }
        return false;
    }

    public List<Vector2Int> SegmentPositions
    {
        get
        {
            var list = new List<Vector2Int>();
            foreach (var s in segments)
            {
                Vector3 pos = s.position;
                list.Add(new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y)));
            }
            return list;
        }
    }

    public Vector2Int GetCurrentDirection() => direction;
    public void SetAI(bool value) => IsAIControlled = value;

    public void SetDirection(Vector2Int dir)
    {
        if (dir + direction != Vector2Int.zero)
            direction = dir;
    }

    public void Die()
    {
        isAlive = false;
        Debug.Log($"<color=red>[{PlayerName}] ĐÃ CHẾT! Final Score: {Score}</color>");
        OnSnakeDied?.Invoke(this);
    }
}