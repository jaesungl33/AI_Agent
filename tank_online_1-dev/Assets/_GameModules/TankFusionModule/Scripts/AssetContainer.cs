using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AssetEntry
{
    public string key;
    public UnityEngine.Object asset;
}

[DisallowMultipleComponent]
public class AssetContainer : MonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeField] private List<AssetEntry> entries = new List<AssetEntry>();

    private Dictionary<string, UnityEngine.Object> lookup = new Dictionary<string, UnityEngine.Object>();

    public T Get<T>(string key) where T : UnityEngine.Object
    {
        if (lookup.TryGetValue(key, out var obj))
            return obj as T;

        Debug.LogWarning($"[AssetContainer] Key not found: {key}", this);
        return null;
    }

    public bool TryGet<T>(string key, out T result) where T : UnityEngine.Object
    {
        if (lookup.TryGetValue(key, out var obj))
        {
            result = obj as T;
            return result != null;
        }
        result = null;
        return false;
    }

    // Build dictionary when loaded
    public void OnAfterDeserialize()
    {
        lookup.Clear();
        foreach (var e in entries)
        {
            if (string.IsNullOrEmpty(e.key) || e.asset == null)
                continue;
            if (!lookup.ContainsKey(e.key))
                lookup.Add(e.key, e.asset);
        }
    }

    public void OnBeforeSerialize() { }
}
