using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    [Header("Localization")]
    [SerializeField] private string localizationKey;
    [SerializeField] private bool updateOnEnable = true;

    private TMP_Text textComponent;

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (updateOnEnable)
        {
            RefreshText();
        }
    }

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

        if (LocalizationManager.Instance != null && !string.IsNullOrEmpty(localizationKey))
        {
            textComponent.text = LocalizationManager.Instance.GetLocalizedString(localizationKey);
        }
        else if (string.IsNullOrEmpty(localizationKey))
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