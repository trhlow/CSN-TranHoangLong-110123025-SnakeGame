using UnityEngine;

/// <summary>
/// ✅ ULTIMATE COLOR FIX DEBUGGER
/// Attach vào GameObject bất kỳ trong Gameplay scene để test
/// </summary>
public class UltimateColorDebugger : MonoBehaviour
{
    [Header("Test Keys")]
    [SerializeField] private KeyCode refreshColorKey = KeyCode.F8;
    [SerializeField] private KeyCode checkSpeedKey = KeyCode.F9;
    [SerializeField] private KeyCode forceColorKey = KeyCode.F10;

    [Header("Test Color")]
    [SerializeField] private Color testColor = Color.red;

    private void Update()
    {
        // F8: Refresh màu cho tất cả snakes
        if (Input.GetKeyDown(refreshColorKey))
        {
            RefreshAllSnakeColors();
        }

        // F9: Check tốc độ
        if (Input.GetKeyDown(checkSpeedKey))
        {
            CheckAllSpeeds();
        }

        // F10: Force apply test color cho Player
        if (Input.GetKeyDown(forceColorKey))
        {
            ForceApplyTestColor();
        }
    }

    private void RefreshAllSnakeColors()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[ColorDebug] ❌ GameManager.Instance is NULL!");
            return;
        }

        var snakes = GameManager.Instance.GetAllSnakes();
        Debug.Log($"[ColorDebug] 🎨 Refreshing colors for {snakes.Count} snake(s)...");

        foreach (var snake in snakes)
        {
            if (snake == null) continue;

            Color snakeColor = snake.SnakeColor;
            Debug.Log($"[ColorDebug] 🐍 {snake.PlayerName}: #{ColorUtility.ToHtmlStringRGB(snakeColor)}");

            // Force refresh color
            snake.SetSnakeColor(snakeColor);
        }
    }

    private void CheckAllSpeeds()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[ColorDebug] ❌ GameManager.Instance is NULL!");
            return;
        }

        var snakes = GameManager.Instance.GetAllSnakes();
        Debug.Log($"[ColorDebug] ⚡ Checking speeds for {snakes.Count} snake(s)...");

        foreach (var snake in snakes)
        {
            if (snake == null) continue;

            float speed = snake.GetMoveInterval();
            Debug.Log($"[ColorDebug] 🐍 {snake.PlayerName}: {speed:F3}s/move (AI: {snake.IsAIControlled})");
        }
    }

    private void ForceApplyTestColor()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[ColorDebug] ❌ GameManager.Instance is NULL!");
            return;
        }

        var player = GameManager.Instance.GetSnakeByID(1);
        if (player != null)
        {
            Debug.Log($"[ColorDebug] 🎨 Forcing test color: #{ColorUtility.ToHtmlStringRGB(testColor)}");
            player.SetSnakeColor(testColor);
            GameManager.Instance.SaveSnakeColor(1, testColor);
        }
        else
        {
            Debug.LogError("[ColorDebug] ❌ Player snake not found!");
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 400, 200));
        GUILayout.Label("=== ULTIMATE COLOR & SPEED DEBUGGER ===");
        GUILayout.Label("F8: Refresh all snake colors");
        GUILayout.Label("F9: Check all speeds");
        GUILayout.Label($"F10: Force apply test color (#{ColorUtility.ToHtmlStringRGB(testColor)})");
        GUILayout.Space(10);

        if (GameManager.Instance != null)
        {
            var snakes = GameManager.Instance.GetAllSnakes();
            GUILayout.Label($"Total Snakes: {snakes.Count}");

            foreach (var snake in snakes)
            {
                if (snake == null) continue;
                GUILayout.Label($"- {snake.PlayerName}: #{ColorUtility.ToHtmlStringRGB(snake.SnakeColor)} @ {snake.GetMoveInterval():F3}s");
            }
        }

        GUILayout.EndArea();
    }
}
