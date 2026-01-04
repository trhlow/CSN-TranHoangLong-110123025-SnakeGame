using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerNameInput : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI placeholderText;

    [Header("Settings")]
    [SerializeField] private int minNameLength = 2;
    [SerializeField] private int maxNameLength = 15;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Start()
    {
        if (nameInputField != null)
        {
            nameInputField.characterLimit = maxNameLength;
            nameInputField.onValueChanged.AddListener(OnNameChanged);
            nameInputField.Select();
            nameInputField.ActivateInputField();
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmClicked);
            confirmButton.interactable = false;
        }

        if (errorText != null)
        {
            errorText.gameObject.SetActive(false);
        }

        UpdateLocalization();
    }

    private void Update()
    {
        // Cho phép nhấn Enter để xác nhận
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (confirmButton != null && confirmButton.interactable)
            {
                OnConfirmClicked();
            }
        }
    }

    private void OnNameChanged(string newName)
    {
        bool isValid = ValidateName(newName);
        
        if (confirmButton != null)
        {
            confirmButton.interactable = isValid;
        }

        if (errorText != null && errorText.gameObject.activeSelf)
        {
            errorText.gameObject.SetActive(false);
        }
    }

    private bool ValidateName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;

        string trimmedName = name.Trim();
        
        if (trimmedName.Length < minNameLength)
            return false;

        if (trimmedName.Length > maxNameLength)
            return false;

        // Kiểm tra chỉ chứa ký tự chữ cái, số và khoảng trắng
        foreach (char c in trimmedName)
        {
            if (!char.IsLetterOrDigit(c) && c != ' ')
                return false;
        }

        return true;
    }

    private void OnConfirmClicked()
    {
        if (nameInputField == null) return;

        string playerName = nameInputField.text.Trim();

        if (!ValidateName(playerName))
        {
            ShowError(GetErrorMessage(playerName));
            return;
        }

        // Lưu tên người chơi
        PlayerNameManager.Instance.SetPlayerName(playerName);

        // Chuyển đến MainMenu
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private string GetErrorMessage(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return LocalizationManager.Instance != null && LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese
                ? "Vui lòng nhập tên của bạn!"
                : "Please enter your name!";
        }

        if (name.Length < minNameLength)
        {
            return LocalizationManager.Instance != null && LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese
                ? $"Tên phải có ít nhất {minNameLength} ký tự!"
                : $"Name must be at least {minNameLength} characters!";
        }

        return LocalizationManager.Instance != null && LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese
            ? "Tên chỉ được chứa chữ cái, số và khoảng trắng!"
            : "Name can only contain letters, numbers and spaces!";
    }

    private void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.gameObject.SetActive(true);
        }
    }

    private void UpdateLocalization()
    {
        if (LocalizationManager.Instance == null) return;

        bool isVietnamese = LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese;

        if (titleText != null)
        {
            titleText.text = isVietnamese ? "NHẬP TÊN CỦA BẠN" : "ENTER YOUR NAME";
        }

        if (placeholderText != null)
        {
            placeholderText.text = isVietnamese ? "Tên của bạn..." : "Your name...";
        }

        if (confirmButton != null)
        {
            TextMeshProUGUI buttonText = confirmButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = isVietnamese ? "XÁC NHẬN" : "CONFIRM";
            }
        }
    }

    private void OnDestroy()
    {
        if (nameInputField != null)
        {
            nameInputField.onValueChanged.RemoveListener(OnNameChanged);
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(OnConfirmClicked);
        }
    }
}
