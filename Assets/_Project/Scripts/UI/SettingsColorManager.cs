using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Quản lý color selection trong Settings
/// </summary>
public class SettingsColorManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform colorButtonsParent;
    [SerializeField] private GameObject colorButtonPrefab;
    [SerializeField] private GameObject selectionIndicator;

    [Header("Available Colors")]
    [SerializeField]
    private Color[] availableColors = new Color[]
    {
        new Color(0.0f, 1.0f, 0.0f),    // Green
        new Color(1.0f, 0.0f, 0.0f),    // Red
        new Color(0.0f, 0.0f, 1.0f),    // Blue
        new Color(1.0f, 1.0f, 0.0f),    // Yellow
        new Color(0.5f, 0.0f, 1.0f),    // Purple
        new Color(1.0f, 0.5f, 0.0f),    // Orange
        new Color(0.0f, 1.0f, 1.0f),    // Cyan
        new Color(1.0f, 0.0f, 1.0f),    // Magenta
        new Color(1.0f, 1.0f, 1.0f),    // White
        new Color(0.3f, 0.3f, 0.3f)     // Gray
    };

    private Color currentColor;
    private List<Button> colorButtons = new List<Button>();

    private void Start()
    {
        CreateColorButtons();
        LoadSavedColor();
    }

    private void CreateColorButtons()
    {
        if (colorButtonPrefab == null || colorButtonsParent == null)
        {
            Debug.LogError("[SettingsColor] Missing prefab or parent!");
            return;
        }

        // Tạo buttons
        for (int i = 0; i < availableColors.Length; i++)
        {
            Color color = availableColors[i];

            GameObject buttonObj = Instantiate(colorButtonPrefab, colorButtonsParent);
            buttonObj.name = $"ColorButton_{i}";

            // Set màu cho Image
            Image img = buttonObj.GetComponent<Image>();
            if (img != null)
            {
                img.color = color;
            }

            // Add listener
            Button btn = buttonObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnColorSelected(color, buttonObj));
                colorButtons.Add(btn);
            }
        }

        Debug.Log($"[SettingsColor] Created {colorButtons.Count} buttons");
    }

    private void OnColorSelected(Color color, GameObject buttonObj)
    {
        currentColor = color;

        // Di chuyển indicator
        if (selectionIndicator != null)
        {
            selectionIndicator.transform.SetParent(buttonObj.transform, false);
            selectionIndicator.transform.localPosition = Vector3.zero;
            selectionIndicator.SetActive(true);
        }

        // Lưu màu
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveSnakeColor(0, color);
        }

        // Play sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

        string hex = ColorUtility.ToHtmlStringRGB(color);
        Debug.Log($"[SettingsColor] Selected: #{hex}");
    }

    private void LoadSavedColor()
    {
        if (GameManager.Instance == null)
            return;

        Color savedColor = GameManager.Instance.LoadSnakeColor(0, availableColors[0]);

        // Tìm button tương ứng
        for (int i = 0; i < availableColors.Length; i++)
        {
            if (ColorsMatch(availableColors[i], savedColor))
            {
                if (i < colorButtons.Count && colorButtons[i] != null)
                {
                    // Simulate click để highlight
                    OnColorSelected(savedColor, colorButtons[i].gameObject);
                    break;
                }
            }
        }
    }

    private bool ColorsMatch(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) < 0.01f &&
               Mathf.Abs(a.g - b.g) < 0.01f &&
               Mathf.Abs(a.b - b.b) < 0.01f;
    }
}