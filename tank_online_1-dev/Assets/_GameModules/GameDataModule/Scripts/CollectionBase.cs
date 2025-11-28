// Date: 28 June 2025
// AI-Generated: This code structure was created with AI assistance

using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Unity.VisualScripting;
using System.IO;
using System;
using System.Threading.Tasks;

public class CollectionBase<T> : ScriptableObject
{
    public const string CollectionPath = "Collection/";
    public int versionCode = 1;
    public string versionName = "1.0.0";
    public int lastUpdated;
    /// <summary>
    /// The list of documents in the collection.
    /// </summary>
    public List<T> documents = new();

    protected void UpdateTimestamp()
    {
        lastUpdated = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
    }

    /// <summary>
    /// Adds a new document to the collection.
    /// </summary>
    /// <param name="document">The document to add.</param>
    public void AddDocument(T document)
    {
        if (documents == null)
        {
            documents = new List<T>();
        }
        documents.Add(document);
        UpdateTimestamp();
        Write();// Save the collection after adding a document
    }

    /// <summary>
    /// Gets a document by its index.
    /// </summary>
    /// <param name="index">The index of the document.</param>
    /// <returns>The document at the specified index.</returns>
    public T GetDocument(int index)
    {
        if (documents != null && index >= 0 && index < documents.Count)
        {
            return documents[index];
        }
        return default(T);
    }

    /// <summary>
    /// Finds a document by comparing a property of type T2 with the provided value.
    /// </summary>
    /// <typeparam name="T2">The type of the property to compare</typeparam>
    /// <param name="propertySelector">Function to extract the property from document</param>
    /// <param name="value">The value to compare against</param>
    /// <returns>The matching document or default(T) if not found</returns>
    public T FindDocumentByProperty<T2>(System.Func<T, T2> propertySelector, T2 value)
    {
        if (documents != null)
        {
            foreach (var doc in documents)
            {
                T2 propValue = propertySelector(doc);
                if ((propValue != null && propValue.Equals(value)) ||
                    (propValue == null && value == null))
                {
                    return doc;
                }
            }
        }
        return default(T);
    }

    public T[] CloneDocumentsByProperty<T2>(System.Func<T, T2> propertySelector, T2 value)
    {
        List<T> foundDocuments = new List<T>();
        if (documents != null)
        {
            foreach (var doc in documents)
            {
                T2 propValue = propertySelector(doc);
                if ((propValue != null && propValue.Equals(value)) ||
                    (propValue == null && value == null))
                {
                    foundDocuments.Add(doc.DeepClone());
                }
            }
        }
        return foundDocuments.ToArray();
    }

    public List<T> GetAllDocuments()
    {
        return documents;
    }

    private string GetPath => nameof(T);

    public virtual void Write()
    {
        // if (documents == null || documents.Count == 0)
        // {
        //     Debug.LogWarning($"No documents to save for {typeof(T).Name} collection.");
        //     return;
        // }

        // try
        // {
        //     string directoryPath = Path.GetDirectoryName(GetPath);
        //     if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
        //     {
        //         Directory.CreateDirectory(directoryPath);
        //     }

        //     string json = JsonConvert.SerializeObject(documents, Formatting.Indented);
        //     File.WriteAllText(GetPath, json);
        //     Debug.Log($"Successfully saved {documents.Count} documents for {typeof(T).Name} collection.");
        // }
        // catch (System.Exception ex)
        // {
        //     Debug.LogError($"Failed to save {typeof(T).Name} collection: {ex.Message}");
        // }
    }

    public virtual void Read()
    {
        string path = GetPath;

        // Check if path is valid
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError($"Invalid path for {typeof(T).Name} collection.");
            return;
        }

        // Check if file exists
        if (!File.Exists(path))
        {
            Debug.LogWarning($"No save file found at {path}");
            return;
        }

        try
        {
            string json = File.ReadAllText(path);

            // Check if JSON is not empty
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning($"File at {path} is empty.");
                return;
            }

            // Deserialize the JSON
            documents = JsonConvert.DeserializeObject<List<T>>(json);

            // Validate the loaded data
            if (documents == null)
            {
                Debug.LogError($"Failed to deserialize {typeof(T).Name} collection data.");
                documents = new List<T>();
            }
            else
            {
                Debug.Log($"Successfully loaded {documents.Count} documents for {typeof(T).Name} collection.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error loading {typeof(T).Name} collection: {ex.Message}");
            documents = new List<T>(); // Initialize an empty list to prevent null references
        }
    }

    public virtual void Delete()
    {
        documents.Clear();
        UpdateTimestamp();
    }
    
    public virtual bool Exists()
    {
        return File.Exists(GetPath);
    }
    
    public virtual async Task WriteAsync()
    {
        if (documents == null || documents.Count == 0)
        {
            Debug.LogWarning($"No documents to save for {typeof(T).Name} collection.");
            return;
        }

        try
        {
            string directoryPath = Path.GetDirectoryName(GetPath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string json = JsonConvert.SerializeObject(documents, Formatting.Indented);
            await File.WriteAllTextAsync(GetPath, json);
            Debug.Log($"Successfully saved {documents.Count} documents for {typeof(T).Name} collection.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to save {typeof(T).Name} collection: {ex.Message}");
        }
    }

    public virtual async Task ReadAsync()
    {
        string path = GetPath;

        // Check if path is valid
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError($"Invalid path for {typeof(T).Name} collection.");
            return;
        }

        // Check if file exists
        if (!File.Exists(path))
        {
            Debug.LogWarning($"No save file found at {path}");
            return;
        }

        try
        {
            string json = await File.ReadAllTextAsync(path);

            // Check if JSON is not empty
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning($"File at {path} is empty.");
                return;
            }

            // Deserialize the JSON
            documents = JsonConvert.DeserializeObject<List<T>>(json);

            // Validate the loaded data
            if (documents == null)
            {
                Debug.LogError($"Failed to deserialize {typeof(T).Name} collection data.");
                documents = new List<T>();
            }
            else
            {
                Debug.Log($"Successfully loaded {documents.Count} documents for {typeof(T).Name} collection.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error loading {typeof(T).Name} collection: {ex.Message}");
            documents = new List<T>(); // Initialize an empty list to prevent null references
        }
    }

    public virtual void DeleteAsync()
    {
        if (File.Exists(GetPath))
        {
            File.Delete(GetPath);
        }
        else
        {
            Debug.LogWarning($"No save file found at {GetPath} to delete.");
        }
    }

    public virtual void AddDocumentAsync(T document)
    {
        if (File.Exists(GetPath))
        {
            File.Delete(GetPath);
        }
        else
        {
            Debug.LogWarning($"No save file found at {GetPath} to delete.");
        }
    }
}
