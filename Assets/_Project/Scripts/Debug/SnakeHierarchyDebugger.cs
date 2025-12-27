using UnityEngine;

/// <summary>
/// Debug tool để xem CHÍNH XÁC hierarchy của Snake
/// Nhấn F5 để print toàn bộ cấu trúc
/// </summary>
public class SnakeHierarchyDebugger : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            DebugSnakeHierarchy();
        }
    }

    [ContextMenu("Debug Snake Hierarchy")]
    public void DebugSnakeHierarchy()
    {
        Debug.Log("========================================");
        Debug.Log("🔍 SNAKE HIERARCHY DEBUGGER");
        Debug.Log("========================================");

        if (GameManager.Instance == null)
        {
            Debug.LogError("❌ GameManager.Instance is NULL!");
            return;
        }

        var snakes = GameManager.Instance.GetAllSnakes();
        Debug.Log($"Found {snakes.Count} snake(s)\n");

        foreach (var snake in snakes)
        {
            if (snake == null) continue;

            Debug.Log($"--- 🐍 {snake.PlayerName} ---");
            Debug.Log($"GameObject: {snake.gameObject.name}");
            Debug.Log($"Direct children: {snake.transform.childCount}");
            
            PrintHierarchy(snake.transform, 0);
            Debug.Log("");
        }

        Debug.Log("========================================\n");
    }

    private void PrintHierarchy(Transform root, int depth)
    {
        string indent = new string(' ', depth * 2);

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            
            // Get components info
            string info = $"{indent}[{i}] {child.name}";
            
            var sr = child.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                info += $" [SR: #{ColorUtility.ToHtmlStringRGB(sr.color)}]";
            }

            var visualizer = child.GetComponent<SnakeSegmentVisualizer>();
            if (visualizer != null)
            {
                info += " [Has Visualizer]";
            }

            if (child.childCount > 0)
            {
                info += $" ({child.childCount} children)";
            }

            Debug.Log(info);

            // Recursive for children
            if (child.childCount > 0)
            {
                PrintHierarchy(child, depth + 1);
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(Screen.width - 310, Screen.height - 100, 300, 90));
        
        GUI.backgroundColor = Color.cyan;
        GUILayout.Box("=== HIERARCHY DEBUGGER ===");
        GUI.backgroundColor = Color.white;
        
        GUILayout.Label("F5: Debug Snake Hierarchy");
        GUILayout.Label("Shows complete GameObject tree");
        
        if (GameManager.Instance != null)
        {
            var snakes = GameManager.Instance.GetAllSnakes();
            GUILayout.Label($"Snakes: {snakes.Count}");
        }

        GUILayout.EndArea();
    }
}
