using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    [Header("Localization")]
    [SerializeField] private string localizationKey;
    [SerializeField] private bool updateOnEnable = true;

    [Header("Vietnamese Font Support")]
    [SerializeField] private TMP_FontAsset vietnameseFont;
    [SerializeField] private bool autoSwitchFont = true;

    private TMP_Text textComponent;
    private TMP_FontAsset originalFont;
    private bool hasRefreshed = false; // ✅ Prevent multiple refreshes

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
        if (textComponent != null)
        {
            originalFont = textComponent.font;
        }
    }

    // ✅ FIX: Dùng Start thay vì OnEnable
    private void Start()
    {
        if (updateOnEnable)
        {
            RefreshText();
        }
    }

    // ✅ REMOVE OnEnable để tránh gọi quá sớm
    // private void OnEnable() { ... }

    public void RefreshText()
    {
        if (textComponent == null)
        {
            textComponent = GetComponent<TMP_Text>();
        }

        if (textComponent == null)
        {
            Debug.LogError("[LocalizedText] Không tìm thấy TMP_Text component!");
            return;
        }

        // ✅ Kiểm tra LocalizationManager đã sẵn sàng chưa
        if (LocalizationManager.Instance == null)
        {
            Debug.LogWarning($"[LocalizedText] LocalizationManager not ready yet on {gameObject.name}");
            return;
        }

        if (!string.IsNullOrEmpty(localizationKey))
        {
            textComponent.text = LocalizationManager.Instance.GetLocalizedString(localizationKey);

            // Auto switch font cho tiếng Việt
            if (autoSwitchFont && vietnameseFont != null)
            {
                if (LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese)
                {
                    textComponent.font = vietnameseFont;
                }
                else if (originalFont != null)
                {
                    textComponent.font = originalFont;
                }
            }

            hasRefreshed = true;
        }
        else
        {
            Debug.LogWarning($"[LocalizedText] Localization key trống trên {gameObject.name}");
        }
    }

    public void SetKey(string key)
    {
        localizationKey = key;
        RefreshText();
    }

    public string GetKey()
    {
        return localizationKey;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying && textComponent != null && !string.IsNullOrEmpty(localizationKey))
        {
            RefreshText();
        }
    }
#endif
}