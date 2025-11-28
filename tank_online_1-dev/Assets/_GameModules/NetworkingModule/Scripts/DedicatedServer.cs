using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using Fusion.TankOnlineModule;
using FusionHelpers;
using UnityEngine;
using UnityEngine.Events;

public class DedicatedServer : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkRunner runnerPrefab;
    [SerializeField] private GameServer gameServer;
    [SerializeField] private LevelManager levelManager;
    public NetworkRunner Runner { get; private set; }
    public GameServer GameServerInstance { get; private set; }
    public bool IsRunning { get; private set; } = false;
    public bool IsApplicationQuitting { get; private set; } = false;
    public event UnityAction OnServerStartedAction = new UnityAction(() => { });
    public event UnityAction OnServerStoppedAction = new UnityAction(() => { });
    public event UnityAction<PlayerRef> OnPlayerJoinedAction = new UnityAction<PlayerRef>(player => { });
    public event UnityAction<PlayerRef> OnPlayerLeftAction = new UnityAction<PlayerRef>(player => { });
    public event UnityAction<PlayerRef, ClientDataEvent> OnPlayerDataUpdatedAction = new UnityAction<PlayerRef, ClientDataEvent>((player, data) => { });
    public event UnityAction<string> OnErrorAction = new UnityAction<string>(message => { });
    private Dictionary<PlayerRef, ClientDataEvent> clients = new Dictionary<PlayerRef, ClientDataEvent>();
    [SerializeField] private ServerDataEvent serverDataEvent;
    private CancellationTokenSource cts;
    private SceneRef loadedScene = SceneRef.None;

    private void Awake()
    {
        InitEventHandlers();
    }

    private async void OnApplicationQuit()
    {
        IsApplicationQuitting = true;
        await StopServer();
    }

    private void InitEventHandlers()
    {
        EventManager.Register<ServerDataEvent>(OnServerDataReceived);

        // Start server event
        OnServerStartedAction = () =>
        {
            Debug.Log("Server started.");
            GameServerInstance = Runner.Spawn(gameServer);
        };

        // Stop server event
        OnServerStoppedAction = () =>
        {
            Debug.Log("Server stopped.");
            clients.Clear();

            _= StartServer(serverDataEvent);
        };

        // Player joined event
        OnPlayerJoinedAction = player =>
        {
            Debug.Log($"Player joined: {player}");
            var data = default(ClientDataEvent);
            clients[player] = data;
            OnPlayerDataUpdatedAction?.Invoke(player, data);
        };

        // Player left event
        OnPlayerLeftAction = player =>
        {
            Debug.Log($"Player left: {player}");
            RemovePlayerData(player);
        };

        // Player data updated event
        OnPlayerDataUpdatedAction = (player, data) =>
        {
            Debug.Log($"Player data updated: {player}, {data}");
            if (Runner.ActivePlayers.Count() == Runner.SessionInfo.MaxPlayers - 1)
            {
                Debug.Log("All players have joined. Ready to start the game.");
                // Implement your game start logic here
                Runner.SessionInfo.IsOpen = false;
                Runner.SessionInfo.IsVisible = false;
                GameServerInstance.ChangeStateByMaster(ServerState.PICKING); // review later
            }
        };

        // Error event
        OnErrorAction = message =>
        {
            Debug.LogError($"Error: {message}");
        };
    }

    private void OnServerDataReceived(ServerDataEvent serverData)
    {
        if (serverData.matchMode == 0 || serverData.matchType == 0)
        {
            Debug.LogWarning("Invalid match mode or type.");
            return;
        }

        _ = StartServer(serverData);
    }

    public async Task StartServer(ServerDataEvent serverData)
    {
        if (IsRunning)
        {
            Debug.LogWarning("Server is already running.");
            return;
        }

        cts = new CancellationTokenSource();
        Dictionary<string, SessionProperty> sessionProperties = new Dictionary<string, SessionProperty>
        {
            { nameof(ServerDataEvent.region), serverData.region.ToString() },
            { nameof(ServerDataEvent.fixedEloName), serverData.fixedEloName.ToString() },
            { nameof(ServerDataEvent.matchType), (byte)serverData.matchType },
            { nameof(ServerDataEvent.matchMode), (byte)serverData.matchMode },
            { nameof(ServerDataEvent.maxPlayers), (byte)serverData.maxPlayers }
        };

        Runner = Instantiate(runnerPrefab);
        Runner.gameObject.name = "DedicatedServerRunner";
        Runner.ProvideInput = false;
        Runner.AddCallbacks(this);
        
        var startGameArgs = new StartGameArgs()
        {
            GameMode = GameMode.Server,
            PlayerCount = serverData.maxPlayers,
            Scene = SceneRef.FromIndex(serverData.sceneIndex),
            SceneManager = levelManager,
            IsOpen = true,
            IsVisible = true,
            SessionProperties = sessionProperties,
        };

        var result = await Runner.StartGame(startGameArgs);

        if (result.Ok)
        {
            IsRunning = true;
            Debug.Log("Dedicated server started successfully.");
            serverDataEvent = serverData;
            serverDataEvent.sessionName = Runner.SessionInfo.Name;
            OnServerStartedAction?.Invoke();
        }
        else
        {
            Debug.LogError($"Failed to start dedicated server: {result.ShutdownReason}");
            OnErrorAction?.Invoke(result.ShutdownReason.ToString()); 
        }
    }

    public async Task StopServer()
    {
        if (!IsRunning)
        {
            Debug.LogWarning("Server is not running.");
            return;
        }

        cts.Cancel();

        if (Runner != null)
        {
            await Task.Delay(3000); // Wait for 3 seconds to allow graceful shutdown
            await Runner.Shutdown();
            Destroy(Runner.gameObject);
            Runner = null;
        }

        IsRunning = false;
        Debug.Log("Dedicated server stopped.");
        OnServerStoppedAction?.Invoke();
    }

    public ClientDataEvent? GetPlayerData(PlayerRef playerRef)
    {
        if (clients.TryGetValue(playerRef, out var data))
        {
            return data;
        }
        return null;
    }

    public List<ClientDataEvent> GetAllPlayerData()
    {
        return new List<ClientDataEvent>(clients.Values);
    }

    public void RemovePlayerData(PlayerRef playerRef)
    {
        if (clients.Remove(playerRef))
        {   
            GameServerInstance.RemovePlayerInSession(playerRef);
            CheckGameOverEarly();
            Debug.Log($"Removed data for player {playerRef}");
        }
    }

    private void CheckGameOverEarly()
    {
        if(GameServerInstance == null || GameServerInstance.ActivePlayState != ServerState.LEVEL)
            return;

        int teamWinner = -1;
        var teams = clients.Values.GroupBy(p => p.teamIndex).ToList();

        // case 1: just one player active
        if (Runner.ActivePlayers.Count() == 1)
        {
            teamWinner = teams[0].Key; // winner
        }
        else if (Runner.ActivePlayers.Count() > 1)
        {
            // case 2: all players in teammate are gone
            foreach (var team in teams)
            {
                if (team.Count() == 0)
                {
                    if (teamWinner == -1)
                        teamWinner = team.Key; // winner
                    else
                    {
                        teamWinner = 2; // draw
                    }
                }
            }
        }
        else if (Runner.ActivePlayers.Count() == 0) teamWinner = 2; // draw

        if (teamWinner != -1)
        {
            // GameServerInstance.GameOver(teamWinner); //review later
            Debug.Log($"Team {teamWinner} has won. Stopping the server.");
            _ = StopServer();
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // this will be called when a player joins the session (including the host)
        OnPlayerJoinedAction?.Invoke(player);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        // this will be called when a player leaves the session (except the quitter)
        OnPlayerLeftAction?.Invoke(player);
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        // Automatically accept all connection requests
        request.Accept();
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList){}
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken){}
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason){}
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
    public void OnSceneLoadStart(NetworkRunner runner){}
}