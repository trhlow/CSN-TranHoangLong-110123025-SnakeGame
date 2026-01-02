using UnityEngine;

public class PlayerNameManager : Singleton<PlayerNameManager>
{
    private const string PLAYER_NAME_KEY = "PlayerName";
    private const string DEFAULT_NAME = "Người chơi";

    private string playerName;

    public string PlayerName => GetPlayerName();

    protected override void Awake()
    {
        base.Awake();
        LoadPlayerName();
    }

    private void LoadPlayerName()
    {
        playerName = PlayerPrefs.GetString(PLAYER_NAME_KEY, DEFAULT_NAME);
        Debug.Log($"[PlayerNameManager] 📂 Loaded name: {playerName}");
    }

    public void SetPlayerName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            Debug.LogWarning("[PlayerNameManager] ⚠️ Tên không hợp lệ!");
            return;
        }

        playerName = newName.Trim();
        PlayerPrefs.SetString(PLAYER_NAME_KEY, playerName);
        PlayerPrefs.Save();

        Debug.Log($"[PlayerNameManager] 💾 Saved name: {playerName}");
    }

    public string GetPlayerName()
    {
        if (string.IsNullOrEmpty(playerName))
        {
            playerName = PlayerPrefs.GetString(PLAYER_NAME_KEY, DEFAULT_NAME);
        }
        return playerName;
    }

    public void ResetToDefault()
    {
        SetPlayerName(DEFAULT_NAME);
    }

    public bool HasPlayerName()
    {
        return !string.IsNullOrEmpty(GetPlayerName()) && GetPlayerName() != DEFAULT_NAME;
    }
}