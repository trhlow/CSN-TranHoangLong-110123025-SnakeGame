using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [Header("Main Panels")]
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("HUD Elements")]
    [SerializeField] private GameObject player1HUD;
    [SerializeField] private GameObject player2HUD;
    [SerializeField] private TMP_Text player1ScoreText;
    [SerializeField] private TMP_Text player2ScoreText;
    [SerializeField] private TMP_Text player1ComboText;
    [SerializeField] private TMP_Text player2ComboText;
    [SerializeField] private TMP_Text timerText;

    [Header("Game Over")]
    [SerializeField] private TMP_Text gameOverTitleText;
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text newHighScoreText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Pause Menu")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseSettingsButton;
    [SerializeField] private Button pauseMainMenuButton;

    [Header("Animation")]
    [SerializeField] private float panelFadeSpeed = 0.3f;
    [SerializeField] private float scoreUpdateSpeed = 0.5f;

    private int lastPlayer1Score = 0;
    private int lastPlayer2Score = 0;
    private Coroutine scoreAnimationCoroutine;

    protected override void Awake()
    {
        base.Awake();
        InitializeUI();
        SetupButtonListeners();
    }

    private void Start()
    {
        ShowGameplayUI();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged.AddListener(OnGameStateChanged);
        }
    }

    private void InitializeUI()
    {
        HideAllPanels();

        if (GameManager.Instance != null)
        {
            bool isMultiplayer = GameManager.Instance.CurrentGameMode != GameManager.GameMode.SinglePlayer;

            if (player1HUD != null) player1HUD.SetActive(true);
            if (player2HUD != null) player2HUD.SetActive(isMultiplayer);
        }

        UpdateScore(1, 0);
        UpdateScore(2, 0);

        if (newHighScoreText != null)
        {
            newHighScoreText.gameObject.SetActive(false);
        }
    }

    private void SetupButtonListeners()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartButtonClicked);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeButtonClicked);

        if (pauseSettingsButton != null)
            pauseSettingsButton.onClick.AddListener(OnPauseSettingsClicked);

        if (pauseMainMenuButton != null)
            pauseMainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
    }

    private void HideAllPanels()
    {
        if (gameplayPanel != null) gameplayPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void ShowGameplayUI()
    {
        HideAllPanels();
        if (gameplayPanel != null)
        {
            gameplayPanel.SetActive(true);
        }
    }

    public void ShowPauseMenu()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("PanelOpen");
        }
    }

    public void HidePauseMenu()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("PanelClose");
        }
    }

    public void ShowGameOver(bool hasWinner, int finalScore, int winnerID)
    {
        StartCoroutine(ShowGameOverCoroutine(hasWinner, finalScore, winnerID));
    }

    private IEnumerator ShowGameOverCoroutine(bool hasWinner, int finalScore, int winnerID)
    {
        yield return new WaitForSecondsRealtime(1f);

        if (gameplayPanel != null)
        {
            gameplayPanel.SetActive(false);
        }

        yield return new WaitForSecondsRealtime(0.5f);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (gameOverTitleText != null)
        {
            string titleKey = hasWinner ? "ui.  game_over.victory" : "ui. game_over.defeat";
            gameOverTitleText.text = LocalizationManager.Instance != null ?
                LocalizationManager.Instance.GetLocalizedString(titleKey) :
                (hasWinner ? "VICTORY!" : "GAME OVER!");
        }

        if (winnerText != null)
        {
            if (hasWinner)
            {
                string playerKey = $"ui.game_over.player{winnerID}_wins";
                winnerText.text = LocalizationManager.Instance != null ?
                    LocalizationManager.Instance.GetLocalizedString(playerKey) :
                    $"Player {winnerID} Wins!";
            }
            else
            {
                winnerText.text = "";
            }
        }

        if (finalScoreText != null)
        {
            yield return StartCoroutine(AnimateScoreText(finalScoreText, 0, finalScore));
        }

        bool isNewHighScore = false;
        if (HighScoreManager.Instance != null)
        {
            isNewHighScore = HighScoreManager.Instance.IsHighScore(finalScore);

            if (isNewHighScore && newHighScoreText != null)
            {
                newHighScoreText.gameObject.SetActive(true);
                StartCoroutine(FlashText(newHighScoreText, 0.5f));
            }
        }
    }

    private IEnumerator AnimateScoreText(TMP_Text text, int fromScore, int toScore)
    {
        float elapsed = 0f;
        float duration = 1.5f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            int currentScore = Mathf.RoundToInt(Mathf.Lerp(fromScore, toScore, Mathf.SmoothStep(0, 1, t)));

            string scoreKey = "ui.game_over.final_score";
            string scoreLabel = LocalizationManager.Instance != null ?
                LocalizationManager.Instance.GetLocalizedString(scoreKey) :
                "Final Score: ";

            text.text = $"{scoreLabel}\n<size=80>{currentScore: N0}</size>";
            yield return null;
        }

        text.text = $"Final Score:\n<size=80>{toScore:N0}</size>";
    }

    private IEnumerator FlashText(TMP_Text text, float interval)
    {
        Color originalColor = text.color;

        for (int i = 0; i < 6; i++)
        {
            text.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
            yield return new WaitForSecondsRealtime(interval);
            text.color = originalColor;
            yield return new WaitForSecondsRealtime(interval);
        }
    }

    public void ShowSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void HideSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void UpdateScore(int playerID, int score)
    {
        TMP_Text scoreText = playerID == 1 ? player1ScoreText : player2ScoreText;
        int lastScore = playerID == 1 ? lastPlayer1Score : lastPlayer2Score;

        if (scoreText != null)
        {
            if (scoreAnimationCoroutine != null)
            {
                StopCoroutine(scoreAnimationCoroutine);
            }
            scoreAnimationCoroutine = StartCoroutine(AnimateScoreUpdate(scoreText, lastScore, score));
        }

        if (playerID == 1)
            lastPlayer1Score = score;
        else
            lastPlayer2Score = score;
    }

    private IEnumerator AnimateScoreUpdate(TMP_Text scoreText, int fromScore, int toScore)
    {
        float elapsed = 0f;

        while (elapsed < scoreUpdateSpeed)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / scoreUpdateSpeed;
            int currentScore = Mathf.RoundToInt(Mathf.Lerp(fromScore, toScore, t));
            scoreText.text = currentScore.ToString("N0");
            yield return null;
        }

        scoreText.text = toScore.ToString("N0");

        StartCoroutine(PulseText(scoreText, 1.2f, 0.2f));
    }

    private IEnumerator PulseText(TMP_Text text, float scale, float duration)
    {
        if (text == null) yield break;

        Vector3 originalScale = text.transform.localScale;
        Vector3 targetScale = originalScale * scale;

        float elapsed = 0f;

        while (elapsed < duration / 2f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / (duration / 2f);
            text.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        elapsed = 0f;

        while (elapsed < duration / 2f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / (duration / 2f);
            text.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        text.transform.localScale = originalScale;
    }

    public void UpdateCombo(int playerID, int comboCount)
    {
        TMP_Text comboText = playerID == 1 ? player1ComboText : player2ComboText;

        if (comboText != null)
        {
            if (comboCount > 1)
            {
                comboText.text = $"COMBO x{comboCount}!  ";
                comboText.gameObject.SetActive(true);
                StartCoroutine(PulseText(comboText, 1.5f, 0.3f));

                StartCoroutine(HideComboAfterDelay(comboText, 2f));
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator HideComboAfterDelay(TMP_Text comboText, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (comboText != null)
        {
            comboText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPlaying)
        {
            UpdateTimer();
        }
    }

    private void UpdateTimer()
    {
        if (timerText != null && GameManager.Instance != null)
        {
            float gameTime = GameManager.Instance.GameTime;
            int minutes = Mathf.FloorToInt(gameTime / 60f);
            int seconds = Mathf.FloorToInt(gameTime % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    public void OnRestartButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    public void OnMainMenuButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMainMenu();
        }
    }

    public void OnResumeButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }

        HidePauseMenu();
    }

    public void OnPauseSettingsClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

        HidePauseMenu();
        ShowSettings();
    }

    private void OnGameStateChanged(GameManager.GameState newState)
    {
        switch (newState)
        {
            case GameManager.GameState.Playing:
                ShowGameplayUI();
                HidePauseMenu();
                break;

            case GameManager.GameState.Paused:
                ShowPauseMenu();
                break;

            case GameManager.GameState.GameOver:
                break;
        }
    }
    public class MainMenuController : MonoBehaviour
    {
        public void OnSinglePlayerClicked()
        {
            GameManager.Instance.SetGameMode(GameManager.GameMode.SinglePlayer);
            GameManager.Instance.LoadGameplay();
        }

        public void OnMultiplayerClicked()
        {
            GameManager.Instance.SetGameMode(GameManager.GameMode.Multiplayer);
            GameManager.Instance.LoadGameplay();
        }

        public void OnVsAIClicked()
        {
            GameManager.Instance.SetGameMode(GameManager.GameMode.VsAI);
            GameManager.Instance.LoadGameplay();
        }
    }
}