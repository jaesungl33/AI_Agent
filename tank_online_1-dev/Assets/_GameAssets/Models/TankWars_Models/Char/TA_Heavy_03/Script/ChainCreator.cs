using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class ChainCreator : MonoBehaviour
{
    [Header("References")]
    public Transform pointA;
    public Transform pointB;
    public GameObject linkPrefab;

    [Header("Chain shape")]
    public bool useDistanceToCalculate = false;
    public int linkCount = 10;
    public float linkSpacing = 0f;

    [Header("Behavior")]
    public bool usePhysics = false;
    public bool autoBuild = false;
    public Vector3 linkLocalForward = Vector3.forward;

    [Header("Joint options (physics)")]
    public bool pinStartToPointA = false;
    public bool pinEndToPointB = true;
    public bool enableCollisionBetweenLinks = true;

    [Header("Editor/Debug")]
    public bool drawGizmos = true;

    private List<Transform> links = new List<Transform>();

    // --- Rebuild internal list when entering Play Mode ---
    private void Awake()
    {
        RefreshLinksList();
    }

    private void OnEnable()
    {
        RefreshLinksList();
    }

    private void RefreshLinksList()
    {
        links.Clear();
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Link_"))
                links.Add(child);
        }
    }

    // === BUILD CHAIN (chỉ tạo 1 lần) ===
    public void BuildChain()
    {
        if (pointA == null || pointB == null || linkPrefab == null)
            return;

        // Làm mới danh sách link hiện có
        RefreshLinksList();

        // Nếu đã có sẵn link trong scene thì không tạo thêm
        if (links.Count > 0)
            return;

        Vector3 start = pointA.position;
        Vector3 end = pointB.position;
        Vector3 dir = (end - start).normalized;

        float dist = Vector3.Distance(start, end);
        float spacing = useDistanceToCalculate ? dist / linkCount : linkSpacing;

        for (int i = 0; i < linkCount; i++)
        {
            GameObject link = Instantiate(linkPrefab, transform);
            link.name = $"Link_{i:D2}";
            link.transform.position = start + dir * spacing * i;
            link.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

            links.Add(link.transform);
        }

        Debug.Log($"[ChainCreator] Chain built with {links.Count} links.");
    }

    // === UPDATE CHAIN (chỉ di chuyển link, không tạo mới) ===
    public void UpdateChainTransform()
    {
        if (pointA == null || pointB == null)
            return;

        if (links == null || links.Count == 0)
        {
            RefreshLinksList();
            if (links.Count == 0)
                return;
        }

        Vector3 start = pointA.position;
        Vector3 end = pointB.position;
        Vector3 dir = (end - start).normalized;
        float dist = Vector3.Distance(start, end);
        float spacing = (useDistanceToCalculate || linkSpacing <= 0f)
            ? dist / (links.Count - 1)
            : linkSpacing;

        for (int i = 0; i < links.Count; i++)
        {
            Transform link = links[i];
            if (link == null) continue;

            link.position = start + dir * spacing * i;
            link.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos || pointA == null || pointB == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pointA.position, pointB.position);
        Gizmos.DrawSphere(pointA.position, 0.05f);
        Gizmos.DrawSphere(pointB.position, 0.05f);
    }

    private void Update()
    {
        if (autoBuild && links.Count == 0)
            BuildChain();

        UpdateChainTransform();
    }
}
