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

    [Header("Language Settings")]
    [SerializeField] private Button vietnameseButton;
    [SerializeField] private Button englishButton;

    [Header("Buttons")]
    [SerializeField] private Button backButton;

    [Header("Player 1 Color Buttons")]
    [SerializeField] private Button p1ColorGreenButton;
    [SerializeField] private Button p1ColorRedButton;
    [SerializeField] private Button p1ColorBlueButton;
    [SerializeField] private Button p1ColorYellowButton;
    [SerializeField] private Button p1ColorPurpleButton;
    [SerializeField] private Button p1ColorOrangeButton;
    [SerializeField] private Button p1ColorCyanButton;
    [SerializeField] private Button p1ColorPinkButton;
    [SerializeField] private Button p1ColorWhiteButton;
    [SerializeField] private Button p1ColorBlackButton;

    [Header("Player 2 Color Buttons")]
    [SerializeField] private Button p2ColorGreenButton;
    [SerializeField] private Button p2ColorRedButton;
    [SerializeField] private Button p2ColorBlueButton;
    [SerializeField] private Button p2ColorYellowButton;
    [SerializeField] private Button p2ColorPurpleButton;
    [SerializeField] private Button p2ColorOrangeButton;
    [SerializeField] private Button p2ColorCyanButton;
    [SerializeField] private Button p2ColorPinkButton;
    [SerializeField] private Button p2ColorWhiteButton;
    [SerializeField] private Button p2ColorBlackButton;

    [Header("Visual Feedback")]
    [SerializeField] private Image player1ColorPreview;
    [SerializeField] private Image player2ColorPreview;
    [SerializeField] private TMP_Text selectedLanguageText;

    private Color player1Color;
    private Color player2Color;
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
        Debug.Log("[SettingsMenuController] OnDisable called - SOMETHING DISABLED THIS!");
    }
    private void LoadSettings()
    {
        // Load Player 1 color
        float r1 = PlayerPrefs.GetFloat("Player1ColorR", 0f);
        float g1 = PlayerPrefs.GetFloat("Player1ColorG", 1f); // Default Green
        float b1 = PlayerPrefs.GetFloat("Player1ColorB", 0f);
        player1Color = new Color(r1, g1, b1, 1f);

        // Load Player 2 color
        float r2 = PlayerPrefs.GetFloat("Player2ColorR", 1f); // Default Red
        float g2 = PlayerPrefs.GetFloat("Player2ColorG", 0f);
        float b2 = PlayerPrefs.GetFloat("Player2ColorB", 0f);
        player2Color = new Color(r2, g2, b2, 1f);
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
    }

    private void SetupListeners()
    {
        // Audio sliders
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        // Language buttons
        if (vietnameseButton != null)
            vietnameseButton.onClick.AddListener(() => SetLanguage(LocalizationManager.Language.Vietnamese));

        if (englishButton != null)
            englishButton.onClick.AddListener(() => SetLanguage(LocalizationManager.Language.English));

        // Back button
        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);

        // Player 1 color buttons
        SetupColorButton(p1ColorGreenButton, Color.green, "Green", 1);
        SetupColorButton(p1ColorRedButton, Color.red, "Red", 1);
        SetupColorButton(p1ColorBlueButton, Color.blue, "Blue", 1);
        SetupColorButton(p1ColorYellowButton, Color.yellow, "Yellow", 1);
        SetupColorButton(p1ColorPurpleButton, new Color(0.5f, 0f, 1f), "Purple", 1);
        SetupColorButton(p1ColorOrangeButton, new Color(1f, 0.5f, 0f), "Orange", 1);
        SetupColorButton(p1ColorCyanButton, Color.cyan, "Cyan", 1);
        SetupColorButton(p1ColorPinkButton, new Color(1f, 0.4f, 0.7f), "Pink", 1);
        SetupColorButton(p1ColorWhiteButton, Color.white, "White", 1);
        SetupColorButton(p1ColorBlackButton, new Color(0.2f, 0.2f, 0.2f), "Black", 1);

        // Player 2 color buttons
        SetupColorButton(p2ColorGreenButton, Color.green, "Green", 2);
        SetupColorButton(p2ColorRedButton, Color.red, "Red", 2);
        SetupColorButton(p2ColorBlueButton, Color.blue, "Blue", 2);
        SetupColorButton(p2ColorYellowButton, Color.yellow, "Yellow", 2);
        SetupColorButton(p2ColorPurpleButton, new Color(0.5f, 0f, 1f), "Purple", 2);
        SetupColorButton(p2ColorOrangeButton, new Color(1f, 0.5f, 0f), "Orange", 2);
        SetupColorButton(p2ColorCyanButton, Color.cyan, "Cyan", 2);
        SetupColorButton(p2ColorPinkButton, new Color(1f, 0.4f, 0.7f), "Pink", 2);
        SetupColorButton(p2ColorWhiteButton, Color.white, "White", 2);
        SetupColorButton(p2ColorBlackButton, new Color(0.2f, 0.2f, 0.2f), "Black", 2);
    }

    private void SetupColorButton(Button button, Color color, string colorName, int playerID)
    {
        if (button == null) return;

        button.onClick.AddListener(() => SetPlayerColor(color, colorName, playerID));

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
            // Play test sound
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
    private void SetPlayerColor(Color color, string colorName, int playerID)
    {
        if (playerID == 1)
        {
            player1Color = color;

            // Save Player 1 color
            PlayerPrefs.SetFloat("Player1ColorR", color.r);
            PlayerPrefs.SetFloat("Player1ColorG", color.g);
            PlayerPrefs.SetFloat("Player1ColorB", color.b);
            PlayerPrefs.Save();

            Debug.Log($"[Settings] Player 1 color: {colorName}");
        }
        else if (playerID == 2)
        {
            player2Color = color;

            // Save Player 2 color
            PlayerPrefs.SetFloat("Player2ColorR", color.r);
            PlayerPrefs.SetFloat("Player2ColorG", color.g);
            PlayerPrefs.SetFloat("Player2ColorB", color.b);
            PlayerPrefs.Save();

            Debug.Log($"[Settings] Player 2 color: {colorName}");
        }

        // Play sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

        // Update ColorPalette if exists
        if (ColorPalette.Instance != null)
        {
            if (playerID == 1)
            {
                ColorPalette.Instance.player1Primary = color;
            }
            else if (playerID == 2)
            {
                ColorPalette.Instance.player2Primary = color;
            }
        }

        UpdateVisuals();
    }
    #endregion

    #region Visual Updates
    private void UpdateVisuals()
    {
        // Update color previews
        if (player1ColorPreview != null)
        {
            player1ColorPreview.color = player1Color;
        }

        if (player2ColorPreview != null)
        {
            player2ColorPreview.color = player2Color;
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

        // Return to main menu
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

        // Reset colors
        SetPlayerColor(Color.green, "Green", 1);
        SetPlayerColor(Color.red, "Red", 2);

        InitializeSettings();
        UpdateVisuals();

        Debug.Log("[Settings] Reset to defaults");
    }
    #endregion
}