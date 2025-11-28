using UnityEngine;
using System.Collections;

public class TunnelTransparency_1Mat : MonoBehaviour
{
    [SerializeField] private MeshRenderer[] m_Meshes;

    [Header("Player Tag")]
    public string playerTag = "Player";

    [Header("Fade Settings")]
    [Tooltip("Thời gian chuyển giữa rõ ↔ trong suốt (giây).")]
    public float fadeDuration = 0.5f;

    [Tooltip("Giá trị opacity khi rõ (1 = đục hoàn toàn)")]
    public float opaqueOpacity = 1f;

    [Tooltip("Giá trị opacity khi trong suốt (0 = hoàn toàn trong suốt)")]
    public float transparentOpacity = 0.3f;

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
            StartFade(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag) && isTransparent)
            StartFade(false);
    }

    private void StartFade(bool toTransparent)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeOpacityRoutine(toTransparent));
    }

    private IEnumerator FadeOpacityRoutine(bool toTransparent)
    {
        isTransparent = toTransparent;

        float start = toTransparent ? opaqueOpacity : transparentOpacity;
        float end = toTransparent ? transparentOpacity : opaqueOpacity;

        float elapsed = 0f;

        // Clone material để không ảnh hưởng đến asset gốc
        foreach (var mesh in m_Meshes)
            mesh.material = new Material(mesh.material);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float current = Mathf.Lerp(start, end, t);

            foreach (var mesh in m_Meshes)
            {
                var mat = mesh.material;
                if (mat.HasProperty("_Opacity"))
                    mat.SetFloat("_Opacity", current);
            }

            yield return null;
        }

        // Gán giá trị cuối cùng
        foreach (var mesh in m_Meshes)
        {
            var mat = mesh.material;
            if (mat.HasProperty("_Opacity"))
                mat.SetFloat("_Opacity", end);
        }

        fadeRoutine = null;
    }
}
