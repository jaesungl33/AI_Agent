using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.IO;

public class MeshRendererSettingsTool : EditorWindow
{
    private DefaultAsset targetFolder;

    // MeshRenderer settings
    private ShadowCastingMode castShadows = ShadowCastingMode.On;
    private bool receiveShadows = true;
    private bool contributeGI = true;
    private ReceiveGI receiveGI = ReceiveGI.Lightmaps;

    private LightProbeUsage lightProbes = LightProbeUsage.BlendProbes;
    private Transform anchorOverride;

    [MenuItem("HB Tools/MeshRenderer Settings Tool")]
    public static void ShowWindow()
    {
        GetWindow<MeshRendererSettingsTool>("MeshRenderer Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("Mesh Renderer Settings Tool", EditorStyles.boldLabel);

        GUILayout.Space(5);
        targetFolder = (DefaultAsset)EditorGUILayout.ObjectField("Target Folder", targetFolder, typeof(DefaultAsset), false);

        GUILayout.Space(10);
        GUILayout.Label("Lighting", EditorStyles.boldLabel);
        castShadows = (ShadowCastingMode)EditorGUILayout.EnumPopup("Cast Shadows", castShadows);
        receiveShadows = EditorGUILayout.Toggle("Receive Shadows", receiveShadows);
        contributeGI = EditorGUILayout.Toggle("Contribute Global Illumination", contributeGI);
        receiveGI = (ReceiveGI)EditorGUILayout.EnumPopup("Receive Global Illumination", receiveGI);

        GUILayout.Space(10);
        GUILayout.Label("Probes", EditorStyles.boldLabel);
        lightProbes = (LightProbeUsage)EditorGUILayout.EnumPopup("Light Probes", lightProbes);
        anchorOverride = (Transform)EditorGUILayout.ObjectField("Anchor Override", anchorOverride, typeof(Transform), true);

        GUILayout.Space(20);
        if (GUILayout.Button("Apply All"))
        {
            ApplySettings();
        }
    }

    private void ApplySettings()
    {
        if (targetFolder == null)
        {
            Debug.LogError("Please assign a target folder.");
            return;
        }

        string folderPath = AssetDatabase.GetAssetPath(targetFolder);
        string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { folderPath });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

        // bỏ qua nếu không phải .prefab
        if (!path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase))
         continue;

GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
if (prefabRoot == null) continue;


            MeshRenderer[] renderers = prefabRoot.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var renderer in renderers)
            {
                Undo.RecordObject(renderer, "Apply MeshRenderer Settings");

                renderer.shadowCastingMode = castShadows;
                renderer.receiveShadows = receiveShadows;
                renderer.receiveGI = receiveGI;

                renderer.lightProbeUsage = lightProbes;
                renderer.probeAnchor = anchorOverride;

                // Contribute GI flag
                GameObjectUtility.SetStaticEditorFlags(renderer.gameObject,
                    contributeGI ? StaticEditorFlags.ContributeGI : 0);
            }

            // lưu lại prefab asset
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Applied MeshRenderer settings to all prefabs in folder: " + folderPath);
    }
}
