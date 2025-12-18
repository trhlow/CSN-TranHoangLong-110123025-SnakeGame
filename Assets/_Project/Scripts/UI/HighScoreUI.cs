using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hiển thị bảng xếp hạng trong UI
/// </summary>
public class HighScoreUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform scoreListContainer;  // Container chứa các entry
    [SerializeField] private GameObject scoreEntryPrefab;   // Prefab cho mỗi entry
    [SerializeField] private TMP_Text noScoresText;         // Text hiện khi chưa có điểm
    [SerializeField] private Button clearButton;            // Button xóa bảng điểm
    [SerializeField] private Button backButton;             // Button quay lại

    [Header("Settings")]
    [SerializeField] private int maxDisplayCount = 10;      // Số lượng hiển thị tối đa
    [SerializeField] private Color highlightColor = Color.yellow; // Màu highlight điểm mới

    private List<GameObject> spawnedEntries = new List<GameObject>();

    private void Start()
    {
        RefreshDisplay();

        // Setup buttons
        if (clearButton != null)
        {
            clearButton.onClick.AddListener(OnClearClicked);
        }
    }

    private void OnEnable()
    {
        // Refresh mỗi khi panel được bật
        RefreshDisplay();
    }

    public void RefreshDisplay()
    {
        // Clear old entries
        ClearEntries();

        if (HighScoreManager.Instance == null)
        {
            Debug.LogWarning("[HighScoreUI] HighScoreManager not found!");
            ShowNoScoresMessage(true);
            return;
        }

        // Get high scores
        List<HighScoreManager.HighScoreEntry> scores = HighScoreManager.Instance.GetHighScores(maxDisplayCount);

        if (scores == null || scores.Count == 0)
        {
            ShowNoScoresMessage(true);
            return;
        }

        ShowNoScoresMessage(false);

        // Create entries
        for (int i = 0; i < scores.Count; i++)
        {
            CreateScoreEntry(i + 1, scores[i]);
        }

        Debug.Log($"[HighScoreUI] Displayed {scores.Count} high scores");
    }

    private void CreateScoreEntry(int rank, HighScoreManager.HighScoreEntry entry)
    {
        GameObject entryObj;

        if (scoreEntryPrefab != null && scoreListContainer != null)
        {
            // Spawn từ prefab
            entryObj = Instantiate(scoreEntryPrefab, scoreListContainer);
        }
        else
        {
            // Tạo simple text nếu không có prefab
            entryObj = new GameObject($"Score_{rank}");
            entryObj.transform.SetParent(scoreListContainer != null ? scoreListContainer : transform);

            TMP_Text text = entryObj.AddComponent<TextMeshProUGUI>();
            text.fontSize = 24;
            text.alignment = TextAlignmentOptions.Left;
        }

        // Fill data
        FillEntryData(entryObj, rank, entry);

        spawnedEntries.Add(entryObj);
    }

    private void FillEntryData(GameObject entryObj, int rank, HighScoreManager.HighScoreEntry entry)
    {
        // Tìm text components trong entry
        TMP_Text[] texts = entryObj.GetComponentsInChildren<TMP_Text>();

        if (texts.Length == 0)
        {
            // Fallback: tạo text đơn giản
            TMP_Text text = entryObj.GetComponent<TMP_Text>();
            if (text != null)
            {
                text.text = $"{rank}. {entry.playerName} - {entry.score} pts ({entry.gameMode})";
            }
            return;
        }

        // Assign data vào các text fields
        // Format: Rank | Name | Score | Date | Mode
        foreach (TMP_Text text in texts)
        {
            if (text.gameObject.name.Contains("Rank"))
            {
                text.text = rank.ToString();
            }
            else if (text.gameObject.name.Contains("Name"))
            {
                text.text = entry.playerName;
            }
            else if (text.gameObject.name.Contains("Score"))
            {
                text.text = entry.score.ToString();
            }
            else if (text.gameObject.name.Contains("Date"))
            {
                text.text = entry.date;
            }
            else if (text.gameObject.name.Contains("Mode"))
            {
                text.text = entry.gameMode;
            }
        }
    }

    private void ClearEntries()
    {
        foreach (GameObject entry in spawnedEntries)
        {
            if (entry != null)
            {
                Destroy(entry);
            }
        }
        spawnedEntries.Clear();
    }

    private void ShowNoScoresMessage(bool show)
    {
        if (noScoresText != null)
        {
            noScoresText.gameObject.SetActive(show);

            if (show && LocalizationManager.Instance != null)
            {
                noScoresText.text = LocalizationManager.Instance.GetLocalizedString("highscore.no_scores");
            }
        }
    }

    private void OnClearClicked()
    {
        Debug.Log("[HighScoreUI] Clear high scores clicked");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        }

        if (HighScoreManager.Instance != null)
        {
            HighScoreManager.Instance.ClearHighScores();
            RefreshDisplay();
        }
    }

    private void OnDestroy()
    {
        if (clearButton != null)
        {
            clearButton.onClick.RemoveListener(OnClearClicked);
        }
    }
}

/* 
===========================================
HƯỚNG DẪN SỬ DỤNG:
===========================================

1. TẠO HIGHSCOREPANEL UI:

Canvas → HighScorePanel
├── Title (Text: "BẢNG XẾP HẠNG")
├── ScrollView
│   └── Content (Vertical Layout Group)
│       └── [Score entries spawn here]
├── NoScoresText (Text: "Chưa có điểm cao")
├── BtnClear (Button)
└── BtnBack (Button)

2. TẠO SCORE ENTRY PREFAB (Optional):

Create UI → Panel → Rename: "ScoreEntry"

ScoreEntry
├── TxtRank (Text)
├── TxtName (Text)
├── TxtScore (Text)
├── TxtDate (Text)
└── TxtMode (Text)

Layout: Horizontal Layout Group
Padding: 10px

Save as Prefab: ScoreEntry.prefab

3. ASSIGN VÀO HIGHSCOREUI:

HighScorePanel → Add Component → HighScoreUI

Inspector:
✅ Score List Container: Content (trong ScrollView)
✅ Score Entry Prefab: ScoreEntry.prefab
✅ No Scores Text: NoScoresText
✅ Clear Button: BtnClear
✅ Back Button: BtnBack
✅ Max Display Count: 10

4. CONNECT BUTTONS:

BtnClear → OnClick(): HighScoreUI.OnClearClicked()
BtnBack → OnClick(): MainMenuManager.OnBackToMainMenu()

5. THÊM LOCALIZATION KEY:

LocalizationManager.cs → LoadLocalizedStrings():
AddString("highscore.no_scores", "Chưa có điểm cao", "No high scores yet");

DONE! ✅
*/