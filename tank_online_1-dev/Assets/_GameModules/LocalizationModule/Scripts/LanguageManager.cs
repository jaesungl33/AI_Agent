using UnityEngine;
using UnityEngine.Localization.Settings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class LanguageManager : MonoBehaviour, IInitializableManager
{
    [Header("Settings")]
    [SerializeField] private string defaultLocaleCode = "en"; // Fallback language
    [SerializeField] private bool autoDetectOnStart = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private const string PREF_KEY_LANGUAGE = "UserLanguage";
    private bool isInitialized = false;

    public UnityAction<bool> OnInitialized { get; set; }

    #region Lifecycle

    private void Start()
    {
        RegisterEvents();
    }
    
    public void Initialize()
    {
        if (autoDetectOnStart)
        {
            StartCoroutine(InitializeLanguageCoroutine());
        }
    }

    private void OnDestroy()
    {
        UnregisterEvents();
    }

    #endregion

    #region Event Management

    private void RegisterEvents()
    {
        EventManager.Register<ChangeLanguageEvent>(OnChangeLanguage);
    }

    private void UnregisterEvents()
    {
        EventManager.Unregister<ChangeLanguageEvent>(OnChangeLanguage);
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initialize language with priority: Saved > System > Default
    /// </summary>
    private IEnumerator InitializeLanguageCoroutine()
    {
        LogDebug("🌍 Initializing language system...");

        // Wait for Localization system to be ready
        yield return LocalizationSettings.InitializationOperation;

        string targetLocaleCode = DetermineLanguage();
        
        yield return SetLanguageCoroutine(targetLocaleCode, true);
        
        isInitialized = true;
        LogDebug($"✅ Language system initialized with: {targetLocaleCode}");
        OnInitialized?.Invoke(true);
    }

    /// <summary>
    /// Determine which language to use
    /// Priority: Saved Preference > System Language > Default English
    /// </summary>
    private string DetermineLanguage()
    {
        // 1. Check saved preference
        if (PlayerPrefs.HasKey(PREF_KEY_LANGUAGE))
        {
            string savedLanguage = PlayerPrefs.GetString(PREF_KEY_LANGUAGE);
            if (IsLocaleAvailable(savedLanguage))
            {
                LogDebug($"📌 Using saved language: {savedLanguage}");
                return savedLanguage;
            }
            else
            {
                LogDebug($"⚠️ Saved language '{savedLanguage}' not available");
            }
        }

        // 2. Try system language
        string systemLanguage = GetSystemLanguageCode();
        if (IsLocaleAvailable(systemLanguage))
        {
            LogDebug($"🖥️ Using system language: {systemLanguage}");
            return systemLanguage;
        }
        else
        {
            LogDebug($"⚠️ System language '{systemLanguage}' not available");
        }

        // 3. Fallback to default
        LogDebug($"🔄 Falling back to default language: {defaultLocaleCode}");
        return defaultLocaleCode;
    }

    /// <summary>
    /// Get system language code (ISO 639-1 format)
    /// </summary>
    private string GetSystemLanguageCode()
    {
        SystemLanguage systemLang = Application.systemLanguage;
        
        // Map Unity's SystemLanguage to ISO 639-1 codes
        Dictionary<SystemLanguage, string> languageMap = new Dictionary<SystemLanguage, string>
        {
            { SystemLanguage.English, "en" },
            { SystemLanguage.Vietnamese, "vi" },
            { SystemLanguage.Chinese, "zh" },
            { SystemLanguage.ChineseSimplified, "zh-Hans" },
            { SystemLanguage.ChineseTraditional, "zh-Hant" },
            { SystemLanguage.Japanese, "ja" },
            { SystemLanguage.Korean, "ko" },
            { SystemLanguage.French, "fr" },
            { SystemLanguage.German, "de" },
            { SystemLanguage.Spanish, "es" },
            { SystemLanguage.Russian, "ru" },
            { SystemLanguage.Portuguese, "pt" },
            { SystemLanguage.Italian, "it" },
            { SystemLanguage.Thai, "th" },
            { SystemLanguage.Indonesian, "id" },
        };

        if (languageMap.TryGetValue(systemLang, out string localeCode))
        {
            return localeCode;
        }

        LogDebug($"⚠️ Unknown system language: {systemLang}, using default");
        return defaultLocaleCode;
    }

    /// <summary>
    /// Check if locale is available in the project
    /// </summary>
    private bool IsLocaleAvailable(string localeCode)
    {
        if (string.IsNullOrEmpty(localeCode))
            return false;

        var locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
        return locale != null;
    }

    #endregion

    #region Language Change

    /// <summary>
    /// Event handler for language change requests
    /// </summary>
    public void OnChangeLanguage(ChangeLanguageEvent e)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("⚠️ LanguageManager not initialized yet!");
            return;
        }

        StartCoroutine(SetLanguageCoroutine(e.LocaleCode, false));
    }

    /// <summary>
    /// Change language and save preference
    /// </summary>
    private IEnumerator SetLanguageCoroutine(string localeCode, bool isInitialSetup)
    {
        if (!isInitialSetup)
        {
            // Wait for Localization to be ready (if not already)
            yield return LocalizationSettings.InitializationOperation;
        }

        var locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);

        if (locale != null)
        {
            LocalizationSettings.SelectedLocale = locale;
            
            // Save preference (only if user manually changed)
            if (!isInitialSetup)
            {
                PlayerPrefs.SetString(PREF_KEY_LANGUAGE, localeCode);
                PlayerPrefs.Save();
                LogDebug($"💾 Language preference saved: {localeCode}");
            }

            LogDebug($"✅ Language switched to: {locale.Identifier.Code}");
            
            // Fire event for other systems to react
            EventManager.TriggerEvent(new LanguageChangedEvent(localeCode));
        }
        else
        {
            Debug.LogError($"❌ Locale '{localeCode}' not found! Available locales:");
            PrintAvailableLocales();
            
            // Fallback to default if requested locale not found
            if (localeCode != defaultLocaleCode)
            {
                LogDebug($"🔄 Falling back to default: {defaultLocaleCode}");
                yield return SetLanguageCoroutine(defaultLocaleCode, isInitialSetup);
            }
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// Get current language code
    /// </summary>
    public string GetCurrentLanguage()
    {
        if (LocalizationSettings.SelectedLocale != null)
        {
            return LocalizationSettings.SelectedLocale.Identifier.Code;
        }
        return defaultLocaleCode;
    }

    /// <summary>
    /// Reset to system language
    /// </summary>
    public void ResetToSystemLanguage()
    {
        PlayerPrefs.DeleteKey(PREF_KEY_LANGUAGE);
        PlayerPrefs.Save();
        
        StartCoroutine(InitializeLanguageCoroutine());
    }

    /// <summary>
    /// Manually set language
    /// </summary>
    public void SetLanguage(string localeCode)
    {
        EventManager.TriggerEvent(new ChangeLanguageEvent(localeCode));
    }

    #endregion

    #region Debug Helpers

    private void LogDebug(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[LanguageManager] {message}");
        }
    }

    private void PrintAvailableLocales()
    {
        if (!showDebugLogs) return;

        var locales = LocalizationSettings.AvailableLocales.Locales;
        Debug.Log($"📋 Available locales ({locales.Count}):");
        
        foreach (var locale in locales)
        {
            Debug.Log($"  - {locale.Identifier.Code} ({locale.LocaleName})");
        }
    }

    [ContextMenu("Print Available Locales")]
    private void DebugPrintLocales()
    {
        PrintAvailableLocales();
    }

    [ContextMenu("Reset Language Preference")]
    private void DebugResetLanguage()
    {
        ResetToSystemLanguage();
    }

    [ContextMenu("Print Current Language")]
    private void DebugPrintCurrentLanguage()
    {
        Debug.Log($"Current Language: {GetCurrentLanguage()}");
    }

    #endregion
}

#region Events

public struct ChangeLanguageEvent
{
    public string LocaleCode;

    public ChangeLanguageEvent(string localeCode)
    {
        LocaleCode = localeCode;
    }
}

/// <summary>
/// Fired when language actually changed
/// </summary>
public struct LanguageChangedEvent
{
    public string NewLocaleCode;

    public LanguageChangedEvent(string newLocaleCode)
    {
        NewLocaleCode = newLocaleCode;
    }
}

#endregion