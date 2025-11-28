using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;

[System.Serializable]
public class UserDataCollectionBase<T> : CollectionBase<T> where T : IUserData
{
    [SerializeField] private string documentId;
    private FirebaseFirestore db => FirestoreManager.Instance?.db;
    
    // Collection name based on the type T
    private string CollectionName => $"{typeof(T).Name}";
    
    // Initialize with user ID
    public void Initialize(string documentId)
    {
        this.documentId = documentId;
        documents.Clear();
        if (string.IsNullOrEmpty(this.documentId))
        {
            Debug.LogError($"UserDataCollectionBase<{typeof(T).Name}>: User ID cannot be null or empty");
        }
    }

    public override async void Read()
    {
        await ReadAsync();
    }

    /// <summary>
    /// Manual conversion from Firestore document to type T using custom converter factory or Newtonsoft.Json
    /// </summary>
    private T ManualConvertFromDocument(DocumentSnapshot document)
    {
        try
        {
            // Use factory for all IUserData types
            var result = FirestoreDocumentConverterFactory.FromFirestoreData<T>(document);
            if (result != null)
            {
                return result;
            }

            // Fallback: Convert the document data to a dictionary
            var documentData = document.ToDictionary();
            
            // Use JsonConvert with lenient settings
            var settings = new Newtonsoft.Json.JsonSerializerSettings
            {
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore,
                Error = (sender, args) => {
                    Debug.LogWarning($"JsonConvert error: {args.ErrorContext.Error.Message}");
                    args.ErrorContext.Handled = true; // Continue despite errors
                }
            };
            
            // Serialize the dictionary to JSON
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(documentData, settings);
            
            // Deserialize JSON to type T
            T instance = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json, settings);
            
            return instance;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to manually convert document to {typeof(T).Name}: {ex.Message}");
            return default(T);
        }
    }

    /// <summary>
    /// Manual conversion from type T to Dictionary for Firestore using custom converter factory or Newtonsoft.Json
    /// </summary>
    private Dictionary<string, object> ManualConvertToDocument(T item)
    {
        try
        {
            // Use factory for all IUserData types
            var result = FirestoreDocumentConverterFactory.ToFirestoreData(item);
            if (result != null && result.Count > 0)
            {
                return result;
            }

            // Fallback: Use JsonConvert with lenient settings
            var settings = new Newtonsoft.Json.JsonSerializerSettings
            {
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore,
                Error = (sender, args) => {
                    Debug.LogWarning($"JsonConvert error during serialization: {args.ErrorContext.Error.Message}");
                    args.ErrorContext.Handled = true; // Continue despite errors
                }
            };
            
            // Serialize the object to JSON
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(item, settings);
            
            // Deserialize JSON to Dictionary
            var dictionary = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json, settings);
            
            return dictionary ?? new Dictionary<string, object>();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to manually convert {typeof(T).Name} to document: {ex.Message}");
            return new Dictionary<string, object>();
        }
    }

    public override async void Write()
    {
         await WriteAsync();
    }

    /// <summary>
    /// Delete a specific document by its ID
    /// </summary>
    /// <param name="documentId">The ID of the document to delete</param>
    public async Task DeleteDocument(string documentId)
    {
        if (!ValidateFirestore()) return;

        try
        {
            DocumentReference docRef = db.Collection(CollectionName).Document(documentId);
            await docRef.DeleteAsync();

            // Remove from local collection
            documents.RemoveAll(d => d.ID == documentId);

            Debug.Log($"Successfully deleted document {documentId} from Firestore for {typeof(T).Name} collection (User: {this.documentId})");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to delete document {documentId} from Firestore (User: {this.documentId}): {ex.Message}");
        }
    }

    /// <summary>
    /// Get a specific document by its ID
    /// </summary>
    /// <param name="documentId">The ID of the document to get</param>
    /// <returns>The document if found, otherwise default(T)</returns>
    public async Task<T> GetDocument(string documentId)
    {
        if (!ValidateFirestore()) return default(T);

        try
        {
            DocumentReference docRef = db.Collection(CollectionName).Document(documentId);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                try
                {
                    // Try to convert using Firebase's built-in converter first
                    return snapshot.ConvertTo<T>();
                }
                catch (System.Exception converterEx)
                {
                    // Fallback: Manual conversion using custom converter or Newtonsoft.Json
                    Debug.LogWarning($"GetDocument Firebase converter failed for {typeof(T).Name}, using manual conversion: {converterEx.Message}");
                    return ManualConvertFromDocument(snapshot);
                }
            }
            else
            {
                Debug.LogWarning($"Document {documentId} not found for {typeof(T).Name} collection (User: {this.documentId})");
                return default(T);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error getting document {documentId} from Firestore (User: {this.documentId}): {ex.Message}");
            return default(T);
        }
    }

    /// <summary>
    /// Check if a specific document exists by its ID
    /// </summary>
    /// <param name="documentId">The ID of the document to check</param>
    /// <returns>True if the document exists, false otherwise</returns>
    public async Task<bool> DocumentExists(string documentId)
    {
        if (!ValidateFirestore()) return false;

        try
        {
            DocumentReference docRef = db.Collection(CollectionName).Document(documentId);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            return snapshot.Exists;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error checking existence of document {documentId} in Firestore (User: {this.documentId}): {ex.Message}");
            return false;
        }
    }

    public override void Delete()
    {
        DeleteAsync();
    }
    
    private bool ValidateFirestore()
    {
        if (db == null)
        {
            Debug.LogError($"FirestoreManager not initialized. Cannot perform Firestore operations for {typeof(T).Name} collection.");
            return false;
        }

        if (string.IsNullOrEmpty(documentId))
        {
            Debug.LogError($"User ID not set. Call Initialize(roleID) before using Firestore operations for {typeof(T).Name} collection.");
            return false;
        }

        return true;
    }

    // Async versions for better performance when you can use async/await
    public override async Task ReadAsync()
    {
        if (!ValidateFirestore()) return;

        try
        {
            // Directly fetch the specific document by its ID instead of querying all documents
            CollectionReference collectionRef = db.Collection(CollectionName);
            DocumentReference documentRef = collectionRef.Document(documentId);
            DocumentSnapshot document = await documentRef.GetSnapshotAsync();

            documents = new List<T>();

            if (document.Exists)
            {
                // For known problematic types, skip Firebase converter and use custom converter directly
                if (typeof(T) == typeof(PlayerDocument) || typeof(T) == typeof(UserDocument) || typeof(T) == typeof(PlayerSettingsDocument))
                {
                    Debug.Log($"ReadAsync Using custom converter for {typeof(T).Name} to avoid Firebase conversion issues");
                    T item = ManualConvertFromDocument(document);
                    if (item != null)
                    {
                        documents.Add(item);
                    }
                }
                else
                {
                    try
                    {
                        // Try to convert using Firebase's built-in converter first
                        T item = document.ConvertTo<T>();
                        documents.Add(item);
                    }
                    catch (System.Exception converterEx)
                    {
                        // Log detailed debug information
                        FirestoreDebugHelper.LogConversionAttempt<T>(document, "ReadAsync", converterEx);
                        
                        // Fallback: Manual conversion using custom converter or Newtonsoft.Json
                        Debug.LogWarning($"ReadAsync Firebase converter failed for {typeof(T).Name}, using manual conversion: {converterEx.Message}");
                        T item = ManualConvertFromDocument(document);
                        if (item != null)
                        {
                            documents.Add(item);
                        }
                    }
                }
            }

            Debug.Log($"Successfully loaded {documents.Count} documents from Firestore for {typeof(T).Name} collection (User: {documentId})");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error loading {typeof(T).Name} collection from Firestore (User: {documentId}): {ex.Message}");
            documents = new List<T>();
            
            // Fallback to base implementation (local file)
            base.Read();
        }
    }

    public override async Task WriteAsync()
    {
        if (!ValidateFirestore()) return;

        if (documents == null || documents.Count == 0)
        {
            Debug.LogWarning($"No documents to save for {typeof(T).Name} collection (User: {documentId})");
            return;
        }

        try
        {
            // Only save the user's specific document
            if (documents.Count > 0)
            {
                T document = documents[0]; // Should only be one document for this user
                
                // Use the documentId as the Firestore document ID
                DocumentReference docRef = db.Collection(CollectionName).Document(documentId);
                
                // For known problematic types, skip Firebase converter and use custom converter directly
                if (typeof(T) == typeof(PlayerDocument) || typeof(T) == typeof(UserDocument) || typeof(T) == typeof(PlayerSettingsDocument))
                {
                    Debug.Log($"WriteAsync Using custom converter for {typeof(T).Name} to avoid Firebase conversion issues");
                    var documentData = ManualConvertToDocument(document);
                    await docRef.SetAsync(documentData);
                }
                else
                {
                    // Try to use Firebase's built-in converter first
                    try
                    {
                        await docRef.SetAsync(document);
                    }
                    catch (System.Exception converterEx)
                    {
                        // Log detailed debug information
                        FirestoreDebugHelper.LogWriteAttempt(document, "WriteAsync");
                        
                        // Fallback: Manual conversion using custom converter or Newtonsoft.Json
                        Debug.LogWarning($"WriteAsync Firebase converter failed for {typeof(T).Name}, using manual conversion: {converterEx.Message}");
                        var documentData = ManualConvertToDocument(document);
                        await docRef.SetAsync(documentData);
                    }
                }
            }

            Debug.Log($"Successfully saved user document to Firestore for {typeof(T).Name} collection (User: {documentId})");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to save {typeof(T).Name} collection to Firestore (User: {documentId}): {ex.Message}");
            
            // Fallback to base implementation (local file)
            base.Write();
        }
    }

    public override async void DeleteAsync()
    {
        if (!ValidateFirestore()) return;

        try
        {
            // Only delete the user's specific document
            DocumentReference docRef = db.Collection(CollectionName).Document(documentId);
            await docRef.DeleteAsync();

            // Clear local data
            documents = new List<T>();

            Debug.Log($"Successfully deleted user document from Firestore for {typeof(T).Name} collection (User: {documentId})");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to delete {typeof(T).Name} collection from Firestore (User: {documentId}): {ex.Message}");

            // Fallback to base implementation (local file)
            base.Delete();
        }
    }

    public override async void AddDocumentAsync(T document)
    {
        if (!ValidateFirestore()) return;

        if (documents == null)
        {
            documents = new List<T>();
        }
        
        // For user-specific collections, we should only have one document per user
        // Replace any existing document with the new one
        documents.Clear();
        documents.Add(document);
        UpdateTimestamp();
        
        try
        {
            // Save directly to the user's document
            DocumentReference docRef = db.Collection(CollectionName).Document(documentId);
            
            // For known problematic types, skip Firebase converter and use custom converter directly
            if (typeof(T) == typeof(PlayerDocument) || typeof(T) == typeof(UserDocument) || typeof(T) == typeof(PlayerSettingsDocument))
            {
                Debug.Log($"AddDocumentAsync Using custom converter for {typeof(T).Name} to avoid Firebase conversion issues");
                var documentData = ManualConvertToDocument(document);
                await docRef.SetAsync(documentData);
            }
            else
            {
                try
                {
                    await docRef.SetAsync(document);
                }
                catch (System.Exception converterEx)
                {
                    // Fallback: Manual conversion using custom converter or Newtonsoft.Json
                    Debug.LogWarning($"AddDocumentAsync Firebase converter failed for {typeof(T).Name}, using manual conversion: {converterEx.Message}");
                    var documentData = ManualConvertToDocument(document);
                    await docRef.SetAsync(documentData);
                }
            }
            
            Debug.Log($"Successfully added user document to Firestore for {typeof(T).Name} collection (User: {documentId})");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to add document to {typeof(T).Name} collection in Firestore (User: {documentId}): {ex.Message}");
        }
    }
    
    public async Task UpdateDocumentAsync(T document)
    {
        //return;//closed for disable firebase

        if (!ValidateFirestore()) return;
        
        try
        {
            // Check if the document already exists
            DocumentReference docRef = db.Collection(CollectionName).Document(documentId);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            
            if (snapshot.Exists)
            {
                // Document exists - update it
                Debug.Log($"UpdateDocumentAsync Document exists, updating {typeof(T).Name} for user {documentId}");
                
                // Update local collection
                var index = documents.FindIndex(d => d.ID.Equals(document.ID));
                if (index >= 0)
                {
                    documents[index] = document;
                }
                else
                {
                    documents.Clear();
                    documents.Add(document);
                }
                
                UpdateTimestamp();
                
                // Update in Firestore
                // For known problematic types, skip Firebase converter and use custom converter directly
                if (typeof(T) == typeof(PlayerDocument) || typeof(T) == typeof(UserDocument) || typeof(T) == typeof(PlayerSettingsDocument))
                {
                    Debug.Log($"UpdateDocumentAsync Using custom converter for {typeof(T).Name} to avoid Firebase conversion issues");
                    var documentData = ManualConvertToDocument(document);
                    Debug.Log($"UpdateDocumentAsync Updating document data: {JsonUtility.ToJson(documentData)}");
                    await docRef.SetAsync(documentData);
                }
                else
                {
                    try
                    {
                        await docRef.SetAsync(document);
                    }
                    catch (System.Exception converterEx)
                    {
                        // Fallback: Manual conversion using custom converter or Newtonsoft.Json
                        Debug.LogWarning($"UpdateDocumentAsync Firebase converter failed for {typeof(T).Name}, using manual conversion: {converterEx.Message}");
                        var documentData = ManualConvertToDocument(document);
                        Debug.Log($"UpdateDocumentAsync Updating document data: {JsonUtility.ToJson(documentData)}");
                        await docRef.SetAsync(documentData);
                    }
                }
                
                Debug.Log($"UpdateDocumentAsync Successfully updated document in Firestore for {typeof(T).Name} collection (User: {documentId})");
            }
            else
            {
                // Document doesn't exist - add new one
                Debug.Log($"UpdateDocumentAsync Document doesn't exist, adding new {typeof(T).Name} for user {documentId}");
                AddDocumentAsync(document);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"UpdateDocumentAsync Failed to update document for {typeof(T).Name} collection in Firestore (User: {documentId}): {ex.Message}");
        }
    }
}
