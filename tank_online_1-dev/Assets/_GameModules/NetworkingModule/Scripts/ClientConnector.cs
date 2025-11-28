using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using Fusion.TankOnlineModule;
using FusionHelpers;
using GDO.Audio;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ClientConnector : MonoBehaviour, INetworkRunnerCallbacks
{
    public enum SearchingState
    {
        None,
        JoinedLobby,
        JoinedSession
    }
  
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private AudioEmitter audioEmitter;
    public SearchingState CurrentState { get; private set; } = SearchingState.None;
    public GameServer GameServer { get; private set; } = null;
    [SerializeField] private NetworkRunner runnerPrefab;
    public NetworkRunner Runner { get; private set; }
    public ClientDataEvent ClientData => clientData;
    public bool IsApplicationQuitting { get; private set; } = false;
    public event UnityAction OnClientCleanupAction;
    public event UnityAction OnClientJoinedSessionAction;
    public event UnityAction<ClientDataEvent> OnClientJoinedLobbyAction;
    public event UnityAction OnClientShutdownAction;
    public event UnityAction<PlayerRef> OnPlayerJoinedAction;
    public event UnityAction<PlayerRef> OnPlayerLeftAction;
    public event UnityAction<PlayerRef, ClientDataEvent> OnPlayerDataUpdatedAction;
    public event UnityAction<string> OnErrorAction;
    [SerializeField] private ClientDataEvent clientData = default;
    private CancellationTokenSource cts;
    private List<SessionInfo> availableSessions = new List<SessionInfo>();
    private int maxTimeoutTry = 10; // 10 attempts
    private Coroutine autoLevelLobbyCoroutine;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        InitEventHandlers();
    }

    private async void OnApplicationQuit()
    {
        IsApplicationQuitting = true;
        await LeftTheSession();
    }

    private void InitEventHandlers()
    {
        EventManager.Register<ClientDataEvent>(OnPlayerDataReceived);

        // Prepare event handlers
        OnClientCleanupAction = () =>
        {
            Debug.Log("Server cleanup initiated.");
            clientData = default;
            if (Runner != null && Runner.IsRunning)
            {
                Runner.RemoveCallbacks(this);
                Runner.ClearRunnerSingletons();
                _ = Runner.Shutdown();
                Runner = null;
            }
        };

        // Join lobby event
        OnClientJoinedLobbyAction = playerDataEvent =>
        {
            Debug.Log("Joined lobby.");
            clientData = playerDataEvent;
            autoLevelLobbyCoroutine = StartCoroutine(AutoLeaveLobby());
        };

        // Join session event
        OnClientJoinedSessionAction = () => EventManager.TriggerEvent(MainMenuEvent.Updating(Runner.ActivePlayers.Count(), Runner.SessionInfo.MaxPlayers));

        // Stop server event
        OnClientShutdownAction = () =>
        {
            Debug.Log("Client shutdown.");
            //EventManager.Emit(GamePhase.CancelSearching);
        };

        OnPlayerLeftAction = player => EventManager.TriggerEvent(MainMenuEvent.Updating(Runner.ActivePlayers.Count(), Runner.SessionInfo.MaxPlayers));
        OnPlayerDataUpdatedAction += (player, data) => Debug.Log($"Player data updated: {player}, {data}");
        OnErrorAction += message => Debug.LogError($"Error: {message}");
    }

    private IEnumerator RequestCreateMyTank()
    {
        while (GameServer == null)
        {
            Debug.Log("Waiting for game server instance...");
            if (Runner.TryGetSingleton(out GameServer gameServerInstance))
            {
                Debug.Log("Game server instance found.");
                GameServer = gameServerInstance;
                GameServer.RPC_CreatePlayer(clientData);
            }
            else
                Debug.LogWarning("Game server instance not found.");
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator AutoLeaveLobby()
    {
        yield return new WaitForSeconds(maxTimeoutTry);
        if (CurrentState == SearchingState.JoinedLobby)
        {
            Debug.Log("Auto leaving lobby due to timeout.");
            EventManager.TriggerEvent(GamePhase.MatchCanceling);
        }
    }

    private void OnPlayerDataReceived(ClientDataEvent playerDataEvent)
    {
        if (playerDataEvent.matchMode == 0 || playerDataEvent.matchType == 0)
        {
            Debug.LogWarning("Maybe this is a cancel searching match event");
            if (CurrentState == SearchingState.JoinedSession)
                _ = LeftTheSession();
            else if (CurrentState == SearchingState.JoinedLobby)
                _ = LeaveTheLobby();
            return;
        }
        else
            _ = JoinTheLobby(playerDataEvent);
    }

    /// <summary>
    /// Join the lobby as a client.
    /// Get the list of available sessions from the lobby.
    /// If a suitable session is found, join that session.
    /// </summary>
    /// <returns></returns>
    public async Task JoinTheLobby(ClientDataEvent playerDataEvent = default)
    {
        if (CurrentState != SearchingState.None)
        {
            Debug.LogWarning("Lobby is already running.");
            return;
        }

        cts = new CancellationTokenSource();

        Runner = Instantiate(runnerPrefab);
        Runner.name = "LobbyClientRunner";
        Runner.ProvideInput = false;
        Runner.AddCallbacks(this);

        var lobbyResult = await Runner.JoinSessionLobby(SessionLobby.ClientServer);

        if (lobbyResult.Ok)
        {
            CurrentState = SearchingState.JoinedLobby;
            Debug.Log("Joined lobby successfully.");
            OnClientJoinedLobbyAction?.Invoke(playerDataEvent);
        }
        else
        {
            Debug.LogError($"Failed to start lobby client: {lobbyResult.ShutdownReason}");
            OnErrorAction?.Invoke(lobbyResult.ShutdownReason.ToString());
        }
    }
    
    public async Task LeaveTheLobby()
    {
        if (CurrentState != SearchingState.JoinedLobby)
        {
            Debug.LogWarning("Lobby is not running.");
            return;
        }

        cts.Cancel();

        if (Runner != null)
        {
            await Runner.Shutdown();
            Destroy(Runner.gameObject);
            Runner = null;
        }

        CurrentState = SearchingState.None;
    }

    private async Task<string> GetSessionName()
    {
        int maxTry = maxTimeoutTry; // 10 attempts
        while (!cts.Token.IsCancellationRequested && maxTry > 0)
        {
            Debug.Log("Searching for available sessions...");
            // find a session match the region and game mode, fixedEloName, maxPlayers
            for (int i = 0; i < availableSessions.Count; i++)
            {
                var session = availableSessions[i];
                if (session.IsOpen &&
                    session.Properties != null &&
                    session.Properties.TryGetValue(nameof(ClientDataEvent.region), out var regionProp) && regionProp == clientData.region.ToString() &&
                    session.Properties.TryGetValue(nameof(ClientDataEvent.fixedEloName), out var fixedEloNameProp) && fixedEloNameProp == clientData.fixedEloName.ToString() &&
                    session.Properties.TryGetValue(nameof(ClientDataEvent.matchType), out var matchTypeProp) && matchTypeProp == clientData.matchType &&
                    session.Properties.TryGetValue(nameof(ClientDataEvent.matchMode), out var matchModeProp) && matchModeProp == clientData.matchMode &&
                    session.Properties.TryGetValue(nameof(ClientDataEvent.maxPlayers), out var maxPlayersProp) && maxPlayersProp == clientData.maxPlayers)
                {
                    Debug.Log($"Found matching session: {session.Name}");
                    return session.Name;
                }

                await Task.Yield();
            }

            await Task.Delay(1000, cts.Token); // Wait 1 second before checking again
            maxTry -= 1;
            Debug.Log($"Retrying... {maxTry} attempts left.");
        }

        return null;
    }

    /// <summary>
    /// Join a session as a client.
    /// </summary>
    /// <param name="playerDataEvent"></param>
    /// <returns></returns>
    public async Task JoinTheSession()
    {
        if (CurrentState == SearchingState.JoinedSession)
        {
            Debug.LogWarning("You are already in a session.");
            return;
        }
        StopCoroutine(autoLevelLobbyCoroutine);

        string sessionName = await GetSessionName();
        if (sessionName == null)
        {
            Debug.LogWarning("No suitable session found to join.");
            EventManager.TriggerEvent(GamePhase.MatchCanceling);
            return;
        }
        
        await LeaveTheLobby();
        await Task.Delay(500);

        cts = new CancellationTokenSource();

        Runner = Instantiate(runnerPrefab);
        Runner.name = "ClientConnectorRunner";
        Runner.AddCallbacks(this);
        Runner.RegisterSingleton(this);
        Runner.ProvideInput = true;
        
        clientData.sessionName = sessionName;
        var startGameArgs = new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = sessionName
        };

        var result = await Runner.StartGame(startGameArgs);

        if (result.Ok)
        {
            CurrentState = SearchingState.JoinedSession;
            clientData.playerRef = Runner.LocalPlayer;
            StartCoroutine(RequestCreateMyTank());
            Debug.Log("Joined session successfully.");
        }
        else
        {
            Debug.LogError($"Failed to join session: {result.ShutdownReason}");
            OnErrorAction?.Invoke(result.ShutdownReason.ToString());
        }
    }

    public async Task LeftTheSession()
    {
        if (CurrentState != SearchingState.JoinedSession)
        {
            Debug.LogWarning("Client is not in a session.");
            return;
        }

        cts.Cancel();

        if (Runner != null)
        {
            await Runner.Shutdown();
            Destroy(Runner.gameObject);
            Runner = null;
        }
        OnClientShutdownAction?.Invoke();
        CurrentState = SearchingState.None;
    }

    public void SetCamera(Player player)
    {
        if(Runner.LocalPlayer != player.Object.InputAuthority) return;

        if (levelManager.IsometricCamera != null)
        {
            levelManager.IsometricCamera.Initialize(player);
        }
        else
        {
            Debug.LogWarning("CameraFollowIsometric is not assigned.");
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // this will be called when a player joins the session (including the host)
        OnClientJoinedSessionAction?.Invoke();
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        // this will be called when a player leaves the session (except the quitter)
        OnPlayerLeftAction?.Invoke(player);
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        // this will be called when the runner is shutdown (only quitter will receive this event)
        CurrentState = SearchingState.None;
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log($"Session list updated. Total sessions: {sessionList.Count}");
        if (sessionList.Count == 0) return;
        availableSessions = sessionList;

        if( CurrentState == SearchingState.JoinedLobby)
            _ = JoinTheSession();
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken){}
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token){}
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnSceneLoadDone(NetworkRunner runner){}
    public void OnSceneLoadStart(NetworkRunner runner) { }
}