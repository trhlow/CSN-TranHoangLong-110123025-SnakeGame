using UnityEngine;

public class FoodAnimation : MonoBehaviour
{
    [Header("Float Animation")]
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float floatAmount = 0.2f;

    [Header("Rotation")]
    [SerializeField] private bool rotate = true;
    [SerializeField] private float rotateSpeed = 50f;

    [Header("Scale Pulse")]
    [SerializeField] private bool scalePulse = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmount = 0.1f;

    private Vector3 startPosition;
    private Vector3 baseScale;

    private void Start()
    {
        startPosition = transform.position;
        baseScale = transform.localScale;
    }

    private void Update()
    {
        // Float up and down
        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.position = startPosition + Vector3.up * yOffset;

        // Rotate
        if (rotate)
        {
            transform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);
        }

        // Scale pulse
        if (scalePulse)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            transform.localScale = baseScale * pulse;
        }
    }

    public void UpdateStartPosition(Vector3 newPos)
    {
        startPosition = newPos;
    }
}