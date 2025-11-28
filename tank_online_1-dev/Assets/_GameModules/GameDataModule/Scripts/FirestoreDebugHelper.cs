using System;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

/// <summary>
/// Utility class for debugging Firebase Firestore issues
/// </summary>
public static class FirestoreDebugHelper
{
    /// <summary>
    /// Log detailed information about a Firestore document
    /// </summary>
    public static void LogDocumentInfo(DocumentSnapshot document, string context = "")
    {
        if (document == null)
        {
            Debug.LogWarning($"FirestoreDebugHelper [{context}]: Document is null");
            return;
        }

        Debug.Log($"FirestoreDebugHelper [{context}]: Document ID: {document.Id}, Exists: {document.Exists}");
        
        if (document.Exists)
        {
            try
            {
                var data = document.ToDictionary();
                Debug.Log($"FirestoreDebugHelper [{context}]: Document data fields count: {data.Count}");
                
                foreach (var kvp in data)
                {
                    var valueType = kvp.Value?.GetType().Name ?? "null";
                    var valueStr = kvp.Value?.ToString() ?? "null";
                    if (valueStr.Length > 100)
                        valueStr = valueStr.Substring(0, 100) + "...";
                    
                    Debug.Log($"FirestoreDebugHelper [{context}]: Field '{kvp.Key}' = {valueStr} (Type: {valueType})");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"FirestoreDebugHelper [{context}]: Error reading document data: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Log detailed information about conversion attempts
    /// </summary>
    public static void LogConversionAttempt<T>(DocumentSnapshot document, string method, Exception ex = null)
    {
        var typeName = typeof(T).Name;
        var context = $"{method}<{typeName}>";
        
        LogDocumentInfo(document, context);
        
        if (ex != null)
        {
            Debug.LogError($"FirestoreDebugHelper [{context}]: Conversion failed: {ex.Message}");
            Debug.LogError($"FirestoreDebugHelper [{context}]: Stack trace: {ex.StackTrace}");
        }
        else
        {
            Debug.Log($"FirestoreDebugHelper [{context}]: Attempting conversion");
        }
    }

    /// <summary>
    /// Log information about data being written to Firestore
    /// </summary>
    public static void LogWriteAttempt<T>(T data, string method)
    {
        var typeName = typeof(T).Name;
        var context = $"{method}<{typeName}>";
        
        if (data == null)
        {
            Debug.LogWarning($"FirestoreDebugHelper [{context}]: Data is null");
            return;
        }

        try
        {
            var json = JsonUtility.ToJson(data, true);
            Debug.Log($"FirestoreDebugHelper [{context}]: Writing data: {json}");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"FirestoreDebugHelper [{context}]: Could not serialize data to JSON: {ex.Message}");
        }

        // Log property values for PlayerDocument specifically
        if (data is PlayerDocument playerDoc)
        {
            Debug.Log($"FirestoreDebugHelper [{context}]: PlayerDocument Details:");
            Debug.Log($"  - roleID: {playerDoc.roleID}");
            Debug.Log($"  - playerName: {playerDoc.playerName}");
            Debug.Log($"  - elo: {playerDoc.elo}");
            Debug.Log($"  - gold: {playerDoc.gold}");
            Debug.Log($"  - diamond: {playerDoc.diamond}");
            Debug.Log($"  - level: {playerDoc.level}");
            Debug.Log($"  - selectedModeIndex: {playerDoc.selectedModeIndex}");
            Debug.Log($"  - tanks count: {playerDoc.tanks?.Count ?? 0}");
        }
    }

    /// <summary>
    /// Check if Firebase Firestore is properly initialized
    /// </summary>
    public static bool CheckFirestoreInitialization()
    {
        try
        {
            var firestore = FirestoreManager.Instance?.db;
            if (firestore == null)
            {
                Debug.LogError("FirestoreDebugHelper: FirestoreManager.Instance.db is null");
                return false;
            }

            Debug.Log("FirestoreDebugHelper: Firestore is properly initialized");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"FirestoreDebugHelper: Error checking Firestore initialization: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Test basic Firestore connectivity
    /// </summary>
    public static async void TestFirestoreConnectivity()
    {
        try
        {
            if (!CheckFirestoreInitialization()) return;

            var firestore = FirestoreManager.Instance.db;
            var testCollection = firestore.Collection("test_connectivity");
            var testDoc = testCollection.Document("test");
            
            Debug.Log("FirestoreDebugHelper: Testing Firestore connectivity...");
            
            // Try to write a simple document
            await testDoc.SetAsync(new Dictionary<string, object> 
            { 
                ["timestamp"] = DateTime.UtcNow.ToString(),
                ["test"] = true 
            });
            
            Debug.Log("FirestoreDebugHelper: Write test successful");
            
            // Try to read it back
            var snapshot = await testDoc.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                Debug.Log("FirestoreDebugHelper: Read test successful");
                
                // Clean up
                await testDoc.DeleteAsync();
                Debug.Log("FirestoreDebugHelper: Connectivity test completed successfully");
            }
            else
            {
                Debug.LogWarning("FirestoreDebugHelper: Read test failed - document not found");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"FirestoreDebugHelper: Connectivity test failed: {ex.Message}");
        }
    }
}