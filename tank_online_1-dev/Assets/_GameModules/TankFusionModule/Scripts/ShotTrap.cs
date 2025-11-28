using DG.Tweening;
using Fusion;
using FusionHelpers;
using UnityEngine;

namespace Fusion.TankOnlineModule
{
	public class ShotTrap : Shot
	{
		[SerializeField] private float delayToHide = 1f;
		Tween tween = null;
		protected override void OnFirstRender(ShotState state)
		{
			base.OnFirstRender(state);
			// Show panel if teammate
			if (panelPrefab)
				panelPrefab.gameObject.SetActive(true);
			if (tween != null)
			{
				tween.Kill();
				tween = null;
			}
			if (!IsMatchTeammate(state.TeamIndex))
				tween = DOVirtual.DelayedCall(delayToHide, () =>
				{
					if (panelPrefab)
						panelPrefab.gameObject.SetActive(false);
				}, ignoreTimeScale: false).SetTarget(this);

		}
		void OnDisable()
		{
			//Clear tween
			if (tween != null)
			{
				tween.Kill();
				tween = null;
			}
		}
	}
}