using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
public class RuntimeCollection<T>
{
    private Dictionary<string, T> _runtimeData = new();
    private CollectionBase<T> _originalCollection;

    public RuntimeCollection(CollectionBase<T> originalCollection)
    {
        _originalCollection = originalCollection;
        // Clone original data 
        foreach (var doc in originalCollection.GetAllDocuments())
        {
            string id = GetDocumentId(doc);
            //Debug.Log($"Cloning document with ID: {id}");
            if (!string.IsNullOrEmpty(id))
            {
                _runtimeData[id] = JsonUtility.FromJson<T>(JsonUtility.ToJson(doc));
            }
        }
    }

    private string GetDocumentId(T document)
    {
        // Try to get ID field through reflection
        var idField = typeof(T).GetField("tankId") ??
                        typeof(T).GetField("hullID") ??
                        typeof(T).GetField("weaponID") ??
                        typeof(T).GetField("abilityID") ??
                        typeof(T).GetField("projectileID") ??
                        typeof(T).GetField("matchID") ??
                        typeof(T).GetField("itemId") ??
                        typeof(T).GetField("$id");

        return idField?.GetValue(document)?.ToString();
    }

    [System.Serializable]
    private class CollectionWrapper<TDoc>
    {
        public int versionCode;
        public string versionName;
        public int lastUpdated;
        public List<TDoc> documents = new();
    }

    public void UpdateFromJson(string json)
    {
        try
        {
            Debug.Log($"1. UpdateFromJson: {json}");

            // Configure Newtonsoft.Json settings
            var settings = new Newtonsoft.Json.JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new Vector2Converter() },
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore,
                ObjectCreationHandling = Newtonsoft.Json.ObjectCreationHandling.Replace,
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
                PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects,
                ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
                {
                    NamingStrategy = new Newtonsoft.Json.Serialization.DefaultNamingStrategy()
                },
            };

            // Try parsing as collection first
            var collection = Newtonsoft.Json.JsonConvert.DeserializeObject<CollectionWrapper<T>>(json, settings);
            if (collection?.documents != null)
            {
                for (int i = 0; i < collection.documents.Count; i++)
                {
                    //Debug.Log($"1. Document {i}");
                    //Debug.Log($"2. Document {Newtonsoft.Json.JsonConvert.SerializeObject(collection.documents[i])}");
                }
                Debug.Log($"3. Collection Version: {collection.versionCode}");
                UpdateDocuments(collection.documents, settings);
                // Update collection metadata
                if (_originalCollection != null)
                {
                    _originalCollection.versionCode = collection.versionCode;
                    _originalCollection.versionName = collection.versionName;
                    _originalCollection.lastUpdated = collection.lastUpdated;
                }
                return;
            }

            // ...existing fallback code...
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error parsing JSON: {e.Message}\nJSON: {json}");
        }
    }

    private void UpdateDocuments(List<T> updates, Newtonsoft.Json.JsonSerializerSettings settings)
    {
        if (updates == null || updates.Count == 0)
        {
            Debug.LogWarning("No documents to update");
            return;
        }
        _originalCollection.Delete();
        foreach (var doc in updates)
        {
            // Deep clone the document using Newtonsoft.Json
            string docJson = Newtonsoft.Json.JsonConvert.SerializeObject(doc, settings);
            var clonedDoc = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(docJson, settings);

            string id = GetDocumentId(clonedDoc);
            if (!string.IsNullOrEmpty(id))
            {
                _runtimeData[id] = clonedDoc;

                // Update original collection if needed
                if (_originalCollection != null)
                {
                    _originalCollection.AddDocument(clonedDoc);
                }
            }
            else
            {
                Debug.LogWarning($"Skipping document with empty ID: {docJson}");
            }
        }
    }

    // public void UpdateFromJson(string json)
    // {
    //     try
    //     {
    //         Debug.Log($"1. UpdateFromJson: {json}");

    //         // Configure Newtonsoft.Json settings
    //         var settings = new Newtonsoft.Json.JsonSerializerSettings
    //         {
    //             NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
    //             MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore,
    //             ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
    //             {
    //                 NamingStrategy = new Newtonsoft.Json.Serialization.DefaultNamingStrategy()
    //             }
    //         };

    //         // Try parsing as collection first
    //         var collection = Newtonsoft.Json.JsonConvert.DeserializeObject<CollectionWrapper<T>>(json, settings);
    //         if (collection?.documents != null)
    //         {
    //             Debug.Log($"2. UpdateFromJson: {collection.documents.Count} documents");
    //             for(int i=0; i < collection.documents.Count; i++)
    //             {
    //                 Debug.Log($"3. Document {i}");
    //                 Debug.Log($"4. Document {Newtonsoft.Json.JsonConvert.SerializeObject(collection.documents[i])}");
    //             }
    //             UpdateDocuments(collection.documents);
    //             // Update collection metadata if needed
    //             if (_originalCollection != null)
    //             {
    //                 _originalCollection.versionCode = collection.versionCode;
    //                 _originalCollection.versionName = collection.versionName;
    //                 _originalCollection.lastUpdated = collection.lastUpdated;
    //                 _originalCollection.Delete();
    //                 foreach (var id in _runtimeData.Keys)
    //                 {
    //                     _originalCollection.AddDocument(_runtimeData[id]);
    //                 }
    //             }
    //             return;
    //         }

    //         // Fallback to direct array parsing
    //         if (json.StartsWith("["))
    //         {
    //             var updates = Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(json, settings);
    //             UpdateDocuments(updates);
    //             return;
    //         }

    //         Debug.LogError("Failed to parse JSON - invalid format");
    //     }
    //     catch (System.Exception e)
    //     {
    //         Debug.LogError($"Error parsing JSON: {e.Message}\nJSON: {json}");
    //     }
    // }

    // private void UpdateDocuments(List<T> updates)
    // {
    //     if (updates == null || updates.Count == 0)
    //     {
    //         Debug.LogWarning("No documents to update");
    //         return;
    //     }

    //     Debug.Log($"Updating {updates.Count} documents in runtime collection");
    //     Debug.Log($"{JsonUtility.ToJson(updates[0])}...");

    //     foreach (var doc in updates)
    //     {
    //         Debug.Log($"Processing document: {JsonUtility.ToJson(doc)}");
    //         string id = GetDocumentId(doc);
    //         Debug.Log($"Updating/Adding doc: {id} -> {JsonUtility.ToJson(doc)}");
    //         if (!string.IsNullOrEmpty(id))
    //         {
    //             _runtimeData[id] = doc;
    //         }
    //         else
    //         {
    //             Debug.LogWarning($"Skipping document with empty ID: {doc}");
    //         }
    //     }
    // }

    public T GetDocument(string id)
    {
        return _runtimeData.TryGetValue(id, out T doc) ? doc : default;
    }

    public List<T> GetAllDocuments()
    {
        return new List<T>(_runtimeData.Values);
    }

}

public class Vector2Converter : JsonConverter
{
    public override bool CanConvert(System.Type objectType)
    {
        return objectType == typeof(Vector2);
    }

    public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject obj = JObject.Load(reader);
        float x = obj["x"] != null ? obj["x"].ToObject<float>() : 0f;
        float y = obj["y"] != null ? obj["y"].ToObject<float>() : 0f;
        return new Vector2(x, y);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        Vector2 v2 = (Vector2)value;
        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(v2.x);
        writer.WritePropertyName("y");
        writer.WriteValue(v2.y);
        writer.WriteEndObject();
    }
}