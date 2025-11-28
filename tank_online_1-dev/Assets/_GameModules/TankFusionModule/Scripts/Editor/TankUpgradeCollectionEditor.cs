using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Reflection;
using System.Linq;

[CustomEditor(typeof(TankUpgradeCollection))]
public class TankUpgradeCollectionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);
        GUILayout.Label("Export / Import Options", EditorStyles.boldLabel);

        TankUpgradeCollection collection = (TankUpgradeCollection)target;

        if (GUILayout.Button("Export to JSON"))
        {
            string path = EditorUtility.SaveFilePanel(
                "Export Collection to JSON",
                "",
                $"{collection.name}.json",
                "json");

            if (!string.IsNullOrEmpty(path))
            {
                string json = JsonUtility.ToJson(collection, true);
                File.WriteAllText(path, json);
                Debug.Log($"Exported to JSON at: {path}");
            }
        }

        if (GUILayout.Button("Import from JSON"))
        {
            string path = EditorUtility.OpenFilePanel(
                "Import Collection from JSON",
                "",
                "json");

            if (!string.IsNullOrEmpty(path))
            {
                string json = File.ReadAllText(path);
                JsonUtility.FromJsonOverwrite(json, collection);
                EditorUtility.SetDirty(collection);
                AssetDatabase.SaveAssets();
                Debug.Log($"Imported from JSON: {path}");
            }
        }

        if (GUILayout.Button("Export to CSV"))
        {
            string path = EditorUtility.SaveFilePanel(
                "Export Collection to CSV",
                "",
                "collection.csv",
                "csv");

            if (!string.IsNullOrEmpty(path))
            {
                ExportToCsv(collection, path);
            }
        }
    }

    private void ExportToCsv(TankUpgradeCollection collection, string path)
    {
        var docs = collection.GetAllDocuments();
        StringBuilder sb = new StringBuilder();

        if (docs == null || docs.Count == 0)
        {
            Debug.LogWarning("Collection is empty. Nothing to export.");
            return;
        }

        // Lấy type của SOTankUpgradeData
        var type = typeof(TankDocument);

        // Lấy tất cả field public và private có [SerializeField]
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        fields = fields.Where(f => f.IsPublic || f.GetCustomAttribute<SerializeField>() != null).ToArray();

        // Header
        sb.AppendLine(string.Join(",", fields.Select(f => f.Name)));

        // Rows
        foreach (var doc in docs)
        {
            var values = fields.Select(f =>
            {
                var value = f.GetValue(doc);

                if (value == null)
                    return "";

                // Escape dấu phẩy và xuống dòng nếu có
                string s = value.ToString();
                if (s.Contains(",") || s.Contains("\n"))
                    s = $"\"{s.Replace("\"", "\"\"")}\"";

                return s;
            });
            sb.AppendLine(string.Join(",", values));
        }

        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        Debug.Log($"Exported to CSV at: {path}");
    }

}
