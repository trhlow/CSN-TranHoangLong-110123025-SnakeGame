using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HighScoreManager : Singleton<HighScoreManager>
{
    [System.Serializable]
    public class HighScoreEntry
    {
        public string playerName;
        public int score;
        public string date;
        public string gameMode;

        public HighScoreEntry(string name, int scoreValue, string mode)
        {
            playerName = name;
            score = scoreValue;
            date = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            gameMode = mode;
        }
    }

    [System.Serializable]
    private class HighScoreList
    {
        public List<HighScoreEntry> entries = new List<HighScoreEntry>();
    }

    [Header("Settings")]
    [SerializeField] private int maxEntries = 10;
    [SerializeField] private string saveKey = "HighScores_v1";

    private HighScoreList highScores = new HighScoreList();

    protected override void Awake()
    {
        base.Awake();
        LoadHighScores();
    }

    public bool TryAddHighScore(string playerName, int score)
    {
        string gameMode = GameManager.Instance != null ?
            GameManager.Instance.CurrentGameMode.ToString() : "Unknown";

        return TryAddHighScore(playerName, score, gameMode);
    }

    public bool TryAddHighScore(string playerName, int score, string gameMode)
    {
        if (highScores.entries.Count >= maxEntries)
        {
            int lowestScore = highScores.entries[highScores.entries.Count - 1].score;
            if (score <= lowestScore)
            {
                return false;
            }
        }

        HighScoreEntry newEntry = new HighScoreEntry(playerName, score, gameMode);
        highScores.entries.Add(newEntry);

        highScores.entries = highScores.entries.OrderByDescending(e => e.score).ToList();

        if (highScores.entries.Count > maxEntries)
        {
            highScores.entries.RemoveRange(maxEntries, highScores.entries.Count - maxEntries);
        }

        SaveHighScores();

        Debug.Log($"[HighScoreManager] Điểm cao mới:   {playerName} - {score}");
        return true;
    }

    public List<HighScoreEntry> GetHighScores()
    {
        return new List<HighScoreEntry>(highScores.entries);
    }

    public List<HighScoreEntry> GetHighScores(int count)
    {
        return highScores.entries.Take(count).ToList();
    }

    public HighScoreEntry GetHighScore(int rank)
    {
        if (rank < 0 || rank >= highScores.entries.Count)
            return null;

        return highScores.entries[rank];
    }

    public int GetHighestScore()
    {
        return highScores.entries.Count > 0 ? highScores.entries[0].score : 0;
    }

    public int GetRank(int score)
    {
        for (int i = 0; i < highScores.entries.Count; i++)
        {
            if (score > highScores.entries[i].score)
            {
                return i + 1;
            }
        }

        return highScores.entries.Count + 1;
    }

    public bool IsHighScore(int score)
    {
        if (highScores.entries.Count < maxEntries)
            return true;

        return score > highScores.entries[highScores.entries.Count - 1].score;
    }

    public void ClearHighScores()
    {
        highScores.entries.Clear();
        SaveHighScores();
        Debug.Log("[HighScoreManager] Đã xóa bảng điểm cao");
    }

    private void SaveHighScores()
    {
        string json = JsonUtility.ToJson(highScores, true);
        PlayerPrefs.SetString(saveKey, json);
        PlayerPrefs.Save();
    }

    private void LoadHighScores()
    {
        if (PlayerPrefs.HasKey(saveKey))
        {
            string json = PlayerPrefs.GetString(saveKey);
            highScores = JsonUtility.FromJson<HighScoreList>(json);
            Debug.Log($"[HighScoreManager] Đã tải {highScores.entries.Count} điểm cao");
        }
        else
        {
            Debug.Log("[HighScoreManager] Chưa có điểm cao lưu");
        }
    }
}