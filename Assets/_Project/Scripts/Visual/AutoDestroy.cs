using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [Header("Lifetime")]
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private bool destroyOnParticleEnd = true;

    private ParticleSystem ps;

    private void Start()
    {
        ps = GetComponent<ParticleSystem>();

        if (ps != null && destroyOnParticleEnd)
        {
            float psLifetime = ps.main.duration + ps.main.startLifetime.constantMax;
            Destroy(gameObject, psLifetime);
        }
        else
        {
            Destroy(gameObject, lifetime);
        }
    }
}