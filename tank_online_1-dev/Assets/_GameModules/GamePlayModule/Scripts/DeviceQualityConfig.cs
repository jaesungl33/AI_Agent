using UnityEngine;
using UnityEngine.Rendering;

public enum DeviceQualityLevel { Low = 0, Medium = 1, High = 2 }

[System.Serializable]
public class DeviceQualityDocument
{
    public string name = "";
    public int qualityLevel = 0;
    public int targetFrameRate = 30;
    public ShadowQuality shadows = ShadowQuality.HardOnly;
    public ShadowResolution shadowResolution = ShadowResolution.Low;
    public float shadowDistance = 20f;
    public int shadowCascades = 0;
    public int antiAliasing = 0;
    public AnisotropicFiltering anisotropicFiltering = AnisotropicFiltering.Disable;
    public bool softParticles = false;
    public bool realtimeReflectionProbes = false;
    public float lodBias = 0.7f;
    public int particleRaycastBudget = 16;
    public int maximumLODLevel = 1;
    public bool allowBloom = true;
    public int targetRamMB = 512;
    public int targetVRAMMB = 512;
    public string[] keywords = new string[0];
}

[CreateAssetMenu(fileName = "DeviceQualityConfig", menuName = "Config/Device Quality Config")]
public class DeviceQualityConfig : ScriptableObject
{
    public DeviceQualityLevel qualityLevel = DeviceQualityLevel.Medium;
    public VolumeProfile volumeProfile;
    public DeviceQualityDocument[] profiles = new DeviceQualityDocument[3]; // 0:Low, 1:Medium, 2:High

    public void ApplyQuality(DeviceQualityLevel level)
    {
        qualityLevel = level;
        int idx = (int)level;
        if (profiles == null || profiles.Length <= idx || profiles[idx] == null)
        {
            Debug.LogError($"DeviceQualityConfig: Profile for {level} not set!");
            return;
        }
        var p = profiles[idx];
        QualitySettings.SetQualityLevel(p.qualityLevel, true);
        Application.targetFrameRate = p.targetFrameRate;
        QualitySettings.shadows = p.shadows;
        QualitySettings.shadowResolution = p.shadowResolution;
        QualitySettings.shadowDistance = p.shadowDistance;
        QualitySettings.shadowCascades = p.shadowCascades;
        QualitySettings.antiAliasing = p.antiAliasing;
        QualitySettings.anisotropicFiltering = p.anisotropicFiltering;
        QualitySettings.softParticles = p.softParticles;
        QualitySettings.realtimeReflectionProbes = p.realtimeReflectionProbes;
        QualitySettings.lodBias = p.lodBias;
        QualitySettings.particleRaycastBudget = p.particleRaycastBudget;
        QualitySettings.maximumLODLevel = p.maximumLODLevel;
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
}