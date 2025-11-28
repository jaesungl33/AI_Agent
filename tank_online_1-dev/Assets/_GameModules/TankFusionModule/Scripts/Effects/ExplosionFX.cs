using GDO.Audio;
using UnityEngine;

namespace Fusion.TankOnlineModule
{
	public class ExplosionFX : AutoReleasedFx
	{
		[SerializeField] private ParticleSystem _particle;

		protected override float Duration => _particle ? _particle.main.duration : 2.0f;
		
		private void OnValidate()
		{
			if (!_particle)
				_particle = GetComponent<ParticleSystem>();
		}

		private new void OnEnable()
		{
			base.OnEnable();
			AudioHelper.PlaySFX(SFX.EXPLOSION1);
			if (_particle)
				_particle.Play();
		}

		private void OnDisable()
		{
			if (_particle)
			{
				_particle.Stop();
				_particle.Clear();
			}
		}
	}
}