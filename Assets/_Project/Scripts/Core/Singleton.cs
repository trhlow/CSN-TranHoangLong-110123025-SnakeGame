using UnityEngine;

/// <summary>
/// Generic Singleton pattern for MonoBehaviour classes
/// Thread-safe implementation with DontDestroyOnLoad support
/// </summary>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    private static readonly object lockObject = new object();
    private static bool applicationIsQuitting = false;

    public static T Instance
    {
        get
        {
            if (applicationIsQuitting)
            {
                Debug.LogWarning($"[Singleton] Instance of {typeof(T)} already destroyed. Returning null.");
                return null;
            }

            lock (lockObject)
            {
                if (instance == null)
                {
                    // Try to find existing instance in scene
#if UNITY_2023_1_OR_NEWER
                    instance = FindFirstObjectByType<T>();
#else
                    instance = FindObjectOfType<T>();
#endif

                    if (instance == null)
                    {
                        // Create new instance if none exists
                        GameObject singletonObject = new GameObject($"[Singleton] {typeof(T).Name}");
                        instance = singletonObject.AddComponent<T>();
                        DontDestroyOnLoad(singletonObject);
                        Debug.Log($"[Singleton] Created new instance of {typeof(T).Name}");
                    }
                }

                return instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning($"[Singleton] Duplicate instance of {typeof(T).Name} detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        instance = this as T;

        // Only DontDestroyOnLoad if at root level
        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning($"[Singleton] {typeof(T).Name} is not at root level. Cannot mark as DontDestroyOnLoad.");
        }

        applicationIsQuitting = false;
    }

    protected virtual void OnApplicationQuit()
    {
        applicationIsQuitting = true;
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}