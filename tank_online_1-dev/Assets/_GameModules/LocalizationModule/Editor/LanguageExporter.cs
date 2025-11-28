using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEditor.Localization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LocalizationModule.Editor
{
    /// <summary>
    /// Editor tool to export Unity Localization tables to CSV files
    /// Exports each String Table to a separate CSV file
    /// </summary>
    public class LanguageExporter : EditorWindow
    {
        #region Fields

        private const string EXPORT_FOLDER_NAME = "LocalizationExports";
        private const string CSV_EXTENSION = ".csv";
        private const char CSV_SEPARATOR = ',';

        private Vector2 scrollPosition;
        private bool includeEmptyEntries = false;
        private bool exportAllTables = true;
        private List<bool> selectedTables = new List<bool>();
        private string exportPath = "";

        #endregion

        #region Menu Items

        [MenuItem("Tools/Localization/Language Exporter")]
        public static void ShowWindow()
        {
            var window = GetWindow<LanguageExporter>("Language Exporter");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }

        #endregion

        #region Unity Callbacks

        private void OnEnable()
        {
            // Calculate export path (same level as LocalizationModule folder)
            string modulePath = "Assets/_GameModules/LocalizationModule";
            string parentPath = Path.GetDirectoryName(modulePath);
            exportPath = Path.Combine(parentPath, EXPORT_FOLDER_NAME);

            RefreshTableList();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            
            DrawHeader();
            EditorGUILayout.Space(10);
            
            DrawExportSettings();
            EditorGUILayout.Space(10);
            
            DrawTableSelection();
            EditorGUILayout.Space(10);
            
            DrawExportButtons();
            EditorGUILayout.Space(10);
            
            DrawInfo();
        }

        #endregion

        #region GUI Drawing

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("Language Exporter", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Export Unity Localization String Tables to CSV files.\n" +
                "Each table will be exported with all available locales.",
                MessageType.Info
            );
        }

        private void DrawExportSettings()
        {
            EditorGUILayout.LabelField("Export Settings", EditorStyles.boldLabel);
            
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Export Path:", exportPath);
            EditorGUI.EndDisabledGroup();

            includeEmptyEntries = EditorGUILayout.Toggle(
                new GUIContent(
                    "Include Empty Entries",
                    "Export entries even if they have no translation"
                ),
                includeEmptyEntries
            );
        }

        private void DrawTableSelection()
        {
            EditorGUILayout.LabelField("String Tables", EditorStyles.boldLabel);

            exportAllTables = EditorGUILayout.Toggle("Export All Tables", exportAllTables);

            if (!exportAllTables)
            {
                EditorGUI.indentLevel++;
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));

                var stringTables = GetAllStringTables();
                
                while (selectedTables.Count < stringTables.Count)
                {
                    selectedTables.Add(true);
                }

                for (int i = 0; i < stringTables.Count; i++)
                {
                    selectedTables[i] = EditorGUILayout.Toggle(
                        stringTables[i].TableCollectionName,
                        selectedTables[i]
                    );
                }

                EditorGUILayout.EndScrollView();
                EditorGUI.indentLevel--;
            }
        }

        private void DrawExportButtons()
        {
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Refresh Tables", GUILayout.Width(120)))
            {
                RefreshTableList();
            }

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Export to CSV", GUILayout.Width(120)))
            {
                ExportToCSV();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
        }

        private void DrawInfo()
        {
            EditorGUILayout.HelpBox(
                "CSV Format:\n" +
                "- First column: Entry Key\n" +
                "- Following columns: Locale translations (e.g., en, vi, zh)\n" +
                "- Files saved to: " + exportPath,
                MessageType.None
            );
        }

        #endregion

        #region Export Logic

        /// <summary>
        /// Main export function
        /// </summary>
        private void ExportToCSV()
        {
            try
            {
                // Create export folder if not exists
                if (!Directory.Exists(exportPath))
                {
                    Directory.CreateDirectory(exportPath);
                    Debug.Log($"Created export directory: {exportPath}");
                }

                var stringTables = GetAllStringTables();
                var tablesToExport = GetTablesToExport(stringTables);

                if (tablesToExport.Count == 0)
                {
                    EditorUtility.DisplayDialog(
                        "No Tables Selected",
                        "Please select at least one table to export.",
                        "OK"
                    );
                    return;
                }

                int exportedCount = 0;

                EditorUtility.DisplayProgressBar("Exporting", "Starting export...", 0f);

                foreach (var table in tablesToExport)
                {
                    float progress = (float)exportedCount / tablesToExport.Count;
                    EditorUtility.DisplayProgressBar(
                        "Exporting",
                        $"Exporting table: {table.TableCollectionName}",
                        progress
                    );

                    ExportTableToCSV(table);
                    exportedCount++;
                }

                EditorUtility.ClearProgressBar();

                // Show success message
                EditorUtility.DisplayDialog(
                    "Export Complete",
                    $"Successfully exported {exportedCount} table(s) to:\n{exportPath}",
                    "OK"
                );

                // Open folder
                EditorUtility.RevealInFinder(exportPath);

                Debug.Log($"✅ Exported {exportedCount} table(s) to {exportPath}");
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog(
                    "Export Failed",
                    $"Error during export:\n{e.Message}",
                    "OK"
                );
                Debug.LogError($"Export failed: {e}");
            }
        }

        /// <summary>
        /// Export a single String Table to CSV
        /// </summary>
        private void ExportTableToCSV(StringTableCollection tableCollection)
        {
            string fileName = $"{tableCollection.TableCollectionName}{CSV_EXTENSION}";
            string filePath = Path.Combine(exportPath, fileName);

            var csvContent = new StringBuilder();

            // Get all available locales from the collection
            var sharedTableData = tableCollection.SharedData;
            if (sharedTableData == null)
            {
                Debug.LogWarning($"No shared data found for {tableCollection.TableCollectionName}");
                return;
            }

            // Get all locales that have this table
            var availableLocales = new List<Locale>();
            foreach (var locale in LocalizationEditorSettings.GetLocales())
            {
                if (locale != null)
                {
                    availableLocales.Add(locale);
                }
            }

            if (availableLocales.Count == 0)
            {
                Debug.LogWarning("No locales found in project");
                return;
            }

            // Build header row: Key, en, vi, zh, ...
            var header = new List<string> { "Key" };
            header.AddRange(availableLocales.Select(l => l.Identifier.Code));
            csvContent.AppendLine(EscapeCSVLine(header));

            // Get all shared entries (keys)
            var sharedEntries = sharedTableData.Entries;
            
            // Iterate through all entries
            foreach (var sharedEntry in sharedEntries)
            {
                var row = new List<string> { sharedEntry.Key };
                bool hasNonEmptyValue = false;

                // Get translation for each locale
                foreach (var locale in availableLocales)
                {
                    string value = "";

                    // Get the StringTable for this locale
                    var stringTable = tableCollection.GetTable(locale.Identifier) as StringTable;
                    
                    if (stringTable != null)
                    {
                        var entry = stringTable.GetEntry(sharedEntry.Key);
                        if (entry != null && !string.IsNullOrEmpty(entry.Value))
                        {
                            value = entry.Value;
                            hasNonEmptyValue = true;
                        }
                    }

                    row.Add(value);
                }

                // Add row if it has values or if we include empty entries
                if (hasNonEmptyValue || includeEmptyEntries)
                {
                    csvContent.AppendLine(EscapeCSVLine(row));
                }
            }

            // Write to file with UTF-8 BOM
            File.WriteAllText(filePath, csvContent.ToString(), new UTF8Encoding(true));

            Debug.Log($"Exported: {fileName} ({sharedEntries.Count} entries)");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get all String Table Collections in the project
        /// </summary>
        private List<StringTableCollection> GetAllStringTables()
        {
            var collections = new List<StringTableCollection>();

            var guids = AssetDatabase.FindAssets("t:StringTableCollection");
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var collection = AssetDatabase.LoadAssetAtPath<StringTableCollection>(path);
                if (collection != null)
                {
                    collections.Add(collection);
                }
            }

            return collections.OrderBy(c => c.TableCollectionName).ToList();
        }

        /// <summary>
        /// Get tables to export based on selection
        /// </summary>
        private List<StringTableCollection> GetTablesToExport(List<StringTableCollection> allTables)
        {
            if (exportAllTables)
            {
                return allTables;
            }

            var result = new List<StringTableCollection>();
            for (int i = 0; i < allTables.Count && i < selectedTables.Count; i++)
            {
                if (selectedTables[i])
                {
                    result.Add(allTables[i]);
                }
            }
            return result;
        }

        /// <summary>
        /// Refresh the table selection list
        /// </summary>
        private void RefreshTableList()
        {
            selectedTables.Clear();
            var tables = GetAllStringTables();
            for (int i = 0; i < tables.Count; i++)
            {
                selectedTables.Add(true);
            }
        }

        /// <summary>
        /// Escape CSV line (handle commas, quotes, newlines)
        /// </summary>
        private string EscapeCSVLine(List<string> fields)
        {
            var escaped = fields.Select(f => EscapeCSVField(f));
            return string.Join(CSV_SEPARATOR.ToString(), escaped);
        }

        /// <summary>
        /// Escape individual CSV field
        /// </summary>
        private string EscapeCSVField(string field)
        {
            if (string.IsNullOrEmpty(field))
            {
                return "";
            }

            // If field contains comma, quote, or newline, wrap in quotes
            if (field.Contains(CSV_SEPARATOR) || 
                field.Contains('"') || 
                field.Contains('\n') || 
                field.Contains('\r'))
            {
                // Escape quotes by doubling them
                field = field.Replace("\"", "\"\"");
                return $"\"{field}\"";
            }

            return field;
        }

        #endregion
    }
}