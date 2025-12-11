using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AnimatedButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Animation")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float clickScale = 0.95f;
    [SerializeField] private float animationSpeed = 0.2f;

    [Header("Audio")]
    [SerializeField] private bool playHoverSound = true;
    [SerializeField] private bool playClickSound = true;
    [SerializeField] private string hoverSoundName = "ButtonHover";
    [SerializeField] private string clickSoundName = "ButtonClick";

    [Header("Visual Effects")]
    [SerializeField] private bool enableGlow = false;
    [SerializeField] private Image glowImage;
    [SerializeField] private Color glowColor = Color.cyan;

    private Button button;
    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isHovering = false;
    private bool isPressed = false;
    private Coroutine scaleCoroutine;

    private void Awake()
    {
        button = GetComponent<Button>();
        originalScale = transform.localScale;
        targetScale = originalScale;

        if (enableGlow && glowImage != null)
        {
            glowImage.color = new Color(glowColor.r, glowColor.g, glowColor.b, 0f);
        }
    }

    private void Update()
    {
        // Smooth scale animation
        if (Vector3.Distance(transform.localScale, targetScale) > 0.001f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime / animationSpeed);
        }

        // Glow pulse when hovering
        if (enableGlow && glowImage != null && isHovering && !isPressed)
        {
            float alpha = (Mathf.Sin(Time.unscaledTime * 5f) + 1f) * 0.25f;
            glowImage.color = new Color(glowColor.r, glowColor.g, glowColor.b, alpha);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable) return;

        isHovering = true;
        targetScale = originalScale * hoverScale;

        if (playHoverSound && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(hoverSoundName);
        }

        if (enableGlow && glowImage != null)
        {
            glowImage.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;

        if (!isPressed)
        {
            targetScale = originalScale;
        }

        if (enableGlow && glowImage != null)
        {
            glowImage.color = new Color(glowColor.r, glowColor.g, glowColor.b, 0f);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable) return;

        isPressed = true;
        targetScale = originalScale * clickScale;

        if (playClickSound && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clickSoundName);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        targetScale = isHovering ? originalScale * hoverScale : originalScale;
    }

    private void OnDisable()
    {
        // Reset state
        transform.localScale = originalScale;
        isHovering = false;
        isPressed = false;
    }
}