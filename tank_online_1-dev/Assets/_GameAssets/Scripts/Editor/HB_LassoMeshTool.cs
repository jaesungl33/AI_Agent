using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

public class HB_LassoMeshTool : EditorWindow
{
    private List<Vector3> lassoPoints = new List<Vector3>();
    private List<Vector3> lassoNormals = new List<Vector3>();
    private bool isDrawing = false;
    private bool snapToGrid = false;
    private float gridSize = 0.5f;
    private int subdivisionLevel = 0;
    [SerializeField] private GameObject[] conformColliders;


    [MenuItem("HB Tools/HB Lasso Mesh Tool")]
    public static void ShowWindow()
    {
        GetWindow<HB_LassoMeshTool>("HB Lasso Mesh Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("Lasso Mesh Drawing Tool", EditorStyles.boldLabel);
        snapToGrid = EditorGUILayout.Toggle("Snap to Grid", snapToGrid);

        if (!isDrawing)
        {
            if (GUILayout.Button("Start Drawing"))
            {
                isDrawing = true;
                lassoPoints.Clear();
                lassoNormals.Clear();
                SceneView.duringSceneGui += OnSceneGUI;
            }
        }
        else
        {
            if (GUILayout.Button("Finish & Create Mesh"))
            {
                isDrawing = false;
                SceneView.duringSceneGui -= OnSceneGUI;
                CreateMeshFromLasso();
            }

            if (GUILayout.Button("Cancel"))
            {
                isDrawing = false;
                lassoPoints.Clear();
                lassoNormals.Clear();
                SceneView.duringSceneGui -= OnSceneGUI;
                SceneView.RepaintAll();
            }
        }

        EditorGUILayout.Space(10);
        GUILayout.Label("Selected Mesh Utilities", EditorStyles.boldLabel);
        subdivisionLevel = EditorGUILayout.IntSlider("Subdivision Level", subdivisionLevel, 0, 3);
        if (GUILayout.Button("Triangulate Selected"))
        {
            TriangulateSelectedMesh(subdivisionLevel);
        }

        EditorGUILayout.Space(10);
        GUILayout.Label("Conform To Surface", EditorStyles.boldLabel);
        SerializedObject so = new SerializedObject(this);
        SerializedProperty prop = so.FindProperty("conformColliders");
        EditorGUILayout.PropertyField(prop, new GUIContent("Collider Objects"), true);
        so.ApplyModifiedProperties();
        if (GUILayout.Button("Conform Selected To Surface"))
        {
            ConformSelectedMeshToSurface();
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 point = hit.point;
                if (snapToGrid)
                {
                    point.x = Mathf.Round(point.x / gridSize) * gridSize;
                    point.y = Mathf.Round(point.y / gridSize) * gridSize;
                    point.z = Mathf.Round(point.z / gridSize) * gridSize;
                }
                lassoPoints.Add(point);
                lassoNormals.Add(hit.normal);
                e.Use();
            }
        }

        Handles.color = Color.green;
        for (int i = 0; i < lassoPoints.Count - 1; i++)
        {
            Handles.DrawLine(lassoPoints[i], lassoPoints[i + 1]);
        }
        if (isDrawing && lassoPoints.Count > 1)
        {
            Handles.DrawLine(lassoPoints[lassoPoints.Count - 1], lassoPoints[0]);
        }

        sceneView.Repaint();
    }

    private void CreateMeshFromLasso()
    {
        if (lassoPoints.Count < 3)
        {
            Debug.LogWarning("Need at least 3 points to create a mesh.");
            return;
        }

        Vector3 avgNormal = Vector3.zero;
        foreach (var n in lassoNormals) avgNormal += n;
        avgNormal.Normalize();

        Vector3 axisX = Vector3.Cross(avgNormal, Vector3.up);
        if (axisX.magnitude < 0.01f) axisX = Vector3.Cross(avgNormal, Vector3.right);
        axisX.Normalize();
        Vector3 axisY = Vector3.Cross(avgNormal, axisX);

        Vector2[] points2D = new Vector2[lassoPoints.Count];
        for (int i = 0; i < lassoPoints.Count; i++)
        {
            Vector3 local = lassoPoints[i] - lassoPoints[0];
            points2D[i] = new Vector2(Vector3.Dot(local, axisX), Vector3.Dot(local, axisY));
        }

        Triangulator triangulator = new Triangulator(points2D);
        int[] indices = triangulator.Triangulate();

        Vector3[] vertices = lassoPoints.ToArray();
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        }

        Vector3 n0 = Vector3.Cross(vertices[indices[1]] - vertices[indices[0]], vertices[indices[2]] - vertices[indices[0]]).normalized;
        if (Vector3.Dot(n0, avgNormal) < 0)
        {
            for (int i = 0; i < indices.Length; i += 3)
            {
                (indices[i + 1], indices[i + 2]) = (indices[i + 2], indices[i + 1]);
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        Vector3 center = mesh.bounds.center;
        for (int i = 0; i < vertices.Length; i++) vertices[i] -= center;
        mesh.vertices = vertices;
        mesh.RecalculateBounds();

        GameObject go = new GameObject("LassoMesh");
        go.transform.position = center;
        var mf = go.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        var mr = go.AddComponent<MeshRenderer>();
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        mr.sharedMaterial = new Material(shader);

        Selection.activeGameObject = go;
    }

    private void TriangulateSelectedMesh(int subdivisionLevel)
    {
        GameObject go = Selection.activeGameObject;
        if (go == null) { Debug.LogWarning("No object selected."); return; }

        MeshFilter mf = go.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
        {
            Debug.LogWarning("Selected object does not have a mesh.");
            return;
        }

        Mesh original = mf.sharedMesh;
        Mesh newMesh = Object.Instantiate(original);

        // Ensure triangles are set (some imported meshes may be quads)
        newMesh.triangles = newMesh.triangles;

        if (subdivisionLevel > 0)
        {
            List<Vector3> verts = new List<Vector3>(newMesh.vertices);
            List<int> tris = new List<int>();

            for (int i = 0; i < newMesh.triangles.Length; i += 3)
            {
                Vector3 a = verts[newMesh.triangles[i]];
                Vector3 b = verts[newMesh.triangles[i + 1]];
                Vector3 c = verts[newMesh.triangles[i + 2]];

                Vector3 ab = (a + b) * 0.5f;
                Vector3 bc = (b + c) * 0.5f;
                Vector3 ca = (c + a) * 0.5f;
                int iA = verts.Count; verts.Add(ab);
                int iB = verts.Count; verts.Add(bc);
                int iC = verts.Count; verts.Add(ca);

                int ia = newMesh.triangles[i];
                int ib = newMesh.triangles[i + 1];
                int ic = newMesh.triangles[i + 2];

                tris.AddRange(new int[] { ia, iA, iC });
                tris.AddRange(new int[] { iA, ib, iB });
                tris.AddRange(new int[] { iC, iB, ic });
                tris.AddRange(new int[] { iA, iB, iC });
            }

            newMesh.vertices = verts.ToArray();
            newMesh.triangles = tris.ToArray();
        }

        newMesh.RecalculateNormals();
        newMesh.RecalculateBounds();

        mf.sharedMesh = newMesh;
        Debug.Log("Triangulated and updated mesh with subdivision: " + subdivisionLevel);
    }

    public class Triangulator
    {
        private List<Vector2> m_points;
        public Triangulator(Vector2[] points) => m_points = new List<Vector2>(points);

        public int[] Triangulate()
        {
            List<int> indices = new List<int>();
            int n = m_points.Count;
            if (n < 3) return indices.ToArray();

            int[] V = new int[n];
            if (Area() > 0) for (int v = 0; v < n; v++) V[v] = v;
            else for (int v = 0; v < n; v++) V[v] = (n - 1) - v;

            int nv = n, count = 2 * nv;
            for (int v = nv - 1; nv > 2;)
            {
                if ((count--) <= 0) return indices.ToArray();
                int u = v; if (nv <= u) u = 0;
                v = u + 1; if (nv <= v) v = 0;
                int w = v + 1; if (nv <= w) w = 0;

                if (Snip(u, v, w, nv, V))
                {
                    int a = V[u], b = V[v], c = V[w];
                    indices.Add(a); indices.Add(b); indices.Add(c);
                    for (int s = v, t = v + 1; t < nv; s++, t++) V[s] = V[t];
                    nv--; count = 2 * nv;
                }
            }
            return indices.ToArray();
        }

        private float Area()
        {
            float A = 0;
            for (int p = m_points.Count - 1, q = 0; q < m_points.Count; p = q++)
            {
                Vector2 pval = m_points[p], qval = m_points[q];
                A += (pval.x * qval.y) - (qval.x * pval.y);
            }
            return A * 0.5f;
        }

        private bool Snip(int u, int v, int w, int n, int[] V)
        {
            Vector2 A = m_points[V[u]], B = m_points[V[v]], C = m_points[V[w]];
            if (Mathf.Epsilon > ((B.x - A.x) * (C.y - A.y) - (B.y - A.y) * (C.x - A.x))) return false;
            for (int p = 0; p < n; p++)
            {
                if ((p == u) || (p == v) || (p == w)) continue;
                if (InsideTriangle(A, B, C, m_points[V[p]])) return false;
            }
            return true;
        }

        private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
        {
            float ax = C.x - B.x, ay = C.y - B.y;
            float bx = A.x - C.x, by = A.y - C.y;
            float cx = B.x - A.x, cy = B.y - A.y;
            float apx = P.x - A.x, apy = P.y - A.y;
            float bpx = P.x - B.x, bpy = P.y - B.y;
            float cpx = P.x - C.x, cpy = P.y - C.y;
            float aCrossBP = ax * bpy - ay * bpx;
            float bCrossCP = bx * cpy - by * cpx;
            float cCrossAP = cx * apy - cy * apx;
            return (aCrossBP >= 0f) && (bCrossCP >= 0f) && (cCrossAP >= 0f);
        }
    }

    private void ConformSelectedMeshToSurface()
    {
        GameObject go = Selection.activeGameObject;
        if (go == null) { Debug.LogWarning("No object selected."); return; }

        MeshFilter mf = go.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
        {
            Debug.LogWarning("Selected object does not have a mesh.");
            return;
        }

        if (conformColliders == null || conformColliders.Length == 0)
        {
            Debug.LogWarning("No collider objects assigned.");
            return;
        }

        Mesh mesh = mf.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        Transform tf = go.transform;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldPt = tf.TransformPoint(vertices[i]);
            Vector3 rayDir = -tf.up;
            bool hitFound = false;

            foreach (var colObj in conformColliders)
            {
                if (colObj == null) continue;
                Collider col = colObj.GetComponent<Collider>();
                if (col == null) continue;

                Ray ray = new Ray(worldPt + rayDir * -0.1f, rayDir);
                if (col.Raycast(ray, out RaycastHit hit, 10f))
                {
                    worldPt = hit.point;
                    hitFound = true;
                    break;
                }
            }

            if (hitFound)
            {
                vertices[i] = tf.InverseTransformPoint(worldPt);
            }
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mf.sharedMesh = mesh;

        Debug.Log("Mesh conformed to collider surface.");
    }

}
