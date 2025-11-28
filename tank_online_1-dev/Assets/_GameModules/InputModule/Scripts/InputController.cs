using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using Unity.VisualScripting;
using UnityEngine;

namespace Fusion.TankOnlineModule
{
	public class InputController : NetworkBehaviour, INetworkRunnerCallbacks
	{
		[SerializeField] private LayerMask _mouseRayMask;
		public static bool fetchInput = true;
		public Player _player;
		private NetworkInputData _inputData = new NetworkInputData();
		[SerializeField] private Vector2 _moveDelta;
		private Vector2 _aimDelta;
		private float rangeRatio;
		private Vector2 _leftPos, _leftDown, _rightPos, _rightDown;
		private bool _leftTouchWasDown, _rightTouchWasDown;
		public MobileInput _mobileInput;
		private int _rightTouchId = -1;
		private uint _buttonReset, _buttonTrigger;
		const float JOY_MOVE_THRESHOLD = 0.05f;
		public override void Spawned()
		{
			if (Object.HasInputAuthority)
				Runner.AddCallbacks(this);
			_leftDown = Vector2.zero;
			_rightDown = Vector2.zero;

		}

		public void OnInput(NetworkRunner runner, NetworkInput input)
		{
			if (_player != null && _player.Object != null )//&& (_player.NetPlayerStage == Player.Stage.Active || _player.NetPlayerStage == Player.Stage.Invisible))
			{
				_inputData.aimDirection = _aimDelta.normalized;
				_inputData.moveDirection = _moveDelta.normalized;
				_inputData.rangeRatio = rangeRatio;
				_inputData.Buttons = _buttonTrigger;
				_buttonReset |= _buttonTrigger;
			}
			input.Set(_inputData);
			_inputData.Buttons = 0;
			_inputData.CancelMask = 0;
			//Debug.LogError();
		}

		private void Update()
		{
			if (_mobileInput == null) _mobileInput = FindAnyObjectByType<MobileInput>();
			if (_mobileInput == null || _player == null || _player.visuals == null) return;

			_buttonTrigger &= ~_buttonReset;

			if (Input.mousePresent)
			{
				HandleMouseInput();
			}
			else if (Input.touchSupported)
			{
				HandleTouchInput();
			}

			if(_player.IsDead)
			{
				_moveDelta = Vector2.zero;
				_aimDelta = Vector2.zero;
			}
		}

		private void HandleMouseInput()
		{
			if (Input.GetMouseButton(0))
				_buttonTrigger |= NetworkInputData.BUTTON_FIRE_PRIMARY;
			if (Input.GetMouseButton(1))
				_buttonTrigger |= NetworkInputData.BUTTON_SLOT_1;

			_moveDelta = Vector2.zero;
			Vector2 isometricUp = new Vector2(-0.5f, 0.5f).normalized;
			Vector2 isometricDown = new Vector2(0.5f, -0.5f).normalized;
			Vector2 isometricLeft = new Vector2(-0.5f, -0.5f).normalized;
			Vector2 isometricRight = new Vector2(0.5f, 0.5f).normalized;

			if (Input.GetKey(KeyCode.W)) _moveDelta += isometricUp * _player.Direction;
			if (Input.GetKey(KeyCode.S)) _moveDelta += isometricDown * _player.Direction;
			if (Input.GetKey(KeyCode.A)) _moveDelta += isometricLeft * _player.Direction;
			if (Input.GetKey(KeyCode.D)) _moveDelta += isometricRight * _player.Direction;

			Vector3 mousePos = Input.mousePosition;
			Ray ray = Camera.main.ScreenPointToRay(mousePos);
			if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _mouseRayMask) && hit.collider != null)
			{
				Vector3 aimDirection = hit.point - _player.visuals.Turret.position;
				_aimDelta = new Vector2(aimDirection.x, aimDirection.z);
			}
		}

		private void HandleTouchInput()
		{
			bool leftIsDown = false, rightIsDown = false;

			foreach (Touch touch in Input.touches)
			{
				HandleLeftJoyTouch(touch, ref leftIsDown);
				HandleSkillInputComps(touch, ref rightIsDown);
			}

			// if (_rightTouchWasDown)
			// 	_buttonTrigger |= NetworkInputData.BUTTON_FIRE_PRIMARY;

			if (!leftIsDown) _moveDelta = Vector2.zero;
			if (!rightIsDown) _aimDelta = Vector2.zero;

			_mobileInput.gameObject.SetActive(true);
			_mobileInput.SetLeft(leftIsDown, _leftPos);

			_mobileInput.SetRight(rightIsDown, _rightPos,
						isNormalAttack: (_rightTouchId != -1 && _touchSkillType.TryGetValue(_rightTouchId, out var skillType) && skillType == AbilityUseType.Weapon));

			_leftTouchWasDown = leftIsDown;
			_rightTouchWasDown = rightIsDown;
		}
		private void HandleLeftJoyTouch(Touch touch, ref bool leftIsDown)
		{
			if (touch.position.x < Screen.width / 3)
			{
				leftIsDown = true;
				_leftPos = touch.position;
				if (_leftTouchWasDown)
				{
					Vector2 touchDelta = 10.0f * touch.deltaPosition / Screen.dpi;
					Vector2 isometricDelta = new Vector2(
						(touchDelta.x - touchDelta.y) * 0.5f,
						(touchDelta.x + touchDelta.y) * 0.5f
					);
					_moveDelta += _player.Direction * isometricDelta;
				}
				else
					_leftDown = touch.position;
			}
		}

		private Dictionary<int, int> _touchSkillIndex = new Dictionary<int, int>(); // fingerId -> skillIndex
		private Dictionary<int, AbilityUseType> _touchSkillType = new Dictionary<int, AbilityUseType>(); // fingerId -> skillType

		private void HandleSkillInputComps(Touch touch, ref bool rightIsDown)
		{
			switch (touch.phase)
			{
				case TouchPhase.Began:
					for (int i = 0; i < _mobileInput.InputComps.Count; i++)
					{
						var item = _mobileInput.InputComps[i];
						if (item == null || !item.gameObject.activeSelf) continue;
						var rect = item.GetComponent<RectTransform>();
						if (rect == null) continue;

						if (RectTransformUtility.RectangleContainsScreenPoint(rect, touch.position, null))
						{
							var skillType = item.GetComponent<GamePlayButtonItem>().skillType;
							_touchSkillIndex[touch.fingerId] = i;
							_touchSkillType[touch.fingerId] = skillType;

							_buttonTrigger |= (1u << i);

							if (skillType == AbilityUseType.Weapon ||
								skillType == AbilityUseType.Direction ||
								skillType == AbilityUseType.Ground ||
								skillType == AbilityUseType.ChannelingDirection)
							{
								if (_mobileInput.RightJoy != null)
									_mobileInput.RightJoy.position = touch.position;
								_rightDown = touch.position;
								_rightPos = touch.position;
								_rightTouchId = touch.fingerId;
								HandleRightJoyTouch(touch, ref rightIsDown);
								_mobileInput.CancelArea?.gameObject.SetActive(skillType != AbilityUseType.Weapon);
							}
							break;
						}
					}
					break;

				case TouchPhase.Moved:
				case TouchPhase.Stationary:
					if (_touchSkillIndex.TryGetValue(touch.fingerId, out int idx) &&
						_touchSkillType.TryGetValue(touch.fingerId, out AbilityUseType skillType2))
					{
						_buttonTrigger |= (1u << idx);

						if (skillType2 == AbilityUseType.Weapon ||
							skillType2 == AbilityUseType.Direction ||
							skillType2 == AbilityUseType.Ground ||
							skillType2 == AbilityUseType.ChannelingDirection)
						{
							HandleRightJoyTouch(touch, ref rightIsDown);
						}
					}
					break;

				case TouchPhase.Ended:
				case TouchPhase.Canceled:
					if (_touchSkillIndex.TryGetValue(touch.fingerId, out int idxEnd) &&
						_touchSkillType.TryGetValue(touch.fingerId, out AbilityUseType skillType3))
					{
						if (skillType3 == AbilityUseType.Weapon ||
							skillType3 == AbilityUseType.Direction ||
							skillType3 == AbilityUseType.Ground ||
							skillType3 == AbilityUseType.ChannelingDirection)
						{
							if (_mobileInput.DefaultRightJoy != null)
								_mobileInput.RightJoy.position = _mobileInput.DefaultRightJoy.position;
						}

						bool isCancel = false;
						if (_mobileInput.CancelArea != null)
						{
							isCancel = RectTransformUtility.RectangleContainsScreenPoint(
								_mobileInput.CancelArea, touch.position, null);
						}
						if (isCancel)
						{
							_inputData.CancelMask |= (1u << idxEnd); // đánh dấu cancel slot này
						}
						_mobileInput.CancelArea?.gameObject.SetActive(false);
					}
					_touchSkillIndex.Remove(touch.fingerId);
					_touchSkillType.Remove(touch.fingerId);
					break;
			}
		}
		private void HandleRightJoyTouch(Touch touch, ref bool rightIsDown)
		{
			// Nếu chưa có touch nào đang điều khiển right joy, kiểm tra touch này có nằm trong vùng right joy không
			if (_rightTouchId == -1 && _mobileInput != null && RectTransformUtility.RectangleContainsScreenPoint(_mobileInput.RightJoy, touch.position, null))
			{
				_rightTouchId = touch.fingerId;
				rightIsDown = true;
				_rightPos = touch.position;
				_rightDown = touch.position;
			}
			// Nếu touch này là touch đang điều khiển right joy
			else if (_rightTouchId == touch.fingerId)
			{
				rightIsDown = true;
				_rightPos = touch.position;

				// Tính toán hướng kéo và smooth bằng Lerp
				Vector2 dragVector = (touch.position - _rightDown) / Screen.dpi;
				if (dragVector.magnitude >= JOY_MOVE_THRESHOLD)
				{
					Vector2 isometricDelta = new Vector2(
						(dragVector.x - dragVector.y) * 0.5f,
						(dragVector.x + dragVector.y) * 0.5f
					);
					_aimDelta = Vector2.Lerp(_aimDelta, _player.Direction * isometricDelta, 0.25f);
					// Cập nhật tỷ lệ phạm vi
					rangeRatio = Mathf.Clamp01(dragVector.magnitude / _mobileInput.GetMaxRightJoyDrag());
				}

				// Nếu touch kết thúc, reset trạng thái
				if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
				{
					_rightTouchId = -1;
					rightIsDown = false;
					_aimDelta = Vector2.zero;
					rangeRatio = 0f;
					_rightPos = Vector2.zero;
					_rightDown = Vector2.zero;
				}
			}
		}

		// Fusion callbacks
		public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
		public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
		public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
		public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
		public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
		public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
		public void OnConnectedToServer(NetworkRunner runner) { }
		public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
		public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
		public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
		public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
		public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
		public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
		public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
		public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
		public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
		public void OnSceneLoadDone(NetworkRunner runner) { }
		public void OnSceneLoadStart(NetworkRunner runner) { }
	}

	public struct NetworkInputData : INetworkInput
	{
		public const uint BUTTON_FIRE_PRIMARY = 1 << 0;
		public const uint BUTTON_SLOT_1 = 1 << 1;
		public const uint BUTTON_SLOT_2 = 1 << 2;
		public const uint BUTTON_SLOT_3 = 1 << 3;
		public uint CancelMask;

		public uint Buttons;
		public Vector2 aimDirection;
		public Vector2 moveDirection;
		public float rangeRatio;

		public bool IsUp(uint button) => !IsDown(button);
		public bool IsDown(uint button) => (Buttons & button) == button;
		public bool WasPressed(uint button, NetworkInputData oldInput) =>
		(oldInput.Buttons & button) == 0 && (Buttons & button) == button;
		public bool WasReleased(uint button, NetworkInputData oldInput) =>
		(oldInput.Buttons & button) == button && (Buttons & button) == 0;
		public int GetSlotBtnDownIndex()
		{
			for (int i = 0; i < 32; i++)
			{
				if (IsDown(1u << i))
					return i; // trả về index
			}
			return -1;
		}
	}
}