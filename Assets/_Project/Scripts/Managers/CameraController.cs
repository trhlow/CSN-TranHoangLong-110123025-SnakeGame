using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("Camera Settings")]
    public Camera cam;
    public Color backgroundColor = new Color(0.06f, 0.05f, 0.16f);

    [Header("Auto Fit Grid")]
    [SerializeField] private bool autoFitGrid = true;
    [SerializeField] private float paddingPercent = 0.1f; // 10% padding

    [Header("Shake Settings")]
    public float defaultShakeDuration = 0.3f;
    public float defaultShakeMagnitude = 0.2f;
    public AnimationCurve shakeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (cam == null)
        {
            cam = GetComponent<Camera>();
        }

        originalPosition = transform.localPosition;
        cam.backgroundColor = backgroundColor;
    }

    private void Start()
    {
        if (autoFitGrid)
        {
            FitCameraToGrid();
        }
    }

    /// <summary>
    /// ✅ Tự động điều chỉnh Camera Size để nhìn thấy toàn bộ Grid
    /// </summary>
    private void FitCameraToGrid()
    {
        if (GridManager.Instance == null)
        {
            Debug.LogWarning("[CameraController] GridManager not found! Cannot auto-fit camera.");
            return;
        }

        // Lấy kích thước Grid
        float gridWidth = GridManager.Instance.GridWidth * GridManager.Instance.CellSize;
        float gridHeight = GridManager.Instance.GridHeight * GridManager.Instance.CellSize;

        // Tính toán Camera Size cần thiết
        float screenAspect = (float)Screen.width / Screen.height;
        float gridAspect = gridWidth / gridHeight;

        float requiredSize;

        if (screenAspect >= gridAspect)
        {
            // Screen rộng hơn → fit theo chiều cao
            requiredSize = gridHeight / 2f;
        }
        else
        {
            // Screen cao hơn → fit theo chiều rộng
            requiredSize = gridWidth / (2f * screenAspect);
        }

        // Thêm padding
        requiredSize *= (1f + paddingPercent);

        // Áp dụng
        cam.orthographicSize = requiredSize;

        // Căn giữa Camera với Grid
        Vector3 gridCenter = GridManager.Instance.GridCenter;
        transform.position = new Vector3(gridCenter.x, gridCenter.y, transform.position.z);
        originalPosition = transform.localPosition;

        Debug.Log($"[CameraController] ✅ Auto-fit camera: Size={requiredSize:F2}, Position={transform.position}");
    }

    public void Shake()
    {
        Shake(defaultShakeDuration, defaultShakeMagnitude);
    }

    public void Shake(float duration, float magnitude)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float strength = shakeCurve.Evaluate(elapsed / duration);

            float x = Random.Range(-1f, 1f) * magnitude * strength;
            float y = Random.Range(-1f, 1f) * magnitude * strength;

            transform.localPosition = new Vector3(
                originalPosition.x + x,
                originalPosition.y + y,
                originalPosition.z
            );

            yield return null;
        }

        transform.localPosition = originalPosition;
        shakeCoroutine = null;
    }

    public void ResetPosition()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }
        transform.localPosition = originalPosition;
    }

    /// <summary>
    /// Gọi khi muốn refresh camera fit (ví dụ sau khi đổi resolution)
    /// </summary>
    public void RefreshCameraFit()
    {
        if (autoFitGrid)
        {
            FitCameraToGrid();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

#if UNITY_EDITOR
    // ✅ Preview trong Editor
    private void OnDrawGizmos()
    {
        if (cam == null) return;

        Gizmos.color = Color.yellow;

        float height = cam.orthographicSize * 2f;
        float width = height * cam.aspect;

        Vector3 center = transform.position;

        // Draw camera bounds
        Gizmos.DrawWireCube(center, new Vector3(width, height, 0));
    }
#endif
}