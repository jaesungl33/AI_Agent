using UnityEngine;

/// <summary>
/// KeepAlive is a utility class that prevents the GameObject it is attached to from being destroyed when loading a new scene.
/// This is useful for objects that need to persist across scene loads, such as managers or singletons.
/// </summary>
public class KeepAlive : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}