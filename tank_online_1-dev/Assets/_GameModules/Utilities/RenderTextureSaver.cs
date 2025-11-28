using UnityEngine;
using System.IO;

public static class RenderTextureSaver
{
    /// <summary>
    /// Lưu RenderTexture thành file PNG.
    /// </summary>
    public static void SaveRenderTextureToPNG(RenderTexture rt, string path)
    {
        // Lưu trạng thái RT đang active
        RenderTexture prev = RenderTexture.active;

        try
        {
            // Gán RT cần đọc
            RenderTexture.active = rt;

            // Tạo Texture2D để copy dữ liệu
            Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();

            // Encode sang PNG
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);

            Debug.Log($"[RenderTextureSaver] Saved PNG to: {path}");

            Object.Destroy(tex); // dọn dẹp
        }
        finally
        {
            // Khôi phục lại RT active
            RenderTexture.active = prev;
        }
    }
}
