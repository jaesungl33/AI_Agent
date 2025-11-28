using UnityEngine;
using UnityEditor;
using System.IO;

public class RenderTextureContextMenu
{
    [MenuItem("Assets/Save RenderTexture as PNG", true)]
    private static bool ValidateSave()
    {
        // Chỉ hiện menu khi chọn đúng RenderTexture
        return Selection.activeObject is RenderTexture;
    }

    [MenuItem("Assets/Save RenderTexture as PNG")]
    private static void SaveSelectedRenderTexture()
    {
        var rt = Selection.activeObject as RenderTexture;
        if (rt == null)
        {
            Debug.LogError("Bạn phải chọn một RenderTexture!");
            return;
        }

        // Chọn chỗ lưu file
        string path = EditorUtility.SaveFilePanel(
            "Save RenderTexture as PNG",
            Application.dataPath,
            rt.name + ".png",
            "png");

        if (string.IsNullOrEmpty(path))
            return;

        SaveRenderTextureToPNG(rt, path);
        AssetDatabase.Refresh();
    }

    private static void SaveRenderTextureToPNG(RenderTexture rt, string path)
    {
        RenderTexture prev = RenderTexture.active;

        try
        {
            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false, true);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);

            Object.DestroyImmediate(tex);
            Debug.Log($"[RenderTextureContextMenu] Saved: {path}");
        }
        finally
        {
            RenderTexture.active = prev;
        }
    }
}
