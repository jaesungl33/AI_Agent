using System;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

/// <summary>
/// Custom converter for PlayerDocument to handle complex types for Firebase Firestore
/// </summary>
public static class PlayerDocumentConverter
{
    /// <summary>
    /// Convert PlayerDocument to Dictionary for Firestore storage
    /// </summary>
    public static Dictionary<string, object> ToFirestoreData(PlayerDocument player)
    {
        if (player == null) return new Dictionary<string, object>();

        var data = new Dictionary<string, object>
        {
            ["roleID"] = player.roleID ?? "",
            ["elo"] = player.elo,
            ["playerName"] = player.playerName ?? "",
            ["playerAvatar"] = player.playerAvatar ?? "",
            ["gold"] = player.gold,
            ["diamond"] = player.diamond,
            ["exp"] = player.exp,
            ["level"] = player.level,
            ["selectedModeIndex"] = player.selectedModeIndex,
            ["tanks"] = player.tanks ?? new List<string>(),
            [nameof(player.selectedTank)] = player.selectedTank ?? "",
            [nameof(player.ownedAIStickers)] = player.ownedAIStickers ?? new List<string>(),
            [nameof(player.ownedDecals)] = player.ownedDecals ?? new List<string>(),
            [nameof(player.ownedStickers)] = player.ownedStickers ?? new List<string>(),
            [nameof(player.ownedWraps)] = player.ownedWraps ?? new List<string>(),
            [nameof(player.formationTanks)] = player.formationTanks ?? new List<string>(),
        };

        // // Convert achievements dictionary to a format Firebase can handle
        // if (player.achievements != null && player.achievements.Count > 0)
        // {
        //     var achievementsData = new Dictionary<string, object>();
        //     foreach (var achievement in player.achievements)
        //     {
        //         achievementsData[achievement.Key] = achievement.Value;
        //     }
        //     data["achievements"] = achievementsData;
        // }
        // else
        // {
        //     data["achievements"] = new Dictionary<string, object>();
        // }

        return data;
    }

    /// <summary>
    /// Convert Firestore DocumentSnapshot to PlayerDocument
    /// </summary>
    public static PlayerDocument FromFirestoreData(DocumentSnapshot document)
    {
        if (!document.Exists) return null;

        try
        {
            var data = document.ToDictionary();
            var player = new PlayerDocument();

            // Basic string fields
            if (data.TryGetValue("roleID", out object roleIdObj))
                player.roleID = roleIdObj?.ToString() ?? "";

            if (data.TryGetValue("playerName", out object nameObj))
                player.playerName = nameObj?.ToString() ?? "";

            if (data.TryGetValue("playerAvatar", out object avatarObj))
                player.playerAvatar = avatarObj?.ToString() ?? "";

            // Numeric fields
            if (data.TryGetValue("elo", out object eloObj))
                player.elo = ConvertToInt(eloObj, 1000);

            if (data.TryGetValue("gold", out object goldObj))
                player.gold = ConvertToInt(goldObj, 0);

            if (data.TryGetValue("diamond", out object diamondObj))
                player.diamond = ConvertToInt(diamondObj, 0);

            if (data.TryGetValue("exp", out object expObj))
                player.exp = ConvertToLong(expObj, 0);

            if (data.TryGetValue("level", out object levelObj))
                player.level = ConvertToInt(levelObj, 1);

            if (data.TryGetValue("selectedModeIndex", out object modeObj))
                player.selectedModeIndex = ConvertToInt(modeObj, 0);

            if (data.TryGetValue("tanks", out object tanksObj))
            {
                player.tanks = ConvertToList(tanksObj);
            }

            if (data.TryGetValue("selectedTank", out object selectedTankObj))
                player.selectedTank = selectedTankObj?.ToString() ?? "";

            if (data.TryGetValue("ownedAIStickers", out object ownedAIStickersObj))
                player.ownedAIStickers = ConvertToList(ownedAIStickersObj);

            if (data.TryGetValue("ownedDecals", out object ownedDecalsObj))
                player.ownedDecals = ConvertToList(ownedDecalsObj);

            if (data.TryGetValue("ownedStickers", out object ownedStickersObj))
                player.ownedStickers = ConvertToList(ownedStickersObj);

            if (data.TryGetValue("ownedWraps", out object ownedWrapsObj))
                player.ownedWraps = ConvertToList(ownedWrapsObj);

            if (data.TryGetValue("formationTanks", out object formationTanksObj))
                player.formationTanks = ConvertToList(formationTanksObj);

            return player;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error converting Firestore document to PlayerDocument: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Convert Firestore Dictionary to PlayerDocument
    /// </summary>
    public static PlayerDocument FromFirestoreData(Dictionary<string, object> data)
    {
        if (data == null) return null;

        try
        {
            var player = new PlayerDocument();

            // Basic string fields
            if (data.TryGetValue("roleID", out object roleIdObj))
                player.roleID = roleIdObj?.ToString() ?? "";

            if (data.TryGetValue("playerName", out object nameObj))
                player.playerName = nameObj?.ToString() ?? "";

            if (data.TryGetValue("playerAvatar", out object avatarObj))
                player.playerAvatar = avatarObj?.ToString() ?? "";

            // Numeric fields
            if (data.TryGetValue("elo", out object eloObj))
                player.elo = ConvertToInt(eloObj, 1000);

            if (data.TryGetValue("gold", out object goldObj))
                player.gold = ConvertToInt(goldObj, 0);

            if (data.TryGetValue("diamond", out object diamondObj))
                player.diamond = ConvertToInt(diamondObj, 0);

            if (data.TryGetValue("exp", out object expObj))
                player.exp = ConvertToLong(expObj, 0);

            if (data.TryGetValue("level", out object levelObj))
                player.level = ConvertToInt(levelObj, 1);

            if (data.TryGetValue("selectedModeIndex", out object modeObj))
                player.selectedModeIndex = ConvertToInt(modeObj, 0);

            // Handle tanks array/list
            if (data.TryGetValue("tanks", out object tanksObj))
            {
                player.tanks = ConvertToList(tanksObj);
            }

            if (data.TryGetValue("selectedTank", out object selectedTankObj))
                player.selectedTank = selectedTankObj?.ToString() ?? "";

            if (data.TryGetValue("ownedAIStickers", out object ownedAIStickersObj))
                player.ownedAIStickers = ConvertToList(ownedAIStickersObj);

            if (data.TryGetValue("ownedDecals", out object ownedDecalsObj))
                player.ownedDecals = ConvertToList(ownedDecalsObj);

            if (data.TryGetValue("ownedStickers", out object ownedStickersObj))
                player.ownedStickers = ConvertToList(ownedStickersObj);

            if (data.TryGetValue("ownedWraps", out object ownedWrapsObj))
                player.ownedWraps = ConvertToList(ownedWrapsObj);
                
            if (data.TryGetValue("formationTanks", out object formationTanksObj))
                player.formationTanks = ConvertToList(formationTanksObj);

            return player;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error converting Firestore data to PlayerDocument: {ex.Message}");
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

    private static long ConvertToLong(object value, long defaultValue = 0)
    {
        if (value == null) return defaultValue;
        if (value is long longVal) return longVal;
        if (value is int intVal) return intVal;
        if (long.TryParse(value.ToString(), out long result)) return result;
        return defaultValue;
    }

    private static List<string> ConvertToList(object value)
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
            foreach (var item in stringArray)
            {
                if (item != null)
                    result.Add(item);
            }
        }
        else if (value is List<string> stringList)
        {
            foreach (var item in stringList)
            {
                if (item != null)
                    result.Add(item);
            }
        }

        return result;
    }

    private static Dictionary<string, object> ConvertToObjectDictionary(object value)
    {
        var result = new Dictionary<string, object>();
        
        if (value == null) return result;
        
        if (value is Dictionary<string, object> dict)
        {
            return dict;
        }
        else if (value is Dictionary<object, object> objDict)
        {
            foreach (var kvp in objDict)
            {
                if (kvp.Key != null)
                {
                    result[kvp.Key.ToString()] = kvp.Value;
                }
            }
        }
        
        return result;
    }

    private static Dictionary<string, long> ConvertToLongDictionary(object value)
    {
        var result = new Dictionary<string, long>();
        
        if (value == null) return result;
        
        if (value is Dictionary<string, object> dict)
        {
            foreach (var kvp in dict)
            {
                result[kvp.Key] = ConvertToLong(kvp.Value, 0);
            }
        }
        
        return result;
    }
}