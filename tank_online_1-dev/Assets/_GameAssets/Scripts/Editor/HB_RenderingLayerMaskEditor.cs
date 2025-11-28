using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class RenderingLayerMaskEditor : EditorWindow
{
    private int selectedLayerIndex = 0;
    private string[] renderingLayerNames;

    [MenuItem("Tools/Rendering Layer Mask Editor")]
    public static void ShowWindow()
    {
        GetWindow<RenderingLayerMaskEditor>("Rendering Layer Mask Editor");
    }

    private void OnEnable()
    {
        // Sử dụng tên mặc định do không thể lấy từ GraphicsSettings
        renderingLayerNames = new string[] { "Layer 0", "Layer 1", "Layer 2", "Layer 3", "Layer 4", "Layer 5", "Layer 6", "Layer 7" };
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Select Rendering Layer Mask", EditorStyles.boldLabel);

        // Hiển thị dropdown cho các rendering layers
        selectedLayerIndex = EditorGUILayout.Popup("Rendering Layer Mask", selectedLayerIndex, renderingLayerNames);

        if (GUILayout.Button("Apply to Selected Objects"))
        {
            ApplyRenderingLayerMaskToSelectedObjects(selectedLayerIndex);
        }
    }

    private void ApplyRenderingLayerMaskToSelectedObjects(int layerIndex)
    {
        // Lấy tất cả các đối tượng được chọn
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            Debug.LogWarning("No objects selected.");
            return;
        }

        uint layerMask = (uint)(1 << layerIndex);

        foreach (GameObject obj in selectedObjects)
        {
            ApplyLayerMaskRecursively(obj, layerMask);
        }

        Debug.Log("Rendering Layer Mask updated for selected objects and their children.");
    }

    private void ApplyLayerMaskRecursively(GameObject obj, uint layerMask)
    {
        // Kiểm tra và cập nhật cho tất cả các component hỗ trợ Rendering Layer Mask
        var componentsWithRenderingLayers = obj.GetComponents<Component>();
        foreach (var component in componentsWithRenderingLayers)
        {
            if (component is Renderer renderer)
            {
                renderer.renderingLayerMask = layerMask;
                Debug.Log($"Updated Rendering Layer Mask for Renderer on {obj.name}.");
            }
            else if (component is Light light)
            {
                SerializedObject serializedLight = new SerializedObject(light);
                SerializedProperty renderingLayerMaskProperty = serializedLight.FindProperty("m_RenderingLayerMask");
                if (renderingLayerMaskProperty != null)
                {
                    renderingLayerMaskProperty.intValue = (int)layerMask;
                    serializedLight.ApplyModifiedProperties();
                    Debug.Log($"Updated Rendering Layer Mask for Light on {obj.name}.");
                }
            }
            else if (component is Terrain terrain)
            {
                terrain.renderingLayerMask = (uint)(int)layerMask;
                Debug.Log($"Updated Rendering Layer Mask for Terrain on {obj.name}.");
            }
        }

        // Gọi đệ quy cho tất cả các đối tượng con
        foreach (Transform child in obj.transform)
        {
            ApplyLayerMaskRecursively(child.gameObject, layerMask);
        }
    }
}
