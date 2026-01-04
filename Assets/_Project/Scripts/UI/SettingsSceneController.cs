using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Đảm bảo Settings Panel luôn hiển thị khi vào scene Settings
/// </summary>
public class SettingsSceneController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject settingsPanel;

    private void Awake()
    {
        // ✅ CRITICAL: Đảm bảo panel được bật NGAY LẬP TỨC
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            Debug.Log("[SettingsScene] Settings Panel enabled in Awake");
        }
        else
        {
            Debug.LogError("[SettingsScene] Settings Panel reference is NULL!");
        }
    }

    private void Start()
    {
        // ✅ Double-check lần nữa trong Start
        if (settingsPanel != null && !settingsPanel.activeSelf)
        {
            settingsPanel.SetActive(true);
            Debug.LogWarning("[SettingsScene] Settings Panel was disabled, re-enabling...");
        }

        // Play menu music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic("menu", true);
        }

        Debug.Log("[SettingsScene] Scene initialized successfully");
    }
    private void Update()
    {
        // ✅ Force panel luôn active mỗi frame (temporary debug)
        if (settingsPanel != null && !settingsPanel.activeSelf)
        {
            Debug.LogWarning("[SettingsScene] Panel was disabled! Re-enabling...");
            settingsPanel.SetActive(true);
        }
    }

    private void OnEnable()
    {
        // ✅ Đăng ký event để catch khi scene được load
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ✅ Đảm bảo panel bật khi scene load
        if (scene.name == "Settings" && settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            Debug.Log("[SettingsScene] Panel enabled after scene load");
        }
    }

    /// <summary>
    /// Gọi từ button "Quay lại"
    /// </summary>
    public void OnBackToMainMenu()
    {
        Debug.Log("[SettingsScene] Returning to Main Menu");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

        SceneManager.LoadScene("MainMenu");
    }
}