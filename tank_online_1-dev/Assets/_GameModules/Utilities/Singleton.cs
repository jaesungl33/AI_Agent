

using UnityEngine;

/// <summary>
/// A generic singleton base class for MonoBehaviour.
/// Use by inheriting: `public class YourClass : Singleton<YourClass>`
/// </summary>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();
    private static bool _isShuttingDown = false;

    /// <summary>
    /// The singleton instance.
    /// </summary>
    public static T Instance
    {
        get
        {
            if (_isShuttingDown)
            {
                Debug.LogWarning($"[Singleton] Instance of {typeof(T)} already destroyed. Returning null.");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    // Try to find existing instance
                    _instance = FindAnyObjectByType<T>();

                    // Create new instance if one doesn't already exist
                    if (_instance == null)
                    {
                        var singletonObject = new GameObject();
                        _instance = singletonObject.AddComponent<T>();
                        singletonObject.name = $"[Singleton] {typeof(T)} (Auto-Created)";
                    }
                }

                return _instance;
            }
        }
    }

    /// <summary>
    /// Unity callback when the script instance is being loaded.
    /// Ensures that only one instance of the singleton exists.
    /// </summary>
    protected virtual void Awake()
    {
        // Ensure that only one instance exists
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Debug.LogWarning($"[Singleton] Instance of {typeof(T)} already exists. Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Unity callback when the application is quitting.
    /// Prevents recreation of singleton during shutdown.
    /// </summary>
    protected virtual void OnApplicationQuit()
    {
        _isShuttingDown = true;
    }

    /// <summary>
    /// Unity callback when the object is destroyed.
    /// Prevents recreation of singleton during shutdown.
    /// </summary>
    protected virtual void OnDestroy()
    {
        _isShuttingDown = true;
    }
}
