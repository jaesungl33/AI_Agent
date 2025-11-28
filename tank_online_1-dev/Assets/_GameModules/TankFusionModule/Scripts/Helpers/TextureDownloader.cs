using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Data.Common;

public class TextureDownloader : Singleton<TextureDownloader>
{
    // Quản lý các url đang tải và callback chờ
    private Dictionary<string, List<Action<Texture2D>>> _pendingCallbacks = new();
    private HashSet<string> _downloadingUrls = new();

    public void Start()
    {
        DatabaseManager.OnCollectionUpdated -= OnCollectionUpdatedHandler;
        DatabaseManager.OnCollectionUpdated += OnCollectionUpdatedHandler;
    }

    private void OnCollectionUpdatedHandler(object collection)
    {
        var type = collection.GetType();
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(RuntimeCollection<>))
        {
            var genericArg = type.GetGenericArguments()[0];
            if (genericArg != typeof(TankWrapDocument))
            {
                return;
            }
        }

        List<string> urls = new List<string>();
        var docs = DatabaseManager.GetDB<TankWrapCollection>();
        if (docs != null)
        {
            foreach (var doc in docs.documents)
            {
                if (!string.IsNullOrEmpty(doc.texturePath))
                {
                    urls.Add(doc.texturePath);
                }
                if (!string.IsNullOrEmpty(doc.iconPath))
                {
                    urls.Add(doc.iconPath);
                }
            }
        }
        if (urls.Count > 0)
            PredownloadResource(urls, "TankDecors");
        Debug.Log($"PredownloadResource triggered for {urls.Count} tank wrap textures.");
    }

    /// <summary>
    /// Tải ngầm các ảnh, chỉ tải nếu chưa có trong Resources hoặc chưa lưu file
    /// </summary>
    public void PredownloadResource(List<string> urls, string subFolder = "TankDecors", Action onCompleted = null)
    {
        // Load toàn bộ texture từ Resources một lần, lưu lại tên
        var resourceTextures = Resources.LoadAll<Texture2D>(subFolder);
        var resourceNames = new HashSet<string>();
        foreach (var tex in resourceTextures)
        {
            if (tex != null)
            {
                resourceNames.Add(tex.name);
                Resources.UnloadAsset(tex); // Giải phóng texture sau khi lấy tên
            }
        }

        int total = urls?.Count ?? 0;
        int finished = 0;
        if (total == 0)
        {
            onCompleted?.Invoke();
            return;
        }

        foreach (var url in urls)
        {
            string fileName = GetFileNameFromURL(url);
            string fileNameNoExt = Path.GetFileNameWithoutExtension(fileName);
            if (string.IsNullOrEmpty(fileName))
            {
                finished++;
                if (finished == total) onCompleted?.Invoke();
                continue;
            }

            // Kiểm tra trong Resources (chỉ so sánh tên, không load lại)
            if (resourceNames.Contains(fileNameNoExt))
            {
                finished++;
                if (finished == total) onCompleted?.Invoke();
                continue;
            }

            // Kiểm tra file đã lưu
            string path = Path.Combine(Application.persistentDataPath, subFolder, fileName);
            if (File.Exists(path))
            {
                finished++;
                if (finished == total) onCompleted?.Invoke();
                continue;
            }

            // Nếu đang tải thì chỉ cần callback khi xong
            if (_downloadingUrls.Contains(url))
            {
                if (!_pendingCallbacks.ContainsKey(url))
                    _pendingCallbacks[url] = new List<Action<Texture2D>>();
                _pendingCallbacks[url].Add(_ =>
                {
                    finished++;
                    if (finished == total) onCompleted?.Invoke();
                });
                continue;
            }

            _downloadingUrls.Add(url);
            DownloadImage(url, fileName, subFolder, tex =>
            {
                _downloadingUrls.Remove(url);
                if (_pendingCallbacks.TryGetValue(url, out var callbacks))
                {
                    foreach (var cb in callbacks)
                        cb?.Invoke(tex);
                    _pendingCallbacks.Remove(url);
                }
                finished++;
                if (finished == total) onCompleted?.Invoke();
            });
        }
    }

    /// <summary>
    /// Load ảnh từ thư mục đã lưu. Nếu chưa có thì download và cache lại.
    /// Nếu url đang tải thì chờ tải xong rồi trả về.
    /// </summary>
    public void LoadImage(string url, GameObject caller, string subFolder = "TankDecors", Action<Texture2D> onComplete = null)
    {
        string fileName = GetFileNameFromURL(url);
        if (string.IsNullOrEmpty(fileName))
        {
            onComplete?.Invoke(null);
            return;
        }

        // Load từ Resources trước
        string resourcePath = Path.Combine(subFolder, Path.GetFileNameWithoutExtension(fileName));
        Texture2D resourceTex = Resources.Load<Texture2D>(resourcePath);
        if (resourceTex != null)
        {
            Debug.Log($"Đã load từ Resources: {resourcePath}");
            if (caller != null)
                onComplete?.Invoke(resourceTex);
            return;
        }

        string path = Path.Combine(Application.persistentDataPath, subFolder, fileName);
        string folderPath = Path.Combine(Application.persistentDataPath, subFolder);
        Debug.Log($"Thư mục lưu file: {folderPath}");

        if (File.Exists(path))
        {
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);

            if (caller != null)
                onComplete?.Invoke(tex);
        }
        else
        {
            // Nếu đang tải thì chờ tải xong rồi trả về
            if (_downloadingUrls.Contains(url))
            {
                if (!_pendingCallbacks.ContainsKey(url))
                    _pendingCallbacks[url] = new List<Action<Texture2D>>();
                _pendingCallbacks[url].Add(texture =>
                {
                    if (caller != null)
                        onComplete?.Invoke(texture);
                });
                return;
            }

            _downloadingUrls.Add(url);
            DownloadImage(url, fileName, subFolder, texture =>
            {
                _downloadingUrls.Remove(url);
                if (_pendingCallbacks.TryGetValue(url, out var callbacks))
                {
                    foreach (var cb in callbacks)
                        cb?.Invoke(texture);
                    _pendingCallbacks.Remove(url);
                }
                if (caller != null)
                    onComplete?.Invoke(texture);
                Debug.Log($"Downloaded: {url}");
            });
        }
    }

    #region Helper
    public static string GetFileNameFromURL(string url)
    {
        if (string.IsNullOrEmpty(url))
            return string.Empty;

        try
        {
            Uri uri = new Uri(url);
            string fileName = Path.GetFileName(uri.LocalPath);
            // Kiểm tra fileName hợp lệ (không rỗng, không chứa ký tự đặc biệt)
            if (string.IsNullOrEmpty(fileName) || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                return string.Empty;
            return fileName;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"GetFileNameFromURL error: {ex.Message}, URL: {url}");
            return string.Empty;
        }
    }

    /// <summary>
    /// Tải ảnh từ URL và lưu file (tự động chạy coroutine, ghi đè nếu trùng tên).
    /// </summary>
    public void DownloadImage(string url, string fileName, string subFolder = "TankDecors", Action<Texture2D> onComplete = null)
    {
        StartCoroutine(DownloadImageCoroutine(url, fileName, subFolder, onComplete));
    }

    private IEnumerator DownloadImageCoroutine(string url, string fileName, string subFolder, Action<Texture2D> onComplete)
    {
        string folderPath = Path.Combine(Application.persistentDataPath, subFolder);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("FileName rỗng hoặc không hợp lệ, không thể ghi file.");
            onComplete?.Invoke(null);
            yield break;
        }

        string filePath = Path.Combine(folderPath, fileName);

        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"❌ Lỗi tải hình: {uwr.error}");
                onComplete?.Invoke(null);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                byte[] bytes = texture.EncodeToPNG();

                try
                {
                    File.WriteAllBytes(filePath, bytes);
                    Debug.Log($"✅ Đã lưu (ghi đè nếu trùng): {filePath}");
                    onComplete?.Invoke(texture);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"❌ Lỗi ghi file: {ex.Message}");
                    onComplete?.Invoke(null);
                }
            }
        }
    }
    #endregion Helper
}