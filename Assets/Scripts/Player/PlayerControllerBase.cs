using System;
using Animation;
using DefaultNamespace;
using Movement;
using Pool;
using Scriptables;
using Settings;
using UnityEngine;

namespace Player {
  public class PlayerControllerBase : MonoBehaviour {
    [SerializeField] protected PlayerStats _stats;
    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;
    private FrameInput _frameInput;
    [SerializeField]protected Vector2 _frameVelocity;
    private bool _cachedQueryStartInColliders;
    [SerializeField] private Animator _animator;

    [SerializeField] protected Transform Head;
    [SerializeField] protected float _flipDeadZone = 1;

    protected Camera _camera;
    protected bool isFlipped;
    protected float rotationCoef = 1f;

    [SerializeField] private PlayerCoords _playerCoords;
    
    [SerializeField] protected LadderMovement _ladderMovement;
    public LadderMovement LadderMovement => _ladderMovement;
    public PlayerCoords PlayerCoords => _playerCoords;
    public Vector2 FrameVelocity => _frameVelocity;

    [SerializeField] private Stamina _stamina;
    public Stamina Stamina => _stamina;
    public PlayerStats Stats => _stats;
    private float _frameLeftGrounded = float.MinValue;
    [SerializeField]private bool grounded;
    private float time;
    //lock player when ui window is open
    protected bool lockPlayer;
    [SerializeField]private bool startFalling;
    private bool wasSprintingOnJump;
    public bool WasSprintingOnJump => wasSprintingOnJump;
    
    public bool Grounded => grounded;
    
    private bool _jumpToConsume;
    private bool _bufferedJumpUsable;
    private bool _endedJumpEarly;
    private bool _coyoteUsable;
    private float _timeJumpWasPressed;
    private AnimatorParameters animParam;

    #region Interface

    public Vector2 FrameInput => _frameInput.Move;
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;

    #endregion
    
    public void SetLockPlayer(bool state) {
      lockPlayer = state;
    }

    public virtual void SetLockHighlight(bool state) {
      
    }
    
    protected virtual void Awake() {
      _rb = GetComponent<Rigidbody2D>();
      _col = GetComponent<CapsuleCollider2D>();

      _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;

      //for lookAt
      _camera = Camera.main;
      isFlipped = false;
      rotationCoef = 1f;
      AnimationEventManager.onFootstep += SpawnFootstepEffect;
      animParam = GameManager.Instance.AnimatorParameters;
    }


    protected virtual void OnDestroy() {
      AnimationEventManager.onFootstep -= SpawnFootstepEffect;
    }


    //move forward or backward
    public float GetMoveForward() {
      return Mathf.Approximately(Mathf.Sign(_frameVelocity.x), Mathf.Sign(transform.localScale.x)) ? 1 : -1;
    }

    protected virtual void Update() {
      time += Time.deltaTime;
      GatherInput();
      LookAtMouse();
    }

    protected virtual void LookAtMouse() {
      FlipX();
    }

    protected virtual void FlipX() {
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
        JumpDown = UserInput.instance.controls.GamePlay.Jump.WasPerformedThisFrame(), //Input.GetButtonDown("Jump"),
        JumpHeld = UserInput.instance.controls.GamePlay.Jump.IsPressed(), //Input.GetButton("Jump"),
        Move = UserInput.instance.GetMovement() //new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"))
      };

      if (_stats.SnapInput) {
        _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < _stats.HorizontalDeadZoneThreshold
          ? 0
          : Mathf.Sign(_frameInput.Move.x);
        _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < _stats.VerticalDeadZoneThreshold
          ? 0
          : Mathf.Sign(_frameInput.Move.y);
      }

      if (_frameInput.JumpDown) {
        _jumpToConsume = true;
        _timeJumpWasPressed = time;
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

    private void CheckCollisions() {
      Physics2D.queriesStartInColliders = false;

      // Ground and Ceiling
      RaycastHit2D hit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down,
        _stats.GrounderDistance, ~_stats.PlayerLayer);
      bool groundHit = hit.collider != null && !hit.collider.isTrigger;
      bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up,
        _stats.GrounderDistance, ~_stats.PlayerLayer);

      // Hit a Ceiling
      if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

      // Landed on the Ground
      if (!grounded && groundHit) {
        grounded = true;
        startFalling = false;
        _coyoteUsable = true;
        _bufferedJumpUsable = true;
        _endedJumpEarly = false;
        GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
        PlayLandingEffect();
      }
      // Left the Ground
      else if (grounded && !groundHit) {
        grounded = false;
        _frameLeftGrounded = time;
        GroundedChanged?.Invoke(false, 0);
      }

      Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }

    private void PlayLandingEffect() {
      ObjectPooler.Instance.SpawnFromPool("LandingEffect", transform.position, Quaternion.identity);
    }

    private void SpawnFootstepEffect() {
      if (Mathf.Approximately(Mathf.Sign(_frameVelocity.x), Mathf.Sign(transform.localScale.x))) {
        if (grounded && Mathf.Abs(_rb.linearVelocity.x) > 1)
          ObjectPooler.Instance.SpawnFromPool("FootstepEffect", transform.position, Quaternion.identity);
      }
    }

    #endregion


    #region Jumping

    private bool HasBufferedJump => _bufferedJumpUsable && time < _timeJumpWasPressed + _stats.JumpBuffer;
    private bool CanUseCoyote => _coyoteUsable && !grounded && time < _frameLeftGrounded + _stats.CoyoteTime;

    private void HandleJump() {
      if (!_endedJumpEarly && !grounded && !_frameInput.JumpHeld && _rb.linearVelocity.y > 0) _endedJumpEarly = true;

      if (!_jumpToConsume && !HasBufferedJump) return;

      if (grounded || CanUseCoyote || _ladderMovement.IsClimbing) ExecuteJump();

      _jumpToConsume = false;
    }

    public void ResetAnimatorMovement() {
      _animator.SetFloat(animParam.VelocityXHash, 0f);
      _animator.SetFloat(animParam.VelocityYHash, 0f);
    }

    private void ExecuteJump() {
      wasSprintingOnJump = _stamina.IsSprinting;
      startFalling = false;
      _ladderMovement.SetClimbing(false, "jump");
      //Debug.LogError("start fall false");
      _animator.SetBool(animParam.FallHash, false);
      _animator.SetTrigger(animParam.JumpHash);
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
        //if we are on the ladder need to calculate deceleration like on ground
        var deceleration = grounded || _ladderMovement.IsOnLadder ? _stats.GroundDeceleration : _stats.AirDeceleration;
        _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
      }
      else {
        _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x,_frameInput.Move.x * GetMaxSpeed(),
          _stats.Acceleration * Time.fixedDeltaTime);
      }

      if (_frameVelocity.x == 0) {
        SetAnimVelocityX(0);
        return;
      }

      var direction = Mathf.Sign(_frameVelocity.x);
      SetAnimVelocityX(direction);
    }

    protected virtual float GetMaxSpeed() {
      var isMovingForward = Mathf.Approximately(Mathf.Sign(_frameVelocity.x), Mathf.Sign(transform.localScale.x));
      //var canSprintInAir = !grounded && wasSprintingOnJump;
      return isMovingForward ? (_stamina.IsSprinting /*&& (grounded || canSprintInAir)*/) ? _stats.SprintSpeed : _stats.MaxSpeed
        : _stats.MaxBackSpeed;
    }

    private void SetAnimVelocityX(float value) {
      _animator.SetFloat(animParam.VelocityXHash, value);
    }

    public virtual void EnableController(bool state) {
      enabled = state;
      _rb.simulated = state;
    }

    public void SetParent(Transform tr) {
      transform.parent = tr;
    }

    public void SetPosition(Vector3 pos) {
      transform.localPosition = pos;
    }

    public void EnableCollider(bool state) {
      _col.enabled = state;
    }

    #endregion

    #region Gravity

    private void HandleGravity() {
      if (grounded && _frameVelocity.y <= 0f) {
        _frameVelocity.y = _stats.GroundingForce;
      }
      else {
        var inAirGravity = _stats.FallAcceleration;
        if (_endedJumpEarly && _frameVelocity.y > 0) inAirGravity *= _stats.JumpEndEarlyGravityModifier;
        _frameVelocity.y =
          Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
        SetFall();
      }
    }
    
    //start falling down set animator param
    private void SetFall() {
      if (!(_frameVelocity.y < 0 /*&& !_ladderMovement.IsOnLadder*/) || startFalling /*|| _ladderMovement.IsOnLadder*/) 
        return;

      if (_ladderMovement.IsOnLadder) {
        _ladderMovement.SetClimbing(true, "fall");
      }
      startFalling = true;
      _animator.SetBool(animParam.FallHash, true);
    }

    #endregion

    private void ApplyMovement() {
      if(!_ladderMovement.IsClimbing) _rb.linearVelocity = _frameVelocity;
    }

#if UNITY_EDITOR
    private void OnValidate() {
      if (_stats == null)
        Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
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