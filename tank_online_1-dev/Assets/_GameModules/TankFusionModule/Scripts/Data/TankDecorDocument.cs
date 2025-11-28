using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class TankDecorDocument
{
    public enum ItemType
    {
        None,
        Wrap,
        Decal,
        Sticker,
        AISticker
    }
    public int itemId;
    public string itemCatalogId;
    public string itemTitle;
    public string texturePath;
    public string iconPath;
    public ItemType itemType;
    public DecorData[] wrapDecalStickerData;

    // Constructor mặc định cho Unity Serialization
    public TankDecorDocument() { }

    // Constructor đầy đủ
    public TankDecorDocument(int itemId, string itemCatalogId, string itemTitle, string texturePath, string iconPath, ItemType itemType, DecorData[] wrapDecalStickerData)
    {
        this.itemId = itemId;
        this.itemCatalogId = itemCatalogId;
        this.itemTitle = itemTitle;
        this.texturePath = texturePath;
        this.iconPath = iconPath;
        this.itemType = itemType;
        this.wrapDecalStickerData = wrapDecalStickerData;
    }
}

[System.Serializable]
public class DecorData
{
    public enum PropertyType { Float, Vector2, String }
    public string tankId;
    public CustomProperty[] customProperties;

    // Constructor mặc định cho Unity Serialization
    public DecorData() { }

    // Constructor đầy đủ
    public DecorData(string tankId, CustomProperty[] customProperties)
    {
        this.tankId = tankId;
        this.customProperties = customProperties;
    }

    
    [System.Serializable]
    public class CustomProperty
    {
        public string propertyName;
        public string propertyType;
        public float propertyValue;
        public string propertyValueTypeString;
        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 propertyValueTypeVector2;
    }
}

[System.Serializable]
public class TankWrapDocument : TankDecorDocument
{
    // Nếu cần, thêm field riêng cho wrap
}

[System.Serializable]
public class TankStickerDocument : TankDecorDocument
{
    // Nếu cần, thêm field riêng cho sticker
}