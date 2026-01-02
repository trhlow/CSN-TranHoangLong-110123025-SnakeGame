using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// ✅ FULL DEBUG VERSION - Snake spawn với logs đầy đủ
/// </summary>
public class GameManager : Singleton<GameManager>
{
    public enum GameState { MainMenu, Playing, Paused, GameOver, Loading }
    public enum GameMode { SinglePlayer, VsAI }

    [Header("Snake Prefabs")]
    [SerializeField] private GameObject playerSnakePrefab;
    [SerializeField] private GameObject aiSnakePrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnDistanceFromCenter = 3f;

    [Header("Game Settings")]
    [SerializeField] private GameMode gameMode = GameMode.SinglePlayer;
    [SerializeField] private int targetScore = 500;
    [SerializeField] private float gameTimeLimit = 300f;
    [SerializeField] private bool hasTimeLimit = false;

    [Header("Initial Food Count")]
    [SerializeField] private int initialFoodCount = 5;

    [Header("Events")]
    public UnityEvent<GameState> OnGameStateChanged = new UnityEvent<GameState>();

    private GameState currentState = GameState.MainMenu;
    private List<SnakeController> snakes = new List<SnakeController>();
    private float gameTime = 0f;

    private Dictionary<int, ComboTracker> comboTrackers = new Dictionary<int, ComboTracker>();
    private const float COMBO_TIMEOUT = 2f;

    private class ComboTracker
    {
        public int count = 0;
        public float lastEatTime = 0f;
    }

    public GameMode CurrentGameMode => gameMode;
    public GameState CurrentState => currentState;
    public float GameTime => gameTime;
    public bool IsPlaying => currentState == GameState.Playing;

    protected override void Awake()
    {
        base.Awake();
        Debug.Log("[GameManager] 🎬 Awake() called");
    }

    // ✅ FIX: Start() với logs đầy đủ
    private void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"[GameManager] 🎬 Start() called in scene: {sceneName}");

        if (sceneName == "MainMenu")
        {
            Debug.Log("[GameManager] 📍 MainMenu scene detected");
            ChangeState(GameState.MainMenu);
        }
        else if (sceneName == "Gameplay")
        {
            Debug.Log("[GameManager] 🎮 Gameplay scene detected, starting initialization...");

            // ✅ Load game mode từ PlayerPrefs
            LoadGameMode();

            // ✅ Đợi 1 frame để scene load xong
            StartCoroutine(InitializeGameplayDelayed());
        }
    }

    // ✅ NEW: Coroutine để đợi scene load hoàn toàn
    private IEnumerator InitializeGameplayDelayed()
    {
        Debug.Log("[GameManager] ⏳ Waiting for scene to fully load...");
        yield return new WaitForEndOfFrame();

        Debug.Log($"[GameManager] ✅ Scene loaded! Initializing with mode: {gameMode}");
        InitializeGameplay();
    }

    private void Update()
    {
        if (currentState != GameState.Playing)
            return;

        gameTime += Time.deltaTime;

        if (hasTimeLimit && gameTime >= gameTimeLimit)
        {
            EndGameTimeLimit();
        }

        if (targetScore > 0)
        {
            CheckTargetScore();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        UpdateComboTrackers();
    }

    #region Game State Management
    public void ChangeState(GameState newState)
    {
        if (currentState == newState)
            return;

        Debug.Log($"[GameManager] 🔄 State: {currentState} → {newState}");
        currentState = newState;
        OnGameStateChanged?.Invoke(newState);

        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                if (UIManager.Instance != null)
                    UIManager.Instance.HidePauseMenu();
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowPauseMenu();
                break;

            case GameState.GameOver:
                Time.timeScale = 0f;
                HandleGameOver();
                break;
        }
    }

    public void TogglePause()
    {
        if (currentState == GameState.Playing)
        {
            ChangeState(GameState.Paused);
        }
        else if (currentState == GameState.Paused)
        {
            ChangeState(GameState.Playing);
        }
    }
    #endregion

    #region Game Mode Management
    public void SetGameMode(GameMode mode)
    {
        gameMode = mode;
        PlayerPrefs.SetInt("GameMode", (int)mode);
        PlayerPrefs.Save();
        Debug.Log($"[GameManager] 💾 Game mode set and saved: {mode}");
    }

    public void SetGameMode(int modeInt)
    {
        SetGameMode((GameMode)modeInt);
    }

    private void LoadGameMode()
    {
        if (PlayerPrefs.HasKey("GameMode"))
        {
            gameMode = (GameMode)PlayerPrefs.GetInt("GameMode");
            Debug.Log($"[GameManager] 📂 Loaded GameMode from PlayerPrefs: {gameMode}");
        }
        else
        {
            Debug.LogWarning("[GameManager] ⚠️ No saved GameMode found, using default: SinglePlayer");
            gameMode = GameMode.SinglePlayer;
        }
    }
    #endregion

    #region Scene Management
    // ✅ FIX: LoadGameplay với logs đầy đủ
    public void LoadGameplay()
    {
        Debug.Log($"[GameManager] ⚙️ LoadGameplay called! Mode: {gameMode}");
        Debug.Log($"[GameManager] 📍 Current scene: {SceneManager.GetActiveScene().name}");

        // Save game mode
        PlayerPrefs.SetInt("GameMode", (int)gameMode);
        PlayerPrefs.Save();
        Debug.Log($"[GameManager] 💾 Saved GameMode to PlayerPrefs: {gameMode}");

        Time.timeScale = 1f;
        ChangeState(GameState.Loading);

        Debug.Log("[GameManager] 🔄 Loading Gameplay scene...");
        SceneManager.LoadScene("Gameplay");
    }

    public void LoadMainMenu()
    {
        Debug.Log("[GameManager] 🏠 Loading MainMenu scene...");
        Time.timeScale = 1f;
        ChangeState(GameState.Loading);
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartGame()
    {
        Debug.Log("[GameManager] 🔄 Restarting game...");
        Time.timeScale = 1f;
        ChangeState(GameState.Loading);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            Debug.Log("[GameManager] ▶️ Resuming game...");
            ChangeState(GameState.Playing);
        }
    }
    #endregion

    #region Gameplay Initialization
    // ✅ FIX: InitializeGameplay với debug logs
    private void InitializeGameplay()
    {
        Debug.Log($"[GameManager] ⚙️ InitializeGameplay START - Mode: {gameMode}");

        ClearSnakes();
        gameTime = 0f;
        comboTrackers.Clear();

        Debug.Log("[GameManager] 🐍 Calling SpawnAllSnakes()...");
        SpawnAllSnakes();

        if (FoodSpawner.Instance != null)
        {
            Debug.Log($"[GameManager] 🍎 Spawning {initialFoodCount} initial foods...");
            FoodSpawner.Instance.SpawnInitialFoods(initialFoodCount);
            FoodSpawner.Instance.StartAutoSpawn();
        }
        else
        {
            Debug.LogError("[GameManager] ❌ FoodSpawner.Instance is NULL!");
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic("gameplay", true);
        }

        ChangeState(GameState.Playing);

        Debug.Log($"[GameManager] ✅ InitializeGameplay DONE! Spawned {snakes.Count} snake(s)");
    }

    // ✅ FIX: SpawnAllSnakes với debug logs đầy đủ
    private void SpawnAllSnakes()
    {
        Debug.Log($"[GameManager] 🐍 SpawnAllSnakes START - Mode: {gameMode}");

        // Check prefabs
        if (playerSnakePrefab == null)
        {
            Debug.LogError("[GameManager] ❌❌❌ playerSnakePrefab is NULL! Assign it in Inspector!");
            return;
        }

        // ✅ FIX: Spawn Player 1 với màu đã lưu
        Vector3 player1SpawnPos = new Vector3(-spawnDistanceFromCenter, 0, 0);
        Color playerColor = LoadSnakeColor(1, Color.green); // Load màu đã chọn trong Settings
        Debug.Log($"[GameManager] 🎨 Player color: #{ColorUtility.ToHtmlStringRGB(playerColor)}");

        // ✅ Lấy tên từ PlayerNameManager
        string playerName = PlayerNameManager.Instance != null ? PlayerNameManager.Instance.PlayerName : "Người chơi";
        Debug.Log($"[GameManager] 👤 Player name: {playerName}");
        var player = SpawnSnake(
            prefab: playerSnakePrefab,
            id: 1,
            color: playerColor,
            isAI: false,
            name: playerName, // ✅ Dùng tên thật
            spawnPos: player1SpawnPos,
            keyUp: KeyCode.W,
            keyDown: KeyCode.S,
            keyLeft: KeyCode.A,
            keyRight: KeyCode.D
        );

        if (player != null)
        {
            snakes.Add(player);
            InitializeComboTracker(1);
            Debug.Log($"[GameManager] ✅ Player 1 spawned successfully at {player1SpawnPos}");
        }
        else
        {
            Debug.LogError("[GameManager] ❌ Failed to spawn Player 1!");
        }

        // Spawn AI if VsAI mode
        if (gameMode == GameMode.VsAI)
        {
            Debug.Log("[GameManager] 🤖 VsAI mode detected, spawning AI...");

            GameObject aiBotPrefab = aiSnakePrefab != null ? aiSnakePrefab : playerSnakePrefab;
            Vector3 aiSpawnPos = new Vector3(spawnDistanceFromCenter, 0, 0);
            Debug.Log($"[GameManager] 📍 AI spawn position: {aiSpawnPos}");
            
            // ✅ FIX: AI luôn là màu cyan, không load từ Settings
            Color aiColor = Color.cyan;
            Debug.Log($"[GameManager] 🤖 AI color: #{ColorUtility.ToHtmlStringRGB(aiColor)} (CYAN - Fixed)");
            var ai = SpawnSnake(
                prefab: aiBotPrefab,
                id: 3,
                color: aiColor,
                isAI: true,
                name: "AI Bot",
                spawnPos: aiSpawnPos
            );

            if (ai != null)
            {
                snakes.Add(ai);
                InitializeComboTracker(3);
                Debug.Log($"[GameManager] ✅ AI Bot spawned successfully at {aiSpawnPos}");
            }
            else
            {
                Debug.LogError("[GameManager] ❌ Failed to spawn AI Bot!");
            }
        }
        else
        {
            Debug.Log("[GameManager] ℹ️ SinglePlayer mode, no AI spawn needed");
        }

        Debug.Log($"[GameManager] 🎮 SpawnAllSnakes DONE. Total snakes: {snakes.Count}");
    }
    public void SaveSnakeColor(int playerID, Color color)
    {
        string key = "PlayerSnakeColor"; // ✅ Xoá playerID
        string colorHex = "#" + ColorUtility.ToHtmlStringRGB(color);
        PlayerPrefs.SetString(key, colorHex);
        PlayerPrefs.Save();
    }

    public Color LoadSnakeColor(int playerID, Color defaultColor)
    {
        string key = "PlayerSnakeColor"; // ✅ Xoá playerID
        if (PlayerPrefs.HasKey(key))
        {
            if (ColorUtility.TryParseHtmlString(PlayerPrefs.GetString(key), out Color savedColor))
                return savedColor;
        }
        return defaultColor;
    }
    private SnakeController SpawnSnake(GameObject prefab, int id, Color color, bool isAI, string name,
        Vector3 spawnPos, KeyCode keyUp = KeyCode.W, KeyCode keyDown = KeyCode.S,
        KeyCode keyLeft = KeyCode.A, KeyCode keyRight = KeyCode.D)
    {
        if (prefab == null)
        {
            Debug.LogError($"[GameManager] ❌ Snake prefab is NULL for {name}!");
            return null;
        }

        Debug.Log($"[GameManager] 🐍 Instantiating {name} at {spawnPos}...");
        GameObject snakeObj = Instantiate(prefab, spawnPos, Quaternion.identity);
        snakeObj.name = name;

        SnakeController snake = snakeObj.GetComponent<SnakeController>();
        if (snake == null)
        {
            Debug.LogError($"[GameManager] ❌ SnakeController component not found on {name}!");
            Destroy(snakeObj);
            return null;
        }

        Debug.Log($"[GameManager] ⚙️ Setting up {name}...");
        snake.Setup(id, color, isAI, name, keyUp, keyDown, keyLeft, keyRight);

        snake.OnSnakeDied += OnSnakeDied;
        snake.OnSnakeEatFood += OnSnakeEatFood;

        Debug.Log($"[GameManager] ✅ {name} spawned and configured (ID: {id}, AI: {isAI})");
        return snake;
    }

    private Color GetPlayerColor(int playerID)
    {
        if (ColorPalette.Instance != null)
        {
            return ColorPalette.Instance.GetPlayerColor(playerID);
        }

        return playerID switch
        {
            1 => Color.green,
            2 => Color.magenta,
            3 => Color.cyan,
            _ => Color.white
        };
    }

    private void ClearSnakes()
    {
        Debug.Log($"[GameManager] 🧹 Clearing {snakes.Count} snake(s)...");
        foreach (var snake in snakes)
        {
            if (snake != null)
            {
                snake.OnSnakeDied -= OnSnakeDied;
                snake.OnSnakeEatFood -= OnSnakeEatFood;
                Destroy(snake.gameObject);
            }
        }
        snakes.Clear();
    }
    #endregion

    #region Event Handlers
    private void OnSnakeDied(SnakeController snake)
    {
        Debug.Log($"<color=red>💀 {snake.PlayerName} died! Final Score: {snake.Score}</color>");

        if (FoodSpawner.Instance != null)
        {
            FoodSpawner.Instance.StopAutoSpawn();
        }

        foreach (var s in snakes)
        {
            if (s != null)
            {
                s.enabled = false;
            }
        }

        ChangeState(GameState.GameOver);
    }

    private void OnSnakeEatFood(Food food)
    {
        if (food == null)
            return;

        Debug.Log($"<color=yellow>🍎 Snake ate {food.Rarity} food (+{food.Points} points)</color>");
    }
    #endregion

    #region Combo System
    private void InitializeComboTracker(int playerID)
    {
        if (!comboTrackers.ContainsKey(playerID))
        {
            comboTrackers[playerID] = new ComboTracker();
        }
    }

    public void RegisterFoodEaten(int playerID)
    {
        if (!comboTrackers.ContainsKey(playerID))
        {
            InitializeComboTracker(playerID);
        }

        ComboTracker tracker = comboTrackers[playerID];
        float currentTime = Time.time;

        if (currentTime - tracker.lastEatTime <= COMBO_TIMEOUT)
        {
            tracker.count++;
        }
        else
        {
            tracker.count = 1;
        }

        tracker.lastEatTime = currentTime;

        if (tracker.count >= 2 && UIManager.Instance != null)
        {
            UIManager.Instance.ShowCombo(tracker.count);
        }
    }

    private void UpdateComboTrackers()
    {
        float currentTime = Time.time;

        foreach (var kvp in comboTrackers)
        {
            ComboTracker tracker = kvp.Value;

            if (currentTime - tracker.lastEatTime > COMBO_TIMEOUT && tracker.count > 0)
            {
                tracker.count = 0;

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ResetCombo();
                }
            }
        }
    }

    public int GetComboCount(int playerID)
    {
        return comboTrackers.ContainsKey(playerID) ? comboTrackers[playerID].count : 0;
    }
    #endregion

    #region Game Over Handling
    private void HandleGameOver()
    {
        Debug.Log("[GameManager] 🏁 Game Over!");

        if (HighScoreManager.Instance != null && snakes.Count > 0)
        {
            var topSnake = GetTopScoreSnake();
            if (topSnake != null)
            {
                HighScoreManager.Instance.TryAddHighScore(topSnake.PlayerName, topSnake.Score);
            }
        }

        if (UIManager.Instance != null)
        {
            int winnerID = GetWinnerID();
            int highestScore = GetHighestScore();
            bool hasWinner = snakes.Count > 1;

            UIManager.Instance.ShowGameOver(hasWinner, highestScore, winnerID);
        }
    }

    private void EndGameTimeLimit()
    {
        Debug.Log("[GameManager] ⏰ Time limit reached!");

        var winner = GetTopScoreSnake();
        if (winner != null)
        {
            Debug.Log($"[GameManager] 🏆 Winner: {winner.PlayerName} with {winner.Score} points");
        }

        ChangeState(GameState.GameOver);
    }

    private void CheckTargetScore()
    {
        foreach (var snake in snakes)
        {
            if (snake != null && snake.Score >= targetScore)
            {
                Debug.Log($"[GameManager] 🏆 {snake.PlayerName} reached target score!");
                ChangeState(GameState.GameOver);
                break;
            }
        }
    }
    #endregion

    #region Utility Methods
    public List<SnakeController> GetAllSnakes()
    {
        snakes.RemoveAll(s => s == null);
        return new List<SnakeController>(snakes);
    }

    public SnakeController GetSnakeByID(int id)
    {
        return snakes.Find(s => s != null && s.PlayerID == id);
    }

    public int GetHighestScore()
    {
        int maxScore = 0;
        foreach (var snake in snakes)
        {
            if (snake != null)
            {
                maxScore = Mathf.Max(maxScore, snake.Score);
            }
        }
        return maxScore;
    }

    private int GetWinnerID()
    {
        var winner = GetTopScoreSnake();
        return winner != null ? winner.PlayerID : 1;
    }

    private SnakeController GetTopScoreSnake()
    {
        SnakeController topSnake = null;
        int maxScore = 0;

        foreach (var snake in snakes)
        {
            if (snake != null && snake.Score > maxScore)
            {
                maxScore = snake.Score;
                topSnake = snake;
            }
        }

        return topSnake;
    }
    #endregion
}