using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// ✅ FIXED: Chọn màu rắn trong Settings - Lưu đúng cho Player
/// </summary>
public class SnakeColorSettingsUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject colorButtonPrefab;
    [SerializeField] private Transform colorButtonsGrid;
    [SerializeField] private GameObject selectionIndicator;

    [Header("Colors")]
    [SerializeField]
    private Color[] snakeColors = new Color[]
    {
        Color.green,        // 0
        Color.red,          // 1
        Color.blue,         // 2
        Color.yellow,       // 3
        new Color(0.5f,0,1f), // 4 - Purple
        new Color(1f, 0.5f, 0f), // 5 - Orange
        Color.magenta,      // 6
        Color.white,        // 7
        Color.grey,         // 8
        Color.black         // 9
    };

    private Color currentSelectedColor;
    private List<Button> colorButtons = new List<Button>();

    public Color GetSelectedColor() => currentSelectedColor;

    public void SetSelectedColor(Color color)
    {
        currentSelectedColor = color;
        for (int i = 0; i < snakeColors.Length; i++)
        {
            if (AreColorsEqual(snakeColors[i], color))
            {
                UpdateSelectionIndicator(i);
                break;
            }
        }
    }

    private void Start()
    {
        GenerateColorButtons();
        LoadSavedSnakeColor();
    }

    private void GenerateColorButtons()
    {
        // Xóa button cũ
        foreach (Transform child in colorButtonsGrid) 
            Destroy(child.gameObject);
        
        colorButtons.Clear();

        // Sinh button mới
        for (int i = 0; i < snakeColors.Length; i++)
        {
            var color = snakeColors[i];
            var buttonObj = Instantiate(colorButtonPrefab, colorButtonsGrid);
            var button = buttonObj.GetComponent<Button>();
            var img = buttonObj.GetComponent<Image>();
            
            if (img != null) 
                img.color = color;

            int colorIndex = i;
            button.onClick.AddListener(() => OnColorSelect(colorIndex));
            colorButtons.Add(button);
        }

        Debug.Log($"[ColorSettings] Generated {snakeColors.Length} color buttons");
    }

    private void OnColorSelect(int index)
    {
        currentSelectedColor = snakeColors[index];
        UpdateSelectionIndicator(index);

        // ✅ FIX: Lưu màu cho Player 1 (người chơi thật)
        GameManager.Instance.SaveSnakeColor(1, currentSelectedColor);
        
        Debug.Log($"[ColorSettings] ✅ Selected color for Player 1: {ColorUtility.ToHtmlStringRGB(currentSelectedColor)}");

        // Play sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("ButtonClick");
    }

    private void UpdateSelectionIndicator(int index)
    {
        if (selectionIndicator == null) return;
        if (index < 0 || index >= colorButtons.Count) return;
        
        selectionIndicator.transform.SetParent(colorButtons[index].transform);
        selectionIndicator.transform.localPosition = Vector3.zero;
        selectionIndicator.SetActive(true);
    }

    private void LoadSavedSnakeColor()
    {
        // ✅ FIX: Load màu đã lưu cho Player 1
        Color saved = GameManager.Instance.LoadSnakeColor(1, snakeColors[0]);
        currentSelectedColor = saved;
        
        Debug.Log($"[ColorSettings] 📂 Loaded saved color: {ColorUtility.ToHtmlStringRGB(saved)}");
        
        // Tự highlight
        for (int i = 0; i < snakeColors.Length; i++)
        {
            if (AreColorsEqual(snakeColors[i], saved))
            {
                UpdateSelectionIndicator(i);
                break;
            }
        }
    }

    private bool AreColorsEqual(Color a, Color b, float tol = 0.01f)
    {
        return Mathf.Abs(a.r - b.r) < tol &&
               Mathf.Abs(a.g - b.g) < tol &&
               Mathf.Abs(a.b - b.b) < tol;
    }
}
