using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SimpleBackground : MonoBehaviour
{
    [Header("Colors")]
    [SerializeField] private Color topColor = new Color(0.06f, 0.05f, 0.16f);
    [SerializeField] private Color bottomColor = new Color(0.14f, 0.14f, 0.24f);

    [Header("Settings")]
    [SerializeField] private int textureWidth = 1920;
    [SerializeField] private int textureHeight = 1080;
    [SerializeField] private bool generateOnStart = true;

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (generateOnStart)
        {
            GenerateGradient();
        }
    }

    [ContextMenu("Generate Gradient")]
    public void GenerateGradient()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer == null)
        {
            Debug.LogError("[SimpleBackground] Không tìm thấy SpriteRenderer!");
            return;
        }

        Texture2D texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGB24, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < textureHeight; y++)
        {
            float t = (float)y / textureHeight;
            Color color = Color.Lerp(bottomColor, topColor, t);

            for (int x = 0; x < textureWidth; x++)
            {
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, textureWidth, textureHeight),
            new Vector2(0.5f, 0.5f),
            100f
        );

        spriteRenderer.sprite = sprite;

        Debug.Log("[SimpleBackground] Đã tạo gradient!");
    }
}