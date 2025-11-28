// Date: 28 June 2025
// AI-Generated: This code structure was created with AI assistance

using UnityEngine;
using Firebase.Analytics;

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // User login tracking
    public void LogUserLogin(string userId)
    {
        FirebaseAnalytics.LogEvent("user_login", new Parameter("user_id", userId));
        Debug.Log($"Logged user_login: {userId}");
    }

    // Start match tracking
    public void LogMatchStart(string mode, string tankId)
    {
        FirebaseAnalytics.LogEvent("match_start",
            new Parameter("mode", mode),
            new Parameter("tank_id", tankId));
        Debug.Log($"Logged match_start: mode={mode}, tankId={tankId}");
    }

    // End match tracking
    public void LogMatchEnd(string result, int score)
    {
        FirebaseAnalytics.LogEvent("match_end",
            new Parameter("result", result),
            new Parameter("score", score));
        Debug.Log($"Logged match_end: result={result}, score={score}");
    }

    // Purchase tank tracking
    public void LogPurchaseTank(string tankId, int price)
    {
        FirebaseAnalytics.LogEvent("purchase_tank",
            new Parameter("tank_id", tankId),
            new Parameter("price", price));
        Debug.Log($"Logged purchase_tank: tankId={tankId}, price={price}");
    }

    // Upgrade tank tracking
    public void LogUpgradeTank(string tankId, string upgradeType, int cost)
    {
        FirebaseAnalytics.LogEvent("upgrade_tank",
            new Parameter("tank_id", tankId),
            new Parameter("upgrade_type", upgradeType),
            new Parameter("cost", cost));
        Debug.Log($"Logged upgrade_tank: tankId={tankId}, upgradeType={upgradeType}, cost={cost}");
    }
}
