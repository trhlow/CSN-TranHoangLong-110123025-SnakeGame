using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsMenuController : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TMP_Text musicVolumeText;
    [SerializeField] private TMP_Text sfxVolumeText;

    [Header("Player Name Settings")]
    [SerializeField] private TMP_Text currentNameText;
    [SerializeField] private Button changeNameButton; // ✅ NÚT ĐỔI TÊN

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

    private void Start()
    {
        LoadSettings();
        InitializeSettings();
        SetupListeners();
        UpdateVisuals();
    }

    private void LoadSettings()
    {
        float r = PlayerPrefs.GetFloat("PlayerColorR", 0f);
        float g = PlayerPrefs.GetFloat("PlayerColorG", 1f);
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

        // ✅ Player Name
        UpdateCurrentNameDisplay();
    }

    private void SetupListeners()
    {
        // Audio
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        // ✅ Change Name Button
        if (changeNameButton != null)
            changeNameButton.onClick.AddListener(OnChangeNameClicked);

        // Language
        if (vietnameseButton != null)
            vietnameseButton.onClick.AddListener(() => SetLanguage(LocalizationManager.Language.Vietnamese));

        if (englishButton != null)
            englishButton.onClick.AddListener(() => SetLanguage(LocalizationManager.Language.English));

        // Back
        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);

        // Colors
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
    private void UpdateCurrentNameDisplay()
    {
        if (currentNameText != null && PlayerNameManager.Instance != null)
        {
            string name = PlayerNameManager.Instance.GetPlayerName();
            currentNameText.text = $"Tên hiện tại: {name}";
        }
    }

    // ✅ NÚT ĐỔI TÊN → LOAD SCENE PlayerNameInput
    private void OnChangeNameClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

        Debug.Log("[Settings] Loading PlayerNameInput scene to change name...");

        // ✅ Đánh dấu là đang đổi tên (không phải lần đầu)
        PlayerPrefs.SetInt("IsChangingName", 1);
        PlayerPrefs.Save();

        // Load scene PlayerNameInput
        SceneManager.LoadScene("PlayerNameInput");
    }
    #endregion

    #region Language Settings
    private void SetLanguage(LocalizationManager.Language language)
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.SetLanguage(language);
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

        PlayerPrefs.SetFloat("PlayerColorR", color.r);
        PlayerPrefs.SetFloat("PlayerColorG", color.g);
        PlayerPrefs.SetFloat("PlayerColorB", color.b);
        PlayerPrefs.Save();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

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
        if (playerColorPreview != null)
        {
            playerColorPreview.color = playerColor;
        }

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
            SceneManager.LoadScene("MainMenu");
        }
    }
    #endregion

    #region Utility
    public void ResetToDefaults()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(0.7f);
            AudioManager.Instance.SetSFXVolume(1f);
        }

        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.SetLanguage(LocalizationManager.Language.Vietnamese);
        }

        SetPlayerColor(Color.green, "Green");

        if (PlayerNameManager.Instance != null)
        {
            PlayerNameManager.Instance.ResetToDefault();
        }

        InitializeSettings();
        UpdateVisuals();
    }
    #endregion
}