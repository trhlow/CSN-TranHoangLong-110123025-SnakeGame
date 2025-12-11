using UnityEngine;

public enum FoodRarity
{
    Common,
    Rare,
    Epic
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
    public int diem => points;
    public DoHiemThucAn doHiem => (DoHiemThucAn)rarity;
    public Color mauThucAn { get; set; }

    private void Awake()
    {
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<CircleCollider2D>();
        }
        collider.isTrigger = true;
        collider.radius = 0.2f;
    }

    public void OnEaten(Vector3 position)
    {
        if (eatSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(eatSound);
        }

        if (eatEffectPrefab != null)
        {
            Instantiate(eatEffectPrefab, position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}

// Legacy enum cho compatibility
public enum DoHiemThucAn
{
    ThuongBinh,
    Hiem,
    CucHiem
}