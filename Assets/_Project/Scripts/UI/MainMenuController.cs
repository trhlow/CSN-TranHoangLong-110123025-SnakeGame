using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject highScorePanel;

    private void Start()
    {
        Debug.Log("[MainMenu] 🎬 MainMenuManager started");

        ShowMainMenu();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic("menu", true);
        }
    }

    #region Panel Management
    private void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (highScorePanel != null) highScorePanel.SetActive(false);
    }

    private void ShowSettings()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (highScorePanel != null) highScorePanel.SetActive(false);
    }

    private void ShowHighScores()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (highScorePanel != null) highScorePanel.SetActive(true);
    }
    #endregion

    #region Button Handlers - Game Modes
    public void OnSinglePlayerClicked()
    {
        Debug.Log("[MainMenu] 🎮 Single Player clicked");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

        if (GameManager.Instance != null)
        {
            Debug.Log("[MainMenu] ✅ GameManager found, setting mode to SinglePlayer");
            GameManager.Instance.SetGameMode(GameManager.GameMode.SinglePlayer);
            GameManager.Instance.LoadGameplay();
        }
        else
        {
            Debug.LogError("[MainMenu] ❌ GameManager. Instance is NULL!");
            SceneManager.LoadScene("Gameplay");
        }
    }

    public void OnVsAIClicked()
    {
        Debug.Log("[MainMenu] 🤖 VS AI clicked");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

        if (GameManager.Instance != null)
        {
            Debug.Log("[MainMenu] ✅ GameManager found, setting mode to VsAI");
            GameManager.Instance.SetGameMode(GameManager.GameMode.VsAI);
            GameManager.Instance.LoadGameplay();
        }
        else
        {
            Debug.LogError("[MainMenu] ❌ GameManager. Instance is NULL!");
            SceneManager.LoadScene("Gameplay");
        }
    }
    #endregion

    #region Button Handlers - Menu Navigation
    public void OnSettingsClicked()
    {
        Debug.Log("[MainMenu] Opening Settings scene...");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("Settings");
    }

    public void OnHighScoresClicked()
    {
        Debug.Log("[MainMenu] 🏆 High Scores clicked");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

        if (highScorePanel != null)
        {
            ShowHighScores();

            HighScoreUI highScoreUI = highScorePanel.GetComponent<HighScoreUI>();
            if (highScoreUI != null)
            {
                highScoreUI.RefreshDisplay();
            }
        }
        else
        {
            Debug.LogWarning("[MainMenu] ⚠️ High Score Panel not assigned!");
        }
    }

    public void OnBackToMainMenu()
    {
        Debug.Log("[MainMenu] 🔙 Back to main menu");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

        ShowMainMenu();
    }
    #endregion

    #region Button Handlers - Quit
    public void OnQuitClicked()
    {
        Debug.Log("[MainMenu] 🚪 Quit clicked");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #endregion
}