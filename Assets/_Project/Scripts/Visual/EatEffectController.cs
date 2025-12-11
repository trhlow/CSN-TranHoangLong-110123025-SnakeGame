using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class EatEffectController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float lifetime = 1.5f;

    private ParticleSystem ps;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void SetColor(Color color)
    {
        if (ps != null)
        {
            var main = ps.main;
            main.startColor = new ParticleSystem.MinMaxGradient(color, Color.white);
        }
    }

    public static GameObject CreateEffect(GameObject prefab, Vector3 position, Color color)
    {
        if (prefab == null) return null;

        GameObject effect = Instantiate(prefab, position, Quaternion.identity);

        EatEffectController controller = effect.GetComponent<EatEffectController>();
        if (controller != null)
        {
            controller.SetColor(color);
        }

        return effect;
    }
}