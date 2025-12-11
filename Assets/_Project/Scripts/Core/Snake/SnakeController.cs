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

    // Gameplay state
    public int Score { get; private set; } = 0;
    public bool IsDead => !isAlive;
    private bool isAlive = true;

    private List<Transform> segments = new List<Transform>();
    private Vector2Int direction = Vector2Int.right;
    private float moveTimer = 0f;

    public event System.Action<SnakeController> OnSnakeDied;

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

        ResetSnake();
    }

    public void ResetSnake()
    {
        // Dọn sạch các segment cũ
        foreach (Transform seg in segments)
        {
            if (seg != null)
                Destroy(seg.gameObject);
        }
        segments.Clear();

        // Spawn HEAD
        var headObj = Instantiate(headPrefab, transform.position, Quaternion.identity, segmentsContainer);
        SpriteRenderer headSR = headObj.GetComponent<SpriteRenderer>();
        if (headSR) headSR.color = SnakeColor;
        segments.Add(headObj.transform);

        // Spawn BODY: initialLength - 2 segments
        Vector3 curPos = transform.position;
        for (int i = 1; i < initialLength - 1; i++)
        {
            curPos -= new Vector3(1, 0, 0); // Xếp theo x, chỉnh lại nếu muốn hướng khác
            var bodyObj = Instantiate(bodyPrefab, curPos, Quaternion.identity, segmentsContainer);
            SpriteRenderer bodySR = bodyObj.GetComponent<SpriteRenderer>();
            if (bodySR) bodySR.color = SnakeColor * 0.9f; // hơi tối đi nếu muốn
            segments.Add(bodyObj.transform);
        }
        // Spawn TAIL
        curPos -= new Vector3(1, 0, 0);
        var tailObj = Instantiate(tailPrefab, curPos, Quaternion.identity, segmentsContainer);
        SpriteRenderer tailSR = tailObj.GetComponent<SpriteRenderer>();
        if (tailSR) tailSR.color = SnakeColor * 0.8f;
        segments.Add(tailObj.transform);

        direction = Vector2Int.right;
        moveTimer = 0f;
        isAlive = true;
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
        // Di chuyển từng segment về vị trí segment phía trước
        for (int i = segments.Count - 1; i > 0; i--)
            segments[i].position = segments[i - 1].position;

        Vector3 moveVect = new Vector3(direction.x, direction.y, 0f);
        segments[0].position += moveVect;

        // TODO: Kiểm tra va chạm, ăn mồi...
    }

    public void Grow()
    {
        // Thêm một body vào trước tail (tail luôn là cuối danh sách)
        Transform lastBody = segments[segments.Count - 2];
        Vector3 newPos = lastBody.position;
        var newBodyObj = Instantiate(bodyPrefab, newPos, Quaternion.identity, segmentsContainer);
        newBodyObj.GetComponent<SpriteRenderer>().color = SnakeColor * 0.9f;

        segments.Insert(segments.Count - 1, newBodyObj.transform);
        Score++;
    }

    public Vector2Int GetHeadPosition()
    {
        Vector3 pos = segments[0].position;
        return new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
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
        OnSnakeDied?.Invoke(this);
    }
}