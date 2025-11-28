using GDO.Audio;
using UnityEngine;

namespace Fusion.TankOnlineModule
{
	public class MuzzleFlash : AutoReleasedFx
	{
		[SerializeField] private ParticleSystem _particleEmitter;
		[SerializeField] private float _timeToFade;

		protected override float Duration => _timeToFade;
		
		public virtual void OnFire(ShotState state)
		{
			AudioHelper.PlaySFX(SFX.EXPLOSION1);
			_particleEmitter.Play();
		}
	}
}