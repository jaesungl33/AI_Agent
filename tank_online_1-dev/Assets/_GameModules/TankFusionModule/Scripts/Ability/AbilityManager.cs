using UnityEngine;
using Fusion;
using System.Collections.Generic;
using System.Linq;

namespace Fusion.TankOnlineModule
{
	/// <summary>
	/// Manages all abilities for a player, including initialization, reset, and visibility.
	/// </summary>
	public class AbilityManager : NetworkBehaviour
	{
		[SerializeField] private AbilityBase[] _abilities;
		[SerializeField] private List<AbilityBase> activeAbilities;
		private Player player;
		public bool IsAbilityHandling
		{
			get
			{
				// Kiểm tra nếu có ability đang hoạt động (duration > 0)
				if (activeAbilities == null) return false;
				foreach (var ability in activeAbilities)
				{
					if (ability != null && ability.TimeElapsed > 0f && ability.IsBlockAutoAim)// && ability.IsTrigged)
						return true;
				}
				return false;
			}
		}
		public bool IsAbilityBlockAutoAim
		{
			get
			{
				// Kiểm tra nếu có ability đang hoạt động (duration > 0)
				if (activeAbilities == null) return false;
				foreach (var ability in activeAbilities)
				{
					if (ability != null && ability.IsBlockAutoAim)
						return true;
				}
				return false;
			}
		}

		#region Fusion Methods
		public override void Render()
		{
		}
		#endregion

		private void OnValidate()
		{
			// Ensure all abilities are assigned and valid
			if (_abilities == null || _abilities.Length == 0)
			{
				_abilities = GetComponentsInChildren<AbilityBase>(true);
			}
		}

		/// <summary>
		/// Initializes all abilities for the given player.
		/// </summary>
		public void Initialize(Player player)
		{
			foreach (var ability in _abilities)
				ability.gameObject.SetActive(false);

			this.player = player;
			activeAbilities = new List<AbilityBase>();
			var playerData = player.PlayerData;
			var _fakeAbilityIDs = new string[] { };

			// //Fake Data for test----------------------------------------------
			// if (playerData.TankId.Contains("scout"))
			// {
			// 	_fakeAbilityIDs = new string[] { "ability.kamikaze" };
			// }
			// else if (playerData.TankId.Contains("assault"))
			// {
			// 	_fakeAbilityIDs = new string[] { "ability.beartrap" };
			// }
			// else if (playerData.TankId.Contains("heavy"))
			// {
			// 	_fakeAbilityIDs = new string[] { "ability.hook" };
			// }
			// Debug.LogFormat("Ability Init TankId: {0}, ability count: {1}", playerData.TankId, playerData.AbilityIDs.Length);
			//---------------------------------------------------------------------------

			var activeAbilityIDs = playerData.AbilityIDs != null && playerData.AbilityIDs.Length > 0 ? playerData.AbilityIDs : _fakeAbilityIDs;
			//activeAbilityIDs = new string[] { "ability.whirlwindchains" };
			if (activeAbilityIDs != null && activeAbilityIDs.Length > 0)
			{
				foreach (var ability in _abilities)
				{
					bool active = activeAbilityIDs.Contains(ability.abilityID);
					// Initialize only active abilities
					if (active)
					{
						ability.gameObject.SetActive(active);
						ability.Initialize(this.player);
						activeAbilities.Add(ability);
					}
				}
			}

			//Load All active ability
			// foreach (var ability in _abilities)
			// {
			// 	if (ability.gameObject.activeSelf)
			// 	{
			// 		ability.Initialize(_player);
			// 		_activeAbilities.Add(ability);
			// 	}
			// }
			ClearAllAbility();
		}

		public void ExitAllAbility()
		{
			foreach (var ability in _abilities)
			{
				ability.Exit();
			}
		}
		public void ClearAllAbility()
		{
			foreach (var ability in _abilities)
			{
				ability.Clear();
			}
		}

		/// <summary>
		/// call active ability by abilityId.
		/// </summary>
		/// <param name="abilityId">The unique identifier of the ability to activate.</param>
		/// <param name="ownerVelocity">The velocity of the owner when activating the ability.</param>
		public void ActivateAbility(string abilityId, Vector3 ownerVelocity)
		{
			if (!player)
				return;
			var ability = System.Array.Find(_abilities, a => a != null && a.abilityID == abilityId);
			if (ability == null || !ability.AvailableActive)
				return;
			ability.Active(Runner, Object.InputAuthority, ownerVelocity);
		}

		public void ActivateAbilityBySlot(int slotIndex, Vector3 ownerVelocity)
		{
			if (!player)
				return;
			if (activeAbilities == null || activeAbilities.Count == 0)
				return;
			if (slotIndex < 0)
				return;
			var ability = activeAbilities.Count > slotIndex ? activeAbilities[slotIndex] : null;
			if (ability == null || !ability.AvailableActive)
				return;
			ability.Active(Runner, Object.InputAuthority, ownerVelocity);
		}

		public float GetAbilityCooldown(int slotIndex)
		{
			if (activeAbilities == null || activeAbilities.Count == 0)
				return 0f;
			if (slotIndex < 0)
				return 0f;
			var ability = activeAbilities.Count > slotIndex ? activeAbilities[slotIndex] : null;
			if (ability == null)
				return 0f;
			return ability.CooldownRemaining;
		}

		public AbilityBase GetAbilityBySlot(int slotIndex)
		{
			if (activeAbilities == null || activeAbilities.Count == 0)
				return null;
			if (slotIndex < 0)
				return null;
			var ability = activeAbilities.Count > slotIndex ? activeAbilities[slotIndex] : null;
			return ability;
		}
	}
}