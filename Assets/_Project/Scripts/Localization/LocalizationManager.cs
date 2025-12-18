using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : Singleton<LocalizationManager>
{
    public enum Language
    {
        Vietnamese = 0,
        English = 1
    }

    [Header("Settings")]
    [SerializeField] private Language currentLanguage = Language.Vietnamese;
    [SerializeField] private string languagePrefsKey = "GameLanguage";

    private Dictionary<string, LocalizedString> localizedStrings = new Dictionary<string, LocalizedString>();

    public Language CurrentLanguage => currentLanguage;

    [System.Serializable]
    private class LocalizedString
    {
        public string vietnamese;
        public string english;

        public LocalizedString(string vi, string en)
        {
            vietnamese = vi;
            english = en;
        }

        public string GetText(Language lang)
        {
            return lang == Language.Vietnamese ? vietnamese : english;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        LoadLocalizedStrings();
        LoadLanguagePreference(); // ✅ Chỉ load, KHÔNG apply

        Debug.Log($"[LocalizationManager] ✅ Initialized with language: {currentLanguage}");
    }

    // ✅ REMOVED: Không còn auto refresh trong Start()
    // private void Start() { RefreshAllLocalizedTexts(); }

    private void LoadLanguagePreference()
    {
        if (PlayerPrefs.HasKey(languagePrefsKey))
        {
            int savedLang = PlayerPrefs.GetInt(languagePrefsKey);
            currentLanguage = (Language)savedLang;
            Debug.Log($"[LocalizationManager] 📂 Loaded saved language: {currentLanguage}");
        }
        else
        {
            // ✅ Lần đầu tiên: Default là Vietnamese
            currentLanguage = Language.Vietnamese;
            Debug.Log($"[LocalizationManager] 🆕 First time, using default: Vietnamese");
        }
    }

    private void LoadLocalizedStrings()
    {
        // Main Menu
        AddString("menu.title", "RẮN SĂN MỒI", "SNAKE GAME");
        AddString("menu.single_player", "Chơi Một Mình", "Single Player");
        AddString("menu.multiplayer", "Chơi Hai Người", "Multiplayer");
        AddString("menu.vs_ai", "Đấu Với Máy", "VS AI");
        AddString("menu.settings", "Cài Đặt", "Settings");
        AddString("menu.high_scores", "Bảng Xếp Hạng", "High Scores");
        AddString("menu.quit", "Thoát", "Quit");

        // Gameplay
        AddString("game.player1", "Người Chơi 1", "Player 1");
        AddString("game.player2", "Người Chơi 2", "Player 2");
        AddString("game.ai", "Máy", "AI");
        AddString("game.score", "Điểm", "Score");
        AddString("game.combo", "Combo", "Combo");
        AddString("game.pause", "Tạm Dừng", "Pause");
        AddString("game.time", "Thời Gian", "Time");

        // Game Over
        AddString("ui.game_over.victory", "CHIẾN THẮNG!", "VICTORY!");
        AddString("ui.game_over.defeat", "THUA CUỘC!", "GAME OVER!");
        AddString("ui.game_over.player1_wins", "Người Chơi 1 Thắng!", "Player 1 Wins!");
        AddString("ui.game_over.player2_wins", "Người Chơi 2 Thắng!", "Player 2 Wins!");
        AddString("ui.game_over.player3_wins", "Máy Thắng!", "AI Wins!");
        AddString("ui.game_over.final_score", "Điểm Cuối", "Final Score");
        AddString("ui.game_over.new_high_score", "ĐIỂM CAO MỚI!", "NEW HIGH SCORE!");
        AddString("ui.game_over.restart", "Chơi Lại", "Restart");
        AddString("ui.game_over.main_menu", "Menu Chính", "Main Menu");

        // Pause Menu
        AddString("pause.title", "TẠM DỪNG", "PAUSED");
        AddString("pause.resume", "Tiếp Tục", "Resume");
        AddString("pause.settings", "Cài Đặt", "Settings");
        AddString("pause.main_menu", "Menu Chính", "Main Menu");
        AddString("pause.quit", "Thoát Game", "Quit Game");

        // Settings
        AddString("settings.title", "CÀI ĐẶT", "SETTINGS");
        AddString("settings.language", "Ngôn Ngữ", "Language");
        AddString("settings.vietnamese", "Tiếng Việt", "Vietnamese");
        AddString("settings.english", "Tiếng Anh", "English");
        AddString("settings.audio", "Âm Thanh", "Audio");
        AddString("settings.music_volume", "Âm Lượng Nhạc", "Music Volume");
        AddString("settings.sfx_volume", "Âm Lượng Hiệu Ứng", "SFX Volume");
        AddString("settings.graphics", "Đồ Họa", "Graphics");
        AddString("settings.quality", "Chất Lượng", "Quality");
        AddString("settings.fullscreen", "Toàn Màn Hình", "Fullscreen");
        AddString("settings.snake_color", "Màu Rắn", "Snake Color");
        AddString("settings.back", "Quay Lại", "Back");
        AddString("settings.apply", "Áp Dụng", "Apply");
        AddString("settings.reset", "Đặt Lại", "Reset");

        // High Scores
        AddString("highscore.title", "BẢNG XẾP HẠNG", "HIGH SCORES");
        AddString("highscore.no_scores","Chưa có điểm cao nào\nHãy chơi để ghi điểm!","No high scores yet\nPlay to set a record!");
        AddString("highscore.rank", "Hạng", "Rank");
        AddString("highscore.name", "Tên", "Name");
        AddString("highscore.score", "Điểm", "Score");
        AddString("highscore.date", "Ngày", "Date");
        AddString("highscore.mode", "Chế Độ", "Mode");
        AddString("highscore.clear", "Xóa Bảng", "Clear Board");
        AddString("highscore.back", "Quay Lại", "Back");

        // Colors
        AddString("color.green", "Xanh Lá", "Green");
        AddString("color.red", "Đỏ", "Red");
        AddString("color.blue", "Xanh Dương", "Blue");
        AddString("color.yellow", "Vàng", "Yellow");
        AddString("color.purple", "Tím", "Purple");
        AddString("color.orange", "Cam", "Orange");
        AddString("color.cyan", "Xanh Ngọc", "Cyan");
        AddString("color.pink", "Hồng", "Pink");
        AddString("color.white", "Trắng", "White");
        AddString("color.black", "Đen", "Black");

        // Food
        AddString("food.common", "Thường", "Common");
        AddString("food.rare", "Hiếm", "Rare");
        AddString("food.epic", "Cực Hiếm", "Epic");

        // Notifications
        AddString("notify.paused", "Đã tạm dừng", "Game Paused");
        AddString("notify.resumed", "Tiếp tục chơi", "Game Resumed");
        AddString("notify.saved", "Đã lưu", "Saved");

        Debug.Log($"[LocalizationManager] ✅ Loaded {localizedStrings.Count} localized strings");
    }

    private void AddString(string key, string vietnamese, string english)
    {
        if (!localizedStrings.ContainsKey(key))
        {
            localizedStrings.Add(key, new LocalizedString(vietnamese, english));
        }
        else
        {
            Debug.LogWarning($"[LocalizationManager] Key already exists: {key}");
        }
    }

    public string GetLocalizedString(string key)
    {
        if (localizedStrings.ContainsKey(key))
        {
            return localizedStrings[key].GetText(currentLanguage);
        }

        Debug.LogWarning($"[LocalizationManager] Missing key: {key}");
        return $"[MISSING: {key}]";
    }

    // ✅ CHỈ đổi ngôn ngữ khi USER CHỌN
    public void SetLanguage(Language newLanguage)
    {
        if (currentLanguage == newLanguage)
        {
            Debug.Log($"[LocalizationManager] Language already set to: {newLanguage}");
            return;
        }

        currentLanguage = newLanguage;

        // ✅ Lưu lựa chọn của user
        PlayerPrefs.SetInt(languagePrefsKey, (int)newLanguage);
        PlayerPrefs.Save();

        // ✅ Apply ngay lập tức
        RefreshAllLocalizedTexts();

        Debug.Log($"[LocalizationManager] ✅ Language changed to: {newLanguage}");
    }

    private void RefreshAllLocalizedTexts()
    {
        LocalizedText[] allLocalizedTexts = FindObjectsByType<LocalizedText>(FindObjectsSortMode.None);

        Debug.Log($"[LocalizationManager] 🔄 Refreshing {allLocalizedTexts.Length} localized texts");

        foreach (LocalizedText localizedText in allLocalizedTexts)
        {
            localizedText.RefreshText();
        }
    }

    public string GetLanguageName(Language lang)
    {
        return lang switch
        {
            Language.Vietnamese => "Tiếng Việt",
            Language.English => "English",
            _ => "Unknown"
        };
    }

    public void ToggleLanguage()
    {
        Language newLang = currentLanguage == Language.Vietnamese ? Language.English : Language.Vietnamese;
        SetLanguage(newLang);
    }
}