// Date: 28 June 2025
// AI-Generated: This code structure was created with AI assistance

using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;

public class FirestoreUserController : MonoBehaviour
{
    FirebaseFirestore db => FirestoreManager.Instance.db;

    public async Task CreateUserData(string userId, TankDocument data)
    {
        DocumentReference docRef = db.Collection("users").Document(userId);
        await docRef.SetAsync(data);
        Debug.Log("User data created in Firestore");
    }

    public async Task<TankDocument> GetUserData(string userId)
    {
        DocumentReference docRef = db.Collection("users").Document(userId);
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
        if (snapshot.Exists)
        {
            TankDocument userData = snapshot.ConvertTo<TankDocument>();
            Debug.Log("User data retrieved");
            return userData;
        }
        else
        {
            Debug.LogWarning("User data does not exist");
            return default(TankDocument);
        }
    }

    public async Task UpdateUserGold(string userId, int newGold)
    {
        DocumentReference docRef = db.Collection("users").Document(userId);
        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { "gold", newGold }
        };
        await docRef.UpdateAsync(updates);
        Debug.Log("User gold updated");
    }
}
