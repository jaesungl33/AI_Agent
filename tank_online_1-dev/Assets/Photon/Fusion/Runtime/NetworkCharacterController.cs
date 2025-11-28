namespace Fusion
{
  using System.Runtime.CompilerServices;
  using System.Runtime.InteropServices;
  using UnityEngine;

  [StructLayout(LayoutKind.Explicit)]
  [NetworkStructWeaved(WORDS + 4)]
  public unsafe struct NetworkCCData : INetworkStruct
  {
    public const int WORDS = NetworkTRSPData.WORDS + 4;
    public const int SIZE = WORDS * 4;

    [FieldOffset(0)]
    public NetworkTRSPData TRSPData;

    [FieldOffset((NetworkTRSPData.WORDS + 0) * Allocator.REPLICATE_WORD_SIZE)]
    int _grounded;

    [FieldOffset((NetworkTRSPData.WORDS + 1) * Allocator.REPLICATE_WORD_SIZE)]
    Vector3Compressed _velocityData;

    public bool Grounded
    {
      get => _grounded == 1;
      set => _grounded = (value ? 1 : 0);
    }

    public Vector3 Velocity
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _velocityData;
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set => _velocityData = value;
    }
  }

  [DisallowMultipleComponent]
  [RequireComponent(typeof(CharacterController))]
  [NetworkBehaviourWeaved(NetworkCCData.WORDS)]
  // ReSharper disable once CheckNamespace
  public sealed unsafe class NetworkCharacterController : NetworkTRSP, INetworkTRSPTeleport, IBeforeAllTicks, IAfterAllTicks, IBeforeCopyPreviousState
  {
    new ref NetworkCCData Data => ref ReinterpretState<NetworkCCData>();

    [Header("Character Controller Settings")]
    public float gravity = -20.0f;
    public float jumpImpulse = 8.0f;
    public float acceleration = 10.0f;
    public float braking = 10.0f;
    public float maxSpeed = 2.0f;
    public float rotationSpeed = 15.0f;

    Tick _initial;
    CharacterController _controller;
    public Vector3 Velocity
    {
      get => Data.Velocity;
      set => Data.Velocity = value;
    }

    public bool Grounded
    {
      get => Data.Grounded;
      set => Data.Grounded = value;
    }

    public void Teleport(Vector3? position = null, Quaternion? rotation = null)
    {
      _controller.enabled = false;
      NetworkTRSP.Teleport(this, transform, position, rotation);
      _controller.enabled = true;
    }


    public void Jump(bool ignoreGrounded = false, float? overrideImpulse = null)
    {
      if (Data.Grounded || ignoreGrounded)
      {
        var newVel = Data.Velocity;
        newVel.y += overrideImpulse ?? jumpImpulse;
        Data.Velocity = newVel;
      }
    }

    public void Move(Vector3 direction, Transform hull = null)
    {
      var deltaTime = Runner.DeltaTime;
      var previousPos = transform.position;
      var moveVelocity = Data.Velocity;

      direction = direction.normalized;

      if (Data.Grounded && moveVelocity.y < 0)
      {
        moveVelocity.y = 0f;
      }

      moveVelocity.y += gravity * Runner.DeltaTime;

      var horizontalVel = default(Vector3);
      horizontalVel.x = moveVelocity.x;
      horizontalVel.z = moveVelocity.z;

      if (direction == default)
      {
        horizontalVel = Vector3.Lerp(horizontalVel, default, braking * deltaTime);
      }
      else
      {
        horizontalVel = Vector3.ClampMagnitude(horizontalVel + direction * acceleration * deltaTime, maxSpeed);

        if (hull != null)
        {
          //chỉ xoay TankVisual, không xoay transform
          hull.rotation = Quaternion.RotateTowards(
              hull.rotation,
              Quaternion.LookRotation(-direction, Vector3.up),
              rotationSpeed * Runner.DeltaTime * 100f // Có thể điều chỉnh hệ số này cho tốc độ xoay phù hợp
          );
        }
        else
        {
          // nếu hull null thì xoay transform
          transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Runner.DeltaTime);
        }
      }

      moveVelocity.x = horizontalVel.x;
      moveVelocity.z = horizontalVel.z;

      _controller.Move(moveVelocity * deltaTime);

      Data.Velocity = (transform.position - previousPos) * Runner.TickRate;
      Data.Grounded = _controller.isGrounded;
    }
    public void ForceMovePhysic(Vector3 targetPosition, float duration, float deltaTime)
    {
      var currentPos = transform.position;
      var distance = Vector3.Distance(currentPos, targetPosition);

      // Nếu đã gần tới vị trí hoặc hết thời gian, set thẳng luôn
      if (distance < 0.01f || duration <= 0f)
      {
        _controller.enabled = false;
        transform.position = targetPosition;
        _controller.enabled = true;
        Data.TRSPData.Position = targetPosition;
        Data.Velocity = Vector3.zero;
        Data.Grounded = _controller.isGrounded;
        return;
      }

      // Tính quãng đường cần đi trong frame này
      float moveStep = distance * (deltaTime / Mathf.Max(duration, 0.01f));
      Vector3 moveDir = (targetPosition - currentPos).normalized;
      Vector3 motion = moveDir * moveStep;

      // Di chuyển trực tiếp, không áp dụng xoay/trượt/gravity
      _controller.Move(motion);

      Data.TRSPData.Position = transform.position;
      Data.Velocity = moveDir * (moveStep / deltaTime);
      Data.Grounded = _controller.isGrounded;
    }
    public void ForceMove(Vector3 targetPosition)
    {
      if (_controller == null) _controller = GetComponent<CharacterController>();

      _controller.detectCollisions = false;
      _controller.enableOverlapRecovery = false;
      Vector3 move = targetPosition - transform.position;
      // Cập nhật velocity cho sync mạng nếu cần
      Data.Velocity = move / Runner.DeltaTime;
    }

    public void ForceMoveInstant(Vector3 targetPosition)
    {
      if (_controller == null) _controller = GetComponent<CharacterController>();
      Vector3 move = targetPosition - transform.position;
      // Move bằng CharacterController để giữ va chạm vật lý
      _controller.Move(move);
      // Cập nhật velocity cho sync mạng nếu cần
      Data.Velocity = move / Runner.DeltaTime;
    }

    public override void Spawned()
    {
      _initial = default;
      TryGetComponent(out _controller);
      // Without disabling and re-enabling the CharacterController here, the first Move call will reset the position to 0,0,0 instead of
      // keeping the position it was spawned at. Presumably disabling it clears some kind of internally cached "previous position" value
      _controller.enabled = false;
      _controller.enabled = true;
      CopyToBuffer();
    }

    public override void Render()
    {
      NetworkTRSP.Render(this, transform, false, false, false, ref _initial);
    }

    void IBeforeAllTicks.BeforeAllTicks(bool resimulation, int tickCount)
    {
      CopyToEngine();
    }

    void IAfterAllTicks.AfterAllTicks(bool resimulation, int tickCount)
    {
      CopyToBuffer();
    }

    void IBeforeCopyPreviousState.BeforeCopyPreviousState()
    {
      CopyToBuffer();
    }

    void Awake()
    {
      TryGetComponent(out _controller);
    }

    void CopyToBuffer()
    {
      Data.TRSPData.Position = transform.position;
      Data.TRSPData.Rotation = transform.rotation;
    }

    void CopyToEngine()
    {
      // CC must be disabled before resetting the transform state
      _controller.enabled = false;

      // set position and rotation
      transform.SetPositionAndRotation(Data.TRSPData.Position, Data.TRSPData.Rotation);

      // Re-enable CC
      _controller.enabled = true;
    }
  }
}