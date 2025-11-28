#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShortcutToolsWindow : EditorWindow
{
    private DefaultAsset selectedFolder;

    private Vector3 copiedPosition;
    private Quaternion copiedRotation;
    private Vector3 copiedScale;
    private bool hasCopiedTransform = false;

    [MenuItem("HB Tools/HB Shortcut Tools")]
    public static void ShowWindow()
    {
        GetWindow<ShortcutToolsWindow>("HB Shortcut Tools");
    }

    private void OnGUI()
    {
        // Visibility Group
        GUILayout.Label("Visibility", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Hide/Unhide Selected"))
        {
            ToggleSceneVisibility();
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Toggle Active State"))
        {
            ToggleActiveState();
        }

        if (GUILayout.Button("Toggle Mesh Renderer"))
        {
            ToggleMeshRendererInHierarchy();
        }

        // Grouping Group
        GUILayout.Space(10);
        GUILayout.Label("Grouping", EditorStyles.boldLabel);

        if (GUILayout.Button("Create Empty Parent"))
        {
            CreateEmptyParent();
        }

        if (GUILayout.Button("Create Empty"))
        {
            CreateEmpty();
        }

        if (GUILayout.Button("Clear Parent"))
        {
            ClearParent();
        }

        // Transform Edit Group
        GUILayout.Space(10);
        GUILayout.Label("Transform Edit", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Copy"))
        {
            CopyTransform();
        }
        if (GUILayout.Button("Paste"))
        {
            PasteTransform();
        }
        GUILayout.EndHorizontal();

        // Selection Group
        GUILayout.Space(10);
        GUILayout.Label("Selection", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Select Parent"))
        {
            SelectParent();
        }
        if (GUILayout.Button("Select Children"))
        {
            SelectChildren();
        }
        GUILayout.EndHorizontal();

        // Prefab Group
        GUILayout.Space(10);
        GUILayout.Label("Prefab", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Save Folder:", GUILayout.Width(100));
        selectedFolder = (DefaultAsset)EditorGUILayout.ObjectField(selectedFolder, typeof(DefaultAsset), false);
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Create Prefab"))
        {
            CreatePrefab();
        }
    }

    private void ToggleSceneVisibility()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            ToggleVisibilityRecursive(obj);
        }
    }

    private void ToggleVisibilityRecursive(GameObject obj)
    {
        bool isVisible = !SceneVisibilityManager.instance.IsHidden(obj);
        if (isVisible)
        {
            SceneVisibilityManager.instance.Hide(obj, false);
        }
        else
        {
            SceneVisibilityManager.instance.Show(obj, false);
        }

        foreach (Transform child in obj.transform)
        {
            ToggleVisibilityRecursive(child.gameObject);
        }
    }

    private void ToggleActiveState()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            obj.SetActive(!obj.activeSelf);
        }
    }

    private void ToggleMeshRendererInHierarchy()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            var renderers = obj.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var renderer in renderers)
            {
                renderer.enabled = !renderer.enabled;
            }
        }
    }

    private void CreateEmptyParent()
    {
        if (Selection.gameObjects.Length > 0)
        {
            Transform rootParent = Selection.gameObjects[0].transform.parent;

            // Lấy tên từ đối tượng đầu tiên
            string parentName = Selection.gameObjects[0].name + "_Grp";
            GameObject parent = new GameObject(parentName);
            Undo.RegisterCreatedObjectUndo(parent, "Create Empty Parent");

            if (rootParent != null)
            {
                parent.transform.SetParent(rootParent, false);
            }

            foreach (GameObject obj in Selection.gameObjects)
            {
                Undo.SetTransformParent(obj.transform, parent.transform, "Set Parent");
            }

            Selection.activeGameObject = parent;
        }
        else
        {
            Debug.LogWarning("No objects selected to group!");
        }
    }

    private void CreateEmpty()
    {
        GameObject emptyGroup = new GameObject("New Empty");
        Undo.RegisterCreatedObjectUndo(emptyGroup, "Create Empty");

        if (Selection.activeGameObject != null)
        {
            // Nếu có đối tượng đang được chọn, tạo group dưới đối tượng đó
            Transform parentTransform = Selection.activeGameObject.transform;
            emptyGroup.transform.SetParent(parentTransform, false);
        }
        else
        {
            // Nếu không có đối tượng được chọn, tạo group trực tiếp trong scene hiện tại
            Scene activeScene = SceneManager.GetActiveScene();
            Debug.Log($"No object selected. Creating group in active scene: {activeScene.name}");
        }

        Selection.activeGameObject = emptyGroup;
    }

    private void ClearParent()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            Undo.SetTransformParent(obj.transform, null, "Clear Parent");
        }
    }

    private void CopyTransform()
    {
        if (Selection.activeGameObject != null)
        {
            Transform selectedTransform = Selection.activeGameObject.transform;
            copiedPosition = selectedTransform.position;
            copiedRotation = selectedTransform.rotation;
            copiedScale = selectedTransform.lossyScale;
            hasCopiedTransform = true;
            Debug.Log("Transform copied!");
        }
        else
        {
            Debug.LogWarning("No object selected to copy transform!");
        }
    }

    private void PasteTransform()
    {
        if (hasCopiedTransform)
        {
            foreach (GameObject obj in Selection.gameObjects)
            {
                Transform objTransform = obj.transform;
                Undo.RecordObject(objTransform, "Paste Transform");
                objTransform.position = copiedPosition;
                objTransform.rotation = copiedRotation;

                Vector3 parentScale = objTransform.parent ? objTransform.parent.lossyScale : Vector3.one;
                objTransform.localScale = new Vector3(
                    copiedScale.x / parentScale.x,
                    copiedScale.y / parentScale.y,
                    copiedScale.z / parentScale.z
                );
            }

            Debug.Log("Transform pasted!");
        }
        else
        {
            Debug.LogWarning("No transform copied to paste!");
        }
    }

    private void SelectParent()
    {
        if (Selection.gameObjects.Length > 0)
        {
            GameObject[] selectedParents = Selection.gameObjects
                .Select(obj => obj.transform.parent?.gameObject)
                .Where(parent => parent != null)
                .Distinct()
                .ToArray();

            if (selectedParents.Length > 0)
            {
                Selection.objects = selectedParents;
            }
            else
            {
                Debug.LogWarning("No parent objects found for the selection!");
            }
        }
        else
        {
            Debug.LogWarning("No objects selected to find parents!");
        }
    }

    private void SelectChildren()
    {
        if (Selection.gameObjects.Length > 0)
        {
            GameObject[] selectedChildren = Selection.gameObjects
                .SelectMany(obj => obj.transform.Cast<Transform>().Select(child => child.gameObject))
                .Distinct()
                .ToArray();

            if (selectedChildren.Length > 0)
            {
                Selection.objects = selectedChildren;
            }
            else
            {
                Debug.LogWarning("No child objects found for the selection!");
            }
        }
        else
        {
            Debug.LogWarning("No objects selected to find children!");
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
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogWarning("Selected asset is not a valid folder.");
            return;
        }

        GameObject parentGroup = new GameObject($"{Selection.gameObjects[0].name}_prefab");
        parentGroup.transform.position = Vector3.zero;

        foreach (GameObject obj in Selection.gameObjects)
        {
            Undo.SetTransformParent(obj.transform, parentGroup.transform, "Group for Prefab");
        }

        string prefabPath = $"{folderPath}/{parentGroup.name}.prefab";
        PrefabUtility.SaveAsPrefabAssetAndConnect(parentGroup, prefabPath, InteractionMode.UserAction);

        Debug.Log($"Prefab created and saved to {prefabPath}");

        DestroyImmediate(parentGroup);
    }
}
#endif