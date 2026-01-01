using UnityEngine;
using System.Collections;

/// <summary>
/// Controller chính cho Gameplay Scene - Quản lý spawn và game flow
/// </summary>
public class GameplaySceneController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private FoodSpawner foodSpawner;
    [SerializeField] private CameraController cameraController;

    [Header("Snake Prefab")]
    [SerializeField] private GameObject snakePrefab; // ✅ QUAN TRỌNG: Kéo prefab vào đây

    [Header("Spawn Settings")]
    [SerializeField] private Vector2Int player1SpawnGrid = new Vector2Int(5, 10);
    [SerializeField] private Vector2Int player2SpawnGrid = new Vector2Int(15, 10);
    [SerializeField] private Vector2Int aiSpawnGrid = new Vector2Int(25, 10);

    [Header("Game Settings")]
    [SerializeField] private GameMode currentGameMode = GameMode.SinglePlayer;
    [SerializeField] private float baseMoveSpeed = 0.2f;
    [SerializeField] private bool autoStartGame = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private SnakeController player1Snake;
    private SnakeController player2Snake;
    private SnakeController aiSnake;

    public enum GameMode
    {
        SinglePlayer,
        Multiplayer,
        VsAI
    }

    private void Start()
    {
        StartCoroutine(InitializeGameplay());
    }

    private IEnumerator InitializeGameplay()
    {
        // ✅ STEP 1: Đợi GridManager ready
        yield return new WaitUntil(() => GridManager.Instance != null);

        if (showDebugLogs)
            Debug.Log("[GameplayScene] ✅ GridManager ready");

        // ✅ STEP 2: Setup camera
        if (cameraController != null)
        {
            cameraController.RefreshCameraFit();
        }

        yield return new WaitForSeconds(0.1f);

        // ✅ STEP 3: Check snake prefab
        if (snakePrefab == null)
        {
            Debug.LogError("[GameplayScene] ❌ SNAKE PREFAB IS NULL! Kéo prefab vào Inspector!");
            yield break;
        }

        // ✅ STEP 4: Spawn snakes theo game mode
        SpawnSnakesForGameMode();

        yield return new WaitForSeconds(0.1f);

        // ✅ STEP 5: Spawn food
        if (foodSpawner != null)
        {
            foodSpawner.ClearAllFood();
            foodSpawner.SpawnInitialFoods(5);
            foodSpawner.StartAutoSpawn();

            if (showDebugLogs)
                Debug.Log("[GameplayScene] ✅ Food spawned");
        }

        // ✅ STEP 6: Start game
        if (autoStartGame && GameManager.Instance != null)
        {
            // Use a public method to start the game, since IsPlaying's setter is not accessible
            GameManager.Instance.StartGame((GameManager.GameMode)currentGameMode);
        }

        if (showDebugLogs)
            Debug.Log("[GameplayScene] ✅ Game started!");
    }

    private void SpawnSnakesForGameMode()
    {
        switch (currentGameMode)
        {
            case GameMode.SinglePlayer:
                SpawnPlayer1();
                break;

            case GameMode.Multiplayer:
                SpawnPlayer1();
                SpawnPlayer2();
                break;

            case GameMode.VsAI:
                SpawnPlayer1();
                SpawnAI();
                break;
        }
    }

    private void SpawnPlayer1()
    {
        if (snakePrefab == null)
        {
            Debug.LogError("[GameplayScene] ❌ Cannot spawn Player 1: snakePrefab is null!");
            return;
        }

        // ✅ Convert grid position to world position
        Vector3 worldPos = GridManager.Instance.GridToWorld(player1SpawnGrid);

        // ✅ Instantiate snake
        GameObject snakeObj = Instantiate(snakePrefab, worldPos, Quaternion.identity);
        snakeObj.name = "Player1_Snake";

        // ✅ Get SnakeController component
        player1Snake = snakeObj.GetComponent<SnakeController>();

        if (player1Snake == null)
        {
            Debug.LogError("[GameplayScene] ❌ Snake prefab doesn't have SnakeController component!");
            Destroy(snakeObj);
            return;
        }

        // ✅ Load saved color hoặc dùng default
        Color player1Color = LoadPlayerColor(0, Color.green);

        // ✅ Setup snake
        player1Snake.Setup(
            playerID: 0,
            color: player1Color,
            isAI: false,
            playerName: "Player 1",
            kUp: KeyCode.W,
            kDown: KeyCode.S,
            kLeft: KeyCode.A,
            kRight: KeyCode.D
        );

        player1Snake.SetMoveSpeed(baseMoveSpeed);

        if (showDebugLogs)
            Debug.Log($"[GameplayScene] ✅ Spawned Player 1 at grid {player1SpawnGrid} (world: {worldPos})");
    }

    private void SpawnPlayer2()
    {
        if (snakePrefab == null) return;

        Vector3 worldPos = GridManager.Instance.GridToWorld(player2SpawnGrid);
        GameObject snakeObj = Instantiate(snakePrefab, worldPos, Quaternion.identity);
        snakeObj.name = "Player2_Snake";

        player2Snake = snakeObj.GetComponent<SnakeController>();
        if (player2Snake == null)
        {
            Destroy(snakeObj);
            return;
        }

        Color player2Color = LoadPlayerColor(1, Color.blue);

        player2Snake.Setup(
            playerID: 1,
            color: player2Color,
            isAI: false,
            playerName: "Player 2",
            kUp: KeyCode.UpArrow,
            kDown: KeyCode.DownArrow,
            kLeft: KeyCode.LeftArrow,
            kRight: KeyCode.RightArrow
        );

        player2Snake.SetMoveSpeed(baseMoveSpeed);

        if (showDebugLogs)
            Debug.Log($"[GameplayScene] ✅ Spawned Player 2 at grid {player2SpawnGrid}");
    }

    private void SpawnAI()
    {
        if (snakePrefab == null) return;

        Vector3 worldPos = GridManager.Instance.GridToWorld(aiSpawnGrid);
        GameObject snakeObj = Instantiate(snakePrefab, worldPos, Quaternion.identity);
        snakeObj.name = "AI_Snake";

        aiSnake = snakeObj.GetComponent<SnakeController>();
        if (aiSnake == null)
        {
            Destroy(snakeObj);
            return;
        }

        // ✅ Check if AIController exists, if not add it
        AIController aiController = snakeObj.GetComponent<AIController>();
        if (aiController == null)
        {
            aiController = snakeObj.AddComponent<AIController>();
        }

        aiSnake.Setup(
            playerID: 2,
            color: Color.red,
            isAI: true,
            playerName: "AI"
        );

        aiSnake.SetMoveSpeed(baseMoveSpeed); // ✅ SAME speed as player

        if (showDebugLogs)
            Debug.Log($"[GameplayScene] ✅ Spawned AI at grid {aiSpawnGrid}");
    }

    private Color LoadPlayerColor(int playerID, Color defaultColor)
    {
        string key = $"Player{playerID}_SnakeColor";

        if (PlayerPrefs.HasKey(key))
        {
            string colorHex = PlayerPrefs.GetString(key);
            Color savedColor;

            if (ColorUtility.TryParseHtmlString(colorHex, out savedColor))
            {
                if (showDebugLogs)
                    Debug.Log($"[GameplayScene] Loaded saved color for Player {playerID}: {colorHex}");
                return savedColor;
            }
        }

        return defaultColor;
    }

    // ✅ Public methods để gọi từ UI
    public void SetGameMode(GameMode mode)
    {
        currentGameMode = mode;
        if (showDebugLogs)
            Debug.Log($"[GameplayScene] Game mode set to: {mode}");
    }

    public void RestartGame()
    {
        // Destroy existing snakes
        if (player1Snake != null) Destroy(player1Snake.gameObject);
        if (player2Snake != null) Destroy(player2Snake.gameObject);
        if (aiSnake != null) Destroy(aiSnake.gameObject);

        // Clear food
        if (foodSpawner != null)
        {
            foodSpawner.ClearAllFood();
        }

        // Clear grid
        if (GridManager.Instance != null)
        {
            GridManager.Instance.ClearGrid();
        }

        // Restart
        StartCoroutine(InitializeGameplay());
    }

    private void Update()
    {
        // ✅ Debug: Press R to restart
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("[GameplayScene] Restarting game...");
            RestartGame();
        }

        // ✅ Debug: Press 1/2/3 to change mode
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetGameMode(GameMode.SinglePlayer);
            RestartGame();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetGameMode(GameMode.Multiplayer);
            RestartGame();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetGameMode(GameMode.VsAI);
            RestartGame();
        }
    }

    private void OnDrawGizmos()
    {
        if (GridManager.Instance == null) return;

        // Draw spawn positions
        Gizmos.color = Color.green;
        Vector3 p1Pos = GridManager.Instance.GridToWorld(player1SpawnGrid);
        Gizmos.DrawWireSphere(p1Pos, 0.5f);

        Gizmos.color = Color.blue;
        Vector3 p2Pos = GridManager.Instance.GridToWorld(player2SpawnGrid);
        Gizmos.DrawWireSphere(p2Pos, 0.5f);

        Gizmos.color = Color.red;
        Vector3 aiPos = GridManager.Instance.GridToWorld(aiSpawnGrid);
        Gizmos.DrawWireSphere(aiPos, 0.5f);
    }
}