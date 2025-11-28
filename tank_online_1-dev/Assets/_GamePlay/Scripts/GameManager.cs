using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;


namespace Fusion.GameSystems
{
    /// <summary>
    /// GameManager is a singleton class that manages the overall game state and transitions between different game states.
    /// It initializes the game state machine and handles game start and stop operations.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        [System.Serializable]
        public class ManagerConfig
        {
            public string name;
            public NetworkType networkType = NetworkType.Both;
            public float initTimeout = 5f; // Timeout in seconds
            public float waitingTime = 5f; // Delay between retries
        }
        public static string fixedRegion = "asia";//default region
        public GameStateMachine GSM { get; private set; }
        public DeviceQualityManager QualityManager { get; private set; }
        public static bool IsApplicationQuitting { get; private set; } = false;
        private string managersPath = "Prefabs";
        [SerializeField] private ManagerConfig[] managers;
        private Dictionary<GamePhase, System.Func<GameStateMachine, IGameState>> stateFactory;
        private GamePhase currentState = GamePhase.None;
        [SerializeField] private DeviceQualityConfig deviceQualityConfig;
        [SerializeField] private GameObject fPSDebug;
        [SerializeField] private GameObject consoleDebug;
        public int countDone = 0;

        protected override void Awake()
        {
            base.Awake();
            StartCoroutine(Initialize());
            InitializeQualityManager();
        }

        /// <summary>
        /// Initialize the DeviceQualityManager
        /// </summary>
        private void InitializeQualityManager()
        {
            QualityManager = new DeviceQualityManager(deviceQualityConfig);
            QualityManager.Initialize();

            Debug.Log($"[GameManager] Quality Manager initialized - {QualityManager.GetQualityInfo()}");
        }

        /// <summary>
        /// Public method to change quality level at runtime
        /// </summary>
        public void ChangeQualityLevel(int level)
        {
            if (QualityManager != null)
            {
                QualityManager.SetManualQualityLevel(level);
                Debug.Log($"[GameManager] Quality level changed to {level}");
            }
        }

        /// <summary>
        /// Get current quality information
        /// </summary>
        public string GetCurrentQualityInfo()
        {
            return QualityManager?.GetQualityInfo() ?? "Quality Manager not initialized";
        }

        private void OnValidate()
        {
            if (managers == null || managers.Length == 0)
            {
                managers = new ManagerConfig[]
                {
                    new() { name = nameof(EventManager),                initTimeout = 5f, waitingTime = 0.2f },
                    new() { name = nameof(UIManager),                   initTimeout = 5f, waitingTime = 0.2f },
                    new() { name = nameof(DatabaseManager),             initTimeout = 5f, waitingTime = 0.2f },
                    new() { name = nameof(FirestoreManager),            initTimeout = 5f, waitingTime = 0.2f },
                    new() { name = nameof(RemoteConfigManager),         initTimeout = 5f, waitingTime = 0.2f },
                    new() { name = nameof(MatchmakingManager),          initTimeout = 5f, waitingTime = 0.2f },
                    new() { name = nameof(IAPManager),                  initTimeout = 5f, waitingTime = 0.2f },
                    new() { name = nameof(GDO.Audio.AudioManager),      initTimeout = 5f, waitingTime = 0.2f },
                    new() { name = nameof(SceneLoader),                 initTimeout = 5f, waitingTime = 0.2f },
                };
            }
        }

        private IEnumerator Initialize()
        {
            stateFactory =
            new()
            {
                { GamePhase.ScreenLoading,                      gsm => new LoadingState(gsm) },
                { GamePhase.MatchIdling,                        gsm => new HomeState(gsm) },
                { GamePhase.MatchSearching,                     gsm => new MatchSearchingState(gsm) },
                { GamePhase.MatchFindCanceling,                 gsm => new MatchCancelingState(gsm) },
                { GamePhase.MatchFound,                         gsm => new MatchFoundState(gsm) },
                { GamePhase.MatchPicking,                       gsm => new MatchPickingState(gsm) },
                { GamePhase.MatchLoading,                       gsm => new MatchLoadingState(gsm) },
                { GamePhase.MatchCountdown,                     gsm => new MatchCountdownState(gsm) },
                { GamePhase.MatchPlaying,                       gsm => new MatchPlayingState(gsm) },
                { GamePhase.MatchEnding,                        gsm => new MatchEndingState(gsm) },
                { GamePhase.MatchLeaving,                       gsm => new MatchLeavingState(gsm) },
                { GamePhase.MatchFinal,                         gsm => new FinalState(gsm) },
                { GamePhase.Auth,                               gsm => new AuthState(gsm) },
                { GamePhase.OnboardingMatchIdling,              gsm => new OnboardingMatchIdlingState(gsm) },
            };

            var initTasks = new List<(string name, bool isLoaded, string error)>();
            var retryDelay = new WaitForSeconds(0.1f); // Cache WaitForSeconds instance
            StartCoroutine(InitManager());
            yield return null;

            // for (int i = 0; i < managers.Length; i++)
            // {
            //     var manager = managers[i];

            //     if (manager.networkType == NetworkType.Unused)
            //     {
            //         continue; // Skip unused managers
            //     }

            //     // Load manager prefab (outside try-catch to avoid yield issues)
            //     float elapsed = 0;
            //     GameObject prefab = null;

            //     // Try to load with retry logic
            //     for (int attempt = 0; attempt < 3 && prefab == null && elapsed < manager.initTimeout; attempt++)
            //     {
            //         prefab = Resources.Load<GameObject>($"{managersPath}/{manager.name}");
            //         if (prefab == null)
            //         {
            //             elapsed += Time.deltaTime;
            //             yield return retryDelay; // Small delay between retries
            //         }
            //     }

            //     if (prefab == null)
            //     {
            //         var error = $"Failed to load manager prefab: {manager.name}";
            //         initTasks.Add((manager.name, false, error));

            //         Debug.LogError(error);
            //     }

            //     try
            //     {
            //         // Create manager instance
            //         var instance = Instantiate(prefab);
            //         if (instance == null)
            //         {
            //             throw new System.Exception($"Failed to instantiate {manager.name}");
            //         }

            //         // Configure manager
            //         instance.name = $"__{manager.name}";
            //         DontDestroyOnLoad(instance);

            //         initTasks.Add((manager.name, true, null));
            //         IInitializableManager initializable = instance.GetComponent<IInitializableManager>();
            //         initializable.OnInitialized = (success) =>
            //         {
            //             if (!success)
            //             {
            //                 Debug.LogError($"Initialization failed for manager: {manager.name}");
            //             }
            //             Debug.Log($"Manager initialized: {manager.name}");
            //             countDone++;

            //             if (countDone == managers.Length)
            //             {
            //                 // Log detailed initialization summary
            //                 var summary = "Manager Initialization Summary:\n" +
            //                             $"Total: {initTasks.Count}\n" +
            //                             $"Succeeded: {initTasks.Count(x => x.isLoaded)}\n" +
            //                             $"Failed: {initTasks.Count(x => !x.isLoaded)}\n\n" +
            //                             string.Join("\n", initTasks.Select(t =>
            //                                 $"- {t.name}: {(t.isLoaded ? "OK" : "FAILED")}" +
            //                                 (!t.isLoaded ? $" ({t.error})" : "")));

            //                 Debug.Log(summary);
            //                 // Only initialize game state if all required managers loaded successfully
            //                 GSM = new GameStateMachine();
            //                 if (EventManager.Instance != null)
            //                 {
            //                     EventManager.Register<GamePhase>(ChangeState);
            //                 }
            //                 else
            //                 {
            //                     Debug.LogError("EventManager not found - cannot initialize game states");
            //                 }
            //                 Debug.Log("GameManager initialized successfully.");
            //             }
            //         };

            //         //show progress
            //         if (i > 0)
            //             EventManager.TriggerEvent(new LoadingScreenParam { currentStep = i + 1, totalSteps = managers.Length });
            //     }
            //     catch (System.Exception e)
            //     {
            //         var error = $"Error initializing {manager.name}: {e.Message}";
            //         initTasks.Add((manager.name, false, error));

            //         Debug.LogError(error);
            //     }

            //     // Wait one frame to allow Awake/Start to complete (outside try-catch)
            //     yield return new WaitForSeconds(manager.waitingTime);
            // }
        }
        
        private IEnumerator InitManager()
        {
            var initTasks = new List<(string name, bool isLoaded, string error)>();
            var retryDelay = new WaitForSeconds(0.1f); // Cache WaitForSeconds instance

            var manager = managers[countDone];

            if (manager.networkType == NetworkType.Unused)
            {
                countDone++;
                StartCoroutine(InitManager());
                yield break; // Skip unused managers
            }

            // Load manager prefab (outside try-catch to avoid yield issues)
            float elapsed = 0;
            GameObject prefab = null;

            // Try to load with retry logic
            for (int attempt = 0; attempt < 3 && prefab == null && elapsed < manager.initTimeout; attempt++)
            {
                prefab = Resources.Load<GameObject>($"{managersPath}/{manager.name}");
                if (prefab == null)
                {
                    elapsed += Time.deltaTime;
                    yield return retryDelay; // Small delay between retries
                }
            }

            if (prefab == null)
            {
                var error = $"Failed to load manager prefab: {manager.name}";
                initTasks.Add((manager.name, false, error));

                Debug.LogError(error);
            }

            try
            {
                // Create manager instance
                var instance = Instantiate(prefab);
                if (instance == null)
                {
                    throw new System.Exception($"Failed to instantiate {manager.name}");
                }

                // Configure manager
                instance.name = $"__{manager.name}";
                DontDestroyOnLoad(instance);

                initTasks.Add((manager.name, true, null));
                IInitializableManager initializable = instance.GetComponent<IInitializableManager>();
                initializable.OnInitialized = (success) =>
                {
                    if (!success)
                    {
                        string message = $"Initialization failed for manager: {manager.name}";
                        Debug.LogError(message);
                        EventManager.TriggerEvent(new ShowPopupEvent(PopupIDs.Inform, new InformPopupParam() { message = message }));
                    }
                    Debug.Log($"Manager initialized: {manager.name}");
                    countDone++;

                    if (countDone >= managers.Length)
                    {
                        // Log detailed initialization summary
                        var summary = "Manager Initialization Summary:\n" +
                                    $"Total: {initTasks.Count}\n" +
                                    $"Succeeded: {initTasks.Count(x => x.isLoaded)}\n" +
                                    $"Failed: {initTasks.Count(x => !x.isLoaded)}\n\n" +
                                    string.Join("\n", initTasks.Select(t =>
                                        $"- {t.name}: {(t.isLoaded ? "OK" : "FAILED")}" +
                                        (!t.isLoaded ? $" ({t.error})" : "")));

                        Debug.Log(summary);
                        // Only initialize game state if all required managers loaded successfully
                        GSM = new GameStateMachine();
                        if (EventManager.Instance != null)
                        {
                            EventManager.Register<GamePhase>(ChangeState);
                        }
                        else
                        {
                            Debug.LogError("EventManager not found - cannot initialize game states");
                        }
                        Debug.Log("GameManager initialized successfully.");
                    }
                    else 
                    {
                        //show progress
                        EventManager.TriggerEvent(new LoadingScreenParam { currentStep = countDone + 1, totalSteps = managers.Length }); 
                        StartCoroutine(InitManager());
                    }
                };
                initializable.Initialize();
            }
            catch (System.Exception e)
            {
                var error = $"Error initializing {manager.name}: {e.Message}";
                initTasks.Add((manager.name, false, error));

                Debug.LogError(error);
            }
        }

        void Update()
        {
            GSM?.Update();
#if UNITY_EDITOR
            fPSDebug.SetActive(true);
            consoleDebug.SetActive(true);
#else
            if (Input.touchCount >= 5 && fPSDebug != null)
            {
                fPSDebug.SetActive(!fPSDebug.activeSelf);
                consoleDebug.SetActive(!consoleDebug.activeSelf);
            }
#endif
        }

        public void ChangeState(GamePhase stateType)
        {
            if (currentState == stateType) return;
            
            currentState = stateType;

            if (stateFactory.TryGetValue(stateType, out var factory))
            {
                GSM.Change(factory(GSM));
            }
            else
            {
                Debug.LogWarning($"Unknown game state: {stateType}");
            }
        }

        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            IsApplicationQuitting = true;
        }

        protected override void OnDestroy()
        {
            if (GSM != null)
            {
                GSM = null;
            }
        }
    }
}

public enum GamePhase
{
    None,
    ScreenLoading,
    MatchIdling,
    MatchSearching,
    MatchCanceling,
    MatchLoading,
    MatchFindCanceling,
    MatchFound,
    MatchPicking,
    MatchCountdown,
    MatchPlaying,
    MatchEnding,
    MatchLeaving,
    MatchFinal,
    Auth,
    MatchReleasing,
    OnboardingMatchIdling
}

public enum NetworkType
{
    Both,
    ServerOnly,
    ClientOnly,
    Unused,
}