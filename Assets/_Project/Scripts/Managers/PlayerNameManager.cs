using UnityEngine;

public class PlayerNameManager : MonoBehaviour
{
    private static PlayerNameManager instance;
    public static PlayerNameManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("PlayerNameManager");
                instance = go.AddComponent<PlayerNameManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private const string PLAYER_NAME_KEY = "PlayerName";
    private string playerName = "";

    public string PlayerName => GetPlayerName();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPlayerName();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void LoadPlayerName()
    {
        playerName = PlayerPrefs.GetString(PLAYER_NAME_KEY, "");
    }

    public void SetPlayerName(string name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            playerName = name.Trim();
            PlayerPrefs.SetString(PLAYER_NAME_KEY, playerName);
            PlayerPrefs.Save();
        }
    }

    public string GetPlayerName()
    {
        if (string.IsNullOrEmpty(playerName))
        {
            playerName = PlayerPrefs.GetString(PLAYER_NAME_KEY, "Player");
        }
        return playerName;
    }

    public bool HasPlayerName()
    {
        return !string.IsNullOrEmpty(GetPlayerName()) && GetPlayerName() != "Player";
    }
}
