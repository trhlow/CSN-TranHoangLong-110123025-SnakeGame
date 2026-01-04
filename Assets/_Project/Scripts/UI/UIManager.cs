using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject highScorePanel;

    [Header("HUD Elements")]
    [SerializeField] private GameObject playerHUD;
    [SerializeField] private TMP_Text playerScoreText;
    [SerializeField] private TMP_Text playerComboText;
    [SerializeField] private TMP_Text playerNameText;

    [Header("Gameplay UI - Timer & Combo")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text comboText;
    [SerializeField] private GameObject comboPanel;
    [SerializeField] private float comboDisplayTime = 2f;

    [Header("Game Over UI")]
    [SerializeField] private TMP_Text gameOverTitleText;
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text newHighScoreText;

    [Header("Pause UI")]
    [SerializeField] private TMP_Text pauseTitleText;

    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float scoreAnimationDuration = 1.5f;

    private Coroutine comboCoroutine;
    private Coroutine scoreAnimationCoroutine;
    private int currentCombo = 0;
    private int lastPlayerScore = 0;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        InitializeUI();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPlaying)
        {
            UpdateTimer();
        }
    }

    private void InitializeUI()
    {
        // Hide all panels initially
        HideAllPanels();

        // Show appropriate panel based on scene
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenu")
        {
            ShowMainMenu();
        }
        else if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Gameplay")
        {
            ShowGameplay();
        }

        // Initialize combo panel
        if (comboPanel != null)
        {
            comboPanel.SetActive(false);
        }

        // Initialize new high score text
        if (newHighScoreText != null)
        {
            newHighScoreText.gameObject.SetActive(false);
        }
    }

    #region Panel Management
    private void HideAllPanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gameplayPanel != null) gameplayPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (highScorePanel != null) highScorePanel.SetActive(false);
    }

    public void ShowMainMenu()
    {
        HideAllPanels();
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }

    public void ShowGameplay()
    {
        HideAllPanels();
        if (gameplayPanel != null)
        {
            gameplayPanel.SetActive(true);
        }

        // Initialize player name
        UpdatePlayerName();
    }

    public void ShowPauseMenu()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);

            if (pauseTitleText != null && LocalizationManager.Instance != null)
            {
                pauseTitleText.text = LocalizationManager.Instance.GetLocalizedString("pause. title");
            }
        }
    }

    public void HidePauseMenu()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
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

    public void ShowHighScores()
    {
        if (highScorePanel != null)
        {
            highScorePanel.SetActive(true);
        }
    }

    public void HideHighScores()
    {
        if (highScorePanel != null)
        {
            highScorePanel.SetActive(false);
        }
    }
    #endregion

    #region Score Display
    public void UpdateScore(int playerID, int score)
    {
        if (playerScoreText != null)
        {
            if (scoreAnimationCoroutine != null)
            {
                StopCoroutine(scoreAnimationCoroutine);
            }
            scoreAnimationCoroutine = StartCoroutine(AnimateScoreUpdate(playerScoreText, lastPlayerScore, score));
        }

        lastPlayerScore = score;
    }

    private IEnumerator AnimateScoreUpdate(TMP_Text scoreText, int fromScore, int toScore)
    {
        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
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
            elapsed += Time.deltaTime;
            float t = elapsed / (duration / 2f);
            text.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        elapsed = 0f;

        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration / 2f);
            text.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        text.transform.localScale = originalScale;
    }

    private void UpdatePlayerName()
    {
        if (playerNameText != null)
        {
            string playerName = PlayerNameManager.Instance != null ?
                PlayerNameManager.Instance.GetPlayerName() : "Người chơi";
            playerNameText.text = playerName;
        }
    }
    #endregion

    #region Timer Display
    private void UpdateTimer()
    {
        if (timerText != null && GameManager.Instance != null)
        {
            float gameTime = GameManager.Instance.GameTime;
            int minutes = Mathf.FloorToInt(gameTime / 60f);
            int seconds = Mathf.FloorToInt(gameTime % 60f);
            timerText.text = $"{minutes:00}:{seconds: 00}";
        }
    }
    #endregion

    #region Combo Display
    public void ShowCombo(int comboCount)
    {
        currentCombo = comboCount;

        if (comboCount < 2)
        {
            if (comboPanel != null)
            {
                comboPanel.SetActive(false);
            }
            return;
        }

        if (comboPanel != null)
        {
            comboPanel.SetActive(true);
        }

        if (comboText != null)
        {
            string comboLabel = LocalizationManager.Instance != null ?
                LocalizationManager.Instance.GetLocalizedString("game.combo") : "Combo";
            comboText.text = $"{comboLabel} x{comboCount}! ";
        }

        if (comboCoroutine != null)
        {
            StopCoroutine(comboCoroutine);
        }
        comboCoroutine = StartCoroutine(HideComboAfterDelay());
    }

    private IEnumerator HideComboAfterDelay()
    {
        yield return new WaitForSeconds(comboDisplayTime);

        if (comboPanel != null)
        {
            comboPanel.SetActive(false);
        }
        currentCombo = 0;
    }

    public void ResetCombo()
    {
        currentCombo = 0;
        if (comboPanel != null)
        {
            comboPanel.SetActive(false);
        }
        if (comboCoroutine != null)
        {
            StopCoroutine(comboCoroutine);
        }
    }
    #endregion

    #region Game Over
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
            string titleKey = hasWinner ? "ui.game_over.victory" : "ui.game_over.defeat";
            gameOverTitleText.text = LocalizationManager.Instance != null ?
                LocalizationManager.Instance.GetLocalizedString(titleKey) :
                (hasWinner ? "CHIẾN THẮNG!" : "THUA CUỘC!");
        }

        if (winnerText != null)
        {
            if (hasWinner)
            {
                string winnerName = GetPlayerNameByID(winnerID);
                string winsLabel = LocalizationManager.Instance != null ?
                    LocalizationManager.Instance.GetLocalizedString("ui.game_over.wins") : "Thắng! ";
                winnerText.text = $"{winnerName} {winsLabel}";
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

        while (elapsed < scoreAnimationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / scoreAnimationDuration;
            int currentScore = Mathf.RoundToInt(Mathf.Lerp(fromScore, toScore, Mathf.SmoothStep(0, 1, t)));

            string scoreLabel = LocalizationManager.Instance != null ?
                LocalizationManager.Instance.GetLocalizedString("ui.game_over.final_score") :
                "Điểm Cuối";

            text.text = $"{scoreLabel}:  {currentScore}";
            yield return null;
        }

        string finalLabel = LocalizationManager.Instance != null ?
            LocalizationManager.Instance.GetLocalizedString("ui.game_over.final_score") :
            "Điểm Cuối";
        text.text = $"{finalLabel}: {toScore}";
    }

    private IEnumerator FlashText(TMP_Text text, float interval)
    {
        while (text.gameObject.activeSelf)
        {
            text.enabled = !text.enabled;
            yield return new WaitForSecondsRealtime(interval);
        }
        text.enabled = true;
    }
    #endregion

    #region Button Handlers
    public void OnResumeClicked()
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

    public void OnRestartClicked()
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

    public void OnMainMenuClicked()
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

    public void OnQuitClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

        Debug.Log("[UIManager] Quit Game");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #endregion

    #region Utility
    public void ShowNotification(string message, float duration = 2f)
    {
        Debug.Log($"[Notification] {message}");
    }

    private string GetPlayerNameByID(int playerID)
    {
        // Nếu là AI (ID = 3)
        if (playerID == 3)
        {
            return LocalizationManager.Instance != null ?
                LocalizationManager.Instance.GetLocalizedString("game.ai") : "Máy";
        }

        // Lấy tên từ GameManager nếu có
        if (GameManager.Instance != null)
        {
            var snake = GameManager.Instance.GetSnakeByID(playerID);
            if (snake != null)
            {
                return snake.PlayerName;
            }
        }

        // Fallback:  lấy từ PlayerNameManager
        if (PlayerNameManager.Instance != null)
        {
            return PlayerNameManager.Instance.GetPlayerName();
        }

        return "Người chơi";
    }
    #endregion
}