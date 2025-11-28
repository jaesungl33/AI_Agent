using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.UI;

namespace Fusion.TankOnlineModule
{
	public class MobileInput : MonoBehaviour
	{
		[SerializeField] private RectTransform _leftJoy;
		[SerializeField] private RectTransform _leftKnob;
		[SerializeField] private RectTransform _leftKnobSub;
		[SerializeField] private RectTransform _rightJoy;
		[SerializeField] private RectTransform _rightKnob;
		[SerializeField] private Image rightKnobDecor;
		[SerializeField] private RectTransform _cancelArea;
		[SerializeField] private List<GamePlayButtonItem> _inputComps;

		public List<GamePlayButtonItem> InputComps => _inputComps;
		public RectTransform RightJoy => _rightJoy;
		public RectTransform CancelArea => _cancelArea;
		public RectTransform DefaultRightJoy
		{
			get
			{
				if (_inputComps != null && _inputComps.Count > 0 && _inputComps[0] != null)
					return _inputComps[0].GetComponent<RectTransform>();
				return null;
			}
		}

		private Transform _canvas;

		private Image rightJoyImage;
		private Image rightKnobImage;
		private void Awake()
		{
			_canvas = GetComponentInParent<Canvas>()?.transform;
			CancelArea?.gameObject.SetActive(false);
			//EventManager.Register<PlayerUpdate>(UpdatePlayer);
			EventManager.Register<ChooseTankBaseInfo>(UpdatePlayer);

			rightJoyImage = _rightJoy.GetComponent<Image>();
			rightKnobImage = _rightKnob.GetComponent<Image>();
		}

		public void OnDisconnect()
		{
			// var runner = FindAnyObjectByType<NetworkRunner>();
			// if (runner != null && !runner.IsShutdown)
			// {
			// 	runner.Shutdown(false);
			// }
		}

		private void SetJoy(RectTransform joy, RectTransform knob, bool isDown, Vector2 current)
		{
			if (!isDown)
			{
				knob.anchoredPosition = Vector2.zero;
				return;
			}

			if (joy == null || knob == null) return;

			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(joy, current, null, out Vector2 localPoint))
			{
				float joyRadius = joy.rect.width * 0.5f;
				if (localPoint.magnitude > joyRadius)
					localPoint = localPoint.normalized * joyRadius;

				knob.anchoredPosition = localPoint * 0.5f;
			}
		}

		public void SetLeft(bool isDown, Vector2 current)
		{
			SetJoy(_leftJoy, _leftKnob, isDown, current);
			// _leftKnobSub set rotation follow _leftKnob direction, hide if _leftKnob is center
			if (_leftKnobSub != null && _leftKnob != null)
			{
				Vector2 direction = _leftKnob.anchoredPosition.normalized;
				bool isCenter = _leftKnob.anchoredPosition == Vector2.zero;
				_leftKnobSub.gameObject.SetActive(!isCenter);
				if (!isCenter)
				{
					float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
					_leftKnobSub.localRotation = Quaternion.Euler(0, 0, angle - 90f);
				}
			}
		}

		public void SetRight(bool isDown, Vector2 current, bool isNormalAttack = true)
		{
			SetJoy(_rightJoy, _rightKnob, isDown, current);
			rightJoyImage.enabled = isDown && isNormalAttack;
			rightKnobImage.enabled = isNormalAttack;
			rightKnobDecor.enabled = !isNormalAttack;
		}

		private void UpdatePlayer(ChooseTankBaseInfo playerData)
		{
			Debug.Log($"MobileInput|UpdatePlayer  PlayerName: {playerData.PlayerName}, TankId: {playerData.TankId}, IsLocalPlayer: {playerData.IsLocalPlayer}");
			if (!playerData.IsLocalPlayer)
				return;
			Player player = GameServer.Instance.MyTank;
			foreach (var inputComp in _inputComps)
			{
				if (inputComp != null)
					inputComp.Init(player);
			}
		}

		public void OnEnable()
		{
			if (GameServer.Instance == null) return;
			if (GameServer.Instance.MyTank == null) return;
			//Debug.LogError("MobileInput|OnEnable");
			Player player = GameServer.Instance.MyTank;
			foreach (var inputComp in _inputComps)
			{
				if (inputComp != null)
					inputComp.Init(player);
			}
		}
		public float GetMaxRightJoyDrag()
		{
			// Lấy bán kính joystick phải (theo đơn vị pixel, đã nhân scale)
			if (_rightJoy != null)
				return _rightJoy.rect.width * 0.5f * _rightJoy.lossyScale.x / Screen.dpi; // chuyển về inch
			return 1.5f; // fallback nếu không có joystick
		}
	}
}