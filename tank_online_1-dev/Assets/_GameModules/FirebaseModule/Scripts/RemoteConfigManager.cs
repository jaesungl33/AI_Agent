using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.RemoteConfig;
using Firebase.Extensions;
using UnityEngine.Events;

public class RemoteConfigManager : MonoBehaviour, IInitializableManager
{
    // Đảm bảo bạn đã thêm các file cấu hình GoogleService-Info.plist và google-services.json vào thư mục Assets
    // và đã cài đặt Firebase Remote Config package (FirebaseRemoteConfig.unitypackage).

    public UnityAction<bool> OnInitialized { get; set; }

    public void Initialize()
    {
        // Kiểm tra và sửa các phụ thuộc của Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                // Firebase đã sẵn sàng để sử dụng
                FirebaseApp app = FirebaseApp.DefaultInstance;
                Debug.Log("Firebase initialized successfully!");

                // Lấy instance của Remote Config
                FirebaseRemoteConfig remoteConfig = FirebaseRemoteConfig.DefaultInstance;

                // Cấu hình cài đặt cho Remote Config
                // MinimumFetchInternalInMilliseconds đặt thời gian tối thiểu giữa các lần fetch
                // Trong môi trường phát triển, bạn có thể đặt giá trị nhỏ hơn để test nhanh
                var configSettings = new ConfigSettings
                {
                    MinimumFetchIntervalInMilliseconds = 3600000 // 1 giờ (1 * 60 * 60 * 1000)
                    // Hoặc 0 trong quá trình phát triển để luôn fetch mới: MinimumFetchInternalInMilliseconds = 0
                };
                remoteConfig.SetConfigSettingsAsync(configSettings).ContinueWithOnMainThread(task =>
                {
                    Debug.Log("Remote Config settings configured.");
                    // Sau khi cấu hình, thiết lập các giá trị mặc định trong ứng dụng
                    SetRemoteConfigDefaults();
                });
            }
            else
            {
                // Firebase không sẵn sàng, xử lý lỗi tại đây
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}\n" +
                               "Firebase Unity SDK is not safe to use here");
                OnInitialized?.Invoke(false);
            }
        });
    }

    // Đặt các giá trị mặc định trong ứng dụng
    private void SetRemoteConfigDefaults()
    {
        Dictionary<string, object> defaults = new Dictionary<string, object>();

        // Thêm các key-value pairs mặc định của bạn
        // Ví dụ:
        defaults.Add("welcome_message", "Welcome to Tank Online!");
        defaults.Add("feature_enabled", true);
        defaults.Add("player_health_initial", 100);

        FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults).ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                Debug.Log("Remote Config defaults set.");
                // Sau khi đặt giá trị mặc định, tiến hành fetch và kích hoạt cấu hình từ server
                FetchAndActivateRemoteConfig();
            }
            else if (task.IsFaulted)
            {
                Debug.LogError("Failed to set Remote Config defaults: " + task.Exception);
                OnInitialized?.Invoke(false);
            }
        });
    }

    // Thực hiện fetch và kích hoạt các giá trị Remote Config
    private async void FetchAndActivateRemoteConfig()
    {
        try
        {
            await FirebaseRemoteConfig.DefaultInstance.FetchAsync(System.TimeSpan.Zero).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Fetch completed.");
                    // Kích hoạt các giá trị đã fetch
                    FirebaseRemoteConfig.DefaultInstance.ActivateAsync().ContinueWithOnMainThread(activateTask =>
                    {
                        if (activateTask.IsCompleted)
                        {
                            Debug.Log("Remote Config values activated.");
                            // Bây giờ bạn có thể truy cập các giá trị
                            ApplyConfig();
                        }
                        else if (activateTask.IsFaulted)
                        {
                            Debug.LogError("Failed to activate Remote Config: " + activateTask.Exception);
                            OnInitialized?.Invoke(false);
                        }
                    });
                }
                else if (task.IsFaulted)
                {
                    Debug.LogError("Failed to fetch Remote Config: " + task.Exception);
                    OnInitialized?.Invoke(false);
                }
                else if (task.IsCanceled)
                {
                    Debug.LogWarning("Fetch cancelled.");
                    OnInitialized?.Invoke(false);
                }
            });
        }
        catch (System.Exception e)
        {
            Debug.LogError("Exception during FetchAndActivateRemoteConfig: " + e);
            OnInitialized?.Invoke(false);
        }
    }

    private void UpdateCollection<TDocument, TCollection>(string  keyName)
        where TCollection : CollectionBase<TDocument>
        where TDocument : class
    {
        string json = FirebaseRemoteConfig.DefaultInstance
                     .GetValue(keyName).StringValue;
        Debug.Log($"Fetched Remote Config for {keyName}: {json}");
        if (!string.IsNullOrEmpty(json))
        {
            Debug.Log($"Updating collection {typeof(TCollection)}");
            DatabaseManager.UpdateRuntimeData<TDocument, TCollection>(json);
        }
    }

    // Hàm để hiển thị hoặc sử dụng các giá trị đã được cấu hình
    private void ApplyConfig()
    {
        OnInitialized?.Invoke(true);
        UpdateCollection<TankHullDocument, TankHullCollection>(nameof(TankHullCollection));
        UpdateCollection<TankWeaponDocument, TankWeaponCollection>(nameof(TankWeaponCollection));
        UpdateCollection<SOUpgradeDefinition, TankUpgradeCollection>(nameof(TankUpgradeCollection));
        UpdateCollection<TankAbilityDocument, TankAbilityCollection>(nameof(TankAbilityCollection));
        UpdateCollection<MatchmakingDocument, MatchmakingCollection>(nameof(MatchmakingCollection));
        UpdateCollection<TankDocument, TankCollection>(nameof(TankCollection));
        UpdateCollection<TankWrapDocument, TankWrapCollection>(nameof(TankWrapCollection));
        UpdateCollection<TankStickerDocument, TankStickerCollection>(nameof(TankStickerCollection));
    }
}
