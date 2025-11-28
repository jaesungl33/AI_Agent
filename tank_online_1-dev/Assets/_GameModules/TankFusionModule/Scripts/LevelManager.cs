using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fusion.GameSystems;
using FusionHelpers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fusion.TankOnlineModule
{
	/// <summary>
	/// The LevelManager controls the map - keeps track of spawn points for players.
	/// </summary>
	/// TODO: This is partially left over from previous SDK versions which had a less capable SceneManager, so could probably be simplified quite a bit
	public class LevelManager : NetworkSceneManagerDefault
	{
		[SerializeField] private CountdownManager _countdownManager;
		[SerializeField] private CameraFollowIsometric cameraFollowIsometric;
		[SerializeField] private MinimapCameraFollow minimapCameraFollow;
		[SerializeField] private CameraScreenFXBehaviour _transitionEffect;
		[SerializeField] private GDO.Audio.AudioEmitter _audioEmitter;
		private LevelBehaviour currentLevel;
		private SceneRef loadedScene = SceneRef.None;
		public CameraFollowIsometric IsometricCamera => cameraFollowIsometric;
		public MinimapCameraFollow MinimapCamera => minimapCameraFollow;
		[SerializeField] private GameObject[] _gameObjects;

		private void Awake()
		{
			Initialize();
		}

		void OnToggle(ToggleEvent toggle)
		{
			foreach (GameObject gameObj in _gameObjects)
			{
				if (gameObj.name == toggle.featureName || toggle.featureName == "level")
				{
					gameObj.SetActive(!gameObj.activeSelf);
				}
			}
		}

		private void Initialize()
		{
			_countdownManager.Reset();
			EventManager.Register<LevelEvent>(LoadLevel);
			EventManager.Register<ToggleEvent>(OnToggle);
			EventManager.Register<ELevelManagerState>(OnGamePhaseChanged);
		}

		private void OnDestroy()
		{
			if (GameManager.IsApplicationQuitting) return;
		}

		public override void Shutdown()
		{
			Debug.Log("LevelManager.Shutdown();");

			// Clear references first
			currentLevel = null;
			InputController.fetchInput = false;

			// Clear local state AFTER base shutdown
			loadedScene = SceneRef.None;

			// Let base class handle proper scene cleanup
			base.Shutdown();
		}
		public SpawnPoint GetPlayerSpawnPoint(int teamIndex, int playerIndex)
		{
			if (currentLevel != null)
				return currentLevel.GetPlayerSpawnPoint(teamIndex, playerIndex);
			return null;
		}

		private void LoadLevel(LevelEvent @event)
		{
			if (Runner != null && Runner.IsSharedModeMasterClient)
			{
				StartCoroutine(LoadLevelCoroutine(@event));
			}
			else
			{
				Debug.LogWarning("Cannot load level: Runner is null or not in authority");
			}
		}

		private IEnumerator LoadLevelCoroutine(LevelEvent @event)
		{
			yield return new WaitForSeconds(1);
			currentLevel = null;
			var newScene = SceneRef.FromIndex(@event.sceneIndex);
			if (newScene == loadedScene)
			{
				Debug.Log($"LevelManager.LoadLevelCoroutine: Scene {newScene} is already loaded, skipping load.");
				yield break;
			}

			// Unload previous scene if valid
			if (loadedScene.IsValid)
			{
				Debug.Log($"LevelManager.UnloadLevel(); - _currentLevel={currentLevel} _loadedScene={loadedScene}");
				var unloadOp = Runner.UnloadScene(loadedScene);

				// Wait for unload to complete
				while (!unloadOp.IsDone)
				{
					yield return null;
				}

				// Check if unload was successful
				if (!unloadOp.IsValid)
				{
					Debug.LogError($"Failed to unload scene {loadedScene}");
					yield break;
				}

				loadedScene = SceneRef.None;
			}

			// Load new scene
			Debug.Log($"LevelManager.LoadLevel({@event.sceneIndex}, {@event.loadSceneMode})");
			var sceneRef = SceneRef.FromIndex(@event.sceneIndex);

			// Use proper Unity LoadSceneParameters with Fusion's load method
			var unityLoadParams = new LoadSceneParameters(@event.loadSceneMode, LocalPhysicsMode.None);

			var loadOp = Runner.LoadScene(sceneRef, unityLoadParams, true);

			// Wait for load to complete
			while (!loadOp.IsDone)
			{
				yield return null;
			}

			// Check if load was successful
			if (!loadOp.IsValid)
			{
				Debug.LogError($"Failed to load scene {sceneRef}");
				yield break;
			}

			// Update loaded scene reference only after successful load
			loadedScene = sceneRef;
			Debug.Log($"Scene {sceneRef} loaded successfully");
		}

		protected override IEnumerator UnloadSceneCoroutine(SceneRef prevScene)
		{
			Debug.Log($"LevelManager.UnloadSceneCoroutine({prevScene});");

			// Wait for CoreGamePlay with timeout
			GameServer coreGameplay = null;
			int attempts = 0;
			const int maxAttempts = 100;

			while (coreGameplay == null && attempts < maxAttempts)
			{
				if (Runner.TryGetSingleton(out coreGameplay))
				{
					break;
				}
				Debug.LogWarning($"Waiting for CoreGamePlay (Attempt {attempts + 1}/{maxAttempts})");
				yield return null;
				attempts++;
			}

			if (coreGameplay == null)
			{
				Debug.LogError($"CoreGamePlay not found after {maxAttempts} attempts during unload!");
				// Still continue with base unload even if CoreGamePlay is missing
				yield return base.UnloadSceneCoroutine(prevScene);
				yield break;
			}

			// Only process if we have a valid previous scene
			if (prevScene.AsIndex > 0)
			{
				yield return new WaitForSeconds(1.0f);

				InputController.fetchInput = false;

				// De-spawning all tanks
				Debug.Log("De-spawning all tanks");

				// Snapshot first so despawns can't modify the collection we're iterating
				var playersSnapshot = coreGameplay.AllPlayers.ToArray();

				foreach (var fusionPlayer in playersSnapshot)
				{
					if (fusionPlayer is Player player)
					{
						Debug.Log($"De-spawning tank {player.PlayerId.AsIndex}:{player}");
						player.TeleportOut();
						yield return new WaitForSeconds(0.1f);
					}
					else
					{
						Debug.LogWarning($"FusionPlayer {fusionPlayer} is not a Player instance");
					}
				}

				yield return new WaitForSeconds(1.5f - coreGameplay.PlayerCount * 0.1f);
			}

			// Always call base implementation
			yield return base.UnloadSceneCoroutine(prevScene);
		}
		protected override IEnumerator OnSceneLoaded(SceneRef newScene, UnityEngine.SceneManagement.Scene loadedScene, NetworkLoadSceneParameters sceneFlags)
		{
			Debug.Log($"LevelManager.OnSceneLoaded({newScene},{loadedScene.name},{sceneFlags});");

			// Call base implementation FIRST
			yield return base.OnSceneLoaded(newScene, loadedScene, sceneFlags);

			if (newScene.AsIndex == 0)
				yield break;

			_transitionEffect.ToggleGlitch(true);
			_audioEmitter.Play();

			yield return null;

			this.loadedScene = newScene;
			Debug.Log($"Loading scene {newScene}");

			// Delay one frame
			yield return null;

			// Activate the next level - Use FindFirstObjectByType instead of deprecated FindAnyObjectByType
			currentLevel = FindFirstObjectByType<LevelBehaviour>();
			if (currentLevel != null)
			{
				// Don't activate immediately - wait for scene to be fully ready
				Debug.Log($"LevelBehaviour found in scene {loadedScene.name}");
			}
			else
			{
				Debug.LogWarning($"No LevelBehaviour found in scene {loadedScene.name}");
			}
			yield return new WaitForSeconds(0.3f);

			Debug.Log($"Stop glitching");
			_transitionEffect.ToggleGlitch(false);
			_audioEmitter.Stop();

			// Wait for CoreGamePlay with timeout
			GameServer gameManager = null;
			int attempts = 0;
			const int maxAttempts = 100;

			while (gameManager == null && attempts < maxAttempts)
			{
				if (Runner.TryGetSingleton(out gameManager))
				{
					break;
				}
				Debug.Log($"Waiting for GameManager to Spawn! (Attempt {attempts + 1}/{maxAttempts})");
				yield return null;
				attempts++;
			}

			if (gameManager == null)
			{
				Debug.LogError($"Failed to find CoreGamePlay after {maxAttempts} attempts!");
				yield break;
			}

			gameManager.lastPlayerStanding = null;

			// Respawn with slight delay between each player
			Debug.Log($"Respawning All {gameManager.PlayerCount} Players");
			var gameManagerAllPlayers = gameManager.AllPlayers.ToArray();
			foreach (FusionPlayer fusionPlayer in gameManagerAllPlayers)
			{
				if (fusionPlayer is Player player)
				{
					player.Reset();
					player.Respawn();
					yield return new WaitForSeconds(0.3f);
				}
				else
				{
					Debug.LogWarning($"FusionPlayer {fusionPlayer} is not a Player instance");
				}
			}
			Debug.Log($"All Players Respawned: " + this.loadedScene.AsIndex + " / " + loadedScene.name);

			// NOW the scene is fully loaded and ready - activate level with lights
			if (currentLevel != null)
			{
				currentLevel.OnSceneLoadingComplete();
				Debug.Log($"[LevelManager] Scene loading complete, level activated with lights");
			}
		}

		private void OnGamePhaseChanged(ELevelManagerState newState)
		{
			// get info from SessionProperties
			int lobbySceneIndex=-1, mapSceneIndex=-1;
			if (Runner.SessionInfo.Properties.TryGetValue(nameof(MatchmakingDocument.LobbySceneIndex), out SessionProperty lobbyProperty))
				int.TryParse(lobbyProperty.PropertyValue.ToString(), out lobbySceneIndex);
			if (Runner.SessionInfo.Properties.TryGetValue(nameof(MatchmakingDocument.MapSceneIndex), out SessionProperty mapProperty))
				int.TryParse(mapProperty.PropertyValue.ToString(), out mapSceneIndex);
			
			if(lobbySceneIndex < 0 || mapSceneIndex < 0)
			{
				Debug.LogError($"Invalid scene indices in SessionProperties: Lobby={lobbySceneIndex}, Map={mapSceneIndex}");
				return;
			}

			Debug.Log($"OnGamePhaseChanged: {newState}, LobbySceneIndex={lobbySceneIndex}, MapSceneIndex={mapSceneIndex}");

			switch (newState)
			{
				case ELevelManagerState.LOBBY:
					if (!Runner.IsSharedModeMasterClient) break;

					// Load the lobby scene
					Runner.LoadScene(SceneRef.FromIndex(lobbySceneIndex), new LoadSceneParameters(LoadSceneMode.Single), true);
					break;

				case ELevelManagerState.GAMEPLAY:
					if (!Runner.IsSharedModeMasterClient) break;

					// Load the gameplay scene
					Runner.LoadScene(SceneRef.FromIndex(mapSceneIndex), new LoadSceneParameters(LoadSceneMode.Single), true);
					break;

				case ELevelManagerState.COUNTDOWN:
					// handled in CoreGamePlay
					StartCoroutine(_countdownManager.Countdown(() =>
					{
						// Set state to playing level
						// if (Runner != null && (runner.IsSharedModeMasterClient))
						// {
						// 	if (Runner.TryGetSingleton(out CoreGamePlay gameManager))
						// 	{
						// 		EventManager.Emit<GamePhase>(GamePhase.MatchPlaying);
						// 	}
						// }
						EventManager.TriggerEvent<GamePhase>(GamePhase.MatchPlaying);
						// Enable inputs after countdow finishes
						InputController.fetchInput = true;
					}));
					break;
			}
		}
	}
}

public enum ELevelManagerState
{
	LOBBY,
	GAMEPLAY, 
	COUNTDOWN,
}