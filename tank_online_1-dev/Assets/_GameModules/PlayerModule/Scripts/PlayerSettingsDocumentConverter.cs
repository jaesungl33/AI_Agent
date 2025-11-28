using System;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

/// <summary>
/// Custom converter for PlayerSettingsDocument to handle complex types for Firebase Firestore
/// </summary>
public static class PlayerSettingsDocumentConverter
{
    /// <summary>
    /// Convert PlayerSettingsDocument to Dictionary for Firestore storage
    /// </summary>
    public static Dictionary<string, object> ToFirestoreData(PlayerSettingsDocument settings)
    {
        if (settings == null) return new Dictionary<string, object>();

        var data = new Dictionary<string, object>
        {
            ["roleID"] = settings.roleID ?? "",
            ["graphicSetting"] = settings.graphicSetting,
            ["enableSfx"] = settings.enableSfx,
            ["enableBgm"] = settings.enableBgm,
            ["enableVibrate"] = settings.enableVibrate,
            ["enableNotification"] = settings.enableNotification
        };

        return data;
    }

    /// <summary>
    /// Convert Firestore DocumentSnapshot to PlayerSettingsDocument
    /// </summary>
    public static PlayerSettingsDocument FromFirestoreData(DocumentSnapshot document)
    {
        if (!document.Exists) return null;

        try
        {
            var data = document.ToDictionary();
            var settings = new PlayerSettingsDocument();

            // Basic string fields
            if (data.TryGetValue("roleID", out object roleIdObj))
                settings.roleID = roleIdObj?.ToString() ?? "";

            // Numeric fields
            if (data.TryGetValue("graphicSetting", out object graphicObj))
                settings.graphicSetting = ConvertToInt(graphicObj, 0);

            // Boolean fields
            if (data.TryGetValue("enableSfx", out object sfxObj))
                settings.enableSfx = ConvertToBool(sfxObj, true);

            if (data.TryGetValue("enableBgm", out object bgmObj))
                settings.enableBgm = ConvertToBool(bgmObj, true);

            if (data.TryGetValue("enableVibrate", out object vibrateObj))
                settings.enableVibrate = ConvertToBool(vibrateObj, true);

            if (data.TryGetValue("enableNotification", out object notificationObj))
                settings.enableNotification = ConvertToBool(notificationObj, true);

            return settings;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error converting Firestore document to PlayerSettingsDocument: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Convert Firestore Dictionary to PlayerSettingsDocument
    /// </summary>
    public static PlayerSettingsDocument FromFirestoreData(Dictionary<string, object> data)
    {
        if (data == null) return null;

        try
        {
            var settings = new PlayerSettingsDocument();

            // Basic string fields
            if (data.TryGetValue("roleID", out object roleIdObj))
                settings.roleID = roleIdObj?.ToString() ?? "";

            // Numeric fields
            if (data.TryGetValue("graphicSetting", out object graphicObj))
                settings.graphicSetting = ConvertToInt(graphicObj, 0);

            // Boolean fields
            if (data.TryGetValue("enableSfx", out object sfxObj))
                settings.enableSfx = ConvertToBool(sfxObj, true);

            if (data.TryGetValue("enableBgm", out object bgmObj))
                settings.enableBgm = ConvertToBool(bgmObj, true);

            if (data.TryGetValue("enableVibrate", out object vibrateObj))
                settings.enableVibrate = ConvertToBool(vibrateObj, true);

            if (data.TryGetValue("enableNotification", out object notificationObj))
                settings.enableNotification = ConvertToBool(notificationObj, true);

            return settings;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error converting Firestore data to PlayerSettingsDocument: {ex.Message}");
            return null;
        }
    }

    private static int ConvertToInt(object value, int defaultValue = 0)
    {
        if (value == null) return defaultValue;
        if (value is int intVal) return intVal;
        if (value is long longVal) return (int)longVal;
        if (int.TryParse(value.ToString(), out int result)) return result;
        return defaultValue;
    }

    private static bool ConvertToBool(object value, bool defaultValue = false)
    {
        if (value == null) return defaultValue;
        if (value is bool boolVal) return boolVal;
        if (bool.TryParse(value.ToString(), out bool result)) return result;
        return defaultValue;
    }
}