using UnityEngine;

/// <summary>
/// Tạo outline (viền) cho sprite để rắn đẹp hơn
/// Attach vào GameObject có SpriteRenderer
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class OutlineEffect : MonoBehaviour
{
    [Header("Outline Settings")]
    [SerializeField] private Color outlineColor = Color.black;
    [SerializeField][Range(0.01f, 0.5f)] private float outlineSize = 0.1f;
    [SerializeField] private bool enableOutline = true;

    private SpriteRenderer spriteRenderer;
    private GameObject[] outlineObjects;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (enableOutline)
        {
            CreateOutline();
        }
    }

    private void CreateOutline()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
        {
            Debug.LogWarning($"[OutlineEffect] No SpriteRenderer or Sprite on {gameObject.name}");
            return;
        }

        // Tạo 4 copies để làm outline (trên, dưới, trái, phải)
        outlineObjects = new GameObject[4];

        Vector3[] offsets = new Vector3[]
        {
            new Vector3(outlineSize, 0, 0),      // Phải
            new Vector3(-outlineSize, 0, 0),     // Trái
            new Vector3(0, outlineSize, 0),      // Trên
            new Vector3(0, -outlineSize, 0)      // Dưới
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject outline = new GameObject($"Outline_{i}");
            outline.transform.SetParent(transform);
            outline.transform.localPosition = offsets[i];
            outline.transform.localRotation = Quaternion.identity;
            outline.transform.localScale = Vector3.one;

            SpriteRenderer outlineSr = outline.AddComponent<SpriteRenderer>();
            outlineSr.sprite = spriteRenderer.sprite;
            outlineSr.color = outlineColor;
            outlineSr.sortingLayerName = spriteRenderer.sortingLayerName;
            outlineSr.sortingOrder = spriteRenderer.sortingOrder - 1;

            outlineObjects[i] = outline;
        }

        Debug.Log($"[OutlineEffect] Created outline for {gameObject.name}");
    }

    /// <summary>
    /// Cập nhật màu outline (gọi từ script khác nếu cần)
    /// </summary>
    public void SetOutlineColor(Color newColor)
    {
        outlineColor = newColor;

        if (outlineObjects != null)
        {
            foreach (GameObject outline in outlineObjects)
            {
                if (outline != null)
                {
                    SpriteRenderer sr = outline.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.color = newColor;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Bật/tắt outline
    /// </summary>
    public void SetOutlineEnabled(bool enabled)
    {
        enableOutline = enabled;

        if (outlineObjects != null)
        {
            foreach (GameObject outline in outlineObjects)
            {
                if (outline != null)
                {
                    outline.SetActive(enabled);
                }
            }
        }
    }

    private void OnDestroy()
    {
        // Cleanup khi object bị destroy
        if (outlineObjects != null)
        {
            foreach (GameObject outline in outlineObjects)
            {
                if (outline != null)
                {
                    Destroy(outline);
                }
            }
        }
    }

#if UNITY_EDITOR
    // Vẽ gizmo trong Scene view để debug
    private void OnDrawGizmosSelected()
    {
        if (!enableOutline) return;

        Gizmos.color = outlineColor;
        Vector3 pos = transform.position;

        // Vẽ outline preview
        Gizmos.DrawWireCube(pos, Vector3.one * (1 + outlineSize * 2));
    }
#endif
}