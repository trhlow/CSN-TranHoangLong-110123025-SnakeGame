using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    private static readonly object lockObject = new object();
    private static bool applicationIsQuitting = false;

    public static T Instance
    {
        get
        {
            if (applicationIsQuitting) return null;
            lock (lockObject)
            {
                if (instance == null)
                {
#if UNITY_2022_1_OR_NEWER
                    instance = Object.FindFirstObjectByType<T>();
#else
                    instance = Object.FindObjectOfType<T>();
#endif
                    if (instance == null)
                    {
                        GameObject singleton = new GameObject($"[Singleton] {typeof(T).Name}");
                        instance = singleton.AddComponent<T>();
                        DontDestroyOnLoad(singleton);
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
            Destroy(gameObject);
            return;
        }
        instance = this as T;
        if (transform.parent == null)
            DontDestroyOnLoad(gameObject);
        else
            Debug.LogWarning($"{typeof(T).Name}: Could not mark as DontDestroyOnLoad because not at root!");

        applicationIsQuitting = false;
    }

    protected virtual void OnApplicationQuit()
    {
        applicationIsQuitting = true;
    }
}