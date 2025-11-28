// Date: 28 June 2025
// AI-Generated: This code structure was created with AI assistance

using System;
using System.Collections.Generic;

[Serializable]
public class Serialization<TKey, TValue>
{
    public List<TKey> keys;
    public List<TValue> values;
    public Dictionary<TKey, TValue> dictionary;

    public Serialization(Dictionary<TKey, TValue> dict)
    {
        keys = new List<TKey>(dict.Keys);
        values = new List<TValue>(dict.Values);
        dictionary = dict;
    }

    public void OnAfterDeserialize()
    {
        dictionary = new Dictionary<TKey, TValue>();
        for (int i = 0; i < keys.Count; i++)
            dictionary.Add(keys[i], values[i]);
    }

    public void OnBeforeSerialize() { }
}
