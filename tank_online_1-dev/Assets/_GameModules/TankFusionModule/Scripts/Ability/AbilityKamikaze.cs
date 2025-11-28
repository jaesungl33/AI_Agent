using Fusion;
using Fusion.Utility;
using FusionHelpers;
using UnityEngine;

namespace Fusion.TankOnlineModule
{
	public class AbilityKamikaze : AbilityBase
	{
		public override AbilityPropertyBase AbilityPropsBase { get; } = new KamikazeAbilityProperty();
		public KamikazeAbilityProperty KamikazeProps => AbilityPropsBase as KamikazeAbilityProperty;
		public override void Initialize(Player player)
		{
			base.Initialize(player);
		}
		public override void Active(NetworkRunner runner, PlayerRef owner, Vector3 ownerVelocity)
		{
			base.Active(runner, owner, ownerVelocity);
			Transform exit = GetExitPoint(Runner.Tick);
			_bullets.Add(runner, new ShotState(player.transform.position, exit.position, Vector3.zero, scale: Vector3.one * KamikazeProps.radius), KamikazeProps.castTime);

			if (player) player.TankAnimState = 1;
			IsBlockAutoAim = false;
		}
		protected override void OnAbilityTrigged()
		{
			base.OnAbilityTrigged();
			_bullets.Process(this, (ref ShotState bullet, int tick) =>
			{
				bullet.Position = this.transform.position;
				ApplyDamage();
				bullet.EndTick = Runner.Tick;
				return true;
			});
		}
		protected override void AbilityUpdateHandler(float deltaTime)
		{
			base.AbilityUpdateHandler(deltaTime);

			//follow player position
			_bullets.Process(this, (ref ShotState bullet, int tick) =>
			{
				bullet.Position = this.transform.position;
				return true;
			});
		}
		public override void Exit()
		{
			base.Exit();
			if (_bullets != null)
				_bullets.Process(this, (ref ShotState bullet, int tick) =>
				{
					bullet.EndTick = Runner.Tick;
					return true;
				});
			if (player) player.TankAnimState = 0;
		}

		private void ApplyDamage()
		{
			Vector3 pos = this.transform.position;
			int maxhp = player.PlayerData.MaxHitpoints;
			int damage = (int)(maxhp * KamikazeProps.damageByHpRatio);
			ApplyAreaDamage(pos, KamikazeProps.radius, damage, 0, _bulletPrefab.HitMask);
			this.player.ExplosionOnDeath = false;
			//this.player.RaiseEvent(new DamageEvent { targetPlayerRef = this.player.PlayerId.AsIndex, damage = maxhp });
			this.player.TakeDamage(new DamageEvent { targetPlayerRef = this.player.PlayerId.AsIndex, damage = int.MaxValue });
		}
	}

	public class KamikazeAbilityProperty: AbilityPropertyBase
    {
		public float radius = 5f;
		public float damageByHpRatio = 0.75f;
    }
}