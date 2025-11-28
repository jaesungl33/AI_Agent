using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;

public class HB_ConvertBillboardTool : EditorWindow
{
    private enum ToolSection
    {
        ToolIntro,
        BillboardCapture,
        FaceToCamera,
        CreatePrefab,
        TerrainTreeConverter,
        ReplaceMultiPrefabs
    }

    private ToolSection selectedSection;
    // Variables for Billboard Capture
    private Camera targetCamera;
    private int resolutionIndex;
    private readonly int[] resolutions = { 256, 512, 1024, 2048, 4096 };
    private DefaultAsset saveFolder;
    private string billboardName = "Billboard";
    private bool createBillboardPrefab = false; // Checkbox for prefab creation
    private bool createSpriteAnimation = false; // Checkbox for sprite creation
    private GameObject billboardModel; // Billboard model slot (GameObject)
    private float billboardMeshScale = 1f; // Billboard mesh scale
    private Shader billboardShader; // Billboard shader slot
    private int spriteLength;
    private AnimationClip selectedAnimationClip;
    private float outputFrameRate = 30f; // Frame rate đầu ra mặc định
    // private string resultMessage = "";

    // Variables for Create Prefab
    private UnityEngine.Object selectedFolder;

    // Variables for Terrain Tree Converter
    private Terrain terrain;

    // Variables for Replace Multi-Prefabs
    private GameObject[] prefabsToReplace = new GameObject[10];
    private int prefabCount = 1;
    private bool replacePosition = true;
    private bool replaceRotation = true;
    private bool replaceScale = true;

    [MenuItem("Tools/HB Convert Billboard Tool")]
    public static void ShowWindow()
    {
        GetWindow<HB_ConvertBillboardTool>("HB Convert Billboard Tool");
    }

    private void OnGUI()
    {
        selectedSection = (ToolSection)EditorGUILayout.EnumPopup("Select Section", selectedSection);
        GUILayout.Space(10);

        switch (selectedSection)
        {
            case ToolSection.ToolIntro:
                DrawToolIntroSection();
                break;
            case ToolSection.BillboardCapture:
                DrawBillboardCaptureSection();
                break;
            case ToolSection.FaceToCamera:
                DrawFaceToCameraSection();
                break;
            case ToolSection.CreatePrefab:
                DrawCreatePrefabSection();
                break;
            case ToolSection.TerrainTreeConverter:
                DrawTerrainTreeConverterSection();
                break;
            case ToolSection.ReplaceMultiPrefabs:
                DrawReplaceMultiPrefabsSection();
                break;
        }
    }
    private void DrawToolIntroSection()
    {
        GUILayout.Label("0. Tool Intro", EditorStyles.boldLabel);
        // Giới thiệu tool và verion
        EditorGUILayout.HelpBox("\nTool dùng để convert các Game Object 3D sang dạng billboard 2D. Bao gồm đối tượng riêng lẻ và cây dùng trong Terrain. Tool có các tính năng theo trình tự 5 bước.\n",
        MessageType.Info);
        EditorGUILayout.HelpBox(
                "\n1. Billboard Capture:\n   - Tạo textures theo view camera.\n   - Tùy chỉnh độ phân giải.\n   - Export định dạng PNG.\n   - Tạo Prefab Billboard.\n\n" +
                "2. Face To Camera:\n   - Xoay đối tượng khớp với hướng camera.\n\n" +
                "3. Create Prefab:\n   - Tạo prefab từ các đối tượng được chọn và lưu vào folder tùy chọn.\n\n" +
                "4. Terrain Tree Converter:\n   - Chuyển đổi các đối tượng cây trong terrain sang Game Object.\n   - Chuyển đổi các đối tượng Game Object trở lại thành dạng cây trong Terrain.\n\n" +
                "5. Replace Multi Prefabs:\n   - Thay thế các prefab được chọn với các prefab mới 1 cách random.\n   - Chọn các giá trị position, rotation, và scale cần chuyển.\n",
                MessageType.Info);
    }
    private void DrawBillboardCaptureSection()
    {
        GUILayout.Label("1. Billboard Capture", EditorStyles.boldLabel);
        // Hướng dẫn chọn các camera
        EditorGUILayout.HelpBox("Chọn Camera dùng để capture Billboard.", MessageType.Info);
        targetCamera = (Camera)EditorGUILayout.ObjectField("Target Camera", targetCamera, typeof(Camera), true);
        // Hướng dẫn chọn gameobject
        EditorGUILayout.HelpBox("Chọn các đối tượng hoặc group trong Hierarchy để capture Billboard.", MessageType.Info);
        resolutionIndex = EditorGUILayout.Popup("Resolution", resolutionIndex, new[] { "256", "512", "1024", "2048", "4096" });
        // Create Billboard Prefab checkbox
        createBillboardPrefab = EditorGUILayout.Toggle("Create Billboard Prefab", createBillboardPrefab);
        if (createBillboardPrefab)
        {
            createSpriteAnimation = false;
            EditorGUILayout.HelpBox("Chọn Billboard object và điền các thông số tùy chỉnh để tạo Billboard prefab", MessageType.Info);
            // Billboard Model slot
            billboardModel = (GameObject)EditorGUILayout.ObjectField("Billboard Model", billboardModel, typeof(GameObject), false);

            // Billboard Mesh Scale
            billboardMeshScale = EditorGUILayout.FloatField("Billboard Mesh Scale", billboardMeshScale);

            // Billboard Shader slot
            billboardShader = (Shader)EditorGUILayout.ObjectField("Billboard Shader", billboardShader, typeof(Shader), false);

            EditorGUILayout.HelpBox("Shader cần có slot Texture2D cho mục Basecolor, với reference tên là _BillboardTexture ", MessageType.Warning);
        }
        createSpriteAnimation = EditorGUILayout.Toggle("Create Sprite Animation", createSpriteAnimation);

        if (createSpriteAnimation)
        {
            createBillboardPrefab = false;
            EditorGUILayout.HelpBox("Updating", MessageType.Info);

            // Add Animation Clip Slot
            selectedAnimationClip = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip", selectedAnimationClip, typeof(AnimationClip), false);

            // Input Frame Rate Output
            outputFrameRate = EditorGUILayout.FloatField("Output Frame Rate", outputFrameRate);

        }

        // Folder để lưu texture
        EditorGUILayout.HelpBox("Chọn thư mục để lưu texture render.", MessageType.Info);
        saveFolder = (DefaultAsset)EditorGUILayout.ObjectField("Save Folder", saveFolder, typeof(DefaultAsset), false);

        // Billboard name
        billboardName = EditorGUILayout.TextField("Billboard Name", billboardName);


        if (GUILayout.Button(createBillboardPrefab ? "Create Billboard" : createSpriteAnimation ? "Create Sprites" : "Render Billboard"))
        {
            RenderBillboard();
        }
    }
    private void RenderBillboard()
    {

        if (targetCamera == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a target camera.", "OK");
            return;
        }

        if (saveFolder == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a save folder.", "OK");
            return;
        }

        string folderPath = AssetDatabase.GetAssetPath(saveFolder);
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            EditorUtility.DisplayDialog("Error", "Selected path is not a valid folder.", "OK");
            return;
        }

        string filePath = Path.Combine(folderPath, billboardName + "_bb.png");

        // Check for existing files
        if (File.Exists(filePath) || File.Exists(Path.Combine(folderPath, billboardName + "bb_M.mat")) || File.Exists(Path.Combine(folderPath, billboardName + "_bb_prefab.prefab")))
        {
            if (!EditorUtility.DisplayDialog("Replace Existing Files", "Files with the same name already exist in the selected folder. Do you want to replace them?", "Yes", "No"))
            {
                return;
            }
        }

        int resolution = resolutions[resolutionIndex];
        RenderTexture renderTexture = new RenderTexture(resolution, resolution, 24);
        Texture2D texWhite = new Texture2D(resolution, resolution, TextureFormat.ARGB32, false);
        Texture2D texBlack = new Texture2D(resolution, resolution, TextureFormat.ARGB32, false);
        Texture2D texTransparent = new Texture2D(resolution, resolution, TextureFormat.ARGB32, false);

        RenderTexture.active = renderTexture;
        targetCamera.targetTexture = renderTexture;


        // Get all selected objects in the hierarchy
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "Please select at least one object in the Hierarchy.", "OK");
            return;
        }

        foreach (GameObject obj in selectedObjects)
        {
            AssignCullingMaskForSelectedHierarchy(obj);
        }

        void AssignCullingMaskForSelectedHierarchy(GameObject obj)
        {
            // Add objects to a temporary layer mask for rendering
            targetCamera.cullingMask |= 1 << obj.layer;
            foreach (Transform child in obj.transform)
            {
                AssignCullingMaskForSelectedHierarchy(child.gameObject);
            }
        }

        void AssignLayerToHierarchy(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                AssignLayerToHierarchy(child.gameObject, layer);
            }
        }

        // Collect lights and skybox objects to always include in the render
        Light[] lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        List<GameObject> alwaysActiveObjects = new List<GameObject>();

        foreach (Light light in lights)
        {
            if (light.gameObject != null)
            {
                alwaysActiveObjects.Add(light.gameObject);
            }
        }

        // Skybox material is handled directly through RenderSettings
        Material skyboxMaterial = RenderSettings.skybox;

        // Temporarily deactivate unselected objects
        GameObject[] allObjects = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        List<GameObject> deactivatedObjects = new List<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (!Array.Exists(selectedObjects, selectedObj => selectedObj == obj || obj.transform.IsChildOf(selectedObj.transform)) &&
                !alwaysActiveObjects.Contains(obj))
            {
                if (obj.activeSelf)
                {
                    obj.SetActive(false);
                    deactivatedObjects.Add(obj);
                }
            }
        }

        /*
        // Create Sprite Animation checkbox
        if (createSpriteAnimation) 
        {
            float lengthInSeconds = selectedAnimationClip.length;
            float originalFrameRate = selectedAnimationClip.frameRate;
            int totalOriginalFrames = Mathf.RoundToInt(lengthInSeconds * originalFrameRate);
            int totalOutputFrames = Mathf.RoundToInt(lengthInSeconds * outputFrameRate);
        }
        */
        // Render with black background
        targetCamera.backgroundColor = Color.black;
        targetCamera.clearFlags = CameraClearFlags.SolidColor;
        targetCamera.Render();
        texBlack.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
        texBlack.Apply();

        // Render with white background
        targetCamera.backgroundColor = Color.white;
        targetCamera.Render();
        texWhite.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
        texWhite.Apply();

        // Reactivate previously deactivated objects
        foreach (GameObject obj in deactivatedObjects)
        {
            obj.SetActive(true);
        }

        // Calculate alpha and color
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                Color colBlack = texBlack.GetPixel(x, y);
                Color colWhite = texWhite.GetPixel(x, y);

                float alpha = colWhite.r - colBlack.r;
                alpha = Mathf.Clamp01(1.0f - alpha);

                Color finalColor = alpha > 0 ? colBlack / alpha : Color.clear;
                finalColor.a = alpha;

                texTransparent.SetPixel(x, y, finalColor);
            }
        }

        texTransparent.Apply();
        byte[] pngData = texTransparent.EncodeToPNG();
        File.WriteAllBytes(filePath, pngData);

        // Refresh asset database to update Unity
        AssetDatabase.Refresh();

        // Load the saved texture as an asset
        Texture2D billboardTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);

        // Create material if shader is provided
        Material billboardMaterial = null;
        if (billboardShader != null)
        {
            string materialPath = Path.Combine(folderPath, billboardName + "_bb_M.mat");
            billboardMaterial = new Material(billboardShader);
            if (billboardMaterial.HasProperty("_BillboardTexture"))
            {
                billboardMaterial.SetTexture("_BillboardTexture", billboardTexture);
                AssetDatabase.CreateAsset(billboardMaterial, materialPath);
                AssetDatabase.SaveAssets();
            }
            else
            {
                Debug.LogWarning("Shader does not have a _BillboardTexture property. Texture assignment skipped.");
            }
        }


        // Create Billboard Prefab checkbox
        createBillboardPrefab = EditorGUILayout.Toggle("Create Billboard Prefab", createBillboardPrefab);
        if (createBillboardPrefab)
        {
            if (billboardModel == null || billboardMaterial == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign both Billboard Model and Billboard Shader.", "OK");
                return;
            }

            // Create an empty parent object
            GameObject parentObject = new GameObject(billboardName + "_bb_prefab");

            // Instantiate the billboard model and set its scale
            GameObject billboardObject = Instantiate(billboardModel, parentObject.transform);
            billboardObject.name = billboardModel.name;
            billboardObject.transform.localScale = Vector3.one * billboardMeshScale;

            // Apply the material to the mesh renderer
            MeshRenderer meshRenderer = billboardObject.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = billboardObject.AddComponent<MeshRenderer>();
            }
            meshRenderer.sharedMaterial = billboardMaterial;

            // Align the billboard object to face the camera
            billboardObject.transform.position = Vector3.zero;
            billboardObject.transform.LookAt(targetCamera.transform.position);
            billboardObject.transform.Rotate(0, 180f, 0); // Flip to face correctly

            // Save as prefab
            string prefabPath = Path.Combine(folderPath, billboardName + "_bb_prefab.prefab");
            PrefabUtility.SaveAsPrefabAsset(parentObject, prefabPath);

            // Instantiate the prefab in the scene
            GameObject instantiatedPrefab = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath));

            // Destroy the temporary parent object used to create the prefab
            DestroyImmediate(parentObject);

            EditorUtility.DisplayDialog("Success", "Billboard prefab saved to " + prefabPath, "OK");
        }

        RenderTexture.active = null;
        targetCamera.targetTexture = null;
        DestroyImmediate(texBlack);
        DestroyImmediate(texWhite);
        DestroyImmediate(texTransparent);

        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success", "Billboard rendered with alpha and saved to " + filePath, "OK");
    }

    private void DrawFaceToCameraSection()
    {
        GUILayout.Label("2. Face To Camera", EditorStyles.boldLabel);
        // Hướng dẫn chọn Camera
        EditorGUILayout.HelpBox("Chọn Camera mục tiêu trong Scene", MessageType.Info);
        targetCamera = (Camera)EditorGUILayout.ObjectField("Target Camera", targetCamera, typeof(Camera), true);
        // Hướng dẫn chọn Game Object 
        EditorGUILayout.HelpBox("Chọn các Game Object cần xoay theo hướng camera.", MessageType.Info);
        if (GUILayout.Button("Face To Camera"))
        {
            FaceSelectedObjectsToCamera();
        }
    }

    private void FaceSelectedObjectsToCamera()
    {
        if (targetCamera == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a target camera.", "OK");
            return;
        }

        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "No objects selected.", "OK");
            return;
        }

        foreach (GameObject obj in selectedObjects)
        {
            Undo.RecordObject(obj.transform, "Face To Camera");
            obj.transform.forward = targetCamera.transform.forward;
        }

        EditorUtility.DisplayDialog("Success", "Objects aligned to camera.", "OK");
    }

    private void DrawCreatePrefabSection()
    {
        GUILayout.Label("3. Create Prefab", EditorStyles.boldLabel);
        // Hướng dẫn chọn folder lưu Prefab
        EditorGUILayout.HelpBox("Chọn folder lưu Prefab sau khi tạo ra.", MessageType.Info);
        selectedFolder = EditorGUILayout.ObjectField("Save Folder", selectedFolder, typeof(DefaultAsset), false);
        // Hướng dẫn chọn đối tượng tạo Prefab
        EditorGUILayout.HelpBox("Chọn các Game Object cần tạo prefab trong Hierarchy và bấm nút Create.", MessageType.Info);
        if (GUILayout.Button("Create Prefab"))
        {
            CreatePrefab();
        }
    }

    private void CreatePrefab()
    {
        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogWarning("No objects selected to create a prefab.");
            return;
        }

        if (selectedFolder == null)
        {
            Debug.LogWarning("Please select a folder to save the prefab.");
            return;
        }

        string folderPath = AssetDatabase.GetAssetPath(selectedFolder);
        GameObject parentGroup = new GameObject($"{Selection.gameObjects[0].name}_prefab");

        foreach (GameObject obj in Selection.gameObjects)
        {
            Undo.SetTransformParent(obj.transform, parentGroup.transform, "Group for Prefab");
        }

        string prefabPath = Path.Combine(folderPath, parentGroup.name + ".prefab");
        PrefabUtility.SaveAsPrefabAsset(parentGroup, prefabPath);
        DestroyImmediate(parentGroup);

        Debug.Log("Prefab created at: " + prefabPath);
    }

    private void DrawTerrainTreeConverterSection()
    {
        GUILayout.Label("4. Terrain Tree Converter", EditorStyles.boldLabel);

        // Hướng dẫn chọn Terrain
        EditorGUILayout.HelpBox("Chọn Terrain cần convert cây.", MessageType.Info);

        // Slot để kéo Terrain
        terrain = (Terrain)EditorGUILayout.ObjectField("Terrain", terrain, typeof(Terrain), true);

        if (terrain == null)
        {
            EditorGUILayout.HelpBox("Vui lòng kéo Terrain vào slot trên.", MessageType.Warning);
            return;
        }

        // Nút chuyển đổi cây từ Terrain thành GameObject
        if (GUILayout.Button("Trees to Game Object"))
        {
            ConvertTreesToGameObjects();
        }

        // Hướng dẫn cho nút convert Trees to Terrain Trees
        EditorGUILayout.HelpBox("Chọn group tổng chứa cây trong Hierarchy để convert lại vào Terrain.", MessageType.Info);

        // Nút chuyển đổi từ GameObject trở lại Terrain
        if (GUILayout.Button("Trees to Terrain Trees"))
        {
            ConvertSelectedGroupToTerrain();
        }
    }
    private void ConvertTreesToGameObjects()
    {
        if (terrain == null)
        {
            Debug.LogError("Terrain không hợp lệ.");
            return;
        }

        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPosition = terrain.transform.position;

        // Tạo group chứa tất cả cây
        string groupName = terrain.name + "_Converted";
        GameObject parentObject = new GameObject(groupName);

        Dictionary<string, GameObject> prefabGroups = new Dictionary<string, GameObject>();

        foreach (TreeInstance treeInstance in terrainData.treeInstances)
        {
            // Vị trí cây
            Vector3 worldPosition = Vector3.Scale(treeInstance.position, terrainData.size) + terrainPosition;

            // Lấy prototype index và prefab tương ứng
            int prototypeIndex = treeInstance.prototypeIndex;
            GameObject treePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                AssetDatabase.GetAssetPath(terrainData.treePrototypes[prototypeIndex].prefab)
            );

            if (treePrefab == null)
            {
                Debug.LogWarning($"Prefab không tìm thấy cho cây có index {prototypeIndex}. Bỏ qua cây này.");
                continue;
            }

            // Tạo hoặc sử dụng group nhỏ cho prefab này
            string prefabGroupName = treePrefab.name + "_grp";
            if (!prefabGroups.ContainsKey(prefabGroupName))
            {
                GameObject group = new GameObject(prefabGroupName);
                group.transform.SetParent(parentObject.transform);
                prefabGroups[prefabGroupName] = group;
            }

            GameObject treeObject = PrefabUtility.InstantiatePrefab(treePrefab) as GameObject;

            // Thiết lập vị trí, scale, rotation
            treeObject.transform.position = worldPosition;
            treeObject.transform.localScale = new Vector3(treeInstance.widthScale, treeInstance.heightScale, treeInstance.widthScale);
            treeObject.transform.rotation = Quaternion.AngleAxis(Mathf.Rad2Deg * treeInstance.rotation, Vector3.up);
            treeObject.transform.SetParent(prefabGroups[prefabGroupName].transform);
        }

        terrainData.treeInstances = new TreeInstance[0];
        terrain.Flush();

        Debug.Log("Chuyển đổi cây từ Terrain thành GameObject hoàn tất.");
    }

    private void ConvertSelectedGroupToTerrain()
    {
        if (terrain == null)
        {
            Debug.LogError("Terrain không hợp lệ.");
            return;
        }

        TerrainData terrainData = terrain.terrainData;
        List<TreeInstance> newTreeInstances = new List<TreeInstance>();

        GameObject selectedGroup = Selection.activeGameObject;
        if (selectedGroup == null || selectedGroup.transform.childCount == 0)
        {
            Debug.LogError("Vui lòng chọn group chứa các cây trong Hierarchy.");
            return;
        }

        foreach (Transform childGroup in selectedGroup.transform)
        {
            foreach (Transform child in childGroup)
            {
                Vector3 localPosition = child.position - terrain.transform.position;
                Vector3 normalizedPosition = new Vector3(
                    localPosition.x / terrainData.size.x,
                    localPosition.y / terrainData.size.y,
                    localPosition.z / terrainData.size.z
                );

                GameObject treePrefab = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
                int prototypeIndex = FindPrototypeIndex(terrainData, treePrefab);

                if (prototypeIndex == -1)
                {
                    Debug.LogWarning($"Không tìm thấy prototype cho {child.name}. Bỏ qua.");
                    continue;
                }

                TreeInstance treeInstance = new TreeInstance
                {
                    position = normalizedPosition,
                    prototypeIndex = prototypeIndex,
                    widthScale = child.localScale.x,
                    heightScale = child.localScale.y,
                    rotation = Mathf.Deg2Rad * child.eulerAngles.y,
                    color = Color.white,
                    lightmapColor = Color.white
                };

                newTreeInstances.Add(treeInstance);
            }
        }

        terrainData.treeInstances = newTreeInstances.ToArray();
        terrain.Flush();

        DestroyImmediate(selectedGroup);

        Debug.Log("Đã chuyển đổi GameObject thành cây trên Terrain và xóa group.");
    }

    private int FindPrototypeIndex(TerrainData terrainData, GameObject prefab)
    {
        for (int i = 0; i < terrainData.treePrototypes.Length; i++)
        {
            if (terrainData.treePrototypes[i].prefab == prefab)
            {
                return i;
            }
        }
        return -1;
    }
    private void DrawReplaceMultiPrefabsSection()
    {
        GUILayout.Label("5. Replace Multi Prefabs", EditorStyles.boldLabel);
        // Hướng dẫn phần replace
        EditorGUILayout.HelpBox("Chọn Prefab mới cần thay thế. Nếu số lượng > 1, kết quả sẽ random thay thế theo tỷ lệ cân bằng.", MessageType.Info);
        prefabCount = EditorGUILayout.IntSlider("Number of Prefabs:", prefabCount, 1, 10);
        for (int i = 0; i < prefabCount; i++)
        {
            prefabsToReplace[i] = (GameObject)EditorGUILayout.ObjectField($"Prefab {i + 1}", prefabsToReplace[i], typeof(GameObject), false);
        }
        // Hướng dẫn chọn các thuộc tính
        EditorGUILayout.HelpBox("Chọn các thuộc tính cần replace.", MessageType.Info);
        replacePosition = EditorGUILayout.Toggle("Replace Position", replacePosition);
        replaceRotation = EditorGUILayout.Toggle("Replace Rotation", replaceRotation);
        replaceScale = EditorGUILayout.Toggle("Replace Scale", replaceScale);
        // Hướng dẫn chọn các đối tượng cần replace
        EditorGUILayout.HelpBox("Chọn các đối tượng cần replace trong Hierarchy.", MessageType.Info);
        if (GUILayout.Button("Replace Selected Objects"))
        {
            ReplaceSelectedObjects();
        }
    }

    private void ReplaceSelectedObjects()
    {
        // Validate prefabs
        bool validPrefabs = false;
        for (int i = 0; i < prefabCount; i++)
        {
            if (prefabsToReplace[i] != null)
            {
                validPrefabs = true;
                break;
            }
        }

        if (!validPrefabs)
        {
            Debug.LogError("Please assign at least one prefab to replace.");
            return;
        }

        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogError("No GameObjects selected in the Hierarchy.");
            return;
        }

        foreach (GameObject selectedObject in selectedObjects)
        {
            // Store original transform properties
            Vector3 originalPosition = selectedObject.transform.position;
            Quaternion originalRotation = selectedObject.transform.rotation;
            Vector3 originalScale = selectedObject.transform.localScale;

            // Randomly select a prefab
            GameObject prefabToUse = null;
            while (prefabToUse == null)
            {
                int randomIndex = UnityEngine.Random.Range(0, prefabCount);
                prefabToUse = prefabsToReplace[randomIndex];
            }

            // Instantiate the new prefab
            GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(prefabToUse);

            if (newObject == null)
            {
                Debug.LogError("Failed to instantiate prefab. Make sure it is a valid prefab.");
                continue;
            }

            // Replace transform properties based on user selection
            if (replacePosition)
            {
                newObject.transform.position = originalPosition;
            }

            if (replaceRotation)
            {
                newObject.transform.rotation = originalRotation;
            }

            if (replaceScale)
            {
                newObject.transform.localScale = originalScale;
            }

            // Parent the new object to the original object's parent
            newObject.transform.SetParent(selectedObject.transform.parent);

            // Destroy the original object
            DestroyImmediate(selectedObject);
        }

        Debug.Log("Prefab replacement complete.");
    }
}
