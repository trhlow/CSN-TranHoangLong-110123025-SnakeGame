using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    private void Start()
    {
        // Play menu music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic("menu", true);
        }
    }

    public void OnSinglePlayerClicked()
    {
        Debug.Log("[MainMenu] Single Player");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameMode(GameManager.GameMode.SinglePlayer);
            GameManager.Instance.LoadGameplay();
        }
        else
        {
            SceneManager.LoadScene("Gameplay");
        }
    }

    public void OnMultiplayerClicked()
    {
        Debug.Log("[MainMenu] Multiplayer");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameMode(GameManager.GameMode.Multiplayer);
            GameManager.Instance.LoadGameplay();
        }
        else
        {
            SceneManager.LoadScene("Gameplay");
        }
    }

    public void OnVsAIClicked()
    {
        Debug.Log("[MainMenu] VS AI");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameMode(GameManager.GameMode.VsAI);
            GameManager.Instance.LoadGameplay();
        }
        else
        {
            SceneManager.LoadScene("Gameplay");
        }
    }

    public void OnHighScoresClicked()
    {
        Debug.Log("[MainMenu] High Scores - Chưa implement");
        // TODO: Mở High Scores panel
    }

    public void OnSettingsClicked()
    {
        Debug.Log("[MainMenu] Settings - Chưa implement");
        // TODO: Mở Settings panel
    }

    public void OnQuitClicked()
    {
        Debug.Log("[MainMenu] Quit");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}