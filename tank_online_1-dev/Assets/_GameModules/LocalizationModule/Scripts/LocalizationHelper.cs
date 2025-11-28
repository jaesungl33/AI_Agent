using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Helper class for quick access to localized strings
/// Supports both sync and async operations
/// </summary>
public static class LocalizationHelper
{
    #region Synchronous API (Simple & Fast)

    /// <summary>
    /// Get localized string by key (synchronous)
    /// Returns key if translation not found
    /// </summary>
    /// <param name="tableName">String Table name (e.g., "UI", "Gameplay")</param>
    /// <param name="entryKey">Entry key in the table</param>
    /// <returns>Localized string or key if not found</returns>
    public static string GetString(string tableName, string entryKey)
    {
        try
        {
            var stringTable = LocalizationSettings.StringDatabase.GetTable(tableName);
            if (stringTable != null)
            {
                var entry = stringTable.GetEntry(entryKey);
                if (entry != null)
                {
                    return entry.LocalizedValue;
                }
            }

            Debug.LogWarning($"[LocalizationHelper] Entry '{entryKey}' not found in table '{tableName}'");
            return entryKey; // Fallback to key
        }
        catch (Exception e)
        {
            Debug.LogError($"[LocalizationHelper] Error getting string: {e.Message}");
            return entryKey;
        }
    }

    public static string GetSmartString(string tableName, string entryKey, Dictionary<string, object> variables)
    {
        try
        {
            var stringTable = LocalizationSettings.StringDatabase.GetTable(tableName);
            if (stringTable != null)
            {
                var entry = stringTable.GetEntry(entryKey);
                if (entry != null)
                {
                    string localizedValue = entry.LocalizedValue;
                    
                    // Replace variables in format {key}
                    if (variables != null && variables.Count > 0)
                    {
                        foreach (var kvp in variables)
                        {
                            string placeholder = "{" + kvp.Key + "}";
                            localizedValue = localizedValue.Replace(placeholder, kvp.Value.ToString());
                        }
                    }
                    
                    return localizedValue;
                }
            }

            Debug.LogWarning($"[LocalizationHelper] Entry '{entryKey}' not found in table '{tableName}'");
            return entryKey;
        }
        catch (Exception e)
        {
            Debug.LogError($"[LocalizationHelper] Error getting string: {e.Message}");
            return entryKey;
        }
    }

    /// <summary>
    /// Get localized string with format arguments (synchronous)
    /// Example: GetString("UI", "welcome_message", playerName)
    /// </summary>
    public static string GetString(string tableName, string entryKey, params object[] args)
    {
        string localizedString = GetString(tableName, entryKey);
        
        if (args != null && args.Length > 0)
        {
            try
            {
                return string.Format(localizedString, args);
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalizationHelper] Format error: {e.Message}");
                return localizedString;
            }
        }

        return localizedString;
    }

    /// <summary>
    /// Get localized string using TableReference (synchronous)
    /// </summary>
    public static string GetString(TableReference tableReference, string entryKey)
    {
        try
        {
            var stringTable = LocalizationSettings.StringDatabase.GetTable(tableReference);
            if (stringTable != null)
            {
                var entry = stringTable.GetEntry(entryKey);
                if (entry != null)
                {
                    return entry.LocalizedValue;
                }
            }

            Debug.LogWarning($"[LocalizationHelper] Entry '{entryKey}' not found in table");
            return entryKey;
        }
        catch (Exception e)
        {
            Debug.LogError($"[LocalizationHelper] Error getting string: {e.Message}");
            return entryKey;
        }
    }

    #endregion

    #region Asynchronous API (For loading heavy content)

    /// <summary>
    /// Get localized string asynchronously
    /// </summary>
    /// <param name="tableName">String Table name</param>
    /// <param name="entryKey">Entry key</param>
    /// <param name="onComplete">Callback with localized string</param>
    public static void GetStringAsync(string tableName, string entryKey, Action<string> onComplete)
    {
        var operation = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(tableName, entryKey);
        
        operation.Completed += (op) =>
        {
            if (op.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                onComplete?.Invoke(op.Result);
            }
            else
            {
                Debug.LogError($"[LocalizationHelper] Failed to load '{entryKey}' from '{tableName}'");
                onComplete?.Invoke(entryKey); // Fallback
            }
        };
    }

    /// <summary>
    /// Get localized string asynchronously with format
    /// </summary>
    public static void GetStringAsync(string tableName, string entryKey, Action<string> onComplete, params object[] args)
    {
        GetStringAsync(tableName, entryKey, (result) =>
        {
            if (args != null && args.Length > 0)
            {
                try
                {
                    result = string.Format(result, args);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[LocalizationHelper] Format error: {e.Message}");
                }
            }
            onComplete?.Invoke(result);
        });
    }

    #endregion

    #region Smart Localized String (LocalizedString wrapper)

    /// <summary>
    /// Get LocalizedString reference (for data binding)
    /// Use this for TextMeshPro LocalizeStringEvent
    /// </summary>
    public static LocalizedString GetLocalizedString(string tableName, string entryKey)
    {
        return new LocalizedString
        {
            TableReference = tableName,
            TableEntryReference = entryKey
        };
    }
    #endregion

    #region Table Management

    /// <summary>
    /// Check if a table exists
    /// </summary>
    public static bool TableExists(string tableName)
    {
        try
        {
            var table = LocalizationSettings.StringDatabase.GetTable(tableName);
            return table != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Check if an entry exists in a table
    /// </summary>
    public static bool EntryExists(string tableName, string entryKey)
    {
        try
        {
            var table = LocalizationSettings.StringDatabase.GetTable(tableName);
            if (table != null)
            {
                var entry = table.GetEntry(entryKey);
                return entry != null;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get all keys from a table
    /// </summary>
    public static List<string> GetAllKeys(string tableName)
    {
        var keys = new List<string>();
        
        try
        {
            var table = LocalizationSettings.StringDatabase.GetTable(tableName);
            if (table != null)
            {
                foreach (var entry in table.Values)
                {
                    keys.Add(entry.Key);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[LocalizationHelper] Error getting keys: {e.Message}");
        }

        return keys;
    }

    #endregion

    #region Current Language Info

    /// <summary>
    /// Get current language code (e.g., "en", "vi")
    /// </summary>
    public static string GetCurrentLanguageCode()
    {
        return LocalizationSettings.SelectedLocale?.Identifier.Code ?? "en";
    }

    /// <summary>
    /// Get current language display name (e.g., "English", "Tiếng Việt")
    /// </summary>
    public static string GetCurrentLanguageName()
    {
        return LocalizationSettings.SelectedLocale?.LocaleName ?? "English";
    }

    /// <summary>
    /// Get all available languages
    /// </summary>
    public static List<(string code, string name)> GetAvailableLanguages()
    {
        var languages = new List<(string, string)>();
        
        var locales = LocalizationSettings.AvailableLocales.Locales;
        foreach (var locale in locales)
        {
            languages.Add((locale.Identifier.Code, locale.LocaleName));
        }

        return languages;
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Format number with current locale settings
    /// </summary>
    public static string FormatNumber(int number)
    {
        return number.ToString("N0", LocalizationSettings.SelectedLocale?.Formatter);
    }

    /// <summary>
    /// Format currency with current locale settings
    /// </summary>
    public static string FormatCurrency(float amount)
    {
        return amount.ToString("C", LocalizationSettings.SelectedLocale?.Formatter);
    }

    /// <summary>
    /// Format date with current locale settings
    /// </summary>
    public static string FormatDate(DateTime date)
    {
        return date.ToString("d", LocalizationSettings.SelectedLocale?.Formatter);
    }

    /// <summary>
    /// Format percentage with current locale settings
    /// </summary>
    public static string FormatPercentage(float value)
    {
        return value.ToString("P", LocalizationSettings.SelectedLocale?.Formatter);
    }

    #endregion

    #region Debug Helpers

    /// <summary>
    /// Print all entries in a table (for debugging)
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void DebugPrintTable(string tableName)
    {
        try
        {
            var table = LocalizationSettings.StringDatabase.GetTable(tableName);
            if (table != null)
            {
                Debug.Log($"📋 Table '{tableName}' entries:");
                foreach (var entry in table.Values)
                {
                    Debug.Log($"  - {entry.Key}: {entry.LocalizedValue}");
                }
            }
            else
            {
                Debug.LogWarning($"Table '{tableName}' not found");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error: {e.Message}");
        }
    }

    /// <summary>
    /// Print all available locales
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void DebugPrintAvailableLocales()
    {
        var locales = LocalizationSettings.AvailableLocales.Locales;
        Debug.Log($"🌍 Available Locales ({locales.Count}):");
        
        foreach (var locale in locales)
        {
            Debug.Log($"  - {locale.Identifier.Code}: {locale.LocaleName}");
        }
    }

    #endregion
}