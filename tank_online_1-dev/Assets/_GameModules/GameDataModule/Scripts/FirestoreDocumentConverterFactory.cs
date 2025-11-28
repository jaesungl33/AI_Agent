using System;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

/// <summary>
/// Universal converter factory for Firebase Firestore documents
/// </summary>
public static class FirestoreDocumentConverterFactory
{
    /// <summary>
    /// Convert any IUserData document to Firestore data using appropriate converter
    /// </summary>
    public static Dictionary<string, object> ToFirestoreData<T>(T document) where T : IUserData
    {
        if (document == null) return new Dictionary<string, object>();

        var type = typeof(T);
        
        try
        {
            // Use specific converters for known types
            if (type == typeof(PlayerDocument))
            {
                return PlayerDocumentConverter.ToFirestoreData(document as PlayerDocument);
            }
            else if (type == typeof(UserDocument))
            {
                return UserDocumentConverter.ToFirestoreData(document as UserDocument);
            }
            else if (type == typeof(PlayerSettingsDocument))
            {
                return PlayerSettingsDocumentConverter.ToFirestoreData(document as PlayerSettingsDocument);
            }
            
            // Fallback: Use reflection-based conversion
            Debug.LogWarning($"No specific converter found for {type.Name}, using generic conversion");
            return ConvertToFirestoreDataGeneric(document);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error converting {type.Name} to Firestore data: {ex.Message}");
            return new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Convert Firestore document to any IUserData type using appropriate converter
    /// </summary>
    public static T FromFirestoreData<T>(DocumentSnapshot document) where T : IUserData
    {
        if (!document.Exists) return default(T);

        var type = typeof(T);
        
        try
        {
            // Use specific converters for known types
            if (type == typeof(PlayerDocument))
            {
                var result = PlayerDocumentConverter.FromFirestoreData(document);
                return (T)(object)result;
            }
            else if (type == typeof(UserDocument))
            {
                var result = UserDocumentConverter.FromFirestoreData(document);
                return (T)(object)result;
            }
            else if (type == typeof(PlayerSettingsDocument))
            {
                var result = PlayerSettingsDocumentConverter.FromFirestoreData(document);
                return (T)(object)result;
            }
            
            // Fallback: Use reflection-based conversion
            Debug.LogWarning($"No specific converter found for {type.Name}, using generic conversion");
            return ConvertFromFirestoreDataGeneric<T>(document);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error converting Firestore document to {type.Name}: {ex.Message}");
            return default(T);
        }
    }

    /// <summary>
    /// Convert Firestore Dictionary to any IUserData type using appropriate converter
    /// </summary>
    public static T FromFirestoreData<T>(Dictionary<string, object> data) where T : IUserData
    {
        if (data == null) return default(T);

        var type = typeof(T);
        
        try
        {
            // Use specific converters for known types
            if (type == typeof(PlayerDocument))
            {
                var result = PlayerDocumentConverter.FromFirestoreData(data);
                return (T)(object)result;
            }
            else if (type == typeof(UserDocument))
            {
                var result = UserDocumentConverter.FromFirestoreData(data);
                return (T)(object)result;
            }
            else if (type == typeof(PlayerSettingsDocument))
            {
                var result = PlayerSettingsDocumentConverter.FromFirestoreData(data);
                return (T)(object)result;
            }
            
            // Fallback: Use reflection-based conversion
            Debug.LogWarning($"No specific converter found for {type.Name}, using generic conversion");
            return ConvertFromFirestoreDataGeneric<T>(data);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error converting Firestore data to {type.Name}: {ex.Message}");
            return default(T);
        }
    }

    /// <summary>
    /// Generic conversion to Firestore data using reflection
    /// </summary>
    private static Dictionary<string, object> ConvertToFirestoreDataGeneric<T>(T document) where T : IUserData
    {
        var data = new Dictionary<string, object>();
        var type = typeof(T);
        var properties = type.GetProperties();

        foreach (var property in properties)
        {
            // Skip computed properties and JsonIgnore properties
            if (property.Name == "ID" || 
                property.GetCustomAttributes(typeof(Newtonsoft.Json.JsonIgnoreAttribute), false).Length > 0)
                continue;

            try
            {
                var value = property.GetValue(document);
                if (value != null)
                {
                    data[property.Name] = value;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to get property {property.Name} from {type.Name}: {ex.Message}");
            }
        }

        return data;
    }

    /// <summary>
    /// Generic conversion from Firestore DocumentSnapshot using reflection
    /// </summary>
    private static T ConvertFromFirestoreDataGeneric<T>(DocumentSnapshot document) where T : IUserData
    {
        if (!document.Exists) return default(T);
        
        try
        {
            var data = document.ToDictionary();
            return ConvertFromFirestoreDataGeneric<T>(data);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in generic conversion from DocumentSnapshot to {typeof(T).Name}: {ex.Message}");
            return default(T);
        }
    }

    /// <summary>
    /// Generic conversion from Firestore Dictionary using reflection
    /// </summary>
    private static T ConvertFromFirestoreDataGeneric<T>(Dictionary<string, object> data) where T : IUserData
    {
        if (data == null) return default(T);

        try
        {
            var instance = Activator.CreateInstance<T>();
            var type = typeof(T);
            var properties = type.GetProperties();

            foreach (var property in properties)
            {
                // Skip computed properties and JsonIgnore properties
                if (property.Name == "ID" || 
                    property.GetCustomAttributes(typeof(Newtonsoft.Json.JsonIgnoreAttribute), false).Length > 0 ||
                    !property.CanWrite)
                    continue;

                if (data.TryGetValue(property.Name, out object value) && value != null)
                {
                    try
                    {
                        // Type conversion
                        var convertedValue = ConvertValue(value, property.PropertyType);
                        property.SetValue(instance, convertedValue);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Failed to set property {property.Name} on {type.Name}: {ex.Message}");
                    }
                }
            }

            return instance;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in generic conversion from Dictionary to {typeof(T).Name}: {ex.Message}");
            return default(T);
        }
    }

    /// <summary>
    /// Convert value to target type
    /// </summary>
    private static object ConvertValue(object value, Type targetType)
    {
        if (value == null) return null;
        if (targetType.IsAssignableFrom(value.GetType())) return value;

        try
        {
            return Convert.ChangeType(value, targetType);
        }
        catch
        {
            // Handle special cases
            if (targetType == typeof(List<string>) && value is List<object> objectList)
            {
                var stringList = new List<string>();
                foreach (var item in objectList)
                {
                    if (item != null)
                        stringList.Add(item.ToString());
                }
                return stringList;
            }

            Debug.LogWarning($"Could not convert {value.GetType()} to {targetType}");
            return null;
        }
    }
}