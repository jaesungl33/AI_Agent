using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(DeviceQualityCollection), menuName = "ScriptableObjects/" + nameof(DeviceQualityCollection), order = 1)]
public class DeviceQualityCollection : CollectionBase<DeviceQualityDocument>
{
    public DeviceQualityDocument GetDeviceQualityDocument(DeviceQualityLevel level)
    {
        return documents.FirstOrDefault(d => d.qualityLevel == (int)level);
    }
}
