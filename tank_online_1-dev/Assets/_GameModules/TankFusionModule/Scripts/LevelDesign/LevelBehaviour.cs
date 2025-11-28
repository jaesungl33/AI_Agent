using System;
using UnityEngine;
using Fusion.GameSystems;

namespace Fusion.TankOnlineModule
{
	public class LevelBehaviour : MonoBehaviour
	{
		// Class for storing the lighting settings of a level
		[System.Serializable]
		public struct LevelLighting
		{
			public Color ambientColor;
			public Color fogColor;
			public bool fog;
		}

		[Header("Level Settings")]
		[SerializeField] private LevelLighting _levelLighting;

		[Header("Level Objects")]
		[SerializeField] private GameObject[] _gameObjs;

		[Header("Light Objects")]
		[SerializeField] private GameObject[] _lightObjects;
		[SerializeField] private bool _activateLightsOnSceneLoaded = true;
		[SerializeField] private float _lightActivationDelay = 0.1f; // Small delay to ensure scene is fully ready

		private SpawnPoint[] _playerSpawnPoints;
		private bool _isSceneFullyLoaded = false;
		private bool _lightsActivated = false;

		// [SerializeField] private GameObject cheatUI;
		private void Awake()
		{
			_playerSpawnPoints = GetComponentsInChildren<SpawnPoint>(true);
			EventManager.Register<ToggleEvent>(OnToggle);

			// Initially deactivate all light objects
			DeactivateAllLights();
		}

		void OnToggle(ToggleEvent toggle) {
			// if (cheatUI != null)
			// {
			// 	if (toggle.featureName == "map")
			// 	{
			// 		cheatUI.SetActive(!cheatUI.activeSelf);
			// 	}
			// }
		}
		
		/// <summary>
		/// Called when scene is fully loaded and ready
		/// </summary>
		private void OnSceneFullyLoaded()
		{
			if (_isSceneFullyLoaded && _lightsActivated)
				return;

			_isSceneFullyLoaded = true;

			Debug.Log("[LevelBehaviour] Scene fully loaded, initializing level...");

			// Set the lighting for the level
			SetLevelLighting();

			// Activate all light objects
			ActivateAllLights();

			Debug.Log("[LevelBehaviour] Level lighting and objects initialized.");
		}

		/// <summary>
		/// Public method called by LevelManager when scene loading is complete
		/// </summary>
		public void OnSceneLoadingComplete()
		{
			OnSceneFullyLoaded();
		}

		/// <summary>
		/// Legacy method - now calls OnSceneFullyLoaded
		/// </summary>
		private void OnSceneLoaded()
		{
			OnSceneFullyLoaded();
		}

		public void Activate()
		{
			// Legacy method - lighting is now handled in OnSceneFullyLoaded
			// Keep for backward compatibility but ensure lights are activated
			if (!_lightsActivated)
			{
				OnSceneFullyLoaded();
			}
		}

		/// <summary>
		/// Deactivate all light objects
		/// </summary>
		private void DeactivateAllLights()
		{
			if (_lightObjects == null) return;

			foreach (GameObject lightObj in _lightObjects)
			{
				if (lightObj != null)
				{
					lightObj.SetActive(false);
				}
			}

			_lightsActivated = false;
			Debug.Log("[LevelBehaviour] All lights deactivated.");
		}

		/// <summary>
		/// Activate all light objects
		/// </summary>
		private void ActivateAllLights()
		{
			if (_lightObjects == null) return;

			foreach (GameObject lightObj in _lightObjects)
			{
				if (lightObj != null)
				{
					lightObj.SetActive(true);
				}
			}

			_lightsActivated = true;
			Debug.Log($"[LevelBehaviour] {_lightObjects.Length} light objects activated.");
		}

		/// <summary>
		/// Toggle lights on/off
		/// </summary>
		public void ToggleLights(bool activate)
		{
			if (activate)
			{
				ActivateAllLights();
			}
			else
			{
				DeactivateAllLights();
			}
		}

		private void SetLevelLighting()
		{
			RenderSettings.ambientLight = _levelLighting.ambientColor;
			RenderSettings.fogColor = _levelLighting.fogColor;
			RenderSettings.fog = _levelLighting.fog;

			Debug.Log($"[LevelBehaviour] Level lighting applied - Ambient: {_levelLighting.ambientColor}, Fog: {_levelLighting.fog}");
		}

		private void OnDestroy()
		{
			if (GameManager.IsApplicationQuitting) return;
			EventManager.Unregister<ToggleEvent>(OnToggle);
		}

		public SpawnPoint GetPlayerSpawnPoint(int teamIndex, int playerIndex)
		{
			if (_playerSpawnPoints == null || _playerSpawnPoints.Length == 0)
			{
				Debug.LogError("No player spawn points found in the level.");
				return null;
			}

			if (teamIndex < 0 || teamIndex >= _playerSpawnPoints.Length)
			{
				Debug.LogError($"Invalid team index: {teamIndex}. Must be between 0 and {_playerSpawnPoints.Length - 1}.");
				return null;
			}

			if (playerIndex < 0 || playerIndex >= _playerSpawnPoints.Length)
			{
				Debug.LogError($"Invalid player index: {playerIndex} for team index: {teamIndex}.");
				return null;
			}

			for (int i = 0; i < _playerSpawnPoints.Length; i++)
			{
				if (_playerSpawnPoints[i].team == teamIndex && _playerSpawnPoints[i].playerIndex == playerIndex)
				{
					return _playerSpawnPoints[i];
				}
			}

			return _playerSpawnPoints[teamIndex].GetComponent<SpawnPoint>();
		}
	}
}