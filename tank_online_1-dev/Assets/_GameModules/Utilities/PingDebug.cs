using TMPro;
using UnityEngine;
using Fusion;
using System.Threading;

public class PingDebug : MonoBehaviour
{
    [Header("Settings")]
    public TextMeshProUGUI pingText;

    [Header("Network Settings")]
    [SerializeField] private bool showNetworkInfo = true;
    [SerializeField] private float networkUpdateInterval = 1.0f; // Update network info every 1 second

    // Cached values for performance
    private bool isInitialized;

    // Network-related variables
    private NetworkRunner networkRunner;
    private float timeSinceNetworkUpdate;
    private int lastPing;
    private string currentRegion = "sa";

    // String builder to reduce GC allocation
    private System.Text.StringBuilder stringBuilder;
    private CancellationTokenSource tokenSource;

    void Start()
    {
        // Initialize string builder for text updates (reduces GC)
        stringBuilder = new System.Text.StringBuilder(32);
        currentRegion = DatabaseManager.GetDB<MatchmakingCollection>().GetActiveDocument().FixedRegion;
    }

    void Update()
    {
        if (!isInitialized) return;

        // Accumulate time and frame count
        float deltaTime = Time.unscaledDeltaTime;
        timeSinceNetworkUpdate += deltaTime;

        // Update network info at specified interval
        if (timeSinceNetworkUpdate >= networkUpdateInterval && showNetworkInfo)
        {
            UpdateNetworkInfo();
            timeSinceNetworkUpdate = 0f;
        }
    }

    public void SetRunner(NetworkRunner runner)
    {
        networkRunner = runner;
        isInitialized = true;
    }

    /// <summary>
    /// Update network information including ping and region
    /// </summary>
    private void UpdateNetworkInfo()
    {
        if (networkRunner != null && networkRunner.IsRunning)
        {
            // Get ping information
            RefreshRegionList();
            ShowPing();
        }
        else
        {
            // Not connected or no runner
            ShowOfflinePing();
        }
    }

    private async void RefreshRegionList()
    {
        tokenSource = new CancellationTokenSource();

        var regions = await NetworkRunner.GetAvailableRegions(cancellationToken: tokenSource.Token);
        foreach (var reg in regions)
        {
            if (reg.RegionCode == currentRegion)
            {
                lastPing = reg.RegionPing;
                break;
            }
        }
    }

    /// <summary>
    /// Display ping information when connected
    /// </summary>
    private void ShowPing()
    {
        if (pingText == null) return;

        stringBuilder.Clear();
        stringBuilder.Append("Ping: ");
        stringBuilder.Append(lastPing.ToString());
        stringBuilder.Append("ms");
        stringBuilder.Append("\nRegion: ");
        stringBuilder.Append(currentRegion);

        pingText.text = stringBuilder.ToString();

        // Color coding for ping
        Color pingColor;
        if (lastPing <= 50)
        {
            pingColor = Color.green; // Excellent
        }
        else if (lastPing <= 100)
        {
            pingColor = Color.yellow; // Good
        }
        else if (lastPing <= 150)
        {
            pingColor = new Color(1f, 0.5f, 0f); // Orange - Fair
        }
        else
        {
            pingColor = Color.red; // Poor
        }

        pingText.color = pingColor;
    }

    /// <summary>
    /// Display offline status when not connected
    /// </summary>
    private void ShowOfflinePing()
    {
        if (pingText == null) return;

        pingText.text = "Status: Offline\nRegion: --";
        pingText.color = Color.gray;
    }

    void OnDestroy()
    {
        // Cleanup
        networkRunner = null;
    }
}