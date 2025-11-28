using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TankPreviewComp : MonoBehaviour
{
    [SerializeField] private RawImage tankPreview;

    private TankPreview currentTankPreviewObject;
    private RenderTexture currentRenderTexture;

    public void HideTankPreview()
    {
        int slotIdx = GetCurrentSlotIndex();
        if (slotIdx != -1)
            previewSlots[slotIdx] = null;

        if (currentTankPreviewObject != null)
        {
            Destroy(currentTankPreviewObject.gameObject);
            currentTankPreviewObject = null;
        }
        if (currentRenderTexture != null)
        {
            currentRenderTexture.Release();
            currentRenderTexture = null;
        }
        if (tankPreview != null)
            tankPreview.texture = null;
    }

    public void ShowTankPreview(string tankId)
    {
        HideTankPreview();
        if (!string.IsNullOrEmpty(tankId))
        {
            GameAssetDocument tankAsset = DatabaseManager.GetDB<GameAssetCollection>().GetGameAssetDocumentById(tankId);
            if (tankAsset != null)
            {
                int slotIdx = GetFreeSlotIndex();
                currentTankPreviewObject = Instantiate(tankAsset.previewAsset as GameObject).GetComponent<TankPreview>();
                currentTankPreviewObject.transform.position = previewStartPos + Vector3.right * previewSpacing * slotIdx;

                // Nếu slot mới, thêm vào list
                if (slotIdx == previewSlots.Count)
                    previewSlots.Add(currentTankPreviewObject);
                else
                    previewSlots[slotIdx] = currentTankPreviewObject;

                currentRenderTexture = new RenderTexture(768, 768, 24, RenderTextureFormat.ARGB32)
                {
                    useMipMap = false,
                    autoGenerateMips = false,
                    filterMode = FilterMode.Bilinear,
                    anisoLevel = 0,
                    antiAliasing = 1,
                    enableRandomWrite = false,
                    wrapMode = TextureWrapMode.Clamp,
                };
                currentRenderTexture.Create();
                currentTankPreviewObject.SetRenderTexture(currentRenderTexture);
                tankPreview.texture = currentRenderTexture;
                DontDestroyOnLoad(currentTankPreviewObject.gameObject);
            }
        }
    }

    public void ChangeWrap(int wrapId, string tankId = "")
    {
        if (currentTankPreviewObject != null)
        {
            currentTankPreviewObject.ReloadWrap(wrapId, tankId);
        }
    }

    public void Update()
    {
        DragRotate();
    }
    private bool isDragging = false;
    private void DragRotate()
    {
        // Mouse
        if (Input.GetMouseButtonDown(0))
        {
            if (tankPreview != null && RectTransformUtility.RectangleContainsScreenPoint(
                tankPreview.rectTransform, Input.mousePosition, null))
            {
                isDragging = true;
            }
        }
        if (Input.GetMouseButtonUp(0))
            isDragging = false;

        // Touch
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                if (tankPreview != null && RectTransformUtility.RectangleContainsScreenPoint(
                    tankPreview.rectTransform, touch.position, null))
                {
                    isDragging = true;
                }
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }

        if (isDragging && currentTankPreviewObject != null)
            currentTankPreviewObject.TouchRotate();
    }


    #region Helper - View Slot Management

    // Danh sách các slot preview, tự động scale theo số lượng tank preview
    public static List<TankPreview> previewSlots = new List<TankPreview>();
    private static Vector3 previewStartPos = new Vector3(0, 0, 0);
    private static float previewSpacing = 10f;

    // Trả về vị trí slot trống đầu tiên
    private int GetFreeSlotIndex()
    {
        for (int i = 0; i < previewSlots.Count; i++)
            if (previewSlots[i] == null)
                return i;
        return previewSlots.Count; // Nếu không có slot trống, dùng slot mới
    }

    // Trả về vị trí slot của tank preview hiện tại
    private int GetCurrentSlotIndex()
    {
        for (int i = 0; i < previewSlots.Count; i++)
            if (previewSlots[i] == currentTankPreviewObject)
                return i;
        return -1;
    }

    #endregion

}