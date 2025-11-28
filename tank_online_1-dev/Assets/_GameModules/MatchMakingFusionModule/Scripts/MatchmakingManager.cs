using System;
using System.Collections;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion.TankOnlineModule;
using FusionHelpers;
using System.Collections.Generic;
using Fusion.GameSystems;
using Fusion.Photon.Realtime;
using UnityEngine.Events;

/// <summary>
/// Find a match based on player characteristics and preferences
/// - Match types include:
///     - SoloMatch: Single player matches
///     - TeamMatch: Matches with teams
///     - CustomMatch: User-defined matches
/// - ELO range from A to B
/// - Match size options (3x3, 5x5)
/// Returns match information including session name and game mode.
/// </summary>
public class MatchmakingManager : Singleton<MatchmakingManager>, INetworkRunnerCallbacks, IInitializableManager
{
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private GameServer gameServerPrefab;
    [SerializeField] private NetworkRunner runnerPrefab;
    [SerializeField] private NetworkRunner runnerInstance;
    // [SerializeField] private List<FusionPlayer> players = new List<FusionPlayer>();
    private StartGameResult findMatchResult;
    private Coroutine matchmakingCoroutine;
    private SOMatchData soMatchPlayerData;

    public UnityAction<bool> OnInitialized { get; set; }

    private GameServer GServer { get; set; }
    public SOMatchData GameMatchData
    {
        get => soMatchPlayerData;
        set => soMatchPlayerData = value;
    }

    public static bool IsFinding { get; set; } = false;
    public static bool IsJoined { get; set; } = false;
    public NetworkRunner RunnerInstance => runnerInstance;


    protected override void Awake()
    {
        base.Awake();
    }

    public void Initialize()
    {
        if (runnerPrefab == null)
        {
            Debug.LogError("[MatchmakingManager] NetworkRunner prefab is not assigned");
            return;
        }
        if (levelManager == null)
        {
            Debug.LogError("[MatchmakingManager] LevelManager is not assigned");
            return;
        }
        if (gameServerPrefab == null)
        {
            Debug.LogError("[MatchmakingManager] GameServer is not assigned");
            return;
        }

        GameMatchData = DatabaseManager.GetDB<SOMatchData>();
        Debug.Log("[MatchmakingManager] Initialized");
        EventManager.Register<MatchmakingEvent>(FindMatchOrCancel);
        // EventManager.Register<FusionPlayer>(AddPlayer);
        EventManager.Register<GamePhase>(OnState);
        OnInitialized?.Invoke(true);
    }
    
    private void OnState(GamePhase state)
    {
        Debug.Log("[MatchmakingManager] Exiting matchmaking due to game left session state: " + state.ToString());

        switch (state)
        {
            case GamePhase.MatchFinal:
                //ResetData();
                break;
            case GamePhase.MatchReleasing:
                // ResetData();
                break;
        }
    }

    public async void ResetData()
    {
        Debug.Log("[MatchmakingManager] Resetting matchmaking data");
        IsFinding = false;
        if (runnerInstance == null) return;
    
        if (GServer != null && GServer.Object != null && GServer.Object.IsValid && GServer.HasStateAuthority)
        {
            runnerInstance.Despawn(GServer.Object);
            GServer = null;
        }

        await runnerInstance.Shutdown();
    }

    // private void AddPlayer(FusionPlayer player)
    // {
    //     if (player != null && !players.Contains(player))
    //     {
    //         players.Add(player);
    //         Debug.Log($"Player {player.NetPlayerName.ToString()} added to matchmaking list.");
    //     }
    // }

    // public List<FusionPlayer> GetAllPlayers()
    // {
    //     return players;
    // }

    private void FindMatchOrCancel(MatchmakingEvent args)
    {
        if (args.isFinding & IsFinding == false)
        {
            IsFinding = true;
            GameMatchData.ClearAll();
            matchmakingCoroutine = StartCoroutine(JoinOrCreateRoomRoutine());
        }
        else
        {
            if (IsFinding)
            {
                IsFinding = false;
                Debug.Log("[MatchmakingManager] Stopping matchmaking");
                StopCoroutine(matchmakingCoroutine);
                matchmakingCoroutine = null;
                CancelMatchmaking();
            }
            else
            {
                Debug.Log("[MatchmakingManager] Matchmaking is not in progress");
            }
        }
    }

    private void CancelMatchmaking()
    {
        if (runnerInstance != null)
        {
            Debug.Log("[MatchmakingManager] Cancelling matchmaking...");
            ResetData();
        }
        else
        {
            Debug.LogWarning("[MatchmakingManager] No active matchmaking session to cancel.");
        }
    }

    private IEnumerator JoinOrCreateRoomRoutine()
    {
        yield return null; // Wait one frame to ensure all players are added
        if (runnerPrefab == null)
        {
            Debug.LogError("[MatchmakingManager] NetworkRunner prefab is not assigned");
            yield break;
        }

        DatabaseManager.GetDB<MatchmakingCollection>(collection =>
        {
            MatchmakingDocument doc = collection.GetActiveDocument();
            if (!object.ReferenceEquals(doc, null))
            {
                JoinRoom(doc);
            }
            else
            {
                Debug.LogError("MatchmakingDocument not found in Database.");
            }
        });
    }

    private void JoinRoom(MatchmakingDocument matchmakingDocument)
    {
        DatabaseManager.GetDB<PlayerCollection>(async playerCollection =>
        {
            if (playerCollection == null)
            {
                Debug.LogError("[MatchmakingManager] PlayerCollection not found in Database.");
                return;
            }

            PlayerDocument playerDocument = playerCollection.GetMine();

            for (int index = 0; index <= matchmakingDocument.MaxRoom; index++)
            {
                runnerInstance = Instantiate(runnerPrefab);
                runnerInstance.ProvideInput = true;
                runnerInstance.AddCallbacks(this);
                //string roomName = string.Format($"{GetSessionName(matchmakingDocument, playerDocument.elo)}{index}").ToLower();
                string roomName = $"{matchmakingDocument.FixedRegion}_{matchmakingDocument.MatchType}_{matchmakingDocument.MatchMode}_{matchmakingDocument.MaxPlayers}_{index}";
                roomName = roomName.ToLower();
                Debug.Log($"[Matchmaking] Trying to join or create room: {roomName}");

                var appSettings = BuildCustomAppSetting(GameManager.fixedRegion);

                findMatchResult = await runnerInstance.StartGame(new StartGameArgs()
                {
                    GameMode = GameMode.Shared,
                    SessionName = roomName,
                    SceneManager = levelManager,
                    PlayerCount = matchmakingDocument.MaxPlayers,
                    CustomPhotonAppSettings = appSettings,
                    SessionProperties = new Dictionary<string, SessionProperty>()
                    {
                        { nameof(MatchmakingDocument.MatchType), matchmakingDocument.MatchType.ToString() },
                        { nameof(MatchmakingDocument.MatchMode), matchmakingDocument.MatchMode.ToString() },
                        { nameof(MatchmakingDocument.MaxPlayers), matchmakingDocument.MaxPlayers.ToString() },
                        { nameof(MatchmakingDocument.FixedEloName), GetEloName(matchmakingDocument, playerDocument.elo) },
                        { nameof(MatchmakingDocument.LobbySceneIndex), matchmakingDocument.lobbySceneIndex.ToString() },
                        { nameof(MatchmakingDocument.MapSceneIndex), matchmakingDocument.MapSceneIndex.ToString() }
                    }
                });

                if (findMatchResult.Ok)
                {
                    Debug.Log($"[Matchmaking] Successfully joined or created room: {roomName}");
                    EventManager.TriggerEvent<GamePhase>(GamePhase.MatchFound);
                    return;
                }
                else
                {
                    Debug.LogWarning($"[Matchmaking] Failed to join/create room '{roomName}', reason: {findMatchResult.ShutdownReason}");
                }
            }
        });
    }

    private FusionAppSettings BuildCustomAppSetting(string region, string customAppID = null, string appVersion = "1.0.0") {

        var appSettings = PhotonAppSettings.Global.AppSettings.GetCopy();;

        appSettings.UseNameServer = true;
        appSettings.AppVersion = appVersion;

        if (string.IsNullOrEmpty(customAppID) == false) {
            appSettings.AppIdFusion = customAppID;
        }

        if (string.IsNullOrEmpty(region) == false) {
            appSettings.FixedRegion = region.ToLower();
        }

        // If the Region is set to China (CN),
        // the Name Server will be automatically changed to the right one
        // appSettings.Server = "ns.photonengine.cn";

        return appSettings;
    }

    private string GetEloName(MatchmakingDocument doc, int elo)
    {
        // Validate data
        if (doc?.PlayerElos == null || doc.PlayerElos.Length == 0)
        {
            Debug.LogWarning("[MatchmakingManager] PlayerElos data is missing");
            return "Unranked";
        }

        // Sort by eloPoint (ascending)
        var sortedElos = doc.PlayerElos.OrderBy(x => x.eloPoint).ToArray();

        // Case 1: ELO is lower than minimum rank
        if (elo < sortedElos[0].eloPoint)
        {
            // Return lowest rank
            return sortedElos[0].eloName;
        }

        // Case 2: Find appropriate rank range
        for (int i = 0; i < sortedElos.Length - 1; i++)
        {
            // Check if ELO falls in this range: [current, next)
            if (elo >= sortedElos[i].eloPoint && elo < sortedElos[i + 1].eloPoint)
            {
                return sortedElos[i].eloName;
            }
        }

        // Case 3: ELO is higher than or equal to highest rank
        return sortedElos[sortedElos.Length - 1].eloName;
    }

    private string GetSessionName(MatchmakingDocument matchmakingDocument, int elo)
    {
        // Generate a session name based on ELO and range
        return $"{matchmakingDocument.matchID}_{GetEloName(matchmakingDocument, elo)}_";
    }

    private void CleanAfterShutdown(NetworkRunner runner)
    {
        if (!GameManager.IsApplicationQuitting)
        {
            EventManager.TriggerEvent(MainMenuEvent.StopFindMatch());
        }
        
        runner.ClearRunnerSingletons();
        runner.RemoveCallbacks(this);
        runnerInstance = null;
        GServer = null;
        IsJoined = false;  
    }

    private void CheckCloseRoom(NetworkRunner runner)
    {
        if (runner.ActivePlayers.Count() >= runner.SessionInfo.MaxPlayers)
        {
            if (runner.IsSharedModeMasterClient)
            {
                runner.SessionInfo.IsOpen = false;
                runner.SessionInfo.IsVisible = false;
            }
        }
    }

    private void UpdateUIFindingStatus(NetworkRunner runner)
    {
        EventManager.TriggerEvent(new MainMenuEvent
        {
            status = MatchmakingStatus.Found,
            playerCount = runner.ActivePlayers.Count(),
            maxPlayers = runner.SessionInfo.MaxPlayers,
            immediate = false
        });
    }

    private void UpdateMasterServer(NetworkRunner runner)
    {
        if (runner.IsSharedModeMasterClient)
        {
            // SPAWN MASTER SERVER IF NOT EXIST
            if (!runner.TryGetSingleton(out GameServer session) && gameServerPrefab != null)
            {
                GServer = runner.Spawn(gameServerPrefab);
            }
            else GServer = session;
            if (GServer == null)
            {
                Debug.LogError("Failed to spawn or get CoreGamePlay singleton.");
                return;
            }
            Debug.Log("MasterServer (CoreGamePlay) instance created or found.");

            // OPEN THE ROOM
            runner.SessionInfo.IsOpen = true;
            runner.SessionInfo.IsVisible = true;
        }
    }

    private void CreateLocalPlayer(PlayerRef player, NetworkRunner runner)
    {
        // CREATE PLAYER FOR LOCAL PLAYER
        PlayerDocument playerDocument = DatabaseManager.GetDB<PlayerCollection>().GetMine();
        TankDocument tankDocument = DatabaseManager.GetDB<TankCollection>().GetActiveTank();

        int wrapId = playerDocument.GetWrapId(tankDocument.tankId);
        runner.WaitForSingleton<GameServer>(session =>
        {
            if (player == runner.LocalPlayer)
            {
                // Initialize local player data
                session.HandleJoinedPlayer(runner, player, playerDocument.playerName, tankDocument.tankId, playerDocument.playerAvatar, wrapId);
            }
            UpdateUIFindingStatus(runner);
        });
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // This will be called on all clients in this session (including the one that joined)
        Debug.Log($"Player {player} joined the session.");
        UpdateMasterServer(runner);
        CheckCloseRoom(runner);
        CreateLocalPlayer(player, runner);

        if (runner.LocalPlayer == player)
            IsJoined = true;
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        // This will be called on all clients in this session (except the one that left)
        if (runner.TryGetSingleton(out GameServer GServer))
        {
            GServer.PlayerLeft(player); // Notify FusionSession about the player leaving
            GServer.CheckEndGame();
        }
        else Debug.LogWarning("GameServer instance is null or invalid in OnPlayerLeft.");

        // // This will be called on all clients in this session (except the one that left)
        // Debug.Log($"Player {player} left the session ");
        // if (GServer != null && GServer.Object != null && GServer.Object.IsValid)
        // {
        //     GServer.PlayerLeft(player); // Notify FusionSession about the player leaving
        //     GServer.CheckEndGame();
        // }
        // else Debug.LogWarning("GameServer instance is null or invalid in OnPlayerLeft.");
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        // This will be called on client going down
        string message = "";
        switch (shutdownReason)
        {
            case ShutdownReason.IncompatibleConfiguration:
                message = "This room already exist in a different game mode!";
                break;
            case ShutdownReason.Ok:
                message = "User terminated network session!";
                break;
            case ShutdownReason.Error:
                message = "Unknown network error!";
                break;
            case ShutdownReason.ServerInRoom:
                message = "There is already a server/host in this room";
                break;
            case ShutdownReason.DisconnectedByPluginLogic:
                message = "The Photon server plugin terminated the network session!";
                break;
            default:
                message = shutdownReason.ToString();
                break;
        }

        Debug.Log($"MatchmakingManager.OnShutdown: {message}");
        CleanAfterShutdown(runner);
    }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        request.Accept();
    }
    
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnSessionListUpdated(NetworkRunner runner, System.Collections.Generic.List<SessionInfo> sessionList)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, System.Collections.Generic.Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        Debug.Log("OnHostMigration for " + runner.name);
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("All clients finished loading the scene " + SceneManager.GetActiveScene().name);
        StartCoroutine(DelayedStartGamePlay(runner));
    }

    private IEnumerator DelayedStartGamePlay(NetworkRunner runner)
    {
        yield return new WaitForSeconds(1f); // Wait for 1 second to ensure all data is ready
        if (runner.TryGetSingleton(out GameServer session))
        {
            session.ChangeStateByMaster(ServerState.COUNTDOWN);
        }
        else
        {
            Debug.LogError("GameServer instance is null or invalid in OnSceneLoadDone.");
        }
    }
    
    public void OnSceneLoadStart(NetworkRunner runner) { }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.Log($"Disconnected from server: {reason}");
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }
}

public struct MatchInfo
{
    public string sessionName;
    public GameMode gameMode;
    public byte maxPlayers;
}

public struct MainMenuEvent
{
    public bool immediate;
    public MatchmakingStatus status;
    public int playerCount;
    public int maxPlayers;
    public string buttonLabel;

    public static MainMenuEvent StopFindMatch()
    {
        return new MainMenuEvent
        {
            status = MatchmakingStatus.Idle,
            playerCount = 0,
            maxPlayers = 0,
            immediate = true
        };
    }

    public static MainMenuEvent Find()
    {
        return new MainMenuEvent
        {
            status = MatchmakingStatus.Finding,
            playerCount = 0,
            maxPlayers = 0,
            immediate = false
        };
    }

    public static MainMenuEvent Updating(int activePlayers, int maxPlayers)
    {
        return new MainMenuEvent
        {
            status = MatchmakingStatus.Updating,
            playerCount = activePlayers,
            maxPlayers = maxPlayers,
            immediate = false
        };
    }
}

public enum MatchmakingStatus
{
    Idle,
    Finding,
    Found,
    Updating
}

public struct MatchmakingEvent
{
    public bool isFinding;

    public static MatchmakingEvent Find()
    {
        // Return a new instance with isFinding set to true
        return new MatchmakingEvent
        {
            isFinding = true
        };
    }

    public static MatchmakingEvent Exit()
    {
        // Return a new instance with isFinding set to false
        return new MatchmakingEvent
        {
            isFinding = false
        };
    }
}

public struct LevelEvent
{
    public int sceneIndex;
    public LoadSceneMode loadSceneMode;

    public LevelEvent(int sceneIndex, LoadSceneMode loadSceneMode)
    {
        this.sceneIndex = sceneIndex;
        this.loadSceneMode = loadSceneMode;
    }

    // Static factory method instead of parameterless constructor
    public static LevelEvent GamePlay()
    {
        MatchmakingDocument doc = DatabaseManager.GetDB<MatchmakingCollection>().GetActiveDocument();
        return new LevelEvent { sceneIndex = doc.MapSceneIndex, loadSceneMode = LoadSceneMode.Additive };
    }
}