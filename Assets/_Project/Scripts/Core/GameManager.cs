using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

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
            Debug.Log("[GameManager] 🎮 Gameplay scene detected, starting initialization.. .");
            LoadGameMode();
            StartCoroutine(InitializeGameplayDelayed());
        }
    }

    private IEnumerator InitializeGameplayDelayed()
    {
        Debug.Log("[GameManager] ⏳ Waiting for scene to fully load...");
        yield return new WaitForEndOfFrame();

        Debug.Log($"[GameManager] ✅ Scene loaded!  Initializing with mode: {gameMode}");
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

        Debug.Log($"[GameManager] 🔄 State:  {currentState} → {newState}");
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
            Debug.LogWarning("[GameManager] ⚠️ No saved GameMode found, using default:  SinglePlayer");
            gameMode = GameMode.SinglePlayer;
        }
    }
    #endregion

    #region Scene Management
    public void LoadGameplay()
    {
        Debug.Log($"[GameManager] ⚙️ LoadGameplay called!  Mode: {gameMode}");
        Debug.Log($"[GameManager] 📍 Current scene: {SceneManager.GetActiveScene().name}");

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
            Debug.Log("[GameManager] ▶️ Resuming game.. .");
            ChangeState(GameState.Playing);
        }
    }
    #endregion

    #region Gameplay Initialization
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
            Debug.LogError("[GameManager] ❌ FoodSpawner. Instance is NULL!");
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic("gameplay", true);
        }

        ChangeState(GameState.Playing);

        Debug.Log($"[GameManager] ✅ InitializeGameplay DONE!  Spawned {snakes.Count} snake(s)");
    }

    private void SpawnAllSnakes()
    {
        Debug.Log($"[GameManager] 🐍 SpawnAllSnakes START - Mode:  {gameMode}");

        if (playerSnakePrefab == null)
        {
            Debug.LogError("[GameManager] ❌❌❌ playerSnakePrefab is NULL!  Assign it in Inspector!");
            return;
        }

        Vector3 playerSpawnPos = new Vector3(-spawnDistanceFromCenter, 0, 0);
        Color playerColor = LoadSnakeColor(1, Color.green);
        Debug.Log($"[GameManager] 🎨 Player color: #{ColorUtility.ToHtmlStringRGB(playerColor)}");

        string playerName = PlayerNameManager.Instance != null ? PlayerNameManager.Instance.PlayerName : "Người chơi";
        Debug.Log($"[GameManager] 👤 Player name: {playerName}");

        var player = SpawnSnake(
            prefab: playerSnakePrefab,
            id: 1,
            color: playerColor,
            isAI: false,
            name: playerName,
            spawnPos: playerSpawnPos,
            keyUp: KeyCode.W,
            keyDown: KeyCode.S,
            keyLeft: KeyCode.A,
            keyRight: KeyCode.D
        );

        if (player != null)
        {
            snakes.Add(player);
            InitializeComboTracker(1);
            Debug.Log($"[GameManager] ✅ Player spawned successfully at {playerSpawnPos}");
        }
        else
        {
            Debug.LogError("[GameManager] ❌ Failed to spawn Player!");
        }

        if (gameMode == GameMode.VsAI)
        {
            Debug.Log("[GameManager] 🤖 VsAI mode detected, spawning AI.. .");

            GameObject aiBotPrefab = aiSnakePrefab != null ? aiSnakePrefab : playerSnakePrefab;
            Vector3 aiSpawnPos = new Vector3(spawnDistanceFromCenter, 0, 0);
            Debug.Log($"[GameManager] 📍 AI spawn position: {aiSpawnPos}");

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

        Debug.Log($"[GameManager] 🎮 SpawnAllSnakes DONE.  Total snakes: {snakes.Count}");
    }

    public void SaveSnakeColor(int playerID, Color color)
    {
        string key = "PlayerSnakeColor";
        string colorHex = "#" + ColorUtility.ToHtmlStringRGB(color);
        PlayerPrefs.SetString(key, colorHex);
        PlayerPrefs.Save();
    }

    public Color LoadSnakeColor(int playerID, Color defaultColor)
    {
        string key = "PlayerSnakeColor";
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

        Debug.Log($"[GameManager] 🐍 Instantiating {name} at {spawnPos}.. .");
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
    // ✅ YÊU CẦU 4: AI chết → Player thắng
    private void OnSnakeDied(SnakeController snake)
    {
        Debug.Log($"<color=red>💀 {snake.PlayerName} died!  Final Score: {snake.Score}</color>");

        // ✅ KIỂM TRA CHẾ ĐỘ VS AI
        if (gameMode == GameMode.VsAI)
        {
            // Nếu AI chết → Player thắng
            if (snake.IsAIControlled)
            {
                Debug.Log("[GameManager] 🏆 AI died → PLAYER WINS!");

                // Dừng game
                if (FoodSpawner.Instance != null)
                {
                    FoodSpawner.Instance.StopAutoSpawn();
                }

                // Disable tất cả snakes
                foreach (var s in snakes)
                {
                    if (s != null)
                    {
                        s.enabled = false;
                    }
                }

                // ✅ Hiện panel winner với Player là người thắng
                ChangeState(GameState.GameOver);
                return;
            }
            // Nếu Player chết → AI thắng (tiếp tục xử lý bình thường)
            else
            {
                Debug.Log("[GameManager] 💀 Player died → AI WINS!");
            }
        }

        // Stop game
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
    // ✅ YÊU CẦU 4:  Xác định winner đúng trong VS AI
    private void HandleGameOver()
    {
        Debug.Log("[GameManager] 🏁 Game Over!");

        // ✅ Tìm snake thắng cuộc
        SnakeController winner = null;
        int highestScore = 0;

        // Trong VS AI:  Kiểm tra snake nào còn sống
        if (gameMode == GameMode.VsAI)
        {
            foreach (var snake in snakes)
            {
                if (snake != null && !snake.IsDead)
                {
                    winner = snake; // Snake còn sống = thắng
                    highestScore = snake.Score;
                    Debug.Log($"[GameManager] 🏆 Winner:  {snake.PlayerName} (still alive)");
                    break;
                }
            }

            // Nếu cả 2 đều chết (hiếm) → so điểm
            if (winner == null)
            {
                winner = GetTopScoreSnake();
                highestScore = winner != null ? winner.Score : 0;
                Debug.Log($"[GameManager] 🏆 Winner by score: {winner?.PlayerName}");
            }
        }
        else
        {
            // SinglePlayer: chỉ có 1 snake
            winner = GetTopScoreSnake();
            highestScore = winner != null ? winner.Score : 0;
        }

        // Save high score
        if (HighScoreManager.Instance != null && winner != null)
        {
            HighScoreManager.Instance.TryAddHighScore(winner.PlayerName, winner.Score);
        }

        // ✅ Hiện UI Game Over
        if (UIManager.Instance != null)
        {
            bool hasWinner = (gameMode == GameMode.VsAI); // VS AI luôn có winner
            int winnerID = winner != null ? winner.PlayerID : 1;

            Debug.Log($"[GameManager] 📊 ShowGameOver: hasWinner={hasWinner}, winnerID={winnerID}, score={highestScore}");
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