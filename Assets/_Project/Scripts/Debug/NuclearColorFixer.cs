using UnityEngine;

/// <summary>
/// NUCLEAR OPTION: Force màu cho TẤT CẢ SpriteRenderer, disable tất cả effects
/// Nhấn F6 để FORCE màu TRỰC TIẾP không qua bất cứ thứ gì
/// </summary>
public class NuclearColorFixer : MonoBehaviour
{
    [Header("Test Color")]
    [SerializeField] private Color forceColor = Color.red;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F6))
        {
            NuclearFix();
        }
    }

    [ContextMenu("NUCLEAR FIX - Force All Colors")]
    public void NuclearFix()
    {
        Debug.Log("💥 NUCLEAR COLOR FIX - STARTING...");
        Debug.Log($"Target color: #{ColorUtility.ToHtmlStringRGB(forceColor)}");

        if (GameManager.Instance == null)
        {
            Debug.LogError("❌ GameManager is NULL!");
            return;
        }

        var snakes = GameManager.Instance.GetAllSnakes();
        Debug.Log($"Found {snakes.Count} snake(s)");

        foreach (var snake in snakes)
        {
            if (snake == null) continue;
            ForceFixSnakeNuclear(snake);
        }

        Debug.Log("💥 NUCLEAR FIX COMPLETE!\n");
    }

    private void ForceFixSnakeNuclear(SnakeController snake)
    {
        Debug.Log($"💥 Nuclear fixing: {snake.PlayerName}");

        // ✅ Tìm TẤT CẢ SpriteRenderer trong snake (recursive)
        var allSpriteRenderers = snake.GetComponentsInChildren<SpriteRenderer>(true);
        
        Debug.Log($"  Found {allSpriteRenderers.Length} SpriteRenderer(s)");

        int fixedCount = 0;
        foreach (var sr in allSpriteRenderers)
        {
            if (sr == null) continue;

            // 🔥 DISABLE all effects trên GameObject này
            var outline = sr.GetComponent<OutlineEffect>();
            if (outline != null)
            {
                outline.enabled = false;
                Debug.Log($"    Disabled OutlineEffect on {sr.name}");
            }

            var glow = sr.GetComponent<GlowEffect>();
            if (glow != null)
            {
                glow.enabled = false;
                Debug.Log($"    Disabled GlowEffect on {sr.name}");
            }

            // 🔥 FORCE màu trực tiếp
            sr.color = forceColor;
            sr.enabled = true;
            
            fixedCount++;
            Debug.Log($"  ✅ [{fixedCount}] {sr.gameObject.name}: FORCED #{ColorUtility.ToHtmlStringRGB(forceColor)}");
        }

        Debug.Log($"💥 {snake.PlayerName}: FORCED {fixedCount} SpriteRenderer(s)\n");
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, Screen.height - 200, 350, 190));
        
        GUI.backgroundColor = Color.red;
        GUILayout.Box("=== NUCLEAR COLOR FIXER ===");
        GUI.backgroundColor = Color.white;
        
        GUILayout.Label("💥 FORCE màu cho TẤT CẢ sprites");
        GUILayout.Label("💥 Disable TẤT CẢ effects");
        GUILayout.Label("");
        GUILayout.Label($"Test Color: #{ColorUtility.ToHtmlStringRGB(forceColor)}");
        GUILayout.Label("");
        GUILayout.Label("F6: NUCLEAR FIX NOW!");
        GUILayout.Label("");
        GUILayout.Label("⚠️ Sử dụng khi mọi thứ khác fail");

        GUILayout.EndArea();
    }
}
