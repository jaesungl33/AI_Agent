


using UnityEngine;
namespace Fusion.TankOnlineModule
{
	public class ShotAnim : Shot
	{
		[Header("Custom Settings")]
		public Transform view;
		public Animator animator;

		public override void ApplyStateToVisual(NetworkBehaviour owner, ShotState state, float t, bool isFirstRender, bool isLastRender)
		{
			base.ApplyStateToVisual(owner, state, t, isFirstRender, isLastRender);
			animator.SetBool("active", state.IsAnimActive);
		}
		protected override void OnCustomHited(ShotState state)
		{
			base.OnCustomHited(state);
		}
	}
}