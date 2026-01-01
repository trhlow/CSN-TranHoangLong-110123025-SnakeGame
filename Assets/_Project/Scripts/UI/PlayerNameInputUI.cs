using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// UI Controller cho màn hình nhập tên
/// </summary>
public class PlayerNameInputUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text placeholderText;
    [SerializeField] private TMP_Text errorText;

    [Header("Settings")]
    [SerializeField] private int minNameLength = 2;
    [SerializeField] private int maxNameLength = 20;
    [SerializeField] private string nextSceneName = "MainMenu";

    private void Start()
    {
        Debug.Log("[PlayerNameInput] Scene started");

        // Setup UI
        SetupUI();

        // Check nếu đã có tên thì skip
        if (PlayerNameManager.Instance != null && PlayerNameManager.Instance.HasPlayerName())
        {
            Debug.Log("[PlayerNameInput] Player already has name, skipping to MainMenu");
            LoadMainMenu();
            return;
        }

        // Focus vào input field
        if (nameInputField != null)
        {
            nameInputField.Select();
            nameInputField.ActivateInputField();
        }
    }

    private void SetupUI()
    {
        // Setup title text
        if (titleText != null)
        {
            if (LocalizationManager.Instance != null)
            {
                titleText.text = LocalizationManager.Instance.GetLocalizedString("enter_your_name");
            }
            else
            {
                titleText.text = "Nhập tên của bạn";
            }
        }

        // Setup placeholder
        if (placeholderText != null)
        {
            placeholderText.text = "Tên người chơi...";
        }

        // Setup button listener
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmClicked);
        }

        // Setup input field listener
        if (nameInputField != null)
        {
            nameInputField.onValueChanged.AddListener(OnNameChanged);
            nameInputField.onSubmit.AddListener(OnSubmit);
        }

        // Hide error initially
        if (errorText != null)
        {
            errorText.gameObject.SetActive(false);
        }
    }

    private void OnNameChanged(string name)
    {
        // Hide error when typing
        if (errorText != null && errorText.gameObject.activeSelf)
        {
            errorText.gameObject.SetActive(false);
        }

        // Giới hạn độ dài
        if (name.Length > maxNameLength)
        {
            nameInputField.text = name.Substring(0, maxNameLength);
        }
    }

    private void OnSubmit(string name)
    {
        OnConfirmClicked();
    }

    private void OnConfirmClicked()
    {
        string playerName = nameInputField.text.Trim();

        // Validate
        if (string.IsNullOrWhiteSpace(playerName))
        {
            ShowError("Vui lòng nhập tên!");
            return;
        }

        if (playerName.Length < minNameLength)
        {
            ShowError($"Tên phải có ít nhất {minNameLength} ký tự!");
            return;
        }

        // Save name
        if (PlayerNameManager.Instance != null)
        {
            PlayerNameManager.Instance.SetPlayerName(playerName);
        }
        else
        {
            Debug.LogError("[PlayerNameInput] PlayerNameManager.Instance is NULL!");
            // Fallback: save directly
            PlayerPrefs.SetString("PlayerName", playerName);
            PlayerPrefs.Save();
        }

        // Play sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

        // Load MainMenu
        LoadMainMenu();
    }

    private void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.gameObject.SetActive(true);
        }

        // Play error sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Error");
        }

        Debug.LogWarning($"[PlayerNameInput] {message}");
    }

    private void LoadMainMenu()
    {
        Debug.Log($"[PlayerNameInput] Loading {nextSceneName}...");
        SceneManager.LoadScene(nextSceneName);
    }

    private void Update()
    {
        // Enter key để confirm
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnConfirmClicked();
        }

        // Escape để skip (dùng default name)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (PlayerNameManager.Instance != null)
            {
                PlayerNameManager.Instance.SetPlayerName("Player");
            }
            LoadMainMenu();
        }
    }
}
