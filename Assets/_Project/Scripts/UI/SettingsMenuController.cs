using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TMP_Text musicVolumeText;
    [SerializeField] private TMP_Text sfxVolumeText;

    [Header("Player Name Settings")]
    [SerializeField] private TMP_InputField playerNameInputField;
    [SerializeField] private Button saveNameButton;
    [SerializeField] private TMP_Text currentNameText;

    [Header("Language Settings")]
    [SerializeField] private Button vietnameseButton;
    [SerializeField] private Button englishButton;

    [Header("Buttons")]
    [SerializeField] private Button backButton;

    [Header("Player Color Buttons")]
    [SerializeField] private Button colorGreenButton;
    [SerializeField] private Button colorRedButton;
    [SerializeField] private Button colorBlueButton;
    [SerializeField] private Button colorYellowButton;
    [SerializeField] private Button colorPurpleButton;
    [SerializeField] private Button colorOrangeButton;
    [SerializeField] private Button colorCyanButton;
    [SerializeField] private Button colorPinkButton;
    [SerializeField] private Button colorWhiteButton;
    [SerializeField] private Button colorBlackButton;

    [Header("Visual Feedback")]
    [SerializeField] private Image playerColorPreview;
    [SerializeField] private TMP_Text selectedLanguageText;

    private Color playerColor;

    private void Awake()
    {
        Debug.Log("[SettingsMenuController] Awake called");
    }

    private void Start()
    {
        Debug.Log("[SettingsMenuController] Start called");
        LoadSettings();
        InitializeSettings();
        SetupListeners();
        UpdateVisuals();
        Debug.Log("[SettingsMenuController] Initialization complete");
    }

    private void OnEnable()
    {
        Debug.Log("[SettingsMenuController] OnEnable called");
    }

    private void OnDisable()
    {
        Debug.Log("[SettingsMenuController] OnDisable called");
    }

    private void LoadSettings()
    {
        // Load Player color
        float r = PlayerPrefs.GetFloat("PlayerColorR", 0f);
        float g = PlayerPrefs.GetFloat("PlayerColorG", 1f); // Default Green
        float b = PlayerPrefs.GetFloat("PlayerColorB", 0f);
        playerColor = new Color(r, g, b, 1f);
    }

    private void InitializeSettings()
    {
        // Audio
        if (AudioManager.Instance != null)
        {
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = AudioManager.Instance.MusicVolume;
                UpdateMusicVolumeText(AudioManager.Instance.MusicVolume);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = AudioManager.Instance.SFXVolume;
                UpdateSFXVolumeText(AudioManager.Instance.SFXVolume);
            }
        }

        // ✅ NEW: Load player name
        if (PlayerNameManager.Instance != null)
        {
            string currentName = PlayerNameManager.Instance.PlayerName;
            if (playerNameInputField != null)
            {
                playerNameInputField.text = currentName;
            }
            if (currentNameText != null)
            {
                currentNameText.text = $"Tên hiện tại: {currentName}";
            }
        }
    }

    private void SetupListeners()
    {
        // Audio sliders
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        // ✅ NEW: Player name input
        if (playerNameInputField != null)
        {
            playerNameInputField.onEndEdit.AddListener(OnPlayerNameChanged);
        }

        if (saveNameButton != null)
        {
            saveNameButton.onClick.AddListener(OnSaveNameClicked);
        }

        // Language buttons
        if (vietnameseButton != null)
            vietnameseButton.onClick.AddListener(() => SetLanguage(LocalizationManager.Language.Vietnamese));

        if (englishButton != null)
            englishButton.onClick.AddListener(() => SetLanguage(LocalizationManager.Language.English));

        // Back button
        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);

        // Player color buttons
        SetupColorButton(colorGreenButton, Color.green, "Green");
        SetupColorButton(colorRedButton, Color.red, "Red");
        SetupColorButton(colorBlueButton, Color.blue, "Blue");
        SetupColorButton(colorYellowButton, Color.yellow, "Yellow");
        SetupColorButton(colorPurpleButton, new Color(0.5f, 0f, 1f), "Purple");
        SetupColorButton(colorOrangeButton, new Color(1f, 0.5f, 0f), "Orange");
        SetupColorButton(colorCyanButton, Color.cyan, "Cyan");
        SetupColorButton(colorPinkButton, new Color(1f, 0.4f, 0.7f), "Pink");
        SetupColorButton(colorWhiteButton, Color.white, "White");
        SetupColorButton(colorBlackButton, new Color(0.2f, 0.2f, 0.2f), "Black");
    }

    private void SetupColorButton(Button button, Color color, string colorName)
    {
        if (button == null) return;

        button.onClick.AddListener(() => SetPlayerColor(color, colorName));

        // Set button color
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = color;
        }
    }

    #region Audio Settings
    private void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }

        UpdateMusicVolumeText(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

        UpdateSFXVolumeText(value);
    }

    private void UpdateMusicVolumeText(float value)
    {
        if (musicVolumeText != null)
        {
            musicVolumeText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }
    }

    private void UpdateSFXVolumeText(float value)
    {
        if (sfxVolumeText != null)
        {
            sfxVolumeText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }
    }
    #endregion

    #region Player Name Settings
    private void OnPlayerNameChanged(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            Debug.LogWarning("[SettingsMenu] Tên không được để trống!");
            if (PlayerNameManager.Instance != null)
            {
                playerNameInputField.text = PlayerNameManager.Instance.PlayerName;
            }
            return;
        }

        // Giới hạn độ dài tên
        if (newName.Length > 15)
        {
            newName = newName.Substring(0, 15);
            playerNameInputField.text = newName;
        }

        SavePlayerName(newName);
    }

    private void OnSaveNameClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

        string newName = playerNameInputField.text.Trim();
        if (!string.IsNullOrWhiteSpace(newName))
        {
            SavePlayerName(newName);
            Debug.Log($"[SettingsMenu] ✅ Đã lưu tên: {newName}");
        }
    }

    private void SavePlayerName(string newName)
    {
        if (PlayerNameManager.Instance != null)
        {
            PlayerNameManager.Instance.SetPlayerName(newName);

            if (currentNameText != null)
            {
                currentNameText.text = $"Tên hiện tại: {newName}";
            }

            Debug.Log($"[SettingsMenu] 👤 Đã đổi tên thành: {newName}");
        }
    }
    #endregion

    #region Language Settings
    private void SetLanguage(LocalizationManager.Language language)
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.SetLanguage(language);
            Debug.Log($"[Settings] Language changed to: {language}");
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

        UpdateVisuals();
    }
    #endregion

    #region Color Settings
    private void SetPlayerColor(Color color, string colorName)
    {
        playerColor = color;

        // Save Player color
        PlayerPrefs.SetFloat("PlayerColorR", color.r);
        PlayerPrefs.SetFloat("PlayerColorG", color.g);
        PlayerPrefs.SetFloat("PlayerColorB", color.b);
        PlayerPrefs.Save();

        Debug.Log($"[Settings] Player color:  {colorName}");

        // Play sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

        // Update ColorPalette if exists
        if (ColorPalette.Instance != null)
        {
            ColorPalette.Instance.playerPrimary = color;
        }

        UpdateVisuals();
    }
    #endregion

    #region Visual Updates
    private void UpdateVisuals()
    {
        // Update color preview
        if (playerColorPreview != null)
        {
            playerColorPreview.color = playerColor;
        }

        // Update language text
        if (selectedLanguageText != null && LocalizationManager.Instance != null)
        {
            string langName = LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese
                ? "Tiếng Việt" : "English";
            selectedLanguageText.text = langName;
        }
    }
    #endregion

    #region Navigation
    private void OnBackClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMainMenu();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
    #endregion

    #region Utility
    public void ResetToDefaults()
    {
        // Reset audio
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(0.7f);
            AudioManager.Instance.SetSFXVolume(1f);
        }

        // Reset language
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.SetLanguage(LocalizationManager.Language.Vietnamese);
        }

        // Reset color
        SetPlayerColor(Color.green, "Green");

        // Reset player name
        if (PlayerNameManager.Instance != null)
        {
            PlayerNameManager.Instance.ResetToDefault();
        }

        InitializeSettings();
        UpdateVisuals();

        Debug.Log("[Settings] Reset to defaults");
    }
    #endregion
}