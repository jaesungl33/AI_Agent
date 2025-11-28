using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TankPartVisual : MonoBehaviour
{
    public MeshRenderer[] meshRenderers;
    public List<GameObject> groupObj;
    Material defaultMaterial;
    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    public void EnableVisual(bool enable)
    {
        if (meshRenderers != null)
        {
            foreach (var renderer in meshRenderers)
            {
                if (renderer != null)
                    renderer.enabled = enable;
            }
        }

        if (groupObj != null && groupObj.Count > 0)
        {
            foreach (var obj in groupObj)
            {
                if (obj != null)
                    obj.SetActive(enable);
            }
        }
    }


    public void DefaultWrapData()
    {
        //cache all renderers' material to avoid modifying shared materials
        if (meshRenderers == null)
        {
            Debug.LogWarning("[DefaultWrapData] meshRenderers is null");
            return;
        }
        defaultMaterial = meshRenderers[0].sharedMaterial;
    }
    public void LoadWrap(int wrapId, string tankId = "")
    {
        if (!this.gameObject.activeSelf)
            return;
        if (meshRenderers == null)
        {
            Debug.LogWarning("[LoadWrap] meshRenderers is null");
            return;
        }

        var wrapDocument = DatabaseManager.GetDB<TankWrapCollection>();
        var wrapData = wrapDocument.GetTankCustomLiveryDocumentById(wrapId);

        if (wrapData == null)
        {
            ApplyDefaultWrap();
            return;
        }

        string fileName = TextureDownloader.GetFileNameFromURL(wrapData.texturePath);

        if (string.IsNullOrEmpty(fileName))
        {
            ApplyDefaultWrap();
            return;
        }

        //linq;
        var decorData = wrapData.wrapDecalStickerData.FirstOrDefault(w => w.tankId == tankId);

        if(decorData == null)
        {
            Debug.LogWarning("[LoadWrap] decorData is null for tankId: " + tankId);
            ApplyDefaultWrap();
            return;
        }
        // Kiểm tra nếu đã có ảnh thì load, nếu chưa thì download
        TextureDownloader.Instance.LoadImage(wrapData.texturePath, this.gameObject, onComplete: (downloadedTexture) =>
        {
            if (downloadedTexture == null)
            {
                ApplyDefaultWrap();
                return;
            }
            ApplyTextureToRenderers(downloadedTexture, decorData.customProperties);
    }   );

    }

    // Hàm phụ để apply texture và custom float property
    private void ApplyTextureToRenderers(Texture texture, DecorData.CustomProperty[] customProps)
    {
        foreach (var renderer in meshRenderers)
        {
            if (renderer != null && renderer.sharedMaterial != null)
            {
                renderer.material = new Material(renderer.sharedMaterial);
                renderer.material.SetTexture("_Wrap_Texture", texture);
                foreach (var customProp in customProps)
                {
                    if (renderer.material.HasProperty(customProp.propertyName))
                    {
                        // Debug.LogWarning("customProp.propertyName: " + customProp.propertyName);
                        // // Thử lấy float
                        // float floatVal = renderer.material.GetFloat(customProp.propertyName);
                        // // Thử lấy vector
                        // Vector4 vectorVal = renderer.material.GetVector(customProp.propertyName);
                        // Debug.LogWarning("floatVal: " + floatVal + ", vectorVal: " + vectorVal);
                        // // Kiểm tra kiểu dữ liệu
                        // if (floatVal != 0f || renderer.material.GetFloat(customProp.propertyName) == 0f) // float luôn trả về số, nên cần logic riêng
                        // {
                        //     renderer.material.SetFloat(customProp.propertyName, customProp.propertyValue);
                        // }
                        // else if (vectorVal != Vector4.zero)
                        // {
                        //     renderer.material.SetVector(customProp.propertyName, customProp.propertyValueTypeVector2);
                        // }
                        // else
                        //     Debug.Log($"{customProp.propertyName} type unknown or not set");

                        if (customProp.propertyType == DecorData.PropertyType.Vector2.ToString())//(customProp.propertyValueTypeVector2 != Vector2.zero)
                        {
                            renderer.material.SetVector(customProp.propertyName, customProp.propertyValueTypeVector2);
                            continue;
                        }
                        if (customProp.propertyType == DecorData.PropertyType.Float.ToString())//(customProp.propertyValue != 0)
                        {
                            //Debug.LogError($"Set float property: {customProp.propertyName} with value: {customProp.propertyValue}");
                            renderer.material.SetFloat(customProp.propertyName, customProp.propertyValue);
                            continue;
                        }
                    }
                }
            }
        }
    }
    // Hàm set wrap về default
    private void ApplyDefaultWrap()
    {
        if (defaultMaterial == null)
        {
            Debug.LogWarning("[ApplyDefaultWrap] defaultMaterial is null");
            return;
        }
        foreach (var renderer in meshRenderers)
        {
            if (renderer != null && renderer.sharedMaterial != null)
            {
                renderer.material = new Material(defaultMaterial);
            }
        }
    }
}