using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class GameAssetGroup
{
    public string groupID; // background music, sfx, bgm, tank, ui, vvv
    public List<GameAssetDocument> assetDocuments;
}

[System.Serializable]
public class GameAssetDocument
{
    public string id;
    public Object mainAsset;
    public Object previewAsset;
    public Sprite icon;
    public List<PreviewAsset> sprites;
}

[System.Serializable]
public class PreviewAsset
{
    public AssetType type;
    public Object asset;
}

public enum AssetType
{
    None,
    MainIcon,
    PreviewIcon,
    ClassIcon,
    ClassTagIcon
}