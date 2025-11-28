using Fusion;
using FusionHelpers;
using UnityEngine;
using GogoGaga.OptimizedRopesAndCables;
using Unity.VisualScripting;
using Fusion.GameSystems;
using ExitGames.Client.Photon.StructWrapping;
namespace Fusion.TankOnlineModule
{

	public class ShotRope : Shot
	{
		[Header("Custom Settings")]
		public Transform view;
		public Transform customVFX;
		public Transform startPoint;
		public Transform endPoint;
		public Rope ropeRenderer;
		Vector3 originDirection;
		public void OnEnable()
		{
			if (view)
				view.gameObject.SetActive(false);
			if (ropeRenderer)
			{
				ropeRenderer.damping = 1;
				ropeRenderer.ropeLength = 1f;
			}
		}
		public override void ApplyStateToVisual(NetworkBehaviour owner, ShotState state, float t, bool isFirstRender, bool isLastRender)
		{
			if (isLastRender)
			{
				if (startPoint != null)
				{
					startPoint.parent = transform;
					startPoint.localPosition = Vector3.zero;
					startPoint.localRotation = Quaternion.identity;
				}
				if (ropeRenderer)
				{
					ropeRenderer.damping = 1;
					ropeRenderer.ropeLength = 1f;
				}
				if (view)
				{
					view.localPosition = Vector3.zero;
					view.gameObject.SetActive(false);
				}
				HitedTransform = null;
				OwnerPlayer = null;
			}

			base.ApplyStateToVisual(owner, state, t, isFirstRender, isLastRender);

			if (isFirstRender)
			{
				customVFX?.gameObject.SetActive(true);
				if (startPoint != null)
				{
					startPoint.parent = null;
					startPoint.position = new Vector3(state.StartPosition.x, state.StartPosition.y + 1f, state.StartPosition.z);

					if (ropeRenderer)
					{
						ropeRenderer.damping = 1;
						ropeRenderer.ropeLength = 1f;
					}
					ropeRenderer.Init();
					if (view)
					{
						view.localPosition = Vector3.zero;
						view.gameObject.SetActive(true);
					}
					if (state.OwnerId > -1)
					{
						byte playerId = (byte)state.OwnerId;
						OwnerPlayer = GameServer.Instance.GetPlayer<Player>(playerId);
					}
				}
				originDirection = transform.forward;
			}
			if (ropeRenderer != null)
			{
				var range = Vector3.Distance(startPoint.position, endPoint.position);
				ropeRenderer.damping = Mathf.Max(1, (int)range);
				ropeRenderer.ropeLength = Mathf.Max(1, (int)range);
			}
			if (view)
			{
				view.rotation = Quaternion.LookRotation(originDirection, Vector3.up);
			}
		}

		Transform HitedTransform;
		public Player OwnerPlayer;
		void Update()
		{
			if (view != null && HitedTransform == null)
			{
				Collider[] hits = Physics.OverlapSphere(view.position, 0.5f);
				foreach (var hit in hits)
				{
					// Bỏ qua chính mình, startPoint, endPoint
					if (hit.transform == this.transform || hit.transform == startPoint || hit.transform == endPoint)
						continue;

					// Chỉ nhận va chạm với Player khác bản thân
					var player = hit.GetComponent<Player>();
					if (player != null && OwnerPlayer && player.PlayerId != OwnerPlayer.PlayerId)
					{
						HitedTransform = hit.transform;
						break;
					}
				}
			}
			if (HitedTransform != null)
			{
				view.position = HitedTransform.position + Vector3.up * 1.5f;
				Debug.Log($"HitedTransform position: {HitedTransform.position}");
			}
		}
		protected override void OnCustomHited(ShotState state)
		{
			base.OnCustomHited(state);
			customVFX?.gameObject.SetActive(false);
		}
	}
}