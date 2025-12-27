using UnityEngine;

/// <summary>
/// ✅ CRITICAL DIAGNOSTIC: Check tại sao màu không hiển thị
/// Attach vào GameObject trong Gameplay scene
/// </summary>
public class ColorDiagnosticTool : MonoBehaviour
{
    [Header("Auto Run")]
    [SerializeField] private bool runOnStart = true;
    [SerializeField] private float checkInterval = 2f;

    private float lastCheckTime = 0f;

    private void Start()
    {
        if (runOnStart)
        {
            Invoke(nameof(RunDiagnostic), 1f);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F11))
        {
            RunDiagnostic();
        }

        // Auto check every interval
        if (Time.time - lastCheckTime > checkInterval)
        {
            RunDiagnostic();
            lastCheckTime = Time.time;
        }
    }

    [ContextMenu("Run Diagnostic")]
    public void RunDiagnostic()
    {
        Debug.Log("=====================================");
        Debug.Log("🔍 COLOR DIAGNOSTIC TOOL - STARTING");
        Debug.Log("=====================================");

        if (GameManager.Instance == null)
        {
            Debug.LogError("❌ GameManager.Instance is NULL!");
            return;
        }

        var snakes = GameManager.Instance.GetAllSnakes();
        Debug.Log($"📊 Found {snakes.Count} snake(s)");

        foreach (var snake in snakes)
        {
            if (snake == null) continue;

            Debug.Log($"\n--- 🐍 {snake.PlayerName} ---");
            Debug.Log($"Snake Color: #{ColorUtility.ToHtmlStringRGB(snake.SnakeColor)}");
            Debug.Log($"Is AI: {snake.IsAIControlled}");
            Debug.Log($"Segment Count: {snake.transform.childCount}");

            // Check each segment
            CheckSnakeSegments(snake);
        }

        Debug.Log("\n=====================================");
        Debug.Log("🔍 DIAGNOSTIC COMPLETE");
        Debug.Log("=====================================\n");
    }

    private void CheckSnakeSegments(SnakeController snake)
    {
        // ✅ CRITICAL FIX: Tìm đúng Segments container
        Transform segmentsContainer = null;
        
        // Thử tìm container tên "Segments"
        foreach (Transform child in snake.transform)
        {
            if (child.name == "Segments" || child.name.Contains("Segment"))
            {
                segmentsContainer = child;
                Debug.Log($"  Found Segments container: {child.name}");
                break;
            }
        }
        
        // Nếu không tìm thấy, dùng snake.transform
        if (segmentsContainer == null)
        {
            Debug.LogWarning($"⚠️ {snake.PlayerName}: No 'Segments' container found, using snake root");
            segmentsContainer = snake.transform;
        }

        int segmentCount = segmentsContainer.childCount;
        Debug.Log($"  Checking {segmentCount} segments in '{segmentsContainer.name}'...");

        for (int i = 0; i < segmentCount; i++)
        {
            Transform segment = segmentsContainer.GetChild(i);
            if (segment == null) continue;

            // Check SpriteRenderer
            var sr = segment.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Debug.Log($"    [{i}] {segment.name}:");
                Debug.Log($"        SpriteRenderer.color: #{ColorUtility.ToHtmlStringRGB(sr.color)}");
                Debug.Log($"        Enabled: {sr.enabled}");
                Debug.Log($"        Sprite: {(sr.sprite != null ? sr.sprite.name : "NULL")}");
            }
            else
            {
                Debug.LogWarning($"    [{i}] {segment.name}: ❌ NO SpriteRenderer!");
            }

            // Check Visualizer
            var visualizer = segment.GetComponent<SnakeSegmentVisualizer>();
            if (visualizer != null)
            {
                Debug.Log($"        Has SnakeSegmentVisualizer: ✅");
            }
            else
            {
                Debug.Log($"        Has SnakeSegmentVisualizer: ❌");
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, Screen.height - 150, 400, 140));
        GUILayout.Label("=== COLOR DIAGNOSTIC TOOL ===");
        GUILayout.Label("F11: Run diagnostic manually");
        GUILayout.Label($"Auto-check every {checkInterval}s");
        
        if (GameManager.Instance != null)
        {
            var snakes = GameManager.Instance.GetAllSnakes();
            GUILayout.Label($"Snakes: {snakes.Count}");
            
            foreach (var snake in snakes)
            {
                if (snake == null) continue;
                Transform container = snake.transform.Find("Segments");
                if (container == null) container = snake.transform;
                GUILayout.Label($"- {snake.PlayerName}: {container.childCount} segments");
            }
        }

        GUILayout.EndArea();
    }
}
