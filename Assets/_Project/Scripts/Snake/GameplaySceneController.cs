using UnityEngine;
using System.Collections;

public class GameplaySceneController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private FoodSpawner foodSpawner;
    [SerializeField] private CameraController cameraController;

    [Header("Snake Prefab")]
    [SerializeField] private GameObject snakePrefab;

    [Header("Spawn Settings")]
    [SerializeField] private Vector2Int playerSpawnGrid = new Vector2Int(5, 10);
    [SerializeField] private Vector2Int aiSpawnGrid = new Vector2Int(25, 10);

    [Header("Game Settings")]
    [SerializeField] private GameMode currentGameMode = GameMode.SinglePlayer;
    [SerializeField] private float baseMoveSpeed = 0.2f;
    [SerializeField] private bool autoStartGame = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private SnakeController playerSnake;
    private SnakeController aiSnake;

    public enum GameMode
    {
        SinglePlayer,
        VsAI
    }

    private void Start()
    {
        StartCoroutine(InitializeGameplay());
    }

    private IEnumerator InitializeGameplay()
    {
        yield return new WaitUntil(() => GridManager.Instance != null);

        if (showDebugLogs)
            Debug.Log("[GameplayScene] ✅ GridManager ready");

        if (cameraController != null)
        {
            cameraController.RefreshCameraFit();
        }

        yield return new WaitForSeconds(0.1f);

        if (snakePrefab == null)
        {
            Debug.LogError("[GameplayScene] ❌ SNAKE PREFAB IS NULL!  Kéo prefab vào Inspector!");
            yield break;
        }

        SpawnSnakesForGameMode();

        yield return new WaitForSeconds(0.1f);

        if (foodSpawner != null)
        {
            foodSpawner.ClearAllFood();
            foodSpawner.SpawnInitialFoods(5);
            foodSpawner.StartAutoSpawn();

            if (showDebugLogs)
                Debug.Log("[GameplayScene] ✅ Food spawned");
        }

        // ✅ FIX: Dùng ChangeState thay vì StartGame
        if (autoStartGame && GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameManager.GameState.Playing);
        }

        if (showDebugLogs)
            Debug.Log("[GameplayScene] ✅ Game started!");
    }

    private void SpawnSnakesForGameMode()
    {
        switch (currentGameMode)
        {
            case GameMode.SinglePlayer:
                SpawnPlayer();
                break;

            case GameMode.VsAI:
                SpawnPlayer();
                SpawnAI();
                break;
        }
    }

    private void SpawnPlayer()
    {
        if (snakePrefab == null)
        {
            Debug.LogError("[GameplayScene] ❌ Cannot spawn Player:  snakePrefab is null!");
            return;
        }

        Vector3 worldPos = GridManager.Instance.GridToWorld(playerSpawnGrid);
        GameObject snakeObj = Instantiate(snakePrefab, worldPos, Quaternion.identity);
        snakeObj.name = "Player_Snake";

        playerSnake = snakeObj.GetComponent<SnakeController>();

        if (playerSnake == null)
        {
            Debug.LogError("[GameplayScene] ❌ Snake prefab doesn't have SnakeController component!");
            Destroy(snakeObj);
            return;
        }

        Color playerColor = LoadPlayerColor(0, Color.green);

        string playerName = PlayerNameManager.Instance != null ?
            PlayerNameManager.Instance.GetPlayerName() : "Người chơi";

        playerSnake.Setup(
            playerID: 0,
            color: playerColor,
            isAI: false,
            playerName: playerName,
            kUp: KeyCode.W,
            kDown: KeyCode.S,
            kLeft: KeyCode.A,
            kRight: KeyCode.D
        );

        playerSnake.SetMoveSpeed(baseMoveSpeed);

        if (showDebugLogs)
            Debug.Log($"[GameplayScene] ✅ Spawned Player at grid {playerSpawnGrid} (world: {worldPos})");
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

        AIController aiController = snakeObj.GetComponent<AIController>();
        if (aiController == null)
        {
            aiController = snakeObj.AddComponent<AIController>();
        }

        aiSnake.Setup(
            playerID: 2,
            color: Color.cyan,
            isAI: true,
            playerName: "AI"
        );

        aiSnake.SetMoveSpeed(baseMoveSpeed);

        if (showDebugLogs)
            Debug.Log($"[GameplayScene] ✅ Spawned AI at grid {aiSpawnGrid}");
    }

    private Color LoadPlayerColor(int playerID, Color defaultColor)
    {
        string key = "PlayerSnakeColor";

        if (PlayerPrefs.HasKey(key))
        {
            string colorHex = PlayerPrefs.GetString(key);
            Color savedColor;

            if (ColorUtility.TryParseHtmlString(colorHex, out savedColor))
            {
                if (showDebugLogs)
                    Debug.Log($"[GameplayScene] Loaded saved color: {colorHex}");
                return savedColor;
            }
        }

        return defaultColor;
    }

    public void SetGameMode(GameMode mode)
    {
        currentGameMode = mode;
        if (showDebugLogs)
            Debug.Log($"[GameplayScene] Game mode set to: {mode}");
    }

    public void RestartGame()
    {
        if (playerSnake != null) Destroy(playerSnake.gameObject);
        if (aiSnake != null) Destroy(aiSnake.gameObject);

        if (foodSpawner != null)
        {
            foodSpawner.ClearAllFood();
        }

        // ✅ FIX:  Dùng ClearAllCells thay vì ClearGrid
        if (GridManager.Instance != null)
        {
            GridManager.Instance.ClearAllCells();
        }

        StartCoroutine(InitializeGameplay());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("[GameplayScene] Restarting game...");
            RestartGame();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetGameMode(GameMode.SinglePlayer);
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

        Gizmos.color = Color.green;
        Vector3 playerPos = GridManager.Instance.GridToWorld(playerSpawnGrid);
        Gizmos.DrawWireSphere(playerPos, 0.5f);

        Gizmos.color = Color.cyan;
        Vector3 aiPos = GridManager.Instance.GridToWorld(aiSpawnGrid);
        Gizmos.DrawWireSphere(aiPos, 0.5f);
    }
}