using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { MainMenu, Playing, Paused, GameOver, Loading }
    public enum GameMode { SinglePlayer, Multiplayer, VsAI }

    [Header("Snake Prefabs")]
    [SerializeField] private GameObject playerSnakePrefab;
    [SerializeField] private GameObject aiSnakePrefab;

    [Header("Game Settings")]
    [SerializeField] private GameMode gameMode = GameMode.SinglePlayer;
    [SerializeField] private int targetScore = 500;
    [SerializeField] private float gameTimeLimit = 300f;
    [SerializeField] private bool hasTimeLimit = false;

    public UnityEvent<GameState> OnGameStateChanged = new UnityEvent<GameState>();
    private List<SnakeController> snakes = new List<SnakeController>();
    private GameState currentState = GameState.MainMenu;
    private float gameTime = 0f;

    public GameMode CurrentGameMode => gameMode;
    public float GameTime => gameTime;
    public bool IsPlaying => currentState == GameState.Playing;
    public List<SnakeController> GetAllSnakes() => new List<SnakeController>(snakes);

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Đọc GameMode lưu ở MainMenu nếu có
        if (PlayerPrefs.HasKey("GameMode"))
        {
            gameMode = (GameMode)PlayerPrefs.GetInt("GameMode");
        }
    }

    private void Start()
    {
        SpawnAllSnakes();
        ChangeState(GameState.Playing);
        gameTime = 0f;
    }

    private void Update()
    {
        if (currentState != GameState.Playing) return;

        gameTime += Time.deltaTime;

        if (hasTimeLimit && gameTime >= gameTimeLimit)
            EndGameTimeLimit();

        if (targetScore > 0)
            CheckTargetScore();

        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    // --- UI/Menu CALL THESE ---
    public void SetGameMode(GameMode mode)
    {
        gameMode = mode;
        PlayerPrefs.SetInt("GameMode", (int)mode);
        PlayerPrefs.Save();
    }

    public void SetGameMode(int modeInt)
    {
        gameMode = (GameMode)modeInt;
        PlayerPrefs.SetInt("GameMode", modeInt);
        PlayerPrefs.Save();
    }

    public void LoadGameplay()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Gameplay");
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
            ChangeState(GameState.Playing);
    }

    public void ChangeState(GameState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        OnGameStateChanged?.Invoke(newState);
        Debug.Log($"Game state changed to: {newState}");
    }

    public void TogglePause()
    {
        if (currentState == GameState.Playing)
            ChangeState(GameState.Paused);
        else if (currentState == GameState.Paused)
            ChangeState(GameState.Playing);
    }
    // --- End UI/Menu ---

    // --- SNAKE SPAWN ---
    private void SpawnAllSnakes()
    {
        ClearSnakes();

        var snake1 = SpawnSnake(playerSnakePrefab, 1, Color.green, false, "Player 1");
        snake1.OnSnakeDied += OnSnakeDied;
        snakes.Add(snake1);

        if (gameMode == GameMode.Multiplayer)
        {
            var snake2 = SpawnSnake(playerSnakePrefab, 2, Color.magenta, false, "Player 2", KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow);
            snake2.OnSnakeDied += OnSnakeDied;
            snakes.Add(snake2);
        }
        else if (gameMode == GameMode.VsAI)
        {
            var ai = SpawnSnake(aiSnakePrefab != null ? aiSnakePrefab : playerSnakePrefab, 3, Color.cyan, true, "AI Bot");
            ai.OnSnakeDied += OnSnakeDied;
            snakes.Add(ai);
        }
    }

    private SnakeController SpawnSnake(GameObject prefab, int id, Color color, bool isAI, string name,
        KeyCode keyUp = KeyCode.W, KeyCode keyDown = KeyCode.S, KeyCode keyLeft = KeyCode.A, KeyCode keyRight = KeyCode.D)
    {
        Vector3 pos = GetSpawnPos(id);
        var go = Instantiate(prefab, pos, Quaternion.identity);
        var snake = go.GetComponent<SnakeController>();
        snake.Setup(id, color, isAI, name, keyUp, keyDown, keyLeft, keyRight);
        return snake;
    }

    private Vector3 GetSpawnPos(int id)
    {
        switch (id)
        {
            case 1: return new Vector3(-5, 0, 0);
            case 2: return new Vector3(5, 0, 0);
            case 3: return new Vector3(0, 5, 0);
            default: return Vector3.zero;
        }
    }

    private void ClearSnakes()
    {
        foreach (var snake in snakes)
        {
            if (snake != null)
            {
                snake.OnSnakeDied -= OnSnakeDied;
                Destroy(snake.gameObject);
            }
        }
        snakes.Clear();
    }

    private void OnSnakeDied(SnakeController snake)
    {
        Debug.Log($"Snake {snake.PlayerID} ({snake.PlayerName}) died!");
        // Xử lý endgame/update UI tại đây
    }

    public SnakeController GetSnakeByID(int id)
    {
        return snakes.Find(s => s.PlayerID == id);
    }

    public int GetHighestScore()
    {
        int max = 0;
        foreach (var snake in snakes)
            max = Mathf.Max(max, snake.Score);
        return max;
    }

    // --- END SNAKE SPAWN ---

    // --- GAME END LOGIC ---
    private void EndGameTimeLimit()
    {
        Debug.Log("End game by time limit!");
        int maxScore = 0;
        SnakeController winner = null;
        foreach (var snake in snakes)
        {
            if (snake.Score > maxScore)
            {
                maxScore = snake.Score;
                winner = snake;
            }
        }
        Debug.Log(winner ? $"Winner: Player {winner.PlayerID}" : "Draw!");
        ChangeState(GameState.GameOver);
    }

    private void CheckTargetScore()
    {
        foreach (var snake in snakes)
        {
            if (snake.Score >= targetScore)
            {
                Debug.Log($"Snake {snake.PlayerID} wins with score {snake.Score}!");
                ChangeState(GameState.GameOver);
                break;
            }
        }
    }
}