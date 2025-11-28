#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class ReplaceObjectNames : EditorWindow
{
    private string findString;     // Chuỗi cần tìm
    private string replaceString; // Chuỗi thay thế
    private string prefix;         // Prefix thêm vào tên đối tượng
    private string suffix;         // Suffix thêm vào tên đối tượng
    private bool enableIndexing;   // Bật chế độ đánh số thứ tự
    private int startIndex = 0;    // Số bắt đầu cho đánh số thứ tự

    [MenuItem("Tools/Replace Object Names")]
    public static void ShowWindow()
    {
        // Mở cửa sổ Editor
        GetWindow<ReplaceObjectNames>("Replace Object Names");
    }

    private void OnGUI()
    {
        // Giao diện input cho Find và Replace
        GUILayout.Label("Find and Replace Object Names", EditorStyles.boldLabel);
        findString = EditorGUILayout.TextField("Find", findString);
        replaceString = EditorGUILayout.TextField("Replace", replaceString);

        // Giao diện input cho Prefix và Suffix
        GUILayout.Space(10);
        GUILayout.Label("Add Prefix and Suffix to Names", EditorStyles.boldLabel);
        prefix = EditorGUILayout.TextField("Prefix", prefix);
        suffix = EditorGUILayout.TextField("Suffix", suffix);

        // Giao diện cho đánh số thứ tự
        GUILayout.Space(10);
        GUILayout.Label("Indexing Options", EditorStyles.boldLabel);
        enableIndexing = EditorGUILayout.Toggle("Enable Indexing", enableIndexing);
        if (enableIndexing)
        {
            startIndex = EditorGUILayout.IntField("Start Index", startIndex);
        }

        // Nút Rename
        GUILayout.Space(10);
        if (GUILayout.Button("Rename"))
        {
            ReplacePrefixSuffixNames();
        }
    }

    private void ReplacePrefixSuffixNames()
    {
        // Lấy danh sách các đối tượng được chọn
        Object[] selectedObjects = Selection.objects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("No objects selected. Please select objects in the Hierarchy or Project.");
            return;
        }

        int currentIndex = startIndex; // Đặt số thứ tự bắt đầu

        // Thực hiện thay thế tên, thêm prefix, suffix và đánh số
        foreach (Object obj in selectedObjects)
        {
            string originalName = obj.name;
            string newName = originalName;

            // Thay thế tên nếu có chuỗi cần tìm
            if (!string.IsNullOrEmpty(findString))
            {
                newName = newName.Replace(findString, replaceString);
            }

            // Thêm prefix nếu có
            if (!string.IsNullOrEmpty(prefix))
            {
                newName = $"{prefix}{newName}";
            }

            // Thêm suffix nếu có
            if (!string.IsNullOrEmpty(suffix))
            {
                newName = $"{newName}{suffix}";
            }

            // Đánh số thứ tự nếu bật chế độ indexing
            if (enableIndexing)
            {
                newName = $"{newName}_{currentIndex}";
                currentIndex++;
            }

            // Đổi tên GameObject trong Hierarchy
            if (obj is GameObject go && string.IsNullOrEmpty(AssetDatabase.GetAssetPath(go)))
            {
                go.name = newName;
            }
            // Đổi tên asset trong Project
            else
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(assetPath) && newName != originalName)
                {
                    AssetDatabase.RenameAsset(assetPath, newName);
                }
            }
        }

        AssetDatabase.SaveAssets(); // Lưu lại thay đổi
        Debug.Log($"Renamed {selectedObjects.Length} object(s).");
    }
}
#endif