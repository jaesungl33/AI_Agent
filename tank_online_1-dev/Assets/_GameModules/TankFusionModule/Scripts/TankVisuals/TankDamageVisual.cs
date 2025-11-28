using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GDO.Audio;

namespace Fusion.TankOnlineModule
{
	public class TankDamageVisual : MonoBehaviour
	{
		private int _previousHealth;
		[SerializeField] private Transform _damageParticleParent;
		[SerializeField] private MeshFilter _tankBotVisual;
		[SerializeField] private MeshFilter _tankTopVisual;
		private bool _active = false;
		private Material _damageMaterial;
		[SerializeField] private float _flashTime = 0.1f;

		[Header("Damage Steps")]
		[SerializeField]
		private List<TankDamageStepVariable> _damageSteps = new List<TankDamageStepVariable>();

		//Internal lists
		private List<ParticleSystem> _drivingDustParticles = new List<ParticleSystem>();
		private List<ParticleSystem> _damageParticles = new List<ParticleSystem>();

		//Remember certain previous values
		private Mesh _currentHullMesh = null;
		private Mesh _currentTurretMesh = null;
		private ParticleSystem _currentDrivingDustParticle = null;
		private GameObject _lastDebri = null;
		private List<GameObject> _activeDebris = new List<GameObject>();

		[Header("Audio")][SerializeField] private AudioClipData _damageSnd;
		[SerializeField] private AudioClipData _explosionSnd;
		[SerializeField] private AudioEmitter _audioEmitter;

		public void Initialize(TankVisual tankVisual, Player player)
		{
			// set color for HP follow team
			//healthBar.color = teamColors[teamIndex];

			_damageMaterial = player.PlayerMaterial;

			_damageParticleParent.parent = transform.parent;


			//Instantiate damage particles
			foreach (TankDamageStepVariable damageStep in _damageSteps)
			{
				//Pre instantiate the hitparticles
				if (damageStep.HitParticles != null)
				{
					InstantiateNewParticles(damageStep.HitParticles, _damageParticles, _damageParticleParent, true);
				}
				else
				{
					_damageParticles.Add(null);
				}

				//Pre instantiate the drivingdust particlesystems
				if (damageStep.DrivingDust != null)
				{
					if (tankVisual.DrivingDustParents != null && tankVisual.DrivingDustParents.Length > 0)
					{
						foreach (var dustParent in tankVisual.DrivingDustParents)
						{
							if (dustParent.childCount > 0) continue; //skip if already has driving dust
							InstantiateNewParticles(damageStep.DrivingDust, _drivingDustParticles, dustParent);
						}
					}
				}
				else
				{
					_drivingDustParticles.Add(null);
				}
			}
			_active = true;
		}

		private void OnDisable()
		{
			if (_damageMaterial != null && _active)
			{
				_damageMaterial.SetFloat("_Transition", 0f);
			}
		}

		//Generalized creation of particlesystem from a list of particleprefabs to a list of particles with a set parent
		void InstantiateNewParticles(ParticleSystem prefab, List<ParticleSystem> particleList, Transform parent, bool useCommonMaterial = false)
		{
			ParticleSystem part = Instantiate(prefab, parent.position, parent.rotation, parent);

			if (useCommonMaterial)
			{
				foreach (Transform child in part.transform)
				{
					//Let all particle effects from this tank use the same base material to prevent multiple instances
					ParticleSystemRenderer partRenderer = child.GetComponent<ParticleSystemRenderer>();
					if (_damageMaterial == null)
					{
						_damageMaterial = new Material(partRenderer.material);
					}

					partRenderer.material = _damageMaterial; //Apply material 
				}
			}

			particleList.Add(part);
		}

		public void OnDeath()
		{
			_audioEmitter.PlayOneShot(_explosionSnd);
			CheckHPToShowVFX(0, 1);
		}

		public void CheckHPToShowVFX(int life, int maxLife)
		{
			if (life == maxLife)
				_previousHealth = maxLife;

			//healthBar.fillAmount = Mathf.Clamp01((float)life / (float)maxLife);
			//Check if health has changed
			if (HealthHasReduced(life))
			{
				float value = 1 - ((float)life / (float)maxLife);
				value = Mathf.Clamp(value, 0, 1);
				int index = CalculateIndex(_damageSteps.Count - 1, value);

				OnDamaged(life <= 0);
				UpdateDrivingDust(index);
				UpdateTankVisuals(index);
				DropParts(index);
				UpdateDamageParticles(index);
			}
		}

		//Check if the health has changed and return the answer - also sets previous health to current health for later use
		bool HealthHasReduced(int life)
		{
			if (_previousHealth > life)
			{
				_previousHealth = life;
				return true;
			}
			return false;
		}

		// Update the tank visuals based on how much health is left
		void UpdateTankVisuals(int index)
		{
			ChangeTankMesh(index, _tankTopVisual, _damageSteps[index].TurretMesh, _currentTurretMesh);
			ChangeTankMesh(index, _tankBotVisual, _damageSteps[index].HullMesh, _currentHullMesh);
		}

		//Common function for changing a MeshFilters mesh based on a value and a mesh-list
		void ChangeTankMesh(int index, MeshFilter meshFilter, Mesh newMesh, Mesh currentMesh)
		{
			if (newMesh == null)
			{
				return;
			}

			if (meshFilter == null)
			{
				return;
			}

			//Only if there are any meshes in the meshlist we change the mesh
			if (newMesh != currentMesh)
			{
				meshFilter.mesh = newMesh; //Change the mesh
				currentMesh = newMesh; //Save the mesh
			}
		}

		// Update the tanks damage particles based on how much health is left
		void UpdateDrivingDust(int index)
		{
			//Get the driving dust particle and check if it is null - if so - return from this function
			ParticleSystem drivingDustParticle = _drivingDustParticles[index];
			if (drivingDustParticle == null)
			{
				return;
			}

			//Stop the current driving dust particle
			if (_currentDrivingDustParticle != null)
				_currentDrivingDustParticle.Stop();

			//Start the new driving dust particle system
			drivingDustParticle.Play();
			_currentDrivingDustParticle = drivingDustParticle;
		}

		//Particle effect happening when damaged
		void UpdateDamageParticles(int index)
		{
			//Get the damage particle and check if it is null - if so - return from this function
			ParticleSystem damageParticle = _damageParticles[index];
			if (damageParticle == null)
			{
				return;
			}

			//Play the damage particle
			damageParticle.transform.position = transform.position;
			damageParticle.Play();
		}

		//Drop parts that stay around until the tank has been respawned
		void DropParts(int index)
		{
			//Get the debri prefab and check if it is null - if so - return from this function
			GameObject debriPrefab = _damageSteps[index].HitDebris;
			if (debriPrefab == null)
			{
				return;
			}

			//Only instantiate new debri if the new debri is not the same as the lastly instantiated debri
			if (debriPrefab != _lastDebri)
			{
				GameObject debris = Instantiate(debriPrefab, transform.position, transform.rotation);
				//closed
				// //Set tank color on debris
				// Material debrisMaterial = null;
				// foreach (Transform child in debris.transform)
				// {
				// 	MeshRenderer meshRenderer = child.GetComponent<MeshRenderer>();
				// 	if (debrisMaterial == null)
				// 	{
				// 		debrisMaterial = new Material(meshRenderer.material);
				// 		debrisMaterial.mainTexture = _damageMaterial.mainTexture;
				// 	}

				// 	meshRenderer.material = debrisMaterial;
				// }

				_activeDebris.Add(debris);
				_lastDebri = debriPrefab;
			}
		}

		private void OnDamaged(bool isDead)
		{
			if (!isDead && _damageMaterial != null && _active)
			{
				StartCoroutine(Flash());
				_audioEmitter.PlayOneShot(_damageSnd);
			}
		}

		//Calculate an index based on the value
		int CalculateIndex(int max, float value)
		{
			return Mathf.FloorToInt(max * value);
		}

		IEnumerator Flash()
		{
			_damageMaterial.SetFloat("_Transition", 1f);
			yield return new WaitForSeconds(_flashTime);
			_damageMaterial.SetFloat("_Transition", 0f);
		}

		public void CleanUpDebris()
		{
			for (int i = _activeDebris.Count - 1; i >= 0; i--)
			{
				Destroy(_activeDebris[i]);
			}

			_activeDebris.Clear();
			_lastDebri = null;
		}

		private void OnDestroy()
		{
			CleanUpDebris();
			if (_damageParticleParent != null)
				Destroy(_damageParticleParent.gameObject);
		}
	}
}