using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerNameInputUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button startButton;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private TMP_Text buttonText; // ✅ Text của button (thay đổi theo context)

    [Header("Settings")]
    [SerializeField] private int minNameLength = 2;
    [SerializeField] private int maxNameLength = 15;

    private bool isChangingName = false; // ✅ Đang đổi tên hay lần đầu? 

    private void Start()
    {
        CheckContext();
        SetupUI();
        SetupListeners();
        LoadExistingName();
    }

    private void CheckContext()
    {
        // ✅ Kiểm tra xem có phải đang đổi tên không
        isChangingName = PlayerPrefs.GetInt("IsChangingName", 0) == 1;

        if (isChangingName)
        {
            PlayerPrefs.DeleteKey("IsChangingName"); // Xoá flag
            PlayerPrefs.Save();
        }
    }

    private void SetupUI()
    {
        // ✅ Đổi title tùy context
        if (titleText != null)
        {
            if (isChangingName)
            {
                titleText.text = "ĐỔI TÊN NGƯỜI CHƠI";
            }
            else
            {
                titleText.text = "NHẬP TÊN CỦA BẠN";
            }
        }

        // ✅ Đổi button text
        if (buttonText != null)
        {
            if (isChangingName)
            {
                buttonText.text = "LƯU TÊN";
            }
            else
            {
                buttonText.text = "BẮT ĐẦU CHƠI";
            }
        }

        // Placeholder
        if (nameInputField != null && nameInputField.placeholder is TMP_Text placeholder)
        {
            placeholder.text = "Tên người chơi... ";
        }

        // Hide error
        if (errorText != null)
        {
            errorText.gameObject.SetActive(false);
        }
    }

    private void SetupListeners()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }

        if (nameInputField != null)
        {
            nameInputField.onValueChanged.AddListener(OnNameChanged);
            nameInputField.characterLimit = maxNameLength;
        }
    }

    private void LoadExistingName()
    {
        // ✅ Nếu đang đổi tên → load tên cũ vào input
        if (isChangingName && PlayerNameManager.Instance != null)
        {
            string currentName = PlayerNameManager.Instance.GetPlayerName();
            if (nameInputField != null)
            {
                nameInputField.text = currentName;
            }
        }
        // ✅ Nếu lần đầu nhưng đã có tên → skip
        else if (!isChangingName && PlayerNameManager.Instance != null && PlayerNameManager.Instance.HasPlayerName())
        {
            SceneManager.LoadScene("MainMenu");
            return;
        }

        // Focus
        if (nameInputField != null)
        {
            nameInputField.Select();
            nameInputField.ActivateInputField();
        }
    }

    private void OnNameChanged(string newName)
    {
        if (errorText != null && errorText.gameObject.activeSelf)
        {
            errorText.gameObject.SetActive(false);
        }
    }

    private void OnStartButtonClicked()
    {
        string playerName = nameInputField != null ? nameInputField.text.Trim() : "";

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

        // Save
        if (PlayerNameManager.Instance != null)
        {
            PlayerNameManager.Instance.SetPlayerName(playerName);
            Debug.Log($"[PlayerNameInput] ✅ Name saved: {playerName}");
        }

        // Play sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

        // ✅ Load scene tùy context
        if (isChangingName)
        {
            SceneManager.LoadScene("Settings"); // Quay lại Settings
        }
        else
        {
            SceneManager.LoadScene("MainMenu"); // Lần đầu → MainMenu
        }
    }

    private void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.gameObject.SetActive(true);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Error");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnStartButtonClicked();
        }

        // ✅ ESC để quay lại (chỉ khi đang đổi tên)
        if (isChangingName && Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("Settings");
        }
    }
}