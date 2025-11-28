using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.RemoteConfig;
using Newtonsoft.Json;
using System;

public class FirebaseConfigCollectionBase<T> : CollectionBase<T>
{
    protected string remoteConfigKey;
    
    protected virtual string GetRemoteConfigKey()
    {
        if (string.IsNullOrEmpty(remoteConfigKey))
        {
            remoteConfigKey = $"{typeof(T).Name}";
        }
        return remoteConfigKey;
    }

    public override async void Read()
    {
        await ReadAsync();
    }

    public override bool Exists()
    {
        // Check if the config exists in Firebase RemoteConfig
        try
        {
            string configKey = GetRemoteConfigKey();
            string value = FirebaseRemoteConfig.DefaultInstance.GetValue(configKey).StringValue;
            
            // If Firebase RemoteConfig has data, consider it as existing
            if (!string.IsNullOrEmpty(value))
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Error checking Firebase RemoteConfig existence: {ex.Message}");
        }
        
        // Fallback to base implementation (local file check)
        return base.Exists();
    }
    
    /// <summary>
    /// Optional method to set custom RemoteConfig key
    /// </summary>
    /// <param name="key">Custom key for RemoteConfig</param>
    public void SetRemoteConfigKey(string key)
    {
        remoteConfigKey = key;
    }

    public override async Task ReadAsync()
    {
        try
        {
            // Fetch the latest config from Firebase
            await FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);
        
            // Activate the fetched config
            await FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
        
            string configKey = GetRemoteConfigKey();
            string jsonData = FirebaseRemoteConfig.DefaultInstance.GetValue(configKey).StringValue;
        
            if (!string.IsNullOrEmpty(jsonData))
            {
                try
                {
                    // Directly deserialize as List<T> without wrapper
                    documents = JsonConvert.DeserializeObject<List<T>>(jsonData);
                
                    if (documents != null)
                    {
                        Debug.Log($"Successfully loaded {documents.Count} documents from Firebase RemoteConfig for {typeof(T).Name}");
                    }
                    else
                    {
                        Debug.LogWarning($"Deserialized data is null for {typeof(T).Name}");
                        documents = new List<T>();
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to deserialize Firebase RemoteConfig data for {typeof(T).Name}: {ex.Message}");
                    // Fallback to local storage
                    base.Read();
                }
            }
            else
            {
                Debug.LogWarning($"No data found in Firebase RemoteConfig for key: {configKey}. Falling back to local storage.");
                base.Read();
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to read from Firebase RemoteConfig: {ex.Message}. Falling back to local storage.");
            // Fallback to base implementation (local file)
            base.Read();
        }
    }
}
