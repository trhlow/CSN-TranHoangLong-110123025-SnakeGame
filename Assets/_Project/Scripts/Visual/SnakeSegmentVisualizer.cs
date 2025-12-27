using UnityEngine;

/// <summary>
/// ✅ FIXED: Component để làm cho snake segments trông đẹp hơn và ÁP DỤNG MÀU ĐÚNG
/// Attach vào Head, Body, Tail prefabs
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SnakeSegmentVisualizer : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private SegmentType segmentType = SegmentType.Body;
    [SerializeField] private bool useGradient = true;
    [SerializeField] private float gradientIntensity = 0.3f;

    private SpriteRenderer spriteRenderer;
    private Color baseColor;
    private bool colorWasSet = false; // ✅ Track if color was explicitly set

    public enum SegmentType
    {
        Head,
        Body,
        Tail
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // ✅ FIX: Only use default color if not explicitly set
        if (!colorWasSet)
        {
            baseColor = spriteRenderer.color;
        }
    }

    private void Start()
    {
        // ✅ FIX: Only apply default style if color wasn't set externally
        if (!colorWasSet)
        {
            ApplyVisualStyle();
        }
    }

    private void ApplyVisualStyle()
    {
        if (spriteRenderer == null) return;

        // Làm tròn sprite
        spriteRenderer.drawMode = SpriteDrawMode.Simple;

        // Thêm gradient dựa vào segment type
        if (useGradient)
        {
            Color targetColor = baseColor;

            switch (segmentType)
            {
                case SegmentType.Head:
                    // Head giữ nguyên màu base
                    targetColor = baseColor;
                    targetColor.a = 1f;
                    break;

                case SegmentType.Body:
                    // Body hơi tối hơn
                    targetColor = baseColor * 0.95f;
                    targetColor.a = 1f;
                    break;

                case SegmentType.Tail:
                    // Tail tối hơn nữa
                    targetColor = baseColor * 0.85f;
                    targetColor.a = 1f;
                    break;
            }

            spriteRenderer.color = targetColor;
        }
        else
        {
            spriteRenderer.color = baseColor;
        }

        // Tăng sorting order để head luôn nằm trên
        if (segmentType == SegmentType.Head)
        {
            spriteRenderer.sortingOrder = 10;
        }
        else if (segmentType == SegmentType.Tail)
        {
            spriteRenderer.sortingOrder = 1;
        }
        else
        {
            spriteRenderer.sortingOrder = 5;
        }
    }

    /// <summary>
    /// ✅ FIX: Set màu và áp dụng ngay lập tức
    /// </summary>
    public void SetColor(Color color)
    {
        baseColor = color;
        colorWasSet = true; // ✅ Mark that color was explicitly set
        
        // ✅ Đảm bảo spriteRenderer đã được init
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        ApplyVisualStyle();
        
        // ✅ Debug log
        if (spriteRenderer != null)
        {
            Debug.Log($"[Visualizer] Set color for {gameObject.name}: #{ColorUtility.ToHtmlStringRGB(spriteRenderer.color)}");
        }
    }

    /// <summary>
    /// Set loại segment và áp dụng style
    /// </summary>
    public void SetSegmentType(SegmentType type)
    {
        segmentType = type;
        
        if (colorWasSet)
        {
            ApplyVisualStyle();
        }
    }

    /// <summary>
    /// Force update màu (dùng khi cần refresh)
    /// </summary>
    public void RefreshColor()
    {
        if (spriteRenderer != null && colorWasSet)
        {
            ApplyVisualStyle();
        }
    }
}
