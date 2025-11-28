#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class GenericCollectionEditor : EditorWindow
{
    // ==== UI State ====
    private Vector2 _leftScroll, _rightScroll;
    private string _search = "";
    private int _selectedCollectionIndex = -1;
    private string _sortField = "";
    private bool _sortAsc = true;
    private const int PageSize = 20;
    private int _page = 0;

    // ==== Data ====
    private List<ScriptableObject> _collections = new();
    private SerializedObject _so;
    private SerializedProperty _docs;
    private SerializedProperty _versionCode, _versionName, _lastUpdated;
    
    // ==== Reflection Cache ====
    private Type _documentType;
    private FieldInfo[] _documentFields;
    private Dictionary<string, Type> _fieldTypes = new();

    [MenuItem("Tools/Generic Collection Editor")]
    public static void Open()
    {
        GetWindow<GenericCollectionEditor>("Collection Editor").Show();
    }

    private void OnEnable() => RefreshCollections();

    private void OnGUI()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            DrawCollectionsPane();
            DrawTablePane();
        }
    }

    // ===== Left Pane: Collections List =====
    private void DrawCollectionsPane()
    {
        using (new EditorGUILayout.VerticalScope(GUILayout.Width(350)))
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Collections", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70)))
                    RefreshCollections();
            }

            using (var sv = new EditorGUILayout.ScrollViewScope(_leftScroll))
            {
                _leftScroll = sv.scrollPosition;

                if (_collections.Count == 0)
                {
                    EditorGUILayout.HelpBox("No Collections found inheriting from CollectionBase.", MessageType.Info);
                }

                for (int i = 0; i < _collections.Count; i++)
                {
                    var collection = _collections[i];
                    if (!collection) continue;

                    using (new EditorGUILayout.VerticalScope("box"))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            var icon = AssetDatabase.GetCachedIcon(AssetDatabase.GetAssetPath(collection));
                            GUILayout.Label(icon, GUILayout.Width(18), GUILayout.Height(18));

                            // Highlight selected collection
                            var originalColor = GUI.backgroundColor;
                            if (_selectedCollectionIndex == i)
                                GUI.backgroundColor = Color.red;

                            bool choose = GUILayout.Toggle(_selectedCollectionIndex == i, collection.name, "Button");
                            if (choose && _selectedCollectionIndex != i) SelectCollection(i);
                            
                            GUI.backgroundColor = originalColor;
                        }

                        // Collection info
                        EditorGUILayout.LabelField("Type:", collection.GetType().Name, EditorStyles.miniLabel);
                        EditorGUILayout.LabelField("Path:", AssetDatabase.GetAssetPath(collection), EditorStyles.miniLabel);
                        EditorGUILayout.LabelField("Documents:", GetDocCount(collection).ToString(), EditorStyles.miniLabel);

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button("Ping", GUILayout.Width(60))) 
                                EditorGUIUtility.PingObject(collection);
                            if (GUILayout.Button("Select", GUILayout.Width(70))) 
                                Selection.activeObject = collection;
                            if (GUILayout.Button("Inspect", GUILayout.Width(70)))
                                EditorGUIUtility.PingObject(collection);
                        }
                    }
                }
            }
        }
    }

    // ===== Right Pane: Table View =====
    private void DrawTablePane()
    {
        using (new EditorGUILayout.VerticalScope())
        {
            if (_so == null || _documentType == null)
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.HelpBox("Select a Collection from the left panel.", MessageType.Info);
                GUILayout.FlexibleSpace();
                return;
            }

            _so.Update();

            // Toolbar
            DrawToolbar();

            // Version Info
            DrawVersionInfo();

            // Table
            using (var sv = new EditorGUILayout.ScrollViewScope(_rightScroll))
            {
                _rightScroll = sv.scrollPosition;

                DrawTableHeader();
                DrawTableRows();
            }

            // Footer
            DrawFooter();
        }
    }

    private void DrawToolbar()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            GUILayout.Label(_so.targetObject.name, EditorStyles.boldLabel);
            GUILayout.Label($"({_documentType.Name})", EditorStyles.miniLabel);

            GUILayout.FlexibleSpace();

            // Sort
            GUILayout.Label("Sort:", GUILayout.Width(32));
            var fieldNames = _documentFields.Select(f => f.Name).Prepend("None").ToArray();
            var currentSortIndex = string.IsNullOrEmpty(_sortField) ? 0 : Array.IndexOf(fieldNames, _sortField);
            if (currentSortIndex < 0) currentSortIndex = 0;
            
            var newSortIndex = EditorGUILayout.Popup(currentSortIndex, fieldNames, EditorStyles.toolbarPopup, GUILayout.Width(120));
            var newSortField = newSortIndex == 0 ? "" : fieldNames[newSortIndex];
            if (newSortField != _sortField) { _sortField = newSortField; _page = 0; Repaint(); }

            if (GUILayout.Button(_sortAsc ? "▲" : "▼", EditorStyles.toolbarButton, GUILayout.Width(24)))
            { _sortAsc = !_sortAsc; Repaint(); }

            GUILayout.Space(8);

            // Search
            string s = GUILayout.TextField(_search, EditorStyles.toolbarSearchField, GUILayout.MinWidth(200));
            if (s != _search) { _search = s; _page = 0; Repaint(); }
            if (GUILayout.Button("×", EditorStyles.toolbarButton, GUILayout.Width(20)))
            { _search = ""; GUI.FocusControl(null); _page = 0; }

            GUILayout.Space(8);
            if (GUILayout.Button("Validate", EditorStyles.toolbarButton, GUILayout.Width(80))) Validate();
            if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(60))) Save();
        }
    }

    private void DrawVersionInfo()
    {
        if (_versionCode != null)
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Version", EditorStyles.boldLabel);
                
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(_versionCode, new GUIContent("Code"), GUILayout.Width(200));
                    EditorGUILayout.PropertyField(_versionName, new GUIContent("Name"), GUILayout.Width(200));
                    EditorGUILayout.PropertyField(_lastUpdated, new GUIContent("Updated"), GUILayout.Width(200));
                    
                    if (GUILayout.Button("Set Now", GUILayout.Width(80)))
                    {
                        _lastUpdated.intValue = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        _so.ApplyModifiedProperties();
                        Touch();
                    }
                }
            }
        }
    }

    private void DrawTableHeader()
    {
        using (new EditorGUILayout.HorizontalScope("box"))
        {
            GUILayout.Label("✓", EditorStyles.boldLabel, GUILayout.Width(20));
            GUILayout.Label("#", EditorStyles.boldLabel, GUILayout.Width(30));

            foreach (var field in _documentFields)
            {
                var width = GetFieldWidth(field);
                if (GUILayout.Button(field.Name, EditorStyles.boldLabel, GUILayout.Width(width)))
                {
                    if (_sortField == field.Name)
                        _sortAsc = !_sortAsc;
                    else
                    {
                        _sortField = field.Name;
                        _sortAsc = true;
                    }
                    _page = 0;
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label("Actions", EditorStyles.boldLabel, GUILayout.Width(100));
        }
    }

    private void DrawTableRows()
    {
        var indices = GetFilteredSortedIndices();
        int start = _page * PageSize;
        int end = Mathf.Min(start + PageSize, indices.Count);

        for (int n = start; n < end; n++)
        {
            DrawRow(indices[n], n + 1);
        }

        if (indices.Count == 0)
        {
            EditorGUILayout.HelpBox("No matching documents found.", MessageType.Info);
        }
    }

    private void DrawRow(int index, int displayIndex)
    {
        var element = _docs.GetArrayElementAtIndex(index);

        using (new EditorGUILayout.HorizontalScope("box"))
        {
            // Selection checkbox
            element.isExpanded = EditorGUILayout.Toggle(element.isExpanded, GUILayout.Width(20));
            
            // Index
            GUILayout.Label(displayIndex.ToString(), GUILayout.Width(30));

            // Fields
            foreach (var field in _documentFields)
            {
                DrawFieldCell(element, field);
            }

            GUILayout.FlexibleSpace();

            // Actions
            using (new EditorGUILayout.HorizontalScope(GUILayout.Width(100)))
            {
                if (GUILayout.Button("Dup", GUILayout.Width(40))) Duplicate(index);
                if (GUILayout.Button("Del", GUILayout.Width(40))) 
                {
                    _docs.DeleteArrayElementAtIndex(index);
                    Touch();
                }
            }
        }
    }

    private void DrawFieldCell(SerializedProperty element, FieldInfo field)
    {
        // Try to find property by field name, handle Unity naming conventions
        var prop = FindPropertySmart(element, field.Name);
        
        var width = GetFieldWidth(field);

        if (prop == null)
        {
            EditorGUILayout.LabelField($"[{field.Name}]", EditorStyles.miniLabel, GUILayout.Width(width));
            return;
        }

        EditorGUI.BeginChangeCheck();

        // Handle different property types
        switch (prop.propertyType)
        {
            case SerializedPropertyType.String:
                prop.stringValue = EditorGUILayout.TextField(prop.stringValue, GUILayout.Width(width));
                break;
            case SerializedPropertyType.Integer:
                prop.intValue = EditorGUILayout.IntField(prop.intValue, GUILayout.Width(width));
                break;
            case SerializedPropertyType.Float:
                prop.floatValue = EditorGUILayout.FloatField(prop.floatValue, GUILayout.Width(width));
                break;
            case SerializedPropertyType.Boolean:
                prop.boolValue = EditorGUILayout.Toggle(prop.boolValue, GUILayout.Width(width));
                break;
            case SerializedPropertyType.Enum:
                prop.enumValueIndex = EditorGUILayout.Popup(prop.enumValueIndex, prop.enumDisplayNames, GUILayout.Width(width));
                break;
            case SerializedPropertyType.ObjectReference:
                prop.objectReferenceValue = EditorGUILayout.ObjectField(prop.objectReferenceValue, field.FieldType, false, GUILayout.Width(width));
                break;
            case SerializedPropertyType.Vector2:
                prop.vector2Value = EditorGUILayout.Vector2Field("", prop.vector2Value, GUILayout.Width(width));
                break;
            case SerializedPropertyType.Vector3:
                prop.vector3Value = EditorGUILayout.Vector3Field("", prop.vector3Value, GUILayout.Width(width));
                break;
            case SerializedPropertyType.Color:
                prop.colorValue = EditorGUILayout.ColorField(prop.colorValue, GUILayout.Width(width));
                break;
            case SerializedPropertyType.Generic:
                if (field.FieldType.IsArray)
                {
                    DrawArrayField(prop, field, width);
                }
                else
                {
                    EditorGUILayout.LabelField($"[{field.FieldType.Name}]", GUILayout.Width(width));
                }
                break;
            default:
                EditorGUILayout.LabelField($"[{prop.propertyType}]", GUILayout.Width(width));
                break;
        }

        if (EditorGUI.EndChangeCheck())
        {
            Touch();
        }
    }

    private void DrawArrayField(SerializedProperty prop, FieldInfo field, float width)
    {
        if (field.FieldType == typeof(string[]))
        {
            // Handle string arrays as comma-separated values
            var values = new List<string>();
            for (int i = 0; i < prop.arraySize; i++)
            {
                values.Add(prop.GetArrayElementAtIndex(i).stringValue ?? "");
            }
            
            string joined = string.Join(", ", values);
            string newJoined = EditorGUILayout.TextField(joined, GUILayout.Width(width));
            
            if (newJoined != joined)
            {
                var parts = newJoined.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                   .Select(x => x.Trim()).ToArray();
                
                prop.arraySize = parts.Length;
                for (int i = 0; i < parts.Length; i++)
                {
                    prop.GetArrayElementAtIndex(i).stringValue = parts[i];
                }
            }
        }
        else
        {
            EditorGUILayout.LabelField($"Array[{prop.arraySize}]", GUILayout.Width(width));
        }
    }

    private float GetFieldWidth(FieldInfo field)
    {
        // Dynamic width based on field type and name
        switch (field.FieldType.Name.ToLower())
        {
            case "boolean": return 50;
            case "int32": case "single": return 80;
            case "vector2": return 120;
            case "vector3": return 160;
            case "color": return 80;
            default:
                if (field.FieldType.IsEnum) return 100;
                if (field.FieldType.IsArray) return 150;
                if (field.Name.ToLower().Contains("id")) return 200;
                if (field.Name.ToLower().Contains("name")) return 150;
                if (field.Name.ToLower().Contains("description")) return 200;
                return 120;
        }
    }

    private void DrawFooter()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("+ Add Document", GUILayout.Width(120))) AddDocument();
            if (GUILayout.Button("Remove Selected", GUILayout.Width(130))) RemoveSelected();

            GUILayout.FlexibleSpace();

            // Pagination
            var totalItems = GetFilteredSortedIndices().Count;
            var totalPages = Mathf.CeilToInt((float)totalItems / PageSize);
            EditorGUILayout.LabelField($"Page {_page + 1} of {Mathf.Max(1, totalPages)} ({totalItems} items)", GUILayout.Width(150));

            GUI.enabled = _page > 0;
            if (GUILayout.Button("Prev", GUILayout.Width(60))) _page--;
            GUI.enabled = (_page + 1) * PageSize < totalItems;
            if (GUILayout.Button("Next", GUILayout.Width(60))) _page++;
            GUI.enabled = true;
        }
    }

    // ===== Filtering & Sorting =====
    private List<int> GetFilteredSortedIndices()
    {
        int count = _docs.arraySize;
        var indices = new List<int>();

        // Filter
        for (int i = 0; i < count; i++)
        {
            if (PassFilter(i)) indices.Add(i);
        }

        // Sort
        if (!string.IsNullOrEmpty(_sortField))
        {
            indices.Sort((a, b) =>
            {
                var cmp = CompareDocuments(a, b, _sortField);
                return _sortAsc ? cmp : -cmp;
            });
        }

        return indices;
    }

    private SerializedProperty FindPropertySmart(SerializedProperty element, string fieldName)
    {
        // Try to find property by field name, handle Unity naming conventions
        var prop = element.FindPropertyRelative(fieldName);
        
        // If not found, try with Unity's naming convention for private fields
        if (prop == null && fieldName.StartsWith("_"))
        {
            prop = element.FindPropertyRelative(fieldName.Substring(1));
        }
        
        // If still not found, try camelCase version
        if (prop == null && char.IsUpper(fieldName[0]))
        {
            var camelCase = char.ToLower(fieldName[0]) + fieldName.Substring(1);
            prop = element.FindPropertyRelative(camelCase);
        }
        
        return prop;
    }

    private bool PassFilter(int index)
    {
        if (string.IsNullOrWhiteSpace(_search)) return true;

        var element = _docs.GetArrayElementAtIndex(index);
        string searchLower = _search.ToLowerInvariant();

        foreach (var field in _documentFields)
        {
            var prop = FindPropertySmart(element, field.Name);
            if (prop != null)
            {
                string value = GetPropertyStringValue(prop).ToLowerInvariant();
                if (value.Contains(searchLower)) return true;
            }
        }

        return false;
    }

    private int CompareDocuments(int indexA, int indexB, string fieldName)
    {
        var elementA = _docs.GetArrayElementAtIndex(indexA);
        var elementB = _docs.GetArrayElementAtIndex(indexB);
        
        var propA = FindPropertySmart(elementA, fieldName);
        var propB = FindPropertySmart(elementB, fieldName);

        if (propA == null || propB == null) return 0;

        switch (propA.propertyType)
        {
            case SerializedPropertyType.String:
                return string.Compare(propA.stringValue, propB.stringValue, StringComparison.OrdinalIgnoreCase);
            case SerializedPropertyType.Integer:
                return propA.intValue.CompareTo(propB.intValue);
            case SerializedPropertyType.Float:
                return propA.floatValue.CompareTo(propB.floatValue);
            case SerializedPropertyType.Boolean:
                return propA.boolValue.CompareTo(propB.boolValue);
            case SerializedPropertyType.Enum:
                return propA.enumValueIndex.CompareTo(propB.enumValueIndex);
            default:
                return 0;
        }
    }

    private string GetPropertyStringValue(SerializedProperty prop)
    {
        switch (prop.propertyType)
        {
            case SerializedPropertyType.String: return prop.stringValue ?? "";
            case SerializedPropertyType.Integer: return prop.intValue.ToString();
            case SerializedPropertyType.Float: return prop.floatValue.ToString();
            case SerializedPropertyType.Boolean: return prop.boolValue.ToString();
            case SerializedPropertyType.Enum: return prop.enumNames[prop.enumValueIndex];
            default: return "";
        }
    }

    // ===== Actions =====
    private void AddDocument()
    {
        int index = _docs.arraySize;
        _docs.InsertArrayElementAtIndex(index);
        var element = _docs.GetArrayElementAtIndex(index);

        // Set default values for new document
        foreach (var field in _documentFields)
        {
            var prop = FindPropertySmart(element, field.Name);
            if (prop != null)
            {
                SetDefaultValue(prop, field);
            }
        }

        Touch();
    }

    private void SetDefaultValue(SerializedProperty prop, FieldInfo field)
    {
        switch (prop.propertyType)
        {
            case SerializedPropertyType.String:
                if (field.Name.ToLower().Contains("id"))
                    prop.stringValue = Guid.NewGuid().ToString("N");
                else if (field.Name.ToLower().Contains("name"))
                    prop.stringValue = $"New {_documentType.Name}";
                else
                    prop.stringValue = "";
                break;
            case SerializedPropertyType.Integer:
                if (field.Name.ToLower().Contains("multiplier")) 
                    prop.intValue = 100;
                else if (field.Name.ToLower().Contains("time") || field.Name.ToLower().Contains("duration"))
                    prop.intValue = 60; // Default 60 seconds for time fields
                else if (field.Name.ToLower().Contains("gold"))
                    prop.intValue = 100; // Default gold values
                else if (field.Name.ToLower().Contains("players"))
                    prop.intValue = 8; // Default max players
                else
                    prop.intValue = 0;
                break;
            case SerializedPropertyType.Float:
                prop.floatValue = 0f;
                break;
            case SerializedPropertyType.Boolean:
                prop.boolValue = false;
                break;
            case SerializedPropertyType.Enum:
                prop.enumValueIndex = 0;
                break;
        }
    }

    private void Duplicate(int index)
    {
        _docs.InsertArrayElementAtIndex(index);
        var duplicate = _docs.GetArrayElementAtIndex(index + 1);

        // Update ID fields to be unique
        foreach (var field in _documentFields)
        {
            if (field.Name.ToLower().Contains("id"))
            {
                var prop = FindPropertySmart(duplicate, field.Name);
                if (prop != null && prop.propertyType == SerializedPropertyType.String)
                {
                    prop.stringValue = Guid.NewGuid().ToString("N");
                }
            }
            else if (field.Name.ToLower().Contains("name"))
            {
                var prop = FindPropertySmart(duplicate, field.Name);
                if (prop != null && prop.propertyType == SerializedPropertyType.String)
                {
                    prop.stringValue += " (Copy)";
                }
            }
            else if (field.Name.ToLower().Contains("selected"))
            {
                var prop = FindPropertySmart(duplicate, field.Name);
                if (prop != null && prop.propertyType == SerializedPropertyType.Boolean)
                {
                    prop.boolValue = false; // Ensure duplicated items are not selected
                }
            }
        }

        Touch();
    }

    private void RemoveSelected()
    {
        for (int i = _docs.arraySize - 1; i >= 0; i--)
        {
            if (_docs.GetArrayElementAtIndex(i).isExpanded)
            {
                _docs.DeleteArrayElementAtIndex(i);
            }
        }
        Touch();
    }

    private void Validate()
    {
        var issues = new List<string>();
        var ids = new HashSet<string>();

        for (int i = 0; i < _docs.arraySize; i++)
        {
            var element = _docs.GetArrayElementAtIndex(i);
            
            foreach (var field in _documentFields)
            {
                var prop = FindPropertySmart(element, field.Name);
                if (prop != null)
                {
                    // Check for missing required fields
                    if (field.Name.ToLower().Contains("name") && prop.propertyType == SerializedPropertyType.String)
                    {
                        if (string.IsNullOrWhiteSpace(prop.stringValue))
                            issues.Add($"Document {i}: Missing {field.Name}");
                    }

                    // Check for duplicate IDs
                    if (field.Name.ToLower().Contains("id") && prop.propertyType == SerializedPropertyType.String)
                    {
                        if (!string.IsNullOrWhiteSpace(prop.stringValue))
                        {
                            if (ids.Contains(prop.stringValue))
                                issues.Add($"Document {i}: Duplicate ID '{prop.stringValue}'");
                            else
                                ids.Add(prop.stringValue);
                        }
                        else
                        {
                            issues.Add($"Document {i}: Missing {field.Name}");
                        }
                    }
                    
                    // Check for invalid values
                    if (prop.propertyType == SerializedPropertyType.Integer)
                    {
                        var minAttr = field.GetCustomAttributes(typeof(MinAttribute), true).FirstOrDefault() as MinAttribute;
                        if (minAttr != null && prop.intValue < minAttr.min)
                        {
                            issues.Add($"Document {i}: {field.Name} ({prop.intValue}) is below minimum ({minAttr.min})");
                        }
                    }
                }
            }
        }

        string message = issues.Count == 0 ? "Validation passed! No issues found." : 
                        $"Found {issues.Count} issues:\n\n" + string.Join("\n", issues);
        
        EditorUtility.DisplayDialog("Validation Results", message, "OK");
    }

    private void Save()
    {
        _so.ApplyModifiedProperties();
        Touch();
        AssetDatabase.SaveAssets();
        Repaint();
    }

    private void Touch()
    {
        if (_lastUpdated != null)
        {
            _lastUpdated.intValue = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _so.ApplyModifiedPropertiesWithoutUndo();
        }
        if (_so?.targetObject) EditorUtility.SetDirty(_so.targetObject);
    }

    // ===== Collection Management =====
    private void RefreshCollections()
    {
        _collections.Clear();

        // Find all ScriptableObjects that inherit from CollectionBase
        var guids = AssetDatabase.FindAssets("t:ScriptableObject");
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            
            if (asset != null && IsCollectionBase(asset.GetType()))
            {
                _collections.Add(asset);
            }
        }

        _collections = _collections.OrderBy(c => c.GetType().Name).ThenBy(c => c.name).ToList();

        // Auto-select first collection
        if (_collections.Count > 0 && (_selectedCollectionIndex < 0 || _selectedCollectionIndex >= _collections.Count))
        {
            SelectCollection(0);
        }

        Repaint();
    }

    private bool IsCollectionBase(Type type)
    {
        var baseType = type.BaseType;
        while (baseType != null)
        {
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition().Name == "CollectionBase`1")
            {
                return true;
            }
            baseType = baseType.BaseType;
        }
        return false;
    }

    private void SelectCollection(int index)
    {
        if (index < 0 || index >= _collections.Count) return;

        _selectedCollectionIndex = index;
        var collection = _collections[index];
        
        _so = new SerializedObject(collection);
        _docs = _so.FindProperty("documents");
        _versionCode = _so.FindProperty("versionCode");
        _versionName = _so.FindProperty("versionName");
        _lastUpdated = _so.FindProperty("lastUpdated");

        // Get document type through reflection
        var collectionType = collection.GetType();
        var baseType = collectionType.BaseType;
        while (baseType != null && (!baseType.IsGenericType || baseType.GetGenericTypeDefinition().Name != "CollectionBase`1"))
        {
            baseType = baseType.BaseType;
        }

        if (baseType != null)
        {
            _documentType = baseType.GetGenericArguments()[0];
            
            // Get both public fields and private serialized fields
            var publicFields = _documentType.GetFields(BindingFlags.Public | BindingFlags.Instance)
                                          .Where(f => !f.IsStatic);
            
            var privateSerializedFields = _documentType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                                                     .Where(f => !f.IsStatic && f.GetCustomAttributes(typeof(SerializeField), true).Length > 0);
            
            _documentFields = publicFields.Concat(privateSerializedFields)
                                        .OrderBy(f => f.Name)
                                        .ToArray();

            _fieldTypes.Clear();
            foreach (var field in _documentFields)
            {
                _fieldTypes[field.Name] = field.FieldType;
            }
        }

        _page = 0;
        _search = "";
        _sortField = "";
        Repaint();
    }

    private int GetDocCount(ScriptableObject collection)
    {
        var so = new SerializedObject(collection);
        return so.FindProperty("documents")?.arraySize ?? 0;
    }
}
#endif