using TMPro;
using UnityEngine;
using Fusion;
using Fusion.GameSystems;
using System.Threading;
using System.Threading.Tasks;

public class FPSDebug : MonoBehaviour
{
    [Header("Settings")]
    public TextMeshProUGUI fpsText;
    [SerializeField] private float updateInterval = 0.5f; // Update every 0.5 seconds (optimized)
    [SerializeField] private bool showFrameTime = true;
    [SerializeField] private bool autoDetectThresholds = true; // Auto-detect from DeviceQualityManager

    [Header("Manual FPS Thresholds (if autoDetect disabled)")]
    [SerializeField] private float goodFPS = 60f;
    [SerializeField] private float averageFPS = 30f;

    // Cached values for performance
    private float timeSinceUpdate;
    private int frameCount;
    private float accumulatedTime;
    private int targetFPS;
    private Color currentColor;
    private bool isInitialized;

    // String builder to reduce GC allocation
    private System.Text.StringBuilder stringBuilder;

    void Start()
    {
        // Initialize string builder for text updates (reduces GC)
        stringBuilder = new System.Text.StringBuilder(32);

        // Get target FPS from DeviceQualityManager
        InitializeFPSThresholds();

        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized || fpsText == null) return;

        // Accumulate time and frame count
        float deltaTime = Time.unscaledDeltaTime;
        accumulatedTime += deltaTime;
        frameCount++;
        timeSinceUpdate += deltaTime;

        // Update display at specified interval
        if (timeSinceUpdate >= updateInterval)
        {
            UpdateFPSDisplay();

            // Reset counters
            timeSinceUpdate = 0f;
            frameCount = 0;
            accumulatedTime = 0f;
        }
    }

    /// <summary>
    /// Initialize FPS thresholds from DeviceQualityManager or use manual values
    /// </summary>
    private void InitializeFPSThresholds()
    {
        if (autoDetectThresholds && GameManager.Instance != null && GameManager.Instance.QualityManager != null)
        {
            // Get target FPS from DeviceQualityManager
            targetFPS = GameManager.Instance.QualityManager.CurrentTargetFrameRate;

            // Set thresholds based on target FPS
            // Good: >= 90% of target
            // Average: >= 50% of target
            goodFPS = targetFPS * 0.9f;
            averageFPS = targetFPS * 0.5f;

            Debug.Log($"[FPSDebug] Auto-detected thresholds - Target: {targetFPS}, Good: {goodFPS:F0}, Average: {averageFPS:F0}");
        }
        else
        {
            // Use manual thresholds
            targetFPS = Application.targetFrameRate > 0 ? Application.targetFrameRate : 60;
            Debug.Log($"[FPSDebug] Using manual thresholds - Good: {goodFPS:F0}, Average: {averageFPS:F0}");
        }
    }

    /// <summary>
    /// Update FPS display - optimized to reduce GC allocation
    /// </summary>
    private void UpdateFPSDisplay()
    {
        if (frameCount == 0 || accumulatedTime <= 0f) return;

        // Calculate average FPS over the interval
        float currentFPS = frameCount / accumulatedTime;

        // Build display text using StringBuilder (reduces GC)
        stringBuilder.Clear();
        stringBuilder.Append("FPS: ");
        stringBuilder.Append(currentFPS.ToString("F0")); // Show whole number for cleaner look

        if (showFrameTime)
        {
            float frameTime = accumulatedTime / frameCount * 1000f; // Convert to milliseconds
            stringBuilder.Append("\n");
            stringBuilder.Append(frameTime.ToString("F1"));
            stringBuilder.Append("ms");
        }

        // Set text (only if changed to reduce overhead)
        fpsText.text = stringBuilder.ToString();

        // Determine color based on performance
        Color newColor;
        if (currentFPS >= goodFPS)
        {
            newColor = Color.green;
        }
        else if (currentFPS >= averageFPS)
        {
            newColor = Color.yellow;
        }
        else
        {
            newColor = Color.red;
        }

        // Only update color if it changed (reduce overhead)
        if (newColor != currentColor)
        {
            currentColor = newColor;
            fpsText.color = currentColor;
        }
    }
}