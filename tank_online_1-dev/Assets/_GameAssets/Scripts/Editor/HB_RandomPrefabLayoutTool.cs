#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using static HB_RandomPrefabLayoutTool;
using static TreeEditor.TreeGroup;

public class HB_RandomPrefabLayoutTool : EditorWindow
{
    [System.Serializable]
    public class PrefabEntry
    {
        public GameObject prefab;
        public float weight = 1.0f;
    }

    private List<PrefabEntry> prefabList = new List<PrefabEntry>();
    private List<GameObject> obstacleList = new List<GameObject>();

    private int numberPerSurface = 5;
    private bool useTotalCount = false;
    private int totalCount = 20;
    private enum DistributionMode { Equal, Random, AreaBased }
    private DistributionMode distributionMode = DistributionMode.Equal;

    private bool randomizeRotationX = false;
    private float rotationXMin = -10f, rotationXMax = 10f;
    private bool randomizeRotationY = true;
    private float rotationYMin = 0f, rotationYMax = 360f;
    private bool randomizeRotationZ = false;
    private float rotationZMin = -10f, rotationZMax = 10f;

    private bool randomizeScale = false;
    private Vector2 scaleRange = new Vector2(0.8f, 1.2f);

    private float offsetHeight = 0f;
    private bool groupUnderParent = true;
    private string parentName = "RandomPrefabs_Group";

    private bool alignToSurfaceNormal = false;
    private enum AlignMode { PositionOnly, PositionAndRotation }
    private AlignMode alignMode = AlignMode.PositionAndRotation;

    private bool avoidOverlap = false;
    private float minDistance = 1.0f;

    private bool avoidObstacle = false;
    private float obstacleSafeRadius = 0.5f;

    private int seed = 0;
    private int lastUsedSeed = 0;

    [System.Serializable]
    private class SavedPrefabData
    {
        public string path;
        public float weight;
    }

    [System.Serializable]
    private class LayoutToolSettings
    {
        public int numberPerSurface;
        public bool useTotalCount;
        public int totalCount;
        public int distributionMode;

        public bool randomizeRotationX;
        public float rotationXMin, rotationXMax;
        public bool randomizeRotationY;
        public float rotationYMin, rotationYMax;
        public bool randomizeRotationZ;
        public float rotationZMin, rotationZMax;

        public bool randomizeScale;
        public Vector2 scaleRange;

        public float offsetHeight;
        public bool groupUnderParent;
        public string parentName;

        public bool alignToSurfaceNormal;
        public int alignMode;

        public bool avoidOverlap;
        public float minDistance;

        public bool avoidObstacle;
        public float obstacleSafeRadius;

        public int seed;

        public List<SavedPrefabData> prefabs = new List<SavedPrefabData>();
    }

    [MenuItem("HB Tools/HB Random Prefab Layout")]
    public static void ShowWindow()
    {
        GetWindow<HB_RandomPrefabLayoutTool>("HB Random Prefab Layout");
    }

    private void OnGUI()
    {
        GUILayout.Label("Prefabs to Randomize", EditorStyles.boldLabel);

        float totalWeight = prefabList.Sum(p => Mathf.Max(p.weight, 0));

        for (int i = 0; i < prefabList.Count; i++)
        {
            GUILayout.BeginHorizontal();
            prefabList[i].prefab = (GameObject)EditorGUILayout.ObjectField(prefabList[i].prefab, typeof(GameObject), false);
            prefabList[i].weight = EditorGUILayout.FloatField(prefabList[i].weight, GUILayout.Width(50));

            if (totalWeight > 0)
            {
                float probability = (prefabList[i].weight / totalWeight) * 100f;
                GUILayout.Label($"{probability:F1}%", GUILayout.Width(50));
            }
            else
            {
                GUILayout.Label("-", GUILayout.Width(50));
            }

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                prefabList.RemoveAt(i);
                i--;
            }
            GUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add Prefab"))
        {
            prefabList.Add(new PrefabEntry());
        }

        GUILayout.Space(10);

        GUILayout.Label("Settings", EditorStyles.boldLabel);
        useTotalCount = EditorGUILayout.Toggle("Use Total Count", useTotalCount);
        if (useTotalCount)
        {
            totalCount = EditorGUILayout.IntField("Total Count", totalCount);
            distributionMode = (DistributionMode)EditorGUILayout.EnumPopup("Distribution Mode", distributionMode);
        }
        else
        {
            numberPerSurface = EditorGUILayout.IntField("Number per Surface", numberPerSurface);
        }

        GUILayout.Space(5);

        GUILayout.Label("Random Rotation", EditorStyles.boldLabel);
        randomizeRotationX = EditorGUILayout.Toggle("Randomize X Rotation", randomizeRotationX);
        if (randomizeRotationX)
        {
            rotationXMin = EditorGUILayout.FloatField("X Rotation Min", rotationXMin);
            rotationXMax = EditorGUILayout.FloatField("X Rotation Max", rotationXMax);
        }
        randomizeRotationY = EditorGUILayout.Toggle("Randomize Y Rotation", randomizeRotationY);
        if (randomizeRotationY)
        {
            rotationYMin = EditorGUILayout.FloatField("Y Rotation Min", rotationYMin);
            rotationYMax = EditorGUILayout.FloatField("Y Rotation Max", rotationYMax);
        }
        randomizeRotationZ = EditorGUILayout.Toggle("Randomize Z Rotation", randomizeRotationZ);
        if (randomizeRotationZ)
        {
            rotationZMin = EditorGUILayout.FloatField("Z Rotation Min", rotationZMin);
            rotationZMax = EditorGUILayout.FloatField("Z Rotation Max", rotationZMax);
        }

        GUILayout.Space(5);

        randomizeScale = EditorGUILayout.Toggle("Randomize Scale", randomizeScale);
        if (randomizeScale)
            scaleRange = EditorGUILayout.Vector2Field("Scale Range (Min, Max)", scaleRange);

        offsetHeight = EditorGUILayout.FloatField("Offset Height", offsetHeight);

        alignToSurfaceNormal = EditorGUILayout.Toggle("Align to Surface Normal", alignToSurfaceNormal);
        if (alignToSurfaceNormal)
        {
            alignMode = (AlignMode)EditorGUILayout.EnumPopup("Align Mode", alignMode);
        }

        avoidOverlap = EditorGUILayout.Toggle("Avoid Overlap", avoidOverlap);
        if (avoidOverlap)
            minDistance = EditorGUILayout.FloatField("Minimum Distance", minDistance);

        GUILayout.Space(10);

        GUILayout.Label("Obstacle Settings", EditorStyles.boldLabel);
        avoidObstacle = EditorGUILayout.Toggle("Avoid Obstacle Colliders", avoidObstacle);
        if (avoidObstacle)
        {
            obstacleSafeRadius = EditorGUILayout.FloatField("Obstacle Safe Radius", obstacleSafeRadius);

            for (int i = 0; i < obstacleList.Count; i++)
            {
                GUILayout.BeginHorizontal();
                obstacleList[i] = (GameObject)EditorGUILayout.ObjectField(obstacleList[i], typeof(GameObject), true);
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    obstacleList.RemoveAt(i);
                    i--;
                }
                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Obstacle Object"))
            {
                obstacleList.Add(null);
            }
        }

        GUILayout.Space(10);

        GUILayout.Label("Random Seed", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        seed = EditorGUILayout.IntField("Seed (0 = Random)", seed);
        if (GUILayout.Button("Reset", GUILayout.Width(50)))
        {
            seed = 0;
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label($"Last Used Seed: {lastUsedSeed}");
        if (GUILayout.Button("Copy", GUILayout.Width(50)))
        {
            EditorGUIUtility.systemCopyBuffer = lastUsedSeed.ToString();
        }
        if (GUILayout.Button("Paste", GUILayout.Width(50)))
        {
            if (int.TryParse(EditorGUIUtility.systemCopyBuffer, out int pastedSeed))
            {
                seed = pastedSeed;
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Export Settings")) SaveSettingsToJsonFile();
        if (GUILayout.Button("Import Settings")) LoadSettingsFromJsonFile();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        if (GUILayout.Button("Randomize Layout"))
        {
            RandomizePrefabs();
        }
    }

    private void RandomizePrefabs()
    {
        if (prefabList.Count == 0 || prefabList.All(p => p.prefab == null))
        {
            Debug.LogWarning("No prefabs assigned.");
            return;
        }

        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("No surface selected.");
            return;
        }

        // Init Seed
        if (seed == 0)
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
        }
        Random.InitState(seed);
        lastUsedSeed = seed;

        Transform parentGroup = null;
        if (groupUnderParent)
        {
            GameObject groupObj = new GameObject(parentName);
            Undo.RegisterCreatedObjectUndo(groupObj, "Create Parent Group");
            parentGroup = groupObj.transform;
        }

        List<GameObject> surfaces = new List<GameObject>(selectedObjects);
        List<Vector3> spawnedPositions = new List<Vector3>();
        List<Collider> obstacleColliders = new List<Collider>();

        if (avoidObstacle)
        {
            foreach (var obj in obstacleList)
            {
                if (obj == null) continue;
                Collider[] colliders = obj.GetComponentsInChildren<Collider>();
                obstacleColliders.AddRange(colliders);
            }
        }

        Dictionary<GameObject, int> surfaceSpawnCounts = new Dictionary<GameObject, int>();

        if (useTotalCount)
        {
            switch (distributionMode)
            {
                case DistributionMode.Equal:
                    int countPerSurface = Mathf.CeilToInt((float)totalCount / surfaces.Count);
                    foreach (var s in surfaces)
                        surfaceSpawnCounts[s] = countPerSurface;
                    break;

                case DistributionMode.Random:
                    int remaining = totalCount;
                    foreach (var s in surfaces)
                    {
                        int randomCount = Random.Range(0, remaining + 1);
                        surfaceSpawnCounts[s] = randomCount;
                        remaining -= randomCount;
                    }
                    if (remaining > 0)
                        surfaceSpawnCounts[surfaces[Random.Range(0, surfaces.Count)]] += remaining;
                    break;

                case DistributionMode.AreaBased:
                    float totalArea = surfaces.Sum(s => s.GetComponent<Renderer>()?.bounds.size.x * s.GetComponent<Renderer>()?.bounds.size.z ?? 0f);
                    foreach (var s in surfaces)
                    {
                        float area = s.GetComponent<Renderer>()?.bounds.size.x * s.GetComponent<Renderer>()?.bounds.size.z ?? 0f;
                        int spawnCount = Mathf.RoundToInt((area / totalArea) * totalCount);
                        surfaceSpawnCounts[s] = spawnCount;
                    }
                    break;
            }
        }
        else
        {
            foreach (var s in surfaces)
                surfaceSpawnCounts[s] = numberPerSurface;
        }

        int actualSpawned = 0;
        foreach (var surface in surfaces)
        {
            Renderer renderer = surface.GetComponent<Renderer>();
            if (renderer == null) continue;

            Bounds bounds = renderer.bounds;
            int spawnAttempts = surfaceSpawnCounts[surface];
            int attempts = 0;

            while (attempts < spawnAttempts)
            {
                PrefabEntry selectedPrefab = GetRandomWeightedPrefab();
                if (selectedPrefab == null || selectedPrefab.prefab == null) continue;

                Vector3 randomPosition = new Vector3(
                    Random.Range(bounds.min.x, bounds.max.x),
                    bounds.max.y + 1000f,  // Cast from above
                    Random.Range(bounds.min.z, bounds.max.z)
                );

                Vector3 spawnPoint;
                Quaternion baseRotation = Quaternion.identity;

                // Raycast để tìm surface (bắt buộc phải hit)
                Ray ray = new Ray(randomPosition, Vector3.down);
                if (Physics.Raycast(ray, out RaycastHit hitInfo, 2000f))
                {
                    spawnPoint = hitInfo.point + hitInfo.normal * offsetHeight;

                    if (alignToSurfaceNormal)
                    {
                        if (alignMode == AlignMode.PositionAndRotation)
                            baseRotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
                        else
                            baseRotation = Quaternion.identity; // giữ rotation gốc
                    }
                }
                else
                {
                    continue; // Raycast fail → skip lần spawn này
                }

                // Avoid Overlap
                if (avoidOverlap)
                {
                    if (spawnedPositions.Any(pos => (pos - spawnPoint).sqrMagnitude < minDistance * minDistance))
                        continue;
                }

                // Avoid Obstacle
                if (avoidObstacle)
                {
                    if (Physics.CheckSphere(spawnPoint, obstacleSafeRadius, ~0, QueryTriggerInteraction.Ignore))
                    {
                        if (obstacleColliders.Any(collider => collider.bounds.SqrDistance(spawnPoint) < obstacleSafeRadius * obstacleSafeRadius))
                            continue;
                    }
                }

                // Instantiate prefab
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab.prefab);
                instance.transform.position = spawnPoint;

                // Apply rotation
                Vector3 eulerRotation = Vector3.zero;
                if (randomizeRotationX)
                    eulerRotation.x = Random.Range(rotationXMin, rotationXMax);
                if (randomizeRotationY)
                    eulerRotation.y = Random.Range(rotationYMin, rotationYMax);
                if (randomizeRotationZ)
                    eulerRotation.z = Random.Range(rotationZMin, rotationZMax);

                instance.transform.rotation = baseRotation * Quaternion.Euler(eulerRotation);

                // Apply scale
                if (randomizeScale)
                {
                    float randomScale = Random.Range(scaleRange.x, scaleRange.y);
                    instance.transform.localScale = Vector3.one * randomScale;
                }

                if (groupUnderParent && parentGroup != null)
                {
                    instance.transform.SetParent(parentGroup);
                }

                spawnedPositions.Add(spawnPoint);
                Undo.RegisterCreatedObjectUndo(instance, "Random Prefab Layout");
                attempts++;
                actualSpawned++;
            }
        }

        if (actualSpawned < (useTotalCount ? totalCount : numberPerSurface * surfaces.Count))
        {
            EditorUtility.DisplayDialog("Warning", $"Only spawned {actualSpawned} objects due to overlap, obstacle, or raycast failure constraints.", "OK");
        }
    }

    private PrefabEntry GetRandomWeightedPrefab()
    {
        float totalWeight = prefabList.Sum(p => Mathf.Max(p.weight, 0));
        float randomPoint = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var entry in prefabList)
        {
            cumulative += Mathf.Max(entry.weight, 0);
            if (randomPoint <= cumulative)
                return entry;
        }

        return prefabList.LastOrDefault();
    }
    private void SaveSettingsToJsonFile()
    {
        string path = EditorUtility.SaveFilePanel("Save Settings as JSON", Application.dataPath, "HB_RandomPrefabLayout_Settings", "json");
        if (string.IsNullOrEmpty(path)) return;

        LayoutToolSettings settings = new LayoutToolSettings
        {
            numberPerSurface = numberPerSurface,
            useTotalCount = useTotalCount,
            totalCount = totalCount,
            distributionMode = (int)distributionMode,

            randomizeRotationX = randomizeRotationX,
            rotationXMin = rotationXMin,
            rotationXMax = rotationXMax,
            randomizeRotationY = randomizeRotationY,
            rotationYMin = rotationYMin,
            rotationYMax = rotationYMax,
            randomizeRotationZ = randomizeRotationZ,
            rotationZMin = rotationZMin,
            rotationZMax = rotationZMax,

            randomizeScale = randomizeScale,
            scaleRange = scaleRange,

            offsetHeight = offsetHeight,
            groupUnderParent = groupUnderParent,
            parentName = parentName,

            alignToSurfaceNormal = alignToSurfaceNormal,
            alignMode = (int)alignMode,

            avoidOverlap = avoidOverlap,
            minDistance = minDistance,

            avoidObstacle = avoidObstacle,
            obstacleSafeRadius = obstacleSafeRadius,

            seed = seed,

            prefabs = prefabList
                .Where(p => p.prefab != null)
                .Select(p => new SavedPrefabData
                {
                    path = AssetDatabase.GetAssetPath(p.prefab),
                    weight = p.weight
                }).ToList()
        };

        string json = JsonUtility.ToJson(settings, true);
        System.IO.File.WriteAllText(path, json);
        Debug.Log($"[HB_RandomPrefabLayoutTool] Settings exported to: {path}");
    }

    private void LoadSettingsFromJsonFile()
    {
        string path = EditorUtility.OpenFilePanel("Load Settings from JSON", Application.dataPath, "json");
        if (string.IsNullOrEmpty(path)) return;

        string json = System.IO.File.ReadAllText(path);
        LayoutToolSettings settings = JsonUtility.FromJson<LayoutToolSettings>(json);

        numberPerSurface = settings.numberPerSurface;
        useTotalCount = settings.useTotalCount;
        totalCount = settings.totalCount;
        distributionMode = (DistributionMode)settings.distributionMode;

        randomizeRotationX = settings.randomizeRotationX;
        rotationXMin = settings.rotationXMin;
        rotationXMax = settings.rotationXMax;
        randomizeRotationY = settings.randomizeRotationY;
        rotationYMin = settings.rotationYMin;
        rotationYMax = settings.rotationYMax;
        randomizeRotationZ = settings.randomizeRotationZ;
        rotationZMin = settings.rotationZMin;
        rotationZMax = settings.rotationZMax;

        randomizeScale = settings.randomizeScale;
        scaleRange = settings.scaleRange;

        offsetHeight = settings.offsetHeight;
        groupUnderParent = settings.groupUnderParent;
        parentName = settings.parentName;

        alignToSurfaceNormal = settings.alignToSurfaceNormal;
        alignMode = (AlignMode)settings.alignMode;

        avoidOverlap = settings.avoidOverlap;
        minDistance = settings.minDistance;

        avoidObstacle = settings.avoidObstacle;
        obstacleSafeRadius = settings.obstacleSafeRadius;

        seed = settings.seed;

        prefabList.Clear();

        foreach (var entry in settings.prefabs)
        {
            GameObject loadedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(entry.path);
            if (loadedPrefab != null)
            {
                prefabList.Add(new PrefabEntry
                {
                    prefab = loadedPrefab,
                    weight = entry.weight
                });
            }
            else
            {
                Debug.LogWarning($"[HB_RandomPrefabLayoutTool] Missing prefab: {entry.path} (may have been deleted or moved)");
            }
        }

        Repaint();
        Debug.Log($"[HB_RandomPrefabLayoutTool] Settings loaded from: {path}");
    }

}


#endif