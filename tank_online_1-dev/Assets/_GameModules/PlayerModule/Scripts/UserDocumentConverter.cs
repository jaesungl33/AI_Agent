using System;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

/// <summary>
/// Custom converter for UserDocument to handle complex types for Firebase Firestore
/// </summary>
public static class UserDocumentConverter
{
    /// <summary>
    /// Convert UserDocument to Dictionary for Firestore storage
    /// </summary>
    public static Dictionary<string, object> ToFirestoreData(UserDocument user)
    {
        if (user == null) return new Dictionary<string, object>();

        var data = new Dictionary<string, object>
        {
            ["userID"] = user.userID ?? "",
            ["roleID"] = user.roleID ?? "",
            ["inactiveRoleIDs"] = user.inactiveRoleIDs ?? new List<string>()
        };

        return data;
    }

    /// <summary>
    /// Convert Firestore DocumentSnapshot to UserDocument
    /// </summary>
    public static UserDocument FromFirestoreData(DocumentSnapshot document)
    {
        if (!document.Exists) return null;

        try
        {
            var data = document.ToDictionary();
            var user = new UserDocument();

            // Basic string fields
            if (data.TryGetValue("userID", out object userIdObj))
                user.userID = userIdObj?.ToString() ?? "";

            if (data.TryGetValue("roleID", out object roleIdObj))
                user.roleID = roleIdObj?.ToString() ?? "";

            // Handle inactiveRoleIDs list
            if (data.TryGetValue("inactiveRoleIDs", out object inactiveRoleIDsObj))
            {
                user.inactiveRoleIDs = ConvertToStringList(inactiveRoleIDsObj);
            }

            return user;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error converting Firestore document to UserDocument: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Convert Firestore Dictionary to UserDocument
    /// </summary>
    public static UserDocument FromFirestoreData(Dictionary<string, object> data)
    {
        if (data == null) return null;

        try
        {
            var user = new UserDocument();

            // Basic string fields
            if (data.TryGetValue("userID", out object userIdObj))
                user.userID = userIdObj?.ToString() ?? "";

            if (data.TryGetValue("roleID", out object roleIdObj))
                user.roleID = roleIdObj?.ToString() ?? "";

            // Handle inactiveRoleIDs list
            if (data.TryGetValue("inactiveRoleIDs", out object inactiveRoleIDsObj))
            {
                user.inactiveRoleIDs = ConvertToStringList(inactiveRoleIDsObj);
            }

            return user;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error converting Firestore data to UserDocument: {ex.Message}");
            return null;
        }
    }

    private static List<string> ConvertToStringList(object value)
    {
        var result = new List<string>();
        
        if (value == null) return result;
        
        if (value is List<object> objectList)
        {
            foreach (var item in objectList)
            {
                if (item != null)
                    result.Add(item.ToString());
            }
        }
        else if (value is string[] stringArray)
        {
            result.AddRange(stringArray);
        }
        else if (value is List<string> stringList)
        {
            result.AddRange(stringList);
        }

        return result;
    }
}