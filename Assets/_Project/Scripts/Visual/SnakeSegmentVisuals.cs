using System.Collections;
using UnityEngine;

public class SnakeSegmentVisuals : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer mainRenderer;
    [SerializeField] private SpriteRenderer glowRenderer;
    [SerializeField] private SpriteRenderer coreGlowRenderer;
    [SerializeField] private TrailRenderer trailRenderer;

    [Header("Settings")]
    [SerializeField] private bool isHead = false;
    [SerializeField] private float headScaleMultiplier = 1.2f;

    [Header("Color")]
    [SerializeField] private Color segmentColor = Color.green;

    private Vector3 baseScale;

    private void Awake()
    {
        baseScale = transform.localScale;

        // Auto-assign if not set
        if (mainRenderer == null)
            mainRenderer = GetComponent<SpriteRenderer>();

        ApplyColor(segmentColor);
    }

    public void SetAsHead(bool head)
    {
        isHead = head;
        transform.localScale = baseScale * (head ? headScaleMultiplier : 1f);
    }

    public void ApplyColor(Color color)
    {
        segmentColor = color;

        if (mainRenderer != null)
            mainRenderer.color = color;

        if (glowRenderer != null)
        {
            Color glowColor = color;
            glowColor.a = 0.5f;
            glowRenderer.color = glowColor;
        }

        if (coreGlowRenderer != null)
        {
            Color coreColor = Color.white;
            coreColor.a = 0.8f;
            coreGlowRenderer.color = coreColor;
        }

        if (trailRenderer != null)
        {
            Gradient gradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(color, 0f),
                new GradientColorKey(color, 1f)
            };
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.5f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            };
            gradient.SetKeys(colorKeys, alphaKeys);
            trailRenderer.colorGradient = gradient;
        }
    }

    public void PlayEatAnimation()
    {
        StartCoroutine(EatAnimationCoroutine());
    }

    private IEnumerator EatAnimationCoroutine()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.3f;

        float duration = 0.1f;
        float elapsed = 0f;

        // Scale up
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            yield return null;
        }

        elapsed = 0f;

        // Scale down
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
            yield return null;
        }

        transform.localScale = originalScale;
    }
}