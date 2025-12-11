using UnityEngine;

public class GlowPulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float minScale = 1.4f;
    [SerializeField] private float maxScale = 1.8f;
    [SerializeField] private bool pulseAlpha = true;
    [SerializeField] private float minAlpha = 0.3f;
    [SerializeField] private float maxAlpha = 0.7f;

    private Vector3 originalScale;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private void Awake()
    {
        originalScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    private void Update()
    {
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

        // Scale animation
        float scale = Mathf.Lerp(minScale, maxScale, t);
        transform.localScale = originalScale * scale;

        // Alpha animation
        if (pulseAlpha && spriteRenderer != null)
        {
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
            Color color = originalColor;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }
}