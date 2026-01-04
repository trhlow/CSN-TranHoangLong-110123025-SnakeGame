using UnityEngine;

/// <summary>
/// Tạo hiệu ứng phát sáng (glow) cho sprite
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class GlowEffect : MonoBehaviour
{
    [Header("Glow Settings")]
    [SerializeField] private Color glowColor = new Color(0, 1, 0, 0.5f); // Xanh lá trong suốt
    [SerializeField][Range(1f, 3f)] private float glowSize = 1.5f;
    [SerializeField][Range(0f, 1f)] private float pulseSpeed = 1f;
    [SerializeField] private bool enablePulse = true;

    private SpriteRenderer spriteRenderer;
    private GameObject glowObject;
    private SpriteRenderer glowRenderer;
    private float pulseTimer = 0f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        CreateGlow();
    }

    private void CreateGlow()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
        {
            Debug.LogWarning($"[GlowEffect] No SpriteRenderer on {gameObject.name}");
            return;
        }

        // Tạo glow object
        glowObject = new GameObject("Glow");
        glowObject.transform.SetParent(transform);
        glowObject.transform.localPosition = Vector3.zero;
        glowObject.transform.localRotation = Quaternion.identity;
        glowObject.transform.localScale = Vector3.one * glowSize;

        // Setup sprite renderer cho glow
        glowRenderer = glowObject.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = spriteRenderer.sprite;
        glowRenderer.color = glowColor;
        glowRenderer.sortingLayerName = spriteRenderer.sortingLayerName;
        glowRenderer.sortingOrder = spriteRenderer.sortingOrder - 2; // Phía sau outline

        Debug.Log($"[GlowEffect] Created glow for {gameObject.name}");
    }

    private void Update()
    {
        if (!enablePulse || glowRenderer == null) return;

        // Pulse effect
        pulseTimer += Time.deltaTime * pulseSpeed;
        float alpha = (Mathf.Sin(pulseTimer * Mathf.PI * 2f) + 1f) * 0.5f; // 0 → 1

        Color currentColor = glowColor;
        currentColor.a = glowColor.a * alpha;
        glowRenderer.color = currentColor;
    }

    /// <summary>
    /// Thay đổi màu glow
    /// </summary>
    public void SetGlowColor(Color newColor)
    {
        glowColor = newColor;
        if (glowRenderer != null)
        {
            glowRenderer.color = glowColor;
        }
    }

    /// <summary>
    /// Bật/tắt pulse
    /// </summary>
    public void SetPulseEnabled(bool enabled)
    {
        enablePulse = enabled;
    }

    private void OnDestroy()
    {
        if (glowObject != null)
        {
            Destroy(glowObject);
        }
    }
}