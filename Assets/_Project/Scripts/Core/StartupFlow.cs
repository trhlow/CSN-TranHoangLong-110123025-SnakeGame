using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script này chạy khi game khởi động để kiểm tra xem người chơi đã đặt tên chưa.
/// Nếu chưa, chuyển đến scene PlayerNameInput.
/// </summary>
public class StartupFlow : MonoBehaviour
{
    [SerializeField] private string playerNameScene = "PlayerNameInput";
    [SerializeField] private string mainMenuScene = "MainMenu";

    private void Start()
    {
        CheckPlayerName();
    }

    private void CheckPlayerName()
    {
        // Kiểm tra nếu chưa có tên người chơi
        if (PlayerNameManager.Instance != null && !PlayerNameManager.Instance.HasPlayerName())
        {
            Debug.Log("[StartupFlow] No player name found, redirecting to PlayerNameInput scene");
            SceneManager.LoadScene(playerNameScene);
        }
        else
        {
            Debug.Log($"[StartupFlow] Player name found: {PlayerNameManager.Instance.GetPlayerName()}, continuing to MainMenu");
            // Nếu đã có tên, tiếp tục vào MainMenu
            if (SceneManager.GetActiveScene().name != mainMenuScene)
            {
                SceneManager.LoadScene(mainMenuScene);
            }
        }
    }
}
