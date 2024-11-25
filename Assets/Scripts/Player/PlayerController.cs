using System;
using Game;
using Game.Actors;
using Pool;
using Scriptables.Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace Player {
  /// <summary>
  /// Hey!
  /// Tarodev here. I built this controller as there was a severe lack of quality & free 2D controllers out there.
  /// I have a premium version on Patreon, which has every feature you'd expect from a polished controller. Link: https://www.patreon.com/tarodev
  /// You can play and compete for best times here: https://tarodev.itch.io/extended-ultimate-2d-controller
  /// If you hve any questions or would like to brag about your score, come to discord: https://discord.gg/tarodev
  /// </summary>
  [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
  public class PlayerController : MonoBehaviour, IPlayerController {
    [SerializeField] private PlayerStats _stats;
    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;
    private FrameInput _frameInput;
    private Vector2 _frameVelocity;
    private bool _cachedQueryStartInColliders;
    [SerializeField] private Animator _animator;

    [SerializeField] private Transform Head;
    [SerializeField] private float _flipDeadZone = 1;

    private Camera _camera;
    private bool isFlipped = false;
    private float rotationCoef = 1f;
    private float angleOffset = 80f;

    [SerializeField] private float _topAngleLimit = 20;
    [SerializeField] private float _bottomAngleLimit = -20;
    [SerializeField] private ActorPlayer _actorPlayer;

    #region Interface

    public Vector2 FrameInput => _frameInput.Move;
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;

    #endregion

    private float _time;

    [Tooltip("Period of time to spawn run particle")]
    [Range(0, .5f)]
    [SerializeField] private float _dustPeriod = .1f;

    private void Awake() {
      _rb = GetComponent<Rigidbody2D>();
      _col = GetComponent<CapsuleCollider2D>();

      _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;

      //for lookAt
      _camera = Camera.main;
      isFlipped = false;
      rotationCoef = 1f;
      AnimationEventManager.onFootstep += SpawnFootstepEffect;
      GameManager.instance.PlayerController = this;
    }

    private void Destroy() {
      AnimationEventManager.onFootstep -= SpawnFootstepEffect;
    }

    private void Update() {
      _time += Time.deltaTime;
      if (_actorPlayer.IsDead)
        return;
        
      GatherInput();
      LookAtMouse();
    }

    private void LookAtMouse() {
      var dir = ((Vector2)_camera.ScreenToWorldPoint(Input.mousePosition) - (Vector2)Head.position);
      FlipX();
      dir.x *= Mathf.Sign(transform.localScale.x);
      // Calculate the target angle based on the direction
      float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

      // Clamp the target angle within the specified limits
      float clampedAngle = Mathf.Clamp(targetAngle, _bottomAngleLimit, _topAngleLimit) * Mathf.Sign(transform.localScale.x);

      // Apply the clamped angle to the head
      Head.rotation = Quaternion.Euler(0, 0, clampedAngle + Mathf.Sign(transform.localScale.x) * 90);
    }

    private void FlipX() {
      Vector2 mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
      var direction = (mousePosition - (Vector2)Head.position).normalized;

      if (Mathf.Abs(mousePosition.x - Head.transform.position.x) > _flipDeadZone) {
        // Flip player
        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Sign(direction.x);
        transform.localScale = localScale;

        rotationCoef = isFlipped ? -1f : 1f;
        direction.x *= rotationCoef;
      }
    }

    private void GatherInput() {
      _frameInput = new FrameInput {
        JumpDown = Input.GetButtonDown("Jump"),
        JumpHeld = Input.GetButton("Jump"),
        Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"))
      };

      if (_stats.SnapInput) {
        _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.x);
        _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.y);
      }

      if (_frameInput.JumpDown) {
        _jumpToConsume = true;
        _timeJumpWasPressed = _time;
      }
    }

    private void FixedUpdate() {
      CheckCollisions();

      HandleJump();
      HandleDirection();
      HandleGravity();

      ApplyMovement();
    }

    #region Collisions

    private float _frameLeftGrounded = float.MinValue;
    private bool _grounded;

    private void CheckCollisions() {
      Physics2D.queriesStartInColliders = false;

      // Ground and Ceiling
      bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, _stats.GrounderDistance, ~_stats.PlayerLayer);
      bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, _stats.GrounderDistance, ~_stats.PlayerLayer);

      // Hit a Ceiling
      if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

      // Landed on the Ground
      if (!_grounded && groundHit) {
        _grounded = true;
        _coyoteUsable = true;
        _bufferedJumpUsable = true;
        _endedJumpEarly = false;
        GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
        PlayLandingEffect();
      }
      // Left the Ground
      else if (_grounded && !groundHit) {
        _grounded = false;
        _frameLeftGrounded = _time;
        GroundedChanged?.Invoke(false, 0);
      }

      Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }

    private void PlayLandingEffect() {
      ObjectPooler.Instance.SpawnFromPool("LandingEffect", transform.position, Quaternion.identity);
    }

    private void SpawnFootstepEffect() {
      if (Mathf.Approximately(Mathf.Sign(_frameVelocity.x), Mathf.Sign(transform.localScale.x))) {
        if (_grounded && Mathf.Abs(_rb.linearVelocity.x) > 1)
          ObjectPooler.Instance.SpawnFromPool("FootstepEffect", transform.position, Quaternion.identity);
      }
    }

    #endregion


    #region Jumping

    private bool _jumpToConsume;
    private bool _bufferedJumpUsable;
    private bool _endedJumpEarly;
    private bool _coyoteUsable;
    private float _timeJumpWasPressed;

    private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
    private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime;

    private void HandleJump() {
      if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.linearVelocity.y > 0) _endedJumpEarly = true;

      if (!_jumpToConsume && !HasBufferedJump) return;

      if (_grounded || CanUseCoyote) ExecuteJump();

      _jumpToConsume = false;
    }

    private void ExecuteJump() {
      _animator.SetBool("JumpDown", false);
      _animator.SetTrigger("Jump");
      _endedJumpEarly = false;
      _timeJumpWasPressed = 0;
      _bufferedJumpUsable = false;
      _coyoteUsable = false;
      _frameVelocity.y = _stats.JumpPower;
      Jumped?.Invoke();
    }

    #endregion

    #region Horizontal

    private void HandleDirection() {
      if (_frameInput.Move.x == 0) {
        var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
        _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
      }
      else {
        _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.Move.x * GetMaxSpeed(), _stats.Acceleration * Time.fixedDeltaTime);
      }

      if (_frameVelocity.x == 0) {
        SetAnimVelocity(0);
        return;
      }

      var direction = Mathf.Sign(_frameVelocity.x);
      SetAnimVelocity(direction);
    }

    private float GetMaxSpeed() {
      return (Mathf.Approximately(Mathf.Sign(_frameVelocity.x), Mathf.Sign(transform.localScale.x)))
        ? _stats.MaxSpeed
        : _stats.MaxBackSpeed;
    }

    private void SetAnimVelocity(float value) {
      _animator.SetFloat("VelocityX", value);
    }

    #endregion

    #region Gravity

    private void HandleGravity() {
      if (_grounded && _frameVelocity.y <= 0f) {
        _frameVelocity.y = _stats.GroundingForce;
      }
      else {
        var inAirGravity = _stats.FallAcceleration;
        if (_endedJumpEarly && _frameVelocity.y > 0) inAirGravity *= _stats.JumpEndEarlyGravityModifier;
        _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);

        //start falling down
        if (_frameVelocity.y < 0) {
          _animator.SetBool("JumpDown", true);
        }

      }
    }

    #endregion

    private void ApplyMovement() => _rb.linearVelocity = _frameVelocity;

#if UNITY_EDITOR
    private void OnValidate() {
      if (_stats == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
    }
#endif
  }

  public struct FrameInput {
    public bool JumpDown;
    public bool JumpHeld;
    public Vector2 Move;
  }

  public interface IPlayerController {
    public event Action<bool, float> GroundedChanged;

    public event Action Jumped;
    public Vector2 FrameInput { get; }
  }
}