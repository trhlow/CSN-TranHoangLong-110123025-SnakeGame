using UnityEngine;

/// <summary>
/// Script debug để test hệ thống lưu/load màu rắn
/// Attach vào GameObject bất kỳ trong scene Settings hoặc Gameplay
/// </summary>
public class ColorSystemDebugger : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool enableDebug = true;
    [SerializeField] private KeyCode testSaveKey = KeyCode.F5;
    [SerializeField] private KeyCode testLoadKey = KeyCode.F6;
    [SerializeField] private KeyCode clearSaveKey = KeyCode.F7;

    [Header("Test Color")]
    [SerializeField] private Color testColor = Color.red;

    private void Update()
    {
        if (!enableDebug) return;

        // Test Save
        if (Input.GetKeyDown(testSaveKey))
        {
            TestSaveColor();
        }

        // Test Load
        if (Input.GetKeyDown(testLoadKey))
        {
            TestLoadColor();
        }

        // Clear Save
        if (Input.GetKeyDown(clearSaveKey))
        {
            ClearColorSave();
        }
    }

    private void TestSaveColor()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[ColorDebug] ❌ GameManager.Instance is NULL!");
            return;
        }

        GameManager.Instance.SaveSnakeColor(1, testColor);
        Debug.Log($"[ColorDebug] ✅ Saved test color: #{ColorUtility.ToHtmlStringRGB(testColor)}");
        Debug.Log($"[ColorDebug] 💾 Key: Player1_SnakeColor");
    }

    private void TestLoadColor()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[ColorDebug] ❌ GameManager.Instance is NULL!");
            return;
        }

        Color loaded = GameManager.Instance.LoadSnakeColor(1, Color.white);
        Debug.Log($"[ColorDebug] 📂 Loaded color: #{ColorUtility.ToHtmlStringRGB(loaded)}");

        // Verify with direct PlayerPrefs access
        string key = "Player1_SnakeColor";
        if (PlayerPrefs.HasKey(key))
        {
            string hexValue = PlayerPrefs.GetString(key);
            Debug.Log($"[ColorDebug] 🔍 PlayerPrefs value: {hexValue}");
        }
        else
        {
            Debug.LogWarning($"[ColorDebug] ⚠️ Key '{key}' not found in PlayerPrefs!");
        }
    }

    private void ClearColorSave()
    {
        string key = "Player1_SnakeColor";
        if (PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
            Debug.Log($"[ColorDebug] 🗑️ Cleared save: {key}");
        }
        else
        {
            Debug.LogWarning($"[ColorDebug] ⚠️ No save to clear for {key}");
        }
    }

    private void OnGUI()
    {
        if (!enableDebug) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.Label("=== Color System Debugger ===");
        GUILayout.Label($"F5: Save test color (#{ColorUtility.ToHtmlStringRGB(testColor)})");
        GUILayout.Label($"F6: Load saved color");
        GUILayout.Label($"F7: Clear saved color");

        if (PlayerPrefs.HasKey("Player1_SnakeColor"))
        {
            string saved = PlayerPrefs.GetString("Player1_SnakeColor");
            GUILayout.Label($"Current: {saved}");
        }
        else
        {
            GUILayout.Label("Current: (None)");
        }

        GUILayout.EndArea();
    }
}
