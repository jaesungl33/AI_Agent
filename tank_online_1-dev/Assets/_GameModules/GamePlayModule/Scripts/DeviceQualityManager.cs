using System.Linq;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.iOS;
#endif
using UnityEngine.Rendering.PostProcessing;

/// <summary>
/// Manages quality settings based on device capabilities and platform.
/// Handles iOS and Android devices with specific optimizations for problematic devices.
/// </summary>
public class DeviceQualityManager
{
    private DeviceQualityConfig deviceQualityConfig;
    #region Public Properties

    public int CurrentQualityLevel { get; private set; }
    public int CurrentTargetFrameRate { get; private set; }
    public string DeviceCategory { get; private set; }

    #endregion

    #region Public Methods
    public DeviceQualityManager(DeviceQualityConfig config)
    {
        deviceQualityConfig = config;
    }

    /// <summary>
    /// Initialize and apply quality settings based on device
    /// </summary>
    public void Initialize()
    {
        // Detect platform and apply appropriate settings
#if UNITY_IOS
        ApplyiOSQualitySettings();
#elif UNITY_ANDROID
        ApplyAndroidQualitySettings();
#else
        ApplyDefaultQualitySettings();
#endif

        // Apply common mobile optimizations
        ApplyCommonMobileOptimizations();

        LogDeviceInfo();
    }

    /// <summary>
    /// Manually adjust quality at runtime
    /// </summary>
    public void SetManualQualityLevel(int level)
    {
        level = Mathf.Clamp(level, 0, 2);

        switch (level)
        {
            case 0:
                ApplyLowQualitySettings("Manual override");
                break;
            case 1:
                ApplyMediumQualitySettings("Manual override");
                break;
            case 2:
                ApplyHighQualitySettings("Manual override");
                break;
        }

        ApplyCommonMobileOptimizations();
    }

    /// <summary>
    /// Get current quality information as string
    /// </summary>
    public string GetQualityInfo()
    {
        return $"Quality: {QualitySettings.GetQualityLevel()} ({DeviceCategory}), " +
               $"FPS Target: {Application.targetFrameRate}, " +
               $"Shadows: {QualitySettings.shadows}, " +
               $"Shadow Distance: {QualitySettings.shadowDistance}m";
    }

    #endregion

    #region Platform-Specific Settings

    /// <summary>
    /// Apply quality settings for iOS devices
    /// </summary>
    private void ApplyiOSQualitySettings()
    {
        string deviceModel = SystemInfo.deviceModel.ToLower();
        int systemMemory = SystemInfo.systemMemorySize;

        Debug.Log($"[iOS] Device: {deviceModel}, RAM: {systemMemory}MB");

        // Check for problematic iOS devices
        if (IsProblematiciOSDevice(deviceModel, systemMemory))
        {
            ApplyLowQualitySettings("Problematic iOS device detected");
            return;
        }

        // iPhone categorization
        if (IsHighEndiPhone(deviceModel, systemMemory))
        {
            ApplyHighQualitySettings("High-end iPhone");
        }
        else if (IsMidRangeiPhone(deviceModel, systemMemory))
        {
            ApplyMediumQualitySettings("Mid-range iPhone");
        }
        else
        {
            ApplyLowQualitySettings("Low-end iPhone");
        }
    }

    /// <summary>
    /// Apply quality settings for Android devices
    /// </summary>
    private void ApplyAndroidQualitySettings()
    {
        string deviceModel = SystemInfo.deviceModel.ToLower();
        string deviceName = SystemInfo.deviceName.ToLower();
        int systemMemory = SystemInfo.systemMemorySize;
        int graphicsMemory = SystemInfo.graphicsMemorySize;

        Debug.Log($"[Android] Model: {deviceModel}, Name: {deviceName}, RAM: {systemMemory}MB, VRAM: {graphicsMemory}MB");

        // Check for problematic Android devices first
        if (IsProblematicAndroidDevice(deviceModel, deviceName, systemMemory))
        {
            ApplyLowQualitySettings("Problematic Android device detected");
            return;
        }

        // Special handling for specific brands
        if (IsXiaomiRedmiDevice(deviceModel, deviceName))
        {
            ApplyRedmiOptimizedSettings(deviceModel, systemMemory);
            return;
        }

        if (IsSamsungDevice(deviceModel, deviceName))
        {
            ApplySamsungOptimizedSettings(deviceModel, systemMemory);
            return;
        }

        // General Android categorization
        if (IsHighEndAndroid(deviceModel, systemMemory, graphicsMemory))
        {
            ApplyHighQualitySettings("High-end Android");
        }
        else if (IsMidRangeAndroid(deviceModel, systemMemory, graphicsMemory))
        {
            ApplyMediumQualitySettings("Mid-range Android");
        }
        else
        {
            ApplyLowQualitySettings("Low-end Android");
        }
    }

    #endregion

    #region Device Detection - iOS

    /// <summary>
    /// Detect high-end iPhone devices
    /// </summary>
    private bool IsHighEndiPhone(string deviceModel, int ram)
    {
        // iPhone 12 and newer, Pro models, Pro Max models
        string[] highEndModels = {
                "iphone14", "iphone15", "iphone16", // Latest models
                "iphone13 pro", "iphone14 pro", "iphone15 pro", "iphone16 pro",
                "iphone13 pro max", "iphone14 pro max", "iphone15 pro max", "iphone16 pro max",
                "iphone12 pro", "iphone12 pro max"
            };

        return ram >= 4096 || highEndModels.Any(model => deviceModel.Contains(model));
    }

    /// <summary>
    /// Detect mid-range iPhone devices
    /// </summary>
    private bool IsMidRangeiPhone(string deviceModel, int ram)
    {
        // iPhone 11, 12, 13 standard models, SE 2nd gen and newer
        string[] midRangeModels = {
                "iphone11", "iphone12", "iphone13",
                "iphone se (2", "iphone se (3"
            };

        return (ram >= 3072 && ram < 4096) || midRangeModels.Any(model => deviceModel.Contains(model));
    }

    /// <summary>
    /// Detect problematic iOS devices that need special handling
    /// </summary>
    private bool IsProblematiciOSDevice(string deviceModel, int ram)
    {
        // Older devices known for performance issues
        string[] problematicModels = {
                "iphone6", "iphone7", "iphone8", // Old A-series chips
                "iphone se (1", // First gen SE with A9 chip
                "iphone x" // Known for throttling issues
            };

        // Devices with low RAM
        if (ram < 2048) return true;

        return problematicModels.Any(model => deviceModel.Contains(model));
    }

    #endregion

    #region Device Detection - Android

    /// <summary>
    /// Detect high-end Android devices
    /// </summary>
    private bool IsHighEndAndroid(string deviceModel, int ram, int vram)
    {
        DeviceQualityDocument deviceQualityDocument = DatabaseManager.GetDB<DeviceQualityCollection>().GetDeviceQualityDocument(DeviceQualityLevel.High);
        if (ram >= deviceQualityDocument.targetRamMB || vram >= deviceQualityDocument.targetVRAMMB) return true;
        return deviceQualityDocument.keywords.Any(keyword => deviceModel.Contains(keyword));
        // string[] highEndKeywords
        //  = {
        //         "snapdragon 8", "snapdragon 888", "snapdragon 8 gen",
        //         "dimensity 9", "dimensity 8200",
        //         "black shark", "rog phone", "legion", "red magic",
        //         "galaxy s21", "galaxy s22", "galaxy s23", "galaxy s24",
        //         "oneplus 9", "oneplus 10", "oneplus 11", "oneplus 12",
        //         "pixel 6", "pixel 7", "pixel 8",
        //         "xiaomi 12 pro", "xiaomi 13", "xiaomi 14"
        //     };
    }

    /// <summary>
    /// Detect mid-range Android devices
    /// </summary>
    private bool IsMidRangeAndroid(string deviceModel, int ram, int vram)
    {
        DeviceQualityDocument deviceQualityDocument = DatabaseManager.GetDB<DeviceQualityCollection>().GetDeviceQualityDocument(DeviceQualityLevel.Medium);
        if (ram >= deviceQualityDocument.targetRamMB || vram >= deviceQualityDocument.targetVRAMMB) return true;
        return deviceQualityDocument.keywords.Any(keyword => deviceModel.Contains(keyword));

        // if ((ram >= 4096 && ram < 8192) || (vram >= 1024 && vram < 2048)) return true;

        // string[] midRangeKeywords = {
        //         "snapdragon 7", "snapdragon 6", "snapdragon 778", "snapdragon 695",
        //         "dimensity 7", "dimensity 6", "dimensity 1200",
        //         "galaxy a5", "galaxy a7", "galaxy m",
        //         "redmi note", "poco x", "poco f"
        //     };

        // return midRangeKeywords.Any(keyword => deviceModel.Contains(keyword));
    }

    /// <summary>
    /// Detect Xiaomi/Redmi devices
    /// </summary>
    private bool IsXiaomiRedmiDevice(string deviceModel, string deviceName)
    {
        string[] xiaomiKeywords = { "redmi", "poco", "xiaomi", "mi " };
        return xiaomiKeywords.Any(keyword => deviceModel.Contains(keyword) || deviceName.Contains(keyword));
    }

    /// <summary>
    /// Detect Samsung devices
    /// </summary>
    private bool IsSamsungDevice(string deviceModel, string deviceName)
    {
        return deviceModel.Contains("samsung") || deviceModel.Contains("galaxy") ||
               deviceName.Contains("samsung") || deviceName.Contains("galaxy");
    }

    /// <summary>
    /// Detect problematic Android devices
    /// </summary>
    private bool IsProblematicAndroidDevice(string deviceModel, string deviceName, int ram)
    {
        // Low RAM devices
        if (ram < 3072) return true; // Less than 3GB RAM

        // Known problematic chipsets
        string[] problematicChipsets = {
                "helio p", "helio a", // MediaTek budget chips with thermal issues
                "snapdragon 4", "snapdragon 425", "snapdragon 450", // Old budget Snapdragon
                "exynos 850", "exynos 9611", // Samsung budget chips with GPU issues
                "mali-400", "mali-450" // Very old GPU
            };

        // Known problematic device models
        string[] problematicDevices = {
                "redmi 6", "redmi 7", "redmi 8", // Old Redmi with thermal issues
                "galaxy a01", "galaxy a02", "galaxy a10", "galaxy a11", // Low-end Samsung
                "moto e", "moto g4", "moto g5", // Old Motorola budget
                "oppo a3", "oppo a5", "oppo a7", // Old budget OPPO
                "vivo y12", "vivo y15", "vivo y17" // Old budget Vivo
            };

        string combinedCheck = (deviceModel + " " + deviceName).ToLower();

        return problematicChipsets.Any(chip => combinedCheck.Contains(chip)) ||
               problematicDevices.Any(device => combinedCheck.Contains(device));
    }

    #endregion

    #region Brand-Specific Optimizations

    /// <summary>
    /// Apply optimized settings for Redmi devices
    /// </summary>
    private void ApplyRedmiOptimizedSettings(string deviceModel, int ram)
    {
        Debug.Log("[Redmi] Applying brand-specific optimizations");

        // Redmi devices often have aggressive power saving
        // Use slightly lower settings than specs suggest
        if (ram >= 8192)
        {
            ApplyMediumQualitySettings("Redmi High-end (conservative)");
            Application.targetFrameRate = 60;
        }
        else if (ram >= 6144)
        {
            ApplyMediumQualitySettings("Redmi Mid-range");
            Application.targetFrameRate = 60; // 60fps more stable than 45 on Redmi
        }
        else if (ram >= 4096)
        {
            ApplyLowQualitySettings("Redmi Budget");
            Application.targetFrameRate = 30;
        }
        else
        {
            ApplyLowQualitySettings("Redmi Low-end");
            Application.targetFrameRate = 30;

            // Extra optimizations for very low-end Redmi
            QualitySettings.shadowDistance = 10f;
            QualitySettings.shadows = ShadowQuality.Disable;
        }

        // Redmi-specific tweaks
        QualitySettings.realtimeReflectionProbes = false; // Redmi GPU struggles with this
        QualitySettings.billboardsFaceCameraPosition = false;

        CurrentTargetFrameRate = Application.targetFrameRate;
    }

    /// <summary>
    /// Apply optimized settings for Samsung devices
    /// </summary>
    private void ApplySamsungOptimizedSettings(string deviceModel, int ram)
    {
        Debug.Log("[Samsung] Applying brand-specific optimizations");

        bool isExynos = deviceModel.Contains("exynos");

        if (ram >= 8192 && !isExynos)
        {
            ApplyHighQualitySettings("Samsung Flagship (Snapdragon)");
        }
        else if (ram >= 8192 && isExynos)
        {
            // Exynos variants run hotter and slower
            ApplyMediumQualitySettings("Samsung Flagship (Exynos - conservative)");
            Application.targetFrameRate = 45; // More stable on Exynos
            CurrentTargetFrameRate = 45;
        }
        else if (ram >= 6144)
        {
            ApplyMediumQualitySettings("Samsung Mid-range");
        }
        else
        {
            ApplyLowQualitySettings("Samsung Budget");
        }

        // Samsung-specific tweaks
        if (isExynos)
        {
            // Exynos GPUs have issues with certain features
            QualitySettings.realtimeReflectionProbes = false;
            QualitySettings.softParticles = false;
        }
    }

    #endregion

    #region Quality Level Application

    /// <summary>
    /// Apply high quality settings
    /// </summary>
    private void ApplyHighQualitySettings(string reason)
    {
        Debug.Log($"<color=green>[Quality] HIGH - {reason}</color>");
        DeviceCategory = reason;
        ApplyQualitySettings(DeviceQualityLevel.High);
    }

    /// <summary>
    /// Apply medium quality settings
    /// </summary>
    private void ApplyMediumQualitySettings(string reason)
    {
        Debug.Log($"<color=yellow>[Quality] MEDIUM - {reason}</color>");
        DeviceCategory = reason;
        ApplyQualitySettings(DeviceQualityLevel.Medium);
    }

    /// <summary>
    /// Apply low quality settings
    /// </summary>
    private void ApplyLowQualitySettings(string reason)
    {
        Debug.Log($"<color=red>[Quality] LOW - {reason}</color>");
        DeviceCategory = reason;
        ApplyQualitySettings(DeviceQualityLevel.Low);
    }

    private void ApplyQualitySettings(DeviceQualityLevel level)
    {
        CurrentQualityLevel = (int) level;
        CurrentTargetFrameRate = deviceQualityConfig.profiles[CurrentQualityLevel].targetFrameRate;
        QualitySettings.SetQualityLevel(CurrentQualityLevel, true);
        Application.targetFrameRate = deviceQualityConfig.profiles[CurrentQualityLevel].targetFrameRate;

        QualitySettings.shadows = deviceQualityConfig.profiles[CurrentQualityLevel].shadows;
        QualitySettings.shadowResolution = deviceQualityConfig.profiles[CurrentQualityLevel].shadowResolution;
        QualitySettings.shadowDistance = deviceQualityConfig.profiles[CurrentQualityLevel].shadowDistance;
        QualitySettings.shadowCascades = deviceQualityConfig.profiles[CurrentQualityLevel].shadowCascades;

        QualitySettings.antiAliasing = deviceQualityConfig.profiles[CurrentQualityLevel].antiAliasing;
        QualitySettings.anisotropicFiltering =  deviceQualityConfig.profiles[CurrentQualityLevel].anisotropicFiltering;
        QualitySettings.softParticles = deviceQualityConfig.profiles[CurrentQualityLevel].softParticles;
        QualitySettings.realtimeReflectionProbes = deviceQualityConfig.profiles[CurrentQualityLevel].realtimeReflectionProbes;

        QualitySettings.lodBias = deviceQualityConfig.profiles[CurrentQualityLevel].lodBias;
        QualitySettings.particleRaycastBudget = deviceQualityConfig.profiles[CurrentQualityLevel].particleRaycastBudget;
        QualitySettings.maximumLODLevel = deviceQualityConfig.profiles[CurrentQualityLevel].maximumLODLevel;

        EnableBloom(deviceQualityConfig.profiles[CurrentQualityLevel].allowBloom);
    }
    
    private void EnableBloom(bool status)
    {
        if (deviceQualityConfig != null && deviceQualityConfig.volumeProfile != null)
        {
            // Disable post-processing effects for low quality
            var profile = deviceQualityConfig.volumeProfile;
            foreach (var setting in profile.components)
            {
                Debug.Log("[Quality] Disabling post-processing effect: " + setting.name);
                if (setting != null && setting.displayName == "Bloom")
                {
                    setting.active = status;
                }
            }
        }
    }

    /// <summary>
    /// Apply default quality settings for non-mobile platforms
    /// </summary>
    private void ApplyDefaultQualitySettings()
    {
        Debug.Log("[Quality] Default settings for Editor/Desktop");

        CurrentQualityLevel = 2;
        CurrentTargetFrameRate = 60;
        DeviceCategory = "Editor/Desktop";

        QualitySettings.SetQualityLevel(2, true);
        Application.targetFrameRate = 60;
    }

    #endregion

    #region Common Optimizations

    /// <summary>
    /// Apply common mobile optimizations regardless of platform
    /// </summary>
    private void ApplyCommonMobileOptimizations()
    {
        // Disable VSync for consistent frame rate
        QualitySettings.vSyncCount = 0;

        // Reduce input lag
        QualitySettings.maxQueuedFrames = 2;

        // Optimize physics
        Physics.defaultSolverIterations = 6;
        Physics.defaultSolverVelocityIterations = 1;

        // Optimize skin weights
        QualitySettings.skinWeights = SkinWeights.TwoBones;

        // Disable unnecessary features
        QualitySettings.billboardsFaceCameraPosition = true;
        QualitySettings.streamingMipmapsActive = false;
    }

    /// <summary>
    /// Log detailed device information
    /// </summary>
    private void LogDeviceInfo()
    {
        Debug.Log($"=== Device Information ===\n" +
                 $"Model: {SystemInfo.deviceModel}\n" +
                 $"Name: {SystemInfo.deviceName}\n" +
                 $"OS: {SystemInfo.operatingSystem}\n" +
                 $"Processor: {SystemInfo.processorType}\n" +
                 $"Processor Count: {SystemInfo.processorCount}\n" +
                 $"RAM: {SystemInfo.systemMemorySize}MB\n" +
                 $"GPU: {SystemInfo.graphicsDeviceName}\n" +
                 $"VRAM: {SystemInfo.graphicsMemorySize}MB\n" +
                 $"GPU Type: {SystemInfo.graphicsDeviceType}\n" +
                 $"Max Texture Size: {SystemInfo.maxTextureSize}\n" +
                 $"Quality Level: {QualitySettings.GetQualityLevel()}\n" +
                 $"Target FPS: {Application.targetFrameRate}\n" +
                 $"========================");
    }

    #endregion
}