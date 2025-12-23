using UnityEngine;

/// <summary>
/// Component để làm cho snake segments trông đẹp hơn
/// Attach vào Head, Body, Tail prefabs
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SnakeSegmentVisualizer : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private SegmentType segmentType = SegmentType.Body;
    [SerializeField] private bool useGradient = true;
    [SerializeField] private float gradientIntensity = 0.3f;

    [Header("Smooth Settings")]
    [SerializeField] private bool smoothMovement = true;
    [SerializeField] private float smoothSpeed = 10f;

    private SpriteRenderer spriteRenderer;
    private Transform targetPosition;
    private Color baseColor;

    public enum SegmentType
    {
        Head,
        Body,
        Tail
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseColor = spriteRenderer.color;
    }

    private void Start()
    {
        ApplyVisualStyle();
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
                    // Head sáng nhất
                    targetColor = baseColor * 1.2f;
                    targetColor.a = 1f;
                    break;

                case SegmentType.Body:
                    // Body màu trung bình
                    targetColor = baseColor * 1.0f;
                    break;

                case SegmentType.Tail:
                    // Tail tối hơn
                    targetColor = baseColor * 0.8f;
                    break;
            }

            spriteRenderer.color = targetColor;
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

    public void SetColor(Color color)
    {
        baseColor = color;
        ApplyVisualStyle();
    }

    public void SetSegmentType(SegmentType type)
    {
        segmentType = type;
        ApplyVisualStyle();
    }
}