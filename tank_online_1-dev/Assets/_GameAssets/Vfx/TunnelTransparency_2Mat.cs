using UnityEngine;
using System.Collections;

public class TunnelTransparency_2Mat : MonoBehaviour
{
    [SerializeField] private MeshRenderer[] m_Meshes;

    [Header("Player Tag")]
    public string playerTag = "Player";

    [Header("Materials")]
    [Tooltip("Material mặc định (đục).")]
    public Material opaqueMaterial;
    [Tooltip("Material trong suốt (xuyên thấu).")]
    public Material transparentMaterial;

    [Header("Fade Settings")]
    [Tooltip("Thời gian chuyển giữa 2 material (giây).")]
    public float fadeDuration = 0.5f;

    private bool isTransparent = false;
    private Coroutine fadeRoutine;

    void Start()
    {
        // Đảm bảo collider là trigger
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning("⚠ Collider chưa bật IsTrigger, đã bật tự động.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && !isTransparent)
        {
            StartFade(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag) && isTransparent)
        {
            StartFade(false);
        }
    }

    private void StartFade(bool toTransparent)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeRoutine(toTransparent));
    }

    private IEnumerator FadeRoutine(bool toTransparent)
    {
        isTransparent = toTransparent;

        // 1️⃣ Lấy material ban đầu và gán material đích
        Material startMat = toTransparent ? opaqueMaterial : transparentMaterial;
        Material endMat = toTransparent ? transparentMaterial : opaqueMaterial;

        // Clone để không làm hỏng global material
        Material[] startInstances = new Material[m_Meshes.Length];
        for (int i = 0; i < m_Meshes.Length; i++)
        {
            startInstances[i] = new Material(startMat);
            m_Meshes[i].material = startInstances[i];
        }

        // 2️⃣ Thực hiện fade mượt (lerp màu giữa 2 material)
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            foreach (var mesh in m_Meshes)
            {
                foreach (var mat in mesh.materials)
                {
                    if (startMat.HasProperty("_Color") && endMat.HasProperty("_Color"))
                    {
                        Color c1 = startMat.color;
                        Color c2 = endMat.color;
                        mat.color = Color.Lerp(c1, c2, t);
                    }

                    // Nếu material có HDR hoặc Emission
                    if (startMat.HasProperty("_EmissionColor") && endMat.HasProperty("_EmissionColor"))
                    {
                        Color e1 = startMat.GetColor("_EmissionColor");
                        Color e2 = endMat.GetColor("_EmissionColor");
                        mat.SetColor("_EmissionColor", Color.Lerp(e1, e2, t));
                    }
                }
            }

            yield return null;
        }

        // 3️⃣ Kết thúc: gán hẳn material đích
        foreach (var mesh in m_Meshes)
        {
            var mats = mesh.materials;
            for (int j = 0; j < mats.Length; j++)
                mats[j] = endMat;
            mesh.materials = mats;
        }

        fadeRoutine = null;
    }
}
