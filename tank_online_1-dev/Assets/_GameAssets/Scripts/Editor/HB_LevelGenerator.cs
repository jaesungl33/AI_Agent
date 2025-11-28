#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class LevelGeneratorTool : EditorWindow
{
    private Texture2D inputMap;
    private int mapWidth = 100;
    private int mapHeight = 100;
    private List<Color32> detectedColors = new List<Color32>();
    private Dictionary<Color32, string> colorToLegend = new Dictionary<Color32, string>(new Color32Comparer());
    private Dictionary<Color32, List<GameObject>> colorToPrefabs = new Dictionary<Color32, List<GameObject>>(new Color32Comparer());
    private Dictionary<Color32, List<bool>> prefabOffsetEnabled = new Dictionary<Color32, List<bool>>(new Color32Comparer());
    private Dictionary<Color32, List<float>> prefabWeights = new Dictionary<Color32, List<float>>(new Color32Comparer());
    private Dictionary<Color32, float> colorSizeBias = new Dictionary<Color32, float>(new Color32Comparer());
    private Dictionary<Color32, bool> offsetFoldouts = new Dictionary<Color32, bool>(new Color32Comparer());
    private Dictionary<Color32, bool> prefabFoldouts = new Dictionary<Color32, bool>(new Color32Comparer());
    private Dictionary<Color32, OffsetSetting> offsetSettings = new Dictionary<Color32, OffsetSetting>(new Color32Comparer());
    private Vector2 scrollPos;
    private string errorMsg = "";
    private bool[,] occupied;
    private int randomSeed = 0;
    private string rootGroupName = "Level";
    private int maxColorsLimit = 20;
    
    // Copy/Paste Offset Settings
    private OffsetSetting copiedOffsetSetting = null;

    // --- Preset Data Classes ---
    [System.Serializable]
    private class PresetData
    {
        public string inputMapPath;
        public int mapWidth;
        public int mapHeight;
        public int randomSeed;
        public string rootGroupName;
        public int maxColorsLimit;
        public List<ColorLegendData> colorLegends = new List<ColorLegendData>();
    }
    [System.Serializable]
    private class ColorLegendData
    {
        public Color32 color;
        public string legend;
        public List<string> prefabPaths = new List<string>();
        public List<bool> prefabOffsetEnabled = new List<bool>();
        public List<float> prefabWeights = new List<float>();
        public float sizeBias = 0.5f;
        public OffsetSetting offsetSetting = new OffsetSetting();
    }
    [System.Serializable]
    private class OffsetSetting
    {
        public bool snapRotation = false;
        public int snapStep = 90;
        public Vector3 rotationMin = Vector3.zero;
        public Vector3 rotationMax = Vector3.zero;

        public bool uniformScale = true;
        public bool scaleSnapEnabled = false;
        public float scaleSnapUnit = 0.1f;
        public Vector3 scaleMin = Vector3.one;
        public Vector3 scaleMax = Vector3.one;

        public Vector3 positionMin = Vector3.zero;
        public Vector3 positionMax = Vector3.zero;
        
        public bool positionSnapEnabled = false;
        public int positionSnapUnit = 1;
    }

    [MenuItem("HB Tools/HB Level Generator")]
    public static void ShowWindow()
    {
        GetWindow<LevelGeneratorTool>("HB Level Generator");
    }
    private void OnGUI()
    {
        GUILayout.Label("HB Level Generator", EditorStyles.boldLabel);

        // Top Menu - 3 nút Reset, Save, Load
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset", GUILayout.ExpandWidth(true)))
        {
            ResetSettings();
        }
        if (GUILayout.Button("Save Preset", GUILayout.ExpandWidth(true)))
        {
            SavePreset();
        }
        if (GUILayout.Button("Load Preset", GUILayout.ExpandWidth(true)))
        {
            LoadPreset();
        }
        GUILayout.EndHorizontal();

        // Separator
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space(5);

        // Map Analyze Section
        GUILayout.Space(10);
        GUILayout.Label("Map Analyze", EditorStyles.largeLabel);
        
        inputMap = (Texture2D)EditorGUILayout.ObjectField("Input Map", inputMap, typeof(Texture2D), false);

        GUILayout.BeginHorizontal();
        mapWidth = EditorGUILayout.IntField("Map Width (unit)", mapWidth);
        mapHeight = EditorGUILayout.IntField("Map Height (unit)", mapHeight);
        GUILayout.EndHorizontal();

        // Max Colors Limit
        maxColorsLimit = EditorGUILayout.IntField("Max Colors Limit", maxColorsLimit);
        if (maxColorsLimit < 1) maxColorsLimit = 1;
        if (maxColorsLimit > 1000) maxColorsLimit = 1000;

        // Random Seed Section
        GUILayout.BeginHorizontal();
        randomSeed = EditorGUILayout.IntField("Random Seed", randomSeed);
        if (GUILayout.Button("Randomize", GUILayout.Width(90)))
        {
            randomSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }
        GUILayout.EndHorizontal();

        // Root Group Name Section
        rootGroupName = EditorGUILayout.TextField("Root Group Name", rootGroupName);

        if (GUILayout.Button("Analyze Map"))
        {
            AnalyzeMap();
        }

        if (!string.IsNullOrEmpty(errorMsg))
        {
            EditorGUILayout.HelpBox(errorMsg, MessageType.Error);
        }

        if (detectedColors.Count > 0)
        {
            // Separator
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space(10);
            
            GUILayout.Label("Detected Colors (" + detectedColors.Count + "):", EditorStyles.boldLabel);
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(300));

            foreach (var color in detectedColors)
            {
                GUILayout.BeginVertical("box");

                // Color, Legend và Action Buttons - tất cả trên 1 hàng
                GUILayout.BeginHorizontal();
                
                // Color Preview
                Rect colorRect = GUILayoutUtility.GetRect(50, 20, GUILayout.ExpandWidth(false));
                EditorGUI.DrawRect(colorRect, new Color32(color.r, color.g, color.b, 255));

                // Legend TextBox với tooltip
                if (!colorToLegend.ContainsKey(color)) colorToLegend[color] = "";
                colorToLegend[color] = EditorGUILayout.TextField(colorToLegend[color], GUILayout.Width(150));
                
                // Tooltip cho Legend
                if (GUILayout.Button("?", GUILayout.Width(20)))
                {
                    EditorUtility.DisplayDialog("Legend Help", "Enter a name for this color group. This name will be used as the group name in the generated hierarchy.", "OK");
                }
                
                // Copy/Paste/Reset Offset Settings Buttons
                GUIContent copyIcon = EditorGUIUtility.IconContent("SaveActive");
                copyIcon.tooltip = "Copy offset settings from this legend";
                if (GUILayout.Button(copyIcon, GUILayout.Width(25)))
                {
                    // Check if legend has at least 1 prefab
                    if (!colorToPrefabs.ContainsKey(color) || colorToPrefabs[color].Count == 0)
                    {
                        EditorUtility.DisplayDialog("Copy Offset Settings", "Please add at least 1 prefab to this legend before copying offset settings.", "OK");
                        return;
                    }
                    
                    if (offsetSettings.ContainsKey(color))
                    {
                        copiedOffsetSetting = new OffsetSetting();
                        copiedOffsetSetting.snapRotation = offsetSettings[color].snapRotation;
                        copiedOffsetSetting.snapStep = offsetSettings[color].snapStep;
                        copiedOffsetSetting.rotationMin = offsetSettings[color].rotationMin;
                        copiedOffsetSetting.rotationMax = offsetSettings[color].rotationMax;
                        copiedOffsetSetting.uniformScale = offsetSettings[color].uniformScale;
                        copiedOffsetSetting.scaleSnapEnabled = offsetSettings[color].scaleSnapEnabled;
                        copiedOffsetSetting.scaleSnapUnit = offsetSettings[color].scaleSnapUnit;
                        copiedOffsetSetting.scaleMin = offsetSettings[color].scaleMin;
                        copiedOffsetSetting.scaleMax = offsetSettings[color].scaleMax;
                        copiedOffsetSetting.positionMin = offsetSettings[color].positionMin;
                        copiedOffsetSetting.positionMax = offsetSettings[color].positionMax;
                        copiedOffsetSetting.positionSnapEnabled = offsetSettings[color].positionSnapEnabled;
                        copiedOffsetSetting.positionSnapUnit = offsetSettings[color].positionSnapUnit;
                        EditorUtility.DisplayDialog("Copy Offset Settings", "Offset settings copied successfully!", "OK");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Copy Offset Settings", "No offset settings to copy for this legend.", "OK");
                    }
                }
                
                GUIContent pasteIcon = EditorGUIUtility.IconContent("Import");
                pasteIcon.tooltip = "Paste offset settings to this legend";
                EditorGUI.BeginDisabledGroup(copiedOffsetSetting == null);
                if (GUILayout.Button(pasteIcon, GUILayout.Width(25)))
                {
                    // Check if legend has at least 1 prefab
                    if (!colorToPrefabs.ContainsKey(color) || colorToPrefabs[color].Count == 0)
                    {
                        EditorUtility.DisplayDialog("Paste Offset Settings", "Please add at least 1 prefab to this legend before pasting offset settings.", "OK");
                        return;
                    }
                    
                    if (copiedOffsetSetting != null)
                    {
                        if (!offsetSettings.ContainsKey(color)) offsetSettings[color] = new OffsetSetting();
                        offsetSettings[color].snapRotation = copiedOffsetSetting.snapRotation;
                        offsetSettings[color].snapStep = copiedOffsetSetting.snapStep;
                        offsetSettings[color].rotationMin = copiedOffsetSetting.rotationMin;
                        offsetSettings[color].rotationMax = copiedOffsetSetting.rotationMax;
                        offsetSettings[color].uniformScale = copiedOffsetSetting.uniformScale;
                        offsetSettings[color].scaleSnapEnabled = copiedOffsetSetting.scaleSnapEnabled;
                        offsetSettings[color].scaleSnapUnit = copiedOffsetSetting.scaleSnapUnit;
                        offsetSettings[color].scaleMin = copiedOffsetSetting.scaleMin;
                        offsetSettings[color].scaleMax = copiedOffsetSetting.scaleMax;
                        offsetSettings[color].positionMin = copiedOffsetSetting.positionMin;
                        offsetSettings[color].positionMax = copiedOffsetSetting.positionMax;
                        offsetSettings[color].positionSnapEnabled = copiedOffsetSetting.positionSnapEnabled;
                        offsetSettings[color].positionSnapUnit = copiedOffsetSetting.positionSnapUnit;
                        EditorUtility.DisplayDialog("Paste Offset Settings", "Offset settings pasted successfully!", "OK");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Paste Offset Settings", "No offset settings to paste. Please copy settings first.", "OK");
                    }
                }
                EditorGUI.EndDisabledGroup();
                
                GUIContent resetIcon = EditorGUIUtility.IconContent("Refresh");
                resetIcon.tooltip = "Reset this legend (remove all prefabs and reset offset settings)";
                if (GUILayout.Button(resetIcon, GUILayout.Width(25)))
                {
                    bool resetConfirmed = EditorUtility.DisplayDialog("Reset Legend", 
                        "Are you sure you want to reset this legend?\n\nThis will remove all prefabs and reset offset settings.", 
                        "Reset", "Cancel");
                    
                    if (resetConfirmed)
                    {
                        // Reset prefabs
                        if (colorToPrefabs.ContainsKey(color)) colorToPrefabs[color].Clear();
                        if (prefabOffsetEnabled.ContainsKey(color)) prefabOffsetEnabled[color].Clear();
                        if (prefabWeights.ContainsKey(color)) prefabWeights[color].Clear();
                        
                        // Reset offset settings
                        if (offsetSettings.ContainsKey(color)) offsetSettings[color] = new OffsetSetting();
                        
                        // Reset size bias
                        if (colorSizeBias.ContainsKey(color)) colorSizeBias[color] = 0.5f;
                        
                        EditorUtility.DisplayDialog("Reset Legend", "Legend reset successfully!", "OK");
                    }
                }
                
                GUILayout.FlexibleSpace();
                
                // Action Buttons
                if (GUILayout.Button("Add Prefab", GUILayout.Width(80)))
                {
                    if (!colorToPrefabs.ContainsKey(color)) colorToPrefabs[color] = new List<GameObject>();
                    if (!prefabOffsetEnabled.ContainsKey(color)) prefabOffsetEnabled[color] = new List<bool>();
                    if (!prefabWeights.ContainsKey(color)) prefabWeights[color] = new List<float>();
                    if (!colorSizeBias.ContainsKey(color)) colorSizeBias[color] = 0.5f;
                    colorToPrefabs[color].Add(null);
                    prefabOffsetEnabled[color].Add(true);
                    prefabWeights[color].Add(1.0f);
                }

                if (GUILayout.Button("Add Folder", GUILayout.Width(80)))
                {
                    string folderPath = EditorUtility.OpenFolderPanel("Select Prefab Folder", Application.dataPath, "");
                    if (!string.IsNullOrEmpty(folderPath))
                    {
                        if (!colorToPrefabs.ContainsKey(color)) colorToPrefabs[color] = new List<GameObject>();
                        if (!prefabOffsetEnabled.ContainsKey(color)) prefabOffsetEnabled[color] = new List<bool>();
                        if (!prefabWeights.ContainsKey(color)) prefabWeights[color] = new List<float>();
                        if (!colorSizeBias.ContainsKey(color)) colorSizeBias[color] = 0.5f;
                        
                        string relativePath = "Assets" + folderPath.Substring(Application.dataPath.Length);
                        string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { relativePath });
                        foreach (var guid in guids)
                        {
                            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                            if (go != null)
                            {
                                colorToPrefabs[color].Add(go);
                                prefabOffsetEnabled[color].Add(true);
                                prefabWeights[color].Add(1.0f);
                            }
                        }
                    }
                }
                
                GUILayout.EndHorizontal();

                if (!colorToPrefabs.ContainsKey(color)) colorToPrefabs[color] = new List<GameObject>();
                if (!prefabOffsetEnabled.ContainsKey(color)) prefabOffsetEnabled[color] = new List<bool>();
                if (!prefabWeights.ContainsKey(color)) prefabWeights[color] = new List<float>();
                if (!colorSizeBias.ContainsKey(color)) colorSizeBias[color] = 0.5f;

                // Prefabs Section - chỉ hiện khi có ít nhất 1 prefab
                if (colorToPrefabs[color].Count > 0)
                {
                    GUILayout.Space(5);
                    
                    // Size Bias Section
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Size Bias:", GUILayout.Width(70));
                    colorSizeBias[color] = GUILayout.HorizontalSlider(colorSizeBias[color], 0f, 1f, GUILayout.ExpandWidth(true));
                    GUILayout.Label(colorSizeBias[color].ToString("F2"), GUILayout.Width(40));
                    
                    // Hiển thị mô tả bias
                    string biasDescription = "";
                    if (colorSizeBias[color] > 0.6f) biasDescription = "(Large Priority)";
                    else if (colorSizeBias[color] < 0.4f) biasDescription = "(Small Priority)";
                    else biasDescription = "(Neutral)";
                    GUILayout.Label(biasDescription, GUILayout.Width(100));
                    GUILayout.EndHorizontal();
                    
                    if (!prefabFoldouts.ContainsKey(color)) prefabFoldouts[color] = true;
                    prefabFoldouts[color] = EditorGUILayout.Foldout(prefabFoldouts[color], "Prefabs (" + colorToPrefabs[color].Count + ")");
                    
                    if (prefabFoldouts[color])
                    {
                        int removeIndex = -1;
                        for (int i = 0; i < colorToPrefabs[color].Count; i++)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Prefab " + (i + 1), GUILayout.Width(60));
                            colorToPrefabs[color][i] = (GameObject)EditorGUILayout.ObjectField(colorToPrefabs[color][i], typeof(GameObject), false, GUILayout.MinWidth(150));

                            if (prefabOffsetEnabled[color].Count <= i)
                                prefabOffsetEnabled[color].Add(true);
                            if (prefabWeights[color].Count <= i)
                                prefabWeights[color].Add(1.0f);

                            prefabOffsetEnabled[color][i] = EditorGUILayout.ToggleLeft("Offset", prefabOffsetEnabled[color][i], GUILayout.Width(70));
                            
                            // Weight field
                            GUILayout.Label("Weight:", GUILayout.Width(50));
                            prefabWeights[color][i] = EditorGUILayout.FloatField(prefabWeights[color][i], GUILayout.Width(50));
                            if (prefabWeights[color][i] < 0f) prefabWeights[color][i] = 0f;

                            if (GUILayout.Button("X", GUILayout.Width(20)))
                            {
                                removeIndex = i;
                            }
                            GUILayout.EndHorizontal();
                        }

                        if (removeIndex >= 0)
                        {
                            colorToPrefabs[color].RemoveAt(removeIndex);
                            if (prefabOffsetEnabled[color].Count > removeIndex)
                                prefabOffsetEnabled[color].RemoveAt(removeIndex);
                            if (prefabWeights[color].Count > removeIndex)
                                prefabWeights[color].RemoveAt(removeIndex);
                        }
                    }
                }

                // Offset Settings Section - chỉ hiện khi có ít nhất 1 prefab
                if (colorToPrefabs[color].Count > 0)
                {
                    if (!offsetFoldouts.ContainsKey(color)) offsetFoldouts[color] = false;
                    offsetFoldouts[color] = EditorGUILayout.Foldout(offsetFoldouts[color], "Offset Settings");

                    if (!offsetSettings.ContainsKey(color)) offsetSettings[color] = new OffsetSetting();
                    OffsetSetting setting = offsetSettings[color];

                    if (offsetFoldouts[color])
                    {
                        // Rotation Section
                        GUILayout.Space(5);
                        EditorGUILayout.LabelField("Rotation", EditorStyles.boldLabel);
                        setting.snapRotation = EditorGUILayout.Toggle("Snap Rotation", setting.snapRotation);
                        if (setting.snapRotation)
                        {
                            setting.snapStep = EditorGUILayout.IntField("Step (deg)", setting.snapStep);
                        }
                        EditorGUILayout.LabelField("Random Range (Min/Max)");
                        setting.rotationMin = EditorGUILayout.Vector3Field("Min", setting.rotationMin);
                        setting.rotationMax = EditorGUILayout.Vector3Field("Max", setting.rotationMax);

                        // Scale Section
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Scale", EditorStyles.boldLabel);
                        setting.uniformScale = EditorGUILayout.Toggle("Uniform Scale", setting.uniformScale);
                        
                        // Scale Snap UI
                        setting.scaleSnapEnabled = EditorGUILayout.Toggle("Scale Snap", setting.scaleSnapEnabled);
                        EditorGUI.BeginDisabledGroup(!setting.scaleSnapEnabled);
                        setting.scaleSnapUnit = EditorGUILayout.FloatField("Snap Unit", setting.scaleSnapUnit);
                        if (setting.scaleSnapUnit <= 0f) setting.scaleSnapUnit = 0.1f;
                        EditorGUI.EndDisabledGroup();
                        
                        EditorGUILayout.LabelField("Random Range (Min/Max)");

                        if (setting.uniformScale)
                        {
                            float minX = EditorGUILayout.FloatField("Min (X)", setting.scaleMin.x);
                            float maxX = EditorGUILayout.FloatField("Max (X)", setting.scaleMax.x);
                            setting.scaleMin = new Vector3(minX, minX, minX);
                            setting.scaleMax = new Vector3(maxX, maxX, maxX);
                        }
                        else
                        {
                            setting.scaleMin = EditorGUILayout.Vector3Field("Min", setting.scaleMin);
                            setting.scaleMax = EditorGUILayout.Vector3Field("Max", setting.scaleMax);
                        }

                        // Position Section
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Position Offset", EditorStyles.boldLabel);
                        
                        // Position Snap UI trong Offset Settings
                        setting.positionSnapEnabled = EditorGUILayout.Toggle("Position Snap", setting.positionSnapEnabled);
                        EditorGUI.BeginDisabledGroup(!setting.positionSnapEnabled);
                        setting.positionSnapUnit = EditorGUILayout.IntField("Snap Unit", setting.positionSnapUnit);
                        EditorGUI.EndDisabledGroup();
                        
                        EditorGUILayout.LabelField("Random Range (Min/Max)");
                        setting.positionMin = EditorGUILayout.Vector3Field("Min", setting.positionMin);
                        setting.positionMax = EditorGUILayout.Vector3Field("Max", setting.positionMax);
                    }
                }

                GUILayout.EndVertical();
            }

            GUILayout.EndScrollView();

            // Separator
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("Generate Layout Map", GUILayout.ExpandWidth(true)))
            {
                GenerateLayout();
            }
        }
    }
    // Main function
    private void GenerateLayout()
    {
        if (inputMap == null) return;

        // Sử dụng seed để random
        Random.InitState(randomSeed);

        string path = AssetDatabase.GetAssetPath(inputMap);
        Texture2D tex = new Texture2D(inputMap.width, inputMap.height);
        byte[] fileData = File.ReadAllBytes(path);
        tex.LoadImage(fileData);

        if (tex.width != mapWidth || tex.height != mapHeight)
        {
            tex = ResizeTexture(tex, mapWidth, mapHeight);
        }

        GameObject levelRoot = new GameObject(string.IsNullOrEmpty(rootGroupName) ? "Level" : rootGroupName);
        levelRoot.transform.position = Vector3.zero;

        Dictionary<Color32, Transform> legendGroups = new Dictionary<Color32, Transform>(new Color32Comparer());
        Dictionary<Color32, Dictionary<string, Transform>> prefabSubGroups = new Dictionary<Color32, Dictionary<string, Transform>>(new Color32Comparer());
        
        foreach (var kvp in colorToPrefabs)
        {
            if (kvp.Value != null && kvp.Value.Count > 0)
            {
                string legend = colorToLegend.ContainsKey(kvp.Key) && !string.IsNullOrEmpty(colorToLegend[kvp.Key]) ? colorToLegend[kvp.Key] : "UnnamedGroup";
                GameObject grp = new GameObject(legend);
                grp.transform.SetParent(levelRoot.transform);
                legendGroups[kvp.Key] = grp.transform;
                
                // Tạo subgroup theo tên prefab
                prefabSubGroups[kvp.Key] = new Dictionary<string, Transform>();
                HashSet<string> prefabNames = new HashSet<string>();
                foreach (var prefab in kvp.Value)
                {
                    if (prefab != null)
                    {
                        prefabNames.Add(prefab.name);
                    }
                }
                foreach (string prefabName in prefabNames)
                {
                    GameObject subGrp = new GameObject(prefabName);
                    subGrp.transform.SetParent(grp.transform);
                    prefabSubGroups[kvp.Key][prefabName] = subGrp.transform;
                }
            }
        }

        Color32[] pixels = tex.GetPixels32();
        Vector2 centerOffset = new Vector2(mapWidth / 2f, mapHeight / 2f);
        occupied = new bool[mapWidth, mapHeight];

        // First pass: initial random fitting
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (occupied[x, y]) continue;

                Color32 pixelColor = pixels[y * mapWidth + x];
                if (!colorToPrefabs.ContainsKey(pixelColor)) continue;
                List<GameObject> prefabList = colorToPrefabs[pixelColor];
                if (prefabList == null || prefabList.Count == 0) continue;

                GameObject selectedPrefab = null;
                Vector3 prefabSize = Vector3.one;
                int attempts = 0;
                do
                {
                    selectedPrefab = SelectPrefabWithWeight(prefabList, pixelColor);
                    attempts++;
                } while (selectedPrefab == null && attempts < 10);

                if (selectedPrefab == null) continue;

                prefabSize = GetPrefabBounds(selectedPrefab);
                int sizeX = Mathf.Max(1, Mathf.RoundToInt(prefabSize.x));
                int sizeZ = Mathf.Max(1, Mathf.RoundToInt(prefabSize.z));

                if (!CanPlace(x, y, sizeX, sizeZ, pixelColor, pixels)) continue;

                // Calculate angle for rotation
                float angleY = Random.Range(0f, 360f);
                PlacePrefab(selectedPrefab, x, y, sizeX, sizeZ, centerOffset, pixelColor, legendGroups, levelRoot, angleY);
            }
        }

        // Second pass: fill leftover gaps with best fit prefabs
        foreach (var kvp in colorToPrefabs)
        {
            Color32 pixelColor = kvp.Key;
            List<GameObject> prefabList = kvp.Value;
            if (prefabList == null || prefabList.Count == 0) continue;

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    if (occupied[x, y]) continue;
                    if (!pixels[y * mapWidth + x].Equals(pixelColor)) continue;

                    // Try to place the largest fitting prefab in pool
                    GameObject bestFit = null;
                    int bestSizeX = 0;
                    int bestSizeZ = 0;
                    float bestWeight = 0f;

                    foreach (var prefab in prefabList)
                    {
                        if (prefab == null) continue;
                        Vector3 size = GetPrefabBounds(prefab);
                        int sx = Mathf.Max(1, Mathf.RoundToInt(size.x));
                        int sz = Mathf.Max(1, Mathf.RoundToInt(size.z));
                        if (CanPlace(x, y, sx, sz, pixelColor, pixels))
                        {
                            // Tính weight cho prefab này
                            int prefabIndex = prefabList.IndexOf(prefab);
                            float baseWeight = prefabWeights.ContainsKey(pixelColor) && prefabIndex < prefabWeights[pixelColor].Count ? prefabWeights[pixelColor][prefabIndex] : 1.0f;
                            float sizeWeight = CalculateSizeWeight(prefab, colorSizeBias.ContainsKey(pixelColor) ? colorSizeBias[pixelColor] : 0.5f);
                            float finalWeight = baseWeight * sizeWeight;
                            
                            // Ưu tiên prefab có weight cao nhất
                            if (finalWeight > bestWeight)
                            {
                                bestFit = prefab;
                                bestSizeX = sx;
                                bestSizeZ = sz;
                                bestWeight = finalWeight;
                            }
                        }
                    }

                    if (bestFit != null)
                    {
                        // Calculate angle for rotation
                        float angleY = Random.Range(0f, 360f);
                        PlacePrefab(bestFit, x, y, bestSizeX, bestSizeZ, centerOffset, pixelColor, legendGroups, levelRoot, angleY);
                    }
                }
            }
        }

        Debug.Log("Layout generation completed.");
    }
    // Sửa PlacePrefab để sử dụng position snap từ OffsetSetting
    private void PlacePrefab(GameObject prefab, int x, int y, int sizeX, int sizeZ, Vector2 centerOffset, Color32 pixelColor, Dictionary<Color32, Transform> legendGroups, GameObject levelRoot, float angleY)
    {
        int prefabIndex = colorToPrefabs[pixelColor].IndexOf(prefab);
        bool useOffset = prefabOffsetEnabled.ContainsKey(pixelColor) && prefabIndex >= 0 && prefabIndex < prefabOffsetEnabled[pixelColor].Count && prefabOffsetEnabled[pixelColor][prefabIndex];

        OffsetSetting offset = offsetSettings.ContainsKey(pixelColor) ? offsetSettings[pixelColor] : new OffsetSetting();

        // Position
        float posX = x + sizeX / 2f - centerOffset.x;
        float posZ = y + sizeZ / 2f - centerOffset.y;
        float posY = 0f;

        if (useOffset)
        {
            float offsetX = Random.Range(offset.positionMin.x, offset.positionMax.x);
            float offsetY = Random.Range(offset.positionMin.y, offset.positionMax.y);
            float offsetZ = Random.Range(offset.positionMin.z, offset.positionMax.z);
            
            // Áp dụng position snap từ OffsetSetting
            if (offset.positionSnapEnabled && offset.positionSnapUnit > 0)
            {
                offsetX = Mathf.Round(offsetX / offset.positionSnapUnit) * offset.positionSnapUnit;
                offsetY = Mathf.Round(offsetY / offset.positionSnapUnit) * offset.positionSnapUnit;
                offsetZ = Mathf.Round(offsetZ / offset.positionSnapUnit) * offset.positionSnapUnit;
            }
            
            posX += offsetX;
            posY = offsetY;
            posZ += offsetZ;
        }

        Vector3 position = new Vector3(posX, posY, posZ);

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.transform.position = position;

        // Rotation
        if (useOffset)
        {
            Vector3 rot = new Vector3(
                Random.Range(offset.rotationMin.x, offset.rotationMax.x),
                Random.Range(offset.rotationMin.y, offset.rotationMax.y),
                Random.Range(offset.rotationMin.z, offset.rotationMax.z)
            );
            if (offset.snapRotation && offset.snapStep > 0)
            {
                rot.x = Mathf.Round(rot.x / offset.snapStep) * offset.snapStep;
                rot.y = Mathf.Round(rot.y / offset.snapStep) * offset.snapStep;
                rot.z = Mathf.Round(rot.z / offset.snapStep) * offset.snapStep;
            }
            instance.transform.eulerAngles = rot;
        }

        // Scale
        if (useOffset)
        {
            if (offset.uniformScale)
            {
                float s = Random.Range(offset.scaleMin.x, offset.scaleMax.x);
                
                // Áp dụng scale snap từ OffsetSetting
                if (offset.scaleSnapEnabled && offset.scaleSnapUnit > 0f)
                {
                    s = Mathf.Round(s / offset.scaleSnapUnit) * offset.scaleSnapUnit;
                }
                
                instance.transform.localScale = new Vector3(s, s, s);
            }
            else
            {
                float sx = Random.Range(offset.scaleMin.x, offset.scaleMax.x);
                float sy = Random.Range(offset.scaleMin.y, offset.scaleMax.y);
                float sz = Random.Range(offset.scaleMin.z, offset.scaleMax.z);
                
                // Áp dụng scale snap từ OffsetSetting
                if (offset.scaleSnapEnabled && offset.scaleSnapUnit > 0f)
                {
                    sx = Mathf.Round(sx / offset.scaleSnapUnit) * offset.scaleSnapUnit;
                    sy = Mathf.Round(sy / offset.scaleSnapUnit) * offset.scaleSnapUnit;
                    sz = Mathf.Round(sz / offset.scaleSnapUnit) * offset.scaleSnapUnit;
                }
                
                instance.transform.localScale = new Vector3(sx, sy, sz);
            }
        }

        if (colorToLegend.ContainsKey(pixelColor) && !string.IsNullOrEmpty(colorToLegend[pixelColor]))
        {
            instance.name = colorToLegend[pixelColor] + "_" + instance.name;
        }

        // Đặt prefab vào subgroup theo tên prefab
        if (legendGroups.ContainsKey(pixelColor))
        {
            // Tìm subgroup theo tên prefab
            Transform parentGroup = legendGroups[pixelColor];
            Transform subGroup = parentGroup.Find(prefab.name);
            if (subGroup != null)
            {
                instance.transform.SetParent(subGroup);
            }
            else
            {
                // Nếu không tìm thấy subgroup, đặt vào group chính
                instance.transform.SetParent(parentGroup);
            }
        }
        else
        {
            instance.transform.SetParent(levelRoot.transform);
        }

        for (int dz = 0; dz < sizeZ; dz++)
        {
            for (int dx = 0; dx < sizeX; dx++)
            {
                int markX = x + dx;
                int markY = y + dz;
                if (markX < mapWidth && markY < mapHeight)
                    occupied[markX, markY] = true;
            }
        }
    }
    private bool CanPlace(int startX, int startY, int sizeX, int sizeY, Color32 targetColor, Color32[] pixels)
    {
    for (int dz = 0; dz < sizeY; dz++)
    {
        for (int dx = 0; dx < sizeX; dx++)
        {
            int checkX = startX + dx;
            int checkY = startY + dz;
            if (checkX >= mapWidth || checkY >= mapHeight) return false;
            if (occupied[checkX, checkY]) return false;
            Color32 col = pixels[checkY * mapWidth + checkX];
            if (!col.Equals(targetColor)) return false;
        }
    }
    return true;
    } 

    private Vector3 GetPrefabBounds(GameObject prefab)
    {
    GameObject temp = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
    Renderer[] renderers = temp.GetComponentsInChildren<Renderer>();
    Bounds bounds = new Bounds(temp.transform.position, Vector3.zero);
    foreach (var rend in renderers)
    {
        bounds.Encapsulate(rend.bounds);
    }
    Vector3 size = bounds.size;
    DestroyImmediate(temp);
    return size;
    }

    private float CalculateSizeWeight(GameObject prefab, float bias)
    {
        Vector3 size = GetPrefabBounds(prefab);
        float area = size.x * size.z;
        
        // Normalize area về 0-1 (giả sử max area là 10x10 = 100)
        float maxArea = 100f;
        float normalizedArea = Mathf.Clamp01(area / maxArea);
        
        // Áp dụng bias
        if (bias > 0.5f) // Ưu tiên lớn
        {
            float biasStrength = (bias - 0.5f) * 2f; // 0-1
            return Mathf.Lerp(0.1f, 1.0f, normalizedArea * biasStrength);
        }
        else // Ưu tiên nhỏ
        {
            float biasStrength = (0.5f - bias) * 2f; // 0-1
            return Mathf.Lerp(0.1f, 1.0f, (1f - normalizedArea) * biasStrength);
        }
    }

    private GameObject SelectPrefabWithWeight(List<GameObject> prefabList, Color32 color)
    {
        if (prefabList == null || prefabList.Count == 0) return null;
        
        // Tính tổng weight
        float totalWeight = 0f;
        List<float> finalWeights = new List<float>();
        
        for (int i = 0; i < prefabList.Count; i++)
        {
            if (prefabList[i] == null) continue;
            
            float baseWeight = prefabWeights.ContainsKey(color) && i < prefabWeights[color].Count ? prefabWeights[color][i] : 1.0f;
            float sizeWeight = CalculateSizeWeight(prefabList[i], colorSizeBias.ContainsKey(color) ? colorSizeBias[color] : 0.5f);
            float finalWeight = baseWeight * sizeWeight;
            
            finalWeights.Add(finalWeight);
            totalWeight += finalWeight;
        }
        
        if (totalWeight <= 0f) return null;
        
        // Chọn prefab dựa trên weight
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        for (int i = 0; i < prefabList.Count; i++)
        {
            if (prefabList[i] == null) continue;
            
            currentWeight += finalWeights[i];
            if (randomValue <= currentWeight)
            {
                return prefabList[i];
            }
        }
        
        return prefabList[0]; // Fallback
    }

    private void AnalyzeMap()
    {
        errorMsg = "";
        detectedColors.Clear();
        colorToLegend.Clear();
        colorToPrefabs.Clear();

        if (inputMap == null)
        {
            errorMsg = "Please assign an input map image.";
            return;
        }

        string path = AssetDatabase.GetAssetPath(inputMap);
        Texture2D readableTex = new Texture2D(inputMap.width, inputMap.height);
        byte[] fileData = File.ReadAllBytes(path);
        readableTex.LoadImage(fileData);

        if (readableTex.width != mapWidth || readableTex.height != mapHeight)
        {
            readableTex = ResizeTexture(readableTex, mapWidth, mapHeight);
        }

        HashSet<Color32> colorSet = new HashSet<Color32>(new Color32Comparer());
        Color32[] pixels = readableTex.GetPixels32();

        foreach (var pixel in pixels)
        {
            colorSet.Add(pixel);
        }

        if (colorSet.Count > maxColorsLimit)
        {
            errorMsg = $"Too many unique colors ({colorSet.Count}). Max allowed is {maxColorsLimit}.";
            return;
        }

        // Warning for high color count
        if (colorSet.Count > 100)
        {
            bool continueAnalysis = EditorUtility.DisplayDialog(
                "High Color Count Warning", 
                $"This map contains {colorSet.Count} unique colors.\n\n" +
                "Processing this many colors may cause performance issues and slow down the tool.\n\n" +
                "Are you sure you want to continue?", 
                "Continue", 
                "Cancel"
            );
            
            if (!continueAnalysis)
            {
                errorMsg = "Analysis cancelled by user.";
                return;
            }
        }

        detectedColors.AddRange(colorSet);
    }

    private Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
    {
        Texture2D result = new Texture2D(newWidth, newHeight, source.format, false);

        for (int y = 0; y < newHeight; y++)
        {
            for (int x = 0; x < newWidth; x++)
            {
                int srcX = Mathf.RoundToInt((float)x / newWidth * source.width);
                int srcY = Mathf.RoundToInt((float)y / newHeight * source.height);
                Color pixel = source.GetPixel(Mathf.Clamp(srcX, 0, source.width - 1), Mathf.Clamp(srcY, 0, source.height - 1));
                result.SetPixel(x, y, pixel);
            }
        }
        result.Apply();
        return result;
    }
    private Vector2Int GetContiguousRegion(Color32[] pixels, Color32 targetColor, int startX, int startY)
    {
        int maxX = startX;
        int maxY = startY;
        while (maxX + 1 < mapWidth && pixels[startY * mapWidth + (maxX + 1)].Equals(targetColor)) maxX++;
        while (maxY + 1 < mapHeight && pixels[(maxY + 1) * mapWidth + startX].Equals(targetColor)) maxY++;
        return new Vector2Int(maxX - startX + 1, maxY - startY + 1);
    }
    private class Color32Comparer : IEqualityComparer<Color32>
    {
        public bool Equals(Color32 a, Color32 b)
        {
            return a.r == b.r && a.g == b.g && a.b == b.b;
        }

        public int GetHashCode(Color32 color)
        {
            return color.r * 1000000 + color.g * 1000 + color.b;
        }
    }

    // --- Preset Functions ---
    private void ResetSettings()
    {
        inputMap = null;
        mapWidth = 100;
        mapHeight = 100;
        randomSeed = 0;
        rootGroupName = "Level";
        maxColorsLimit = 20;
        detectedColors.Clear();
        colorToLegend.Clear();
        colorToPrefabs.Clear();
        prefabOffsetEnabled.Clear();
        offsetFoldouts.Clear();
        offsetSettings.Clear();
        prefabFoldouts.Clear();
        copiedOffsetSetting = null;
        errorMsg = "";
        occupied = null;
        scrollPos = Vector2.zero;
        // Force UI repaint
        Repaint();
    }
    private void SavePreset()
    {
        PresetData preset = new PresetData();
        preset.inputMapPath = inputMap != null ? AssetDatabase.GetAssetPath(inputMap) : "";
        preset.mapWidth = mapWidth;
        preset.mapHeight = mapHeight;
        preset.randomSeed = randomSeed;
        preset.rootGroupName = rootGroupName;
        preset.maxColorsLimit = maxColorsLimit;
        foreach (var color in detectedColors)
        {
            ColorLegendData data = new ColorLegendData();
            data.color = color;
            data.legend = colorToLegend.ContainsKey(color) ? colorToLegend[color] : "";
            if (colorToPrefabs.ContainsKey(color))
            {
                foreach (var prefab in colorToPrefabs[color])
                {
                    data.prefabPaths.Add(prefab != null ? AssetDatabase.GetAssetPath(prefab) : "");
                }
            }
            if (prefabOffsetEnabled.ContainsKey(color))
            {
                data.prefabOffsetEnabled.AddRange(prefabOffsetEnabled[color]);
            }
            if (prefabWeights.ContainsKey(color))
            {
                data.prefabWeights.AddRange(prefabWeights[color]);
            }
            data.sizeBias = colorSizeBias.ContainsKey(color) ? colorSizeBias[color] : 0.5f;
            if (offsetSettings.ContainsKey(color))
            {
                data.offsetSetting = offsetSettings[color];
            }
            preset.colorLegends.Add(data);
        }
        string json = JsonUtility.ToJson(preset, true);
        string path = EditorUtility.SaveFilePanel("Save Preset", Application.dataPath, "LevelPreset.json", "json");
        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Save Preset", "Preset saved successfully!", "OK");
        }
    }
    private void LoadPreset()
    {
        string path = EditorUtility.OpenFilePanel("Load Preset", Application.dataPath, "json");
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;
        string json = File.ReadAllText(path);
        PresetData preset = JsonUtility.FromJson<PresetData>(json);
        
        // Apply preset
        inputMap = !string.IsNullOrEmpty(preset.inputMapPath) ? AssetDatabase.LoadAssetAtPath<Texture2D>(preset.inputMapPath) : null;
        mapWidth = preset.mapWidth;
        mapHeight = preset.mapHeight;
        randomSeed = preset.randomSeed;
        rootGroupName = string.IsNullOrEmpty(preset.rootGroupName) ? "Level" : preset.rootGroupName;
        maxColorsLimit = preset.maxColorsLimit;
        
        // Load preset data
        detectedColors.Clear();
        colorToLegend.Clear();
        colorToPrefabs.Clear();
        prefabOffsetEnabled.Clear();
        prefabWeights.Clear(); // Clear weights
        colorSizeBias.Clear(); // Clear size bias
        offsetSettings.Clear();
        offsetFoldouts.Clear();
        prefabFoldouts.Clear();
        copiedOffsetSetting = null; // Reset copied offset setting
        
        foreach (var data in preset.colorLegends)
        {
            detectedColors.Add(data.color);
            colorToLegend[data.color] = data.legend;
            var prefabList = new List<GameObject>();
            foreach (var prefabPath in data.prefabPaths)
            {
                if (!string.IsNullOrEmpty(prefabPath))
                {
                    var go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    prefabList.Add(go);
                }
                else
                {
                    prefabList.Add(null);
                }
            }
            colorToPrefabs[data.color] = prefabList;
            prefabOffsetEnabled[data.color] = new List<bool>(data.prefabOffsetEnabled);
            prefabWeights[data.color] = new List<float>(data.prefabWeights); // Load weights
            colorSizeBias[data.color] = data.sizeBias; // Load size bias
            offsetSettings[data.color] = data.offsetSetting != null ? data.offsetSetting : new OffsetSetting();
            offsetFoldouts[data.color] = false;
            prefabFoldouts[data.color] = true;
        }
        
        // Check if current map has new colors
        if (inputMap != null)
        {
            string path2 = AssetDatabase.GetAssetPath(inputMap);
            Texture2D readableTex = new Texture2D(inputMap.width, inputMap.height);
            byte[] fileData = File.ReadAllBytes(path2);
            readableTex.LoadImage(fileData);

            if (readableTex.width != mapWidth || readableTex.height != mapHeight)
            {
                readableTex = ResizeTexture(readableTex, mapWidth, mapHeight);
            }

            HashSet<Color32> currentColors = new HashSet<Color32>(new Color32Comparer());
            Color32[] pixels = readableTex.GetPixels32();

            foreach (var pixel in pixels)
            {
                currentColors.Add(pixel);
            }

            // Find new colors and missing colors
            List<Color32> newColors = new List<Color32>();
            List<Color32> missingColors = new List<Color32>();
            
            // Check for new colors in current map
            foreach (var color in currentColors)
            {
                if (!detectedColors.Contains(color))
                {
                    newColors.Add(color);
                }
            }
            
            // Check for missing colors (colors in preset but not in current map)
            foreach (var color in detectedColors)
            {
                if (!currentColors.Contains(color))
                {
                    missingColors.Add(color);
                }
            }

            // Add new colors with default settings
            if (newColors.Count > 0)
            {
                foreach (var newColor in newColors)
                {
                    detectedColors.Add(newColor);
                    colorToLegend[newColor] = "";
                    colorToPrefabs[newColor] = new List<GameObject>();
                    prefabOffsetEnabled[newColor] = new List<bool>();
                    prefabWeights[newColor] = new List<float>(); // Initialize weights for new colors
                    colorSizeBias[newColor] = 0.5f; // Initialize size bias for new colors
                    offsetSettings[newColor] = new OffsetSetting();
                    offsetFoldouts[newColor] = false;
                    prefabFoldouts[newColor] = true;
                }
            }
            
            // Remove missing colors from all dictionaries
            if (missingColors.Count > 0)
            {
                foreach (var missingColor in missingColors)
                {
                    detectedColors.Remove(missingColor);
                    colorToLegend.Remove(missingColor);
                    colorToPrefabs.Remove(missingColor);
                    prefabOffsetEnabled.Remove(missingColor);
                    prefabWeights.Remove(missingColor);
                    colorSizeBias.Remove(missingColor);
                    offsetSettings.Remove(missingColor);
                    offsetFoldouts.Remove(missingColor);
                    prefabFoldouts.Remove(missingColor);
                }
            }
            
            // Show appropriate message
            string message = "Preset loaded successfully!";
            if (newColors.Count > 0 && missingColors.Count > 0)
            {
                message = $"Preset loaded successfully!\nFound {newColors.Count} new color(s) in current map.\nRemoved {missingColors.Count} color(s) that no longer exist in current map.";
            }
            else if (newColors.Count > 0)
            {
                message = $"Preset loaded successfully!\nFound {newColors.Count} new color(s) in current map.";
            }
            else if (missingColors.Count > 0)
            {
                message = $"Preset loaded successfully!\nRemoved {missingColors.Count} color(s) that no longer exist in current map.";
            }
            
            EditorUtility.DisplayDialog("Load Preset", message, "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Load Preset", "Preset loaded successfully!", "OK");
        }
    }
}
#endif
