using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : Singleton<LocalizationManager>
{
    public enum Language
    {
        Vietnamese,
        English
    }

    [Header("Settings")]
    [SerializeField] private Language currentLanguage = Language.Vietnamese;

    private Dictionary<string, Dictionary<Language, string>> localizedStrings =
        new Dictionary<string, Dictionary<Language, string>>();

    public Language CurrentLanguage => currentLanguage;

    protected override void Awake()
    {
        base.Awake();
        LoadLocalizedStrings();
        LoadSavedLanguage();
    }

    private void LoadLocalizedStrings()
    {
        // Main Menu
        AddString("menu. title", "RẮN SĂN MỒI", "SNAKE GAME");
        AddString("menu.single_player", "Chơi Một Mình", "Single Player");
        AddString("menu.multiplayer", "Chơi Hai Người", "Multiplayer");
        AddString("menu.vs_ai", "Đấu Với Máy", "VS AI");
        AddString("menu.settings", "Cài Đặt", "Settings");
        AddString("menu.  high_scores", "Bảng Xếp Hạng", "High Scores");
        AddString("menu.quit", "Thoát", "Quit");

        // Gameplay
        AddString("game.player1", "Người Chơi 1", "Player 1");
        AddString("game.player2", "Người Chơi 2", "Player 2");
        AddString("game.score", "Điểm", "Score");
        AddString("game.combo", "Combo", "Combo");
        AddString("game. pause", "Tạm Dừng", "Pause");

        // Game Over
        AddString("ui.game_over. victory", "CHIẾN THẮNG!", "VICTORY!");
        AddString("ui.game_over.defeat", "THUA CUỘC!", "GAME OVER!");
        AddString("ui.game_over.player1_wins", "Người Chơi 1 Thắng!", "Player 1 Wins!");
        AddString("ui.game_over.player2_wins", "Người Chơi 2 Thắng!", "Player 2 Wins!");
        AddString("ui.  game_over.player3_wins", "Máy Thắng!", "AI Wins!");
        AddString("ui.game_over.final_score", "Điểm Cuối", "Final Score");
        AddString("ui.game_over.new_high_score", "ĐIỂM CAO MỚI!", "NEW HIGH SCORE!");
        AddString("ui.  game_over.restart", "Chơi Lại", "Restart");
        AddString("ui.game_over.main_menu", "Menu Chính", "Main Menu");

        // Pause Menu
        AddString("pause.title", "TẠM DỪNG", "PAUSED");
        AddString("pause.resume", "Tiếp Tục", "Resume");
        AddString("pause.settings", "Cài Đặt", "Settings");
        AddString("pause.main_menu", "Menu Chính", "Main Menu");

        // Settings
        AddString("settings.  title", "CÀI ĐẶT", "SETTINGS");
        AddString("settings. language", "Ngôn Ngữ", "Language");
        AddString("settings.vietnamese", "Tiếng Việt", "Vietnamese");
        AddString("settings.english", "Tiếng Anh", "English");
        AddString("settings.audio", "Âm Thanh", "Audio");
        AddString("settings.music_volume", "Âm Lượng Nhạc", "Music Volume");
        AddString("settings.sfx_volume", "Âm Lượng Hiệu Ứng", "SFX Volume");
        AddString("settings.back", "Quay Lại", "Back");

        Debug.Log($"[LocalizationManager] Đã tải {localizedStrings.Count} chuỗi địa phương hóa");
    }

    private void AddString(string key, string vietnamese, string english)
    {
        if (!localizedStrings.ContainsKey(key))
        {
            localizedStrings[key] = new Dictionary<Language, string>();
        }

        localizedStrings[key][Language.Vietnamese] = vietnamese;
        localizedStrings[key][Language.English] = english;
    }

    public string GetLocalizedString(string key)
    {
        if (localizedStrings.ContainsKey(key))
        {
            if (localizedStrings[key].ContainsKey(currentLanguage))
            {
                return localizedStrings[key][currentLanguage];
            }
        }

        Debug.LogWarning($"[LocalizationManager] Không tìm thấy key '{key}'");
        return key;
    }

    public string GetLocalizedString(string key, Language language)
    {
        if (localizedStrings.ContainsKey(key))
        {
            if (localizedStrings[key].ContainsKey(language))
            {
                return localizedStrings[key][language];
            }
        }

        return key;
    }

    public void SetLanguage(Language language)
    {
        currentLanguage = language;
        SaveLanguage();
        RefreshAllLocalizedTexts();

        Debug.Log($"[LocalizationManager] Đổi ngôn ngữ:   {language}");
    }

    public void ToggleLanguage()
    {
        currentLanguage = currentLanguage == Language.Vietnamese ?
            Language.English : Language.Vietnamese;

        SetLanguage(currentLanguage);
    }

    private void RefreshAllLocalizedTexts()
    {
        LocalizedText[] localizedTexts = FindObjectsOfType<LocalizedText>(true);
        foreach (LocalizedText text in localizedTexts)
        {
            text.RefreshText();
        }
    }

    private void SaveLanguage()
    {
        PlayerPrefs.SetInt("Language", (int)currentLanguage);
        PlayerPrefs.Save();
    }

    private void LoadSavedLanguage()
    {
        if (PlayerPrefs.HasKey("Language"))
        {
            currentLanguage = (Language)PlayerPrefs.GetInt("Language");
        }
    }
}