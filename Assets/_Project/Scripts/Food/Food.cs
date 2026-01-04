using UnityEngine;

public enum FoodRarity
{
    Common,   // 70% - 10 điểm
    Rare,     // 25% - 25 điểm
    Epic      // 5%  - 50 điểm
}

[RequireComponent(typeof(Collider2D))]
public class Food : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private FoodRarity rarity = FoodRarity.Common;
    [SerializeField] private int points = 10;
    [SerializeField] private float spawnChance = 0.7f;

    [Header("Audio")]
    [SerializeField] private AudioClip eatSound;

    [Header("Effects")]
    [SerializeField] private GameObject eatEffectPrefab;

    public FoodRarity Rarity => rarity;
    public int Points => points;
    public float SpawnChance => spawnChance;

    // ✅ Legacy support
    public int diem => points;
    public DoHiemThucAn doHiem => (DoHiemThucAn)rarity;
    public Color mauThucAn { get; set; }

    private void Awake()
    {
        // ✅ SET TAG - QUAN TRỌNG!
        gameObject.tag = "Food";

        // ✅ Setup Collider2D
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<CircleCollider2D>();
        }
        collider.isTrigger = true;
        collider.radius = 0.35f; // Tăng lên một chút để dễ ăn hơn

        // ✅ Set layer (optional - để dễ filter collision)
        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    private void Start()
    {
        // ✅ Auto set points based on rarity nếu chưa set
        if (points == 0)
        {
            points = rarity switch
            {
                FoodRarity.Common => 10,
                FoodRarity.Rare => 25,
                FoodRarity.Epic => 50,
                _ => 10
            };
        }
    }

    /// <summary>
    /// Gọi khi food bị ăn - play effects và destroy
    /// </summary>
    public void OnEaten(Vector3 position)
    {
        // ✅ Play eat sound
        if (eatSound != null && AudioManager.Instance != null)
        {
            string soundKey = rarity switch
            {
                FoodRarity.Rare => "Eat_Rare",
                FoodRarity.Epic => "Eat_Epic",
                _ => "Eat_Common"
            };
            AudioManager.Instance.PlaySFX(soundKey);
        }

        // ✅ Spawn eat effect
        if (eatEffectPrefab != null)
        {
            GameObject effect = Instantiate(eatEffectPrefab, position, Quaternion.identity);

            // Set effect color based on food color
            EatEffectController effectCtrl = effect.GetComponent<EatEffectController>();
            if (effectCtrl != null)
            {
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                Color foodColor = sr != null ? sr.color : Color.white;
                effectCtrl.SetColor(foodColor);
            }
        }

        // ✅ Camera shake based on rarity
        if (CameraController.Instance != null)
        {
            float shakeMagnitude = rarity switch
            {
                FoodRarity.Epic => 0.3f,
                FoodRarity.Rare => 0.2f,
                _ => 0.1f
            };
            CameraController.Instance.Shake(0.15f, shakeMagnitude);
        }

        // ✅ Destroy
        Destroy(gameObject);
    }

    // ✅ Helper method để set food properties programmatically
    public void SetProperties(FoodRarity newRarity, Color color)
    {
        rarity = newRarity;
        points = newRarity switch
        {
            FoodRarity.Rare => 25,
            FoodRarity.Epic => 50,
            _ => 10
        };

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = color;
        }

        mauThucAn = color;
    }

    private void OnValidate()
    {
        // ✅ Auto update points in editor
        if (points == 0)
        {
            points = rarity switch
            {
                FoodRarity.Common => 10,
                FoodRarity.Rare => 25,
                FoodRarity.Epic => 50,
                _ => 10
            };
        }
    }
}

// ✅ Legacy enum cho compatibility
public enum DoHiemThucAn
{
    ThuongBinh,  // Common
    Hiem,        // Rare
    CucHiem      // Epic
}