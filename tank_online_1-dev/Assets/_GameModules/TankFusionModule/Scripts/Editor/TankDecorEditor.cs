using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Reflection;
using System.Linq;

[CustomEditor(typeof(TankWrapCollection))]
public class TankWrapCollectionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);
        GUILayout.Label("Export / Import Options", EditorStyles.boldLabel);

        TankWrapCollection collection = (TankWrapCollection)target;
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

    private void ExportToCsv(TankWrapCollection collection, string path)
    {
        var docs = collection.GetAllDocuments();
        StringBuilder sb = new StringBuilder();

        if (docs == null || docs.Count == 0)
        {
            Debug.LogWarning("Collection is empty. Nothing to export.");
            return;
        }

        var type = typeof(TankWrapDocument);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.IsPublic || f.GetCustomAttribute<SerializeField>() != null).ToArray();

        sb.AppendLine(string.Join(",", fields.Select(f => f.Name)));

        foreach (var doc in docs)
        {
            var values = fields.Select(f =>
            {
                var value = f.GetValue(doc);
                if (value == null) return "";
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

[CustomEditor(typeof(TankStickerCollection))]
public class TankStickerCollectionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);
        GUILayout.Label("Export / Import Options", EditorStyles.boldLabel);

        TankStickerCollection collection = (TankStickerCollection)target;
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

    private void ExportToCsv(TankStickerCollection collection, string path)
    {
        var docs = collection.GetAllDocuments();
        StringBuilder sb = new StringBuilder();

        if (docs == null || docs.Count == 0)
        {
            Debug.LogWarning("Collection is empty. Nothing to export.");
            return;
        }

        var type = typeof(TankStickerDocument);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.IsPublic || f.GetCustomAttribute<SerializeField>() != null).ToArray();

        sb.AppendLine(string.Join(",", fields.Select(f => f.Name)));

        foreach (var doc in docs)
        {
            var values = fields.Select(f =>
            {
                var value = f.GetValue(doc);
                if (value == null) return "";
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