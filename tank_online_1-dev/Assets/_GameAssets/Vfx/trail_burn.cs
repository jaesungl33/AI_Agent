using UnityEngine;

public class WheelSkidFlat : MonoBehaviour
{
    [Header("Wheel Settings")]
    public Transform wheel;
    public GameObject skidPrefab;

    [Header("Line Settings")]
    public float skidWidth = 0.25f;
    public float lifeTime = 5f;
    public float fadeInTime = 0.3f;
    public float fadeOutTime = 1f;

    [Header("Ground Detection")]
    public LayerMask groundLayer;
    public float minMoveDistance = 0.01f;

    [Header("Texture Settings")]
    public float textureUnitLength = 3f;
    public float textureRepeatMultiplier = 1f;
    public bool useRoundedRepeat = false;

    [Header("Fade Curve")]
    public AnimationCurve fadeOutCurve = AnimationCurve.Linear(0, 1, 1, 0);

    private LineRenderer line;
    private int pointIndex = 0;
    private float fadeTimer = 0f;
    private Vector3 lastPos;

    private Vector2 defaultTiling = Vector2.one;
    private Vector2 defaultOffset = Vector2.zero;

    void Start()
    {
        lastPos = transform.position;
    }

    void Update()
    {
        float moveDistance = Vector3.Distance(transform.position, lastPos);

        if (moveDistance > minMoveDistance)
        {
            if (line == null)
                CreateNewLine();

            UpdateLine();
        }

        UpdateFade();

        lastPos = transform.position;
    }

    void CreateNewLine()
    {
        GameObject skidObj = Instantiate(skidPrefab);
        line = skidObj.GetComponent<LineRenderer>();

        line.positionCount = 0;
        line.startWidth = skidWidth;
        line.endWidth = skidWidth;

        line.alignment = LineAlignment.TransformZ;
        line.transform.rotation = Quaternion.LookRotation(Vector3.up);

        SetLineAlpha(0f);

        pointIndex = 0;
        fadeTimer = 0f;

        if (line.material != null)
        {
            defaultTiling = line.material.mainTextureScale;
            defaultOffset = line.material.mainTextureOffset;
        }

        Destroy(skidObj, lifeTime);
    }

    void UpdateLine()
    {
        if (line == null || wheel == null) return;

        Vector3 pos;

        // Raycast xuống đất
        if (Physics.Raycast(wheel.position, Vector3.down, out RaycastHit hit, 2f, groundLayer))
        {
            pos = hit.point + Vector3.up * 0.01f;
        }
        else
        {
            pos = wheel.position;
        }

        // Thêm point mới
        line.positionCount = pointIndex + 1;
        line.SetPosition(pointIndex, pos);
        pointIndex++;

        // Tính tổng chiều dài line
        float lineLength = 0f;
        for (int i = 1; i < line.positionCount; i++)
        {
            lineLength += Vector3.Distance(line.GetPosition(i - 1), line.GetPosition(i));
        }

        // Auto repeat
        if (line.material != null && textureUnitLength > 0f)
        {
            float repeatCount = (lineLength / textureUnitLength) * textureRepeatMultiplier;

            if (useRoundedRepeat)
                repeatCount = Mathf.Max(1, Mathf.RoundToInt(repeatCount));
            else
                repeatCount = Mathf.Max(1f, repeatCount);  // ✅ đảm bảo không bao giờ < 1

            Vector2 newTiling = new Vector2(repeatCount, defaultTiling.y);
            line.material.mainTextureScale = newTiling;
            line.material.mainTextureOffset = defaultOffset;
        }
    }

    void UpdateFade()
    {
        if (line == null) return;

        fadeTimer += Time.deltaTime;

        float alphaStart = 1f;
        float alphaEnd = 1f;

        if (fadeTimer < fadeInTime)
        {
            float a = Mathf.Clamp01(fadeTimer / fadeInTime);
            alphaStart = a;
            alphaEnd = a;
        }
        else if (fadeTimer > lifeTime - fadeOutTime)
        {
            float t = (fadeTimer - (lifeTime - fadeOutTime)) / fadeOutTime;

            alphaStart = fadeOutCurve.Evaluate(t * 1.2f);
            alphaEnd = fadeOutCurve.Evaluate(t * 0.8f);
        }

        SetLineGradient(alphaStart, alphaEnd);
    }

    void SetLineAlpha(float alpha)
    {
        SetLineGradient(alpha, alpha);
    }

    void SetLineGradient(float alphaStart, float alphaEnd)
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(alphaStart, 0f),
                new GradientAlphaKey(alphaEnd, 1f)
            }
        );
        line.colorGradient = gradient;
    }
}
