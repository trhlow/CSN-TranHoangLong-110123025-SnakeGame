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
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private Button vietnameseButton;
    [SerializeField] private Button englishButton;

    [Header("Graphics Settings")]
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Buttons")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button applyButton;

    private void Start()
    {
        InitializeSettings();
        SetupListeners();
    }

    private void InitializeSettings()
    {
        // Load audio settings
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

        // Load language settings
        if (LocalizationManager.Instance != null && languageDropdown != null)
        {
            languageDropdown.value = (int)LocalizationManager.Instance.CurrentLanguage;
        }

        // Load graphics settings
        if (qualityDropdown != null)
        {
            qualityDropdown.value = QualitySettings.GetQualityLevel();
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
        }
    }

    private void SetupListeners()
    {
        // Audio sliders
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        // Language
        if (languageDropdown != null)
        {
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        }

        if (vietnameseButton != null)
        {
            vietnameseButton.onClick.AddListener(() => SetLanguage(LocalizationManager.Language.Vietnamese));
        }

        if (englishButton != null)
        {
            englishButton.onClick.AddListener(() => SetLanguage(LocalizationManager.Language.English));
        }

        // Graphics
        if (qualityDropdown != null)
        {
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        }

        // Navigation buttons
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }

        if (applyButton != null)
        {
            applyButton.onClick.AddListener(OnApplyClicked);
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
    private void OnLanguageChanged(int index)
    {
        SetLanguage((LocalizationManager.Language)index);
    }

    private void SetLanguage(LocalizationManager.Language language)
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.SetLanguage(language);
            Debug.Log($"[SettingsMenu] Đổi ngôn ngữ:   {language}");
        }
    }
    #endregion

    #region Graphics Settings
    private void OnQualityChanged(int index)
    {
        QualitySettings.SetQualityLevel(index);
        Debug.Log($"[SettingsMenu] Đổi chất lượng:   {QualitySettings.names[index]}");
    }

    private void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        Debug.Log($"[SettingsMenu] Toàn màn hình:  {isFullscreen}");
    }
    #endregion

    #region Navigation
    private void OnBackClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideSettings();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnApplyClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

        // Apply settings (already saved in real-time)
        Debug.Log("[SettingsMenu] Đã áp dụng cài đặt");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideSettings();
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

        QualitySettings.SetQualityLevel(2); // Medium
        Screen.fullScreen = true;

        InitializeSettings();

        Debug.Log("[SettingsMenu] Đã đặt lại cài đặt mặc định");
    }
    #endregion
}