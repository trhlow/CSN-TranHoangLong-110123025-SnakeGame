using UnityEngine;

/// <summary>
/// ✅ INSTANT COLOR FIXER
/// Nhấn F12 để FORCE update màu TẤT CẢ snakes NGAY LẬP TỨC
/// </summary>
public class InstantColorFixer : MonoBehaviour
{
    [Header("Hot Keys")]
    [SerializeField] private KeyCode fixAllKey = KeyCode.F12;
    [SerializeField] private KeyCode fixPlayerKey = KeyCode.F1;
    [SerializeField] private KeyCode fixAIKey = KeyCode.F2;

    private void Update()
    {
        // F12: Fix tất cả
        if (Input.GetKeyDown(fixAllKey))
        {
            FixAllSnakeColors();
        }

        // F1: Fix chỉ Player
        if (Input.GetKeyDown(fixPlayerKey))
        {
            FixSnakeByID(1);
        }

        // F2: Fix chỉ AI
        if (Input.GetKeyDown(fixAIKey))
        {
            FixSnakeByID(3);
        }
    }

    [ContextMenu("Fix All Snake Colors NOW!")]
    public void FixAllSnakeColors()
    {
        Debug.Log("🔧 INSTANT COLOR FIX - STARTING...");

        if (GameManager.Instance == null)
        {
            Debug.LogError("❌ GameManager.Instance is NULL!");
            return;
        }

        var snakes = GameManager.Instance.GetAllSnakes();
        Debug.Log($"Found {snakes.Count} snake(s) to fix");

        foreach (var snake in snakes)
        {
            if (snake == null) continue;
            ForceFixSnake(snake);
        }

        Debug.Log("✅ INSTANT COLOR FIX - COMPLETE!\n");
    }

    private void FixSnakeByID(int playerID)
    {
        if (GameManager.Instance == null) return;

        var snake = GameManager.Instance.GetSnakeByID(playerID);
        if (snake != null)
        {
            ForceFixSnake(snake);
        }
        else
        {
            Debug.LogWarning($"⚠️ Snake with ID {playerID} not found!");
        }
    }

    private void ForceFixSnake(SnakeController snake)
    {
        Debug.Log($"🎨 Fixing {snake.PlayerName} (Color: #{ColorUtility.ToHtmlStringRGB(snake.SnakeColor)})");

        // ✅ CRITICAL FIX: Tìm đúng Segments container
        Transform segmentsContainer = null;
        
        // Thử tìm container tên "Segments"
        foreach (Transform child in snake.transform)
        {
            if (child.name == "Segments" || child.name.Contains("Segment"))
            {
                segmentsContainer = child;
                Debug.Log($"  Found container: {child.name} with {child.childCount} children");
                break;
            }
        }
        
        // Nếu không tìm thấy, dùng snake.transform
        if (segmentsContainer == null)
        {
            Debug.LogWarning($"  No Segments container, using snake root");
            segmentsContainer = snake.transform;
        }

        int fixedCount = 0;
        int totalSegments = segmentsContainer.childCount;

        for (int i = 0; i < totalSegments; i++)
        {
            Transform segment = segmentsContainer.GetChild(i);
            if (segment == null) continue;

            // Determine segment type
            bool isHead = i == 0;
            bool isTail = i == totalSegments - 1;
            bool isBody = !isHead && !isTail;

            // Calculate color
            Color targetColor;
            if (isHead)
                targetColor = snake.SnakeColor;
            else if (isTail)
                targetColor = snake.SnakeColor * 0.85f;
            else
                targetColor = snake.SnakeColor * 0.95f;

            // FORCE set SpriteRenderer
            var sr = segment.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = targetColor;
                fixedCount++;
                
                string type = isHead ? "Head" : isTail ? "Tail" : "Body";
                Debug.Log($"  [{i}] {segment.name} ({type}): #{ColorUtility.ToHtmlStringRGB(targetColor)}");
            }
            else
            {
                Debug.LogWarning($"  [{i}] {segment.name}: NO SpriteRenderer!");
            }
        }

        Debug.Log($"✅ {snake.PlayerName}: Fixed {fixedCount}/{totalSegments} segments\n");
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(Screen.width - 310, 10, 300, 120));
        
        GUI.backgroundColor = Color.yellow;
        GUILayout.Box("=== INSTANT COLOR FIXER ===");
        GUI.backgroundColor = Color.white;
        
        GUILayout.Label("F12: Fix ALL snakes");
        GUILayout.Label("F1: Fix Player only");
        GUILayout.Label("F2: Fix AI only");
        
        if (GameManager.Instance != null)
        {
            var snakes = GameManager.Instance.GetAllSnakes();
            GUILayout.Label($"\nSnakes: {snakes.Count}");
        }

        GUILayout.EndArea();
    }
}
