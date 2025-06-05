using System;
using Actors;
using Animation;
using Movement;
using Scriptables;
using Spine.Unity;
using UnityEngine;

namespace Player {
  public class PlayerControllerBase : MonoBehaviour {
    protected PlayerStats playerStats;
    protected Rigidbody2D _rb;
    private CapsuleCollider2D _col;
    private FrameInput _frameInput;
    [SerializeField] protected Vector2 _frameVelocity;
    private bool _cachedQueryStartInColliders;
    [SerializeField] private Animator _animator;
    
    [SerializeField] protected float _flipDeadZone = 1;

    [SerializeField] protected ActorBase actor;
    protected Camera _camera;
    protected bool isFlipped;
    protected float rotationCoef = 1f;

    [SerializeField] private PlayerCoords _playerCoords;

    [SerializeField] protected LadderMovement _ladderMovement;
    public LadderMovement LadderMovement => _ladderMovement;
    public PlayerCoords PlayerCoords => _playerCoords;
    public Vector2 FrameVelocity => _frameVelocity;

    [SerializeField] protected StaminaBase stamina;
    public ActorBase Actor => actor;
    public StaminaBase Stamina => stamina;

    // public PlayerStats PlayerStats => _stats;
    private float _frameLeftGrounded = float.MinValue;
    [SerializeField] private bool grounded;

    private float time;

    //lock player when ui window is open
    protected bool lockPlayer;
    [SerializeField] private bool startFalling;
    private bool wasSprintingOnJump;
    public bool WasSprintingOnJump => wasSprintingOnJump;

    public bool Grounded => grounded;
    public PlayerStats PlayerStats => playerStats;

    private bool _jumpToConsume;
    private bool _bufferedJumpUsable;
    private bool _endedJumpEarly;
    private bool _coyoteUsable;
    private float _timeJumpWasPressed;
    private AnimatorParameters animParam;

    private GameObject jumpPs;
    private GameObject fallingPs;
    private float startYPos;
    #region Interface

    public Vector2 FrameInput => _frameInput.Move;
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;

    #endregion
 
    public void SetLockPlayer(bool state) {
      lockPlayer = state;
    }

    public virtual void SetLockHighlight(bool state, string reason = "") {
    }

    protected virtual void Awake() {
      playerStats = GetComponent<PlayerStats>();
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

    protected virtual void Start() { }

    protected virtual void OnDestroy() {
      AnimationEventManager.onFootstep -= SpawnFootstepEffect;
    }

    protected virtual void JumpSound() {
      GameManager.Instance.AudioController.PlayPlayerJump();
    }

    protected virtual void JumpLandSound() {
      GameManager.Instance.AudioController.PlayPlayerJumpLanding();
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

    protected virtual void FlipX() { }

    private void GatherInput() {
      _frameInput = new FrameInput {
        JumpDown = GameManager.Instance.UserInput.controls.GamePlay.Jump
          .WasPerformedThisFrame(),
        JumpHeld = GameManager.Instance.UserInput.controls.GamePlay.Jump.IsPressed(),
        Move = GameManager.Instance.UserInput.GetMovement() 
      };

      if (PlayerStats.StatsObject.snapInput) {
        _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < PlayerStats.StatsObject.horizontalDeadZoneThreshold
          ? 0
          : Mathf.Sign(_frameInput.Move.x);
        _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < PlayerStats.StatsObject.verticalDeadZoneThreshold
          ? 0
          : Mathf.Sign(_frameInput.Move.y);
      }

      if (_frameInput.JumpDown) {
        _jumpToConsume = true;
        _timeJumpWasPressed = time;
      }
    }

    protected virtual void FixedUpdate() {
      CheckCollisions();

      HandleJump();
      HandleDirection();
      HandleGravity();

      ApplyMovement();
    }

    #region Collisions

    protected void CheckCollisions() {
      Physics2D.queriesStartInColliders = false;

      // Ground and Ceiling
      RaycastHit2D hit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down,
        PlayerStats.StatsObject.grounderDistance, ~PlayerStats.StatsObject.playerLayer);
      bool groundHit = hit.collider != null && !hit.collider.isTrigger;
      bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up,
        PlayerStats.StatsObject.grounderDistance, ~PlayerStats.StatsObject.playerLayer);

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

      SetAnimBool(animParam.GroundedHash, grounded);
      Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }

    private void SetAnimBool(int hash, bool state) {
      _animator.SetBool(hash, state);
    }
    
    public void ResetRobotToDefault() {
      //need to reset when only isBroken
      if (_animator.GetBool("IsBroken")) {
        _animator.SetBool("IsBroken", false);
        _animator.SetTrigger("Repair");
      }
    }

    private void PlayLandingEffect() {
      if (fallingPs != null && fallingPs.activeSelf) fallingPs.SetActive(false);
      GameManager.Instance.PoolEffects.SpawnFromPool("LandingEffect", transform.position, Quaternion.identity);
      JumpLandSound();
    }

    private void SpawnFootstepEffect() {
      if (Mathf.Approximately(Mathf.Sign(_frameVelocity.x), Mathf.Sign(transform.localScale.x))) {
        if (grounded && Mathf.Abs(_rb.linearVelocity.x) > 1) {
          GameManager.Instance.PoolEffects.SpawnFromPool("FootstepEffect", transform.position, Quaternion.identity);
        }
      }
    }

    #endregion


    #region Jumping

    private bool HasBufferedJump =>
      _bufferedJumpUsable && time < _timeJumpWasPressed + PlayerStats.StatsObject.jumpBuffer;

    private bool CanUseCoyote =>
      _coyoteUsable && !grounded && time < _frameLeftGrounded + PlayerStats.StatsObject.coyoteTime;

    protected void HandleJump() {
      if (actor != null && actor.IsDead) {
        return;
      }

      if (jumpPs != null && jumpPs.activeSelf) jumpPs.transform.position = transform.position;

      if (!_endedJumpEarly && !grounded && !_frameInput.JumpHeld && _rb.linearVelocity.y > 0) _endedJumpEarly = true;

      if (!_jumpToConsume && !HasBufferedJump) return;

      if (grounded || CanUseCoyote || _ladderMovement.IsClimbing) ExecuteJump();

      _jumpToConsume = false;
    }

    public void ResetAnimatorMovement() {
      _animator.SetFloat(animParam.VelocityXHash, 0f);
      _animator.SetFloat(animParam.VelocityYHash, 0f);
    }

    public void ResetAttackParam() {
      _animator.SetBool("IsAttacking", false);
    }

    public void SetAnimatorRespawn() {
      _animator.SetTrigger("Respawn");
    }

    private void ExecuteJump() {
      wasSprintingOnJump = stamina.IsSprinting;
      startFalling = false;
      _ladderMovement.SetClimbing(false, "jump");
      //Debug.LogError("start fall false");
      _animator.SetBool(animParam.FallHash, false);
      _animator.SetTrigger(animParam.JumpHash);
      _endedJumpEarly = false;
      _timeJumpWasPressed = 0;
      _bufferedJumpUsable = false;
      _coyoteUsable = false;
      _frameVelocity.y = PlayerStats.StatsObject.jumpPower;
      jumpPs = GameManager.Instance.PoolEffects.SpawnFromPool("JumpEffect", transform.position, Quaternion.identity).gameObject;
      Jumped?.Invoke();
      startYPos = transform.position.y;
      
      JumpSound();
    }

    public void RestoreHealth() {
      playerStats.AddHealth(playerStats.MaxHealth);
      actor.Respawn();
    }

    #endregion

    #region Horizontal

    protected void HandleDirection() {
      if (_frameInput.Move.x == 0) {
        //if we are on the ladder need to calculate deceleration like on ground
        var deceleration = grounded || _ladderMovement.IsOnLadder
          ? PlayerStats.StatsObject.groundDeceleration
          : PlayerStats.StatsObject.airDeceleration;
        _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
      }
      else {
        _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.Move.x * GetMaxSpeed(),
          PlayerStats.StatsObject.acceleration * Time.fixedDeltaTime);
      }

      if (_frameVelocity.x == 0) {
        SetAnimVelocityX(0);
        return;
      }

      
      var direction = Mathf.Sign(_frameVelocity.x) * GetVelocityParam();
      SetAnimVelocityX(direction);
    }

    //get animator param depend on currentMaxSpeed
    //1(-1) for walk and 2(-2) for sprint
    private int GetVelocityParam() {
      return GetMaxSpeed() > PlayerStats.MaxSpeed ? 2 : 1;
    }

    protected virtual float GetMaxSpeed() {
      var isMovingForward = Mathf.Approximately(Mathf.Sign(_frameVelocity.x), Mathf.Sign(transform.localScale.x));
      /*return isMovingForward ? (stamina.IsSprinting) ? PlayerStats.SprintSpeed : PlayerStats.MaxSpeed
        : PlayerStats.MaxBackSpeed;*/
      if (isMovingForward) {
        return stamina.IsSprinting ? PlayerStats.SprintSpeed : PlayerStats.MaxSpeed;
      }
      else {
        return stamina.IsSprinting ? PlayerStats.SprintBackSpeed : PlayerStats.MaxBackSpeed;
      }
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
    
    public void SetRotation(Vector3 rotation) {
      transform.localEulerAngles = rotation;
    }

    public void EnableCollider(bool state) {
      _col.enabled = state;
    }

    //reset flip when we sit in robot
    public void ResetLocalScale() {
      transform.localScale = Vector3.one;
    }

    #endregion

    #region Gravity

    protected void HandleGravity() {
      if (grounded && _frameVelocity.y <= 0f) {
        _frameVelocity.y = PlayerStats.StatsObject.groundingForce;
      }
      else {
        if (_endedJumpEarly) {
          if(transform.position.y - startYPos < PlayerStats.StatsObject.minJumpHeight)
            _endedJumpEarly = false;
        }
        
        var inAirGravity = PlayerStats.StatsObject.fallAcceleration;
        if (_endedJumpEarly && _frameVelocity.y > 0)
          inAirGravity *= PlayerStats.StatsObject.jumpEndEarlyGravityModifier;
        _frameVelocity.y =
          Mathf.MoveTowards(_frameVelocity.y, -PlayerStats.StatsObject.maxFallSpeed,
            inAirGravity * Time.fixedDeltaTime);
        SetFall();
      }
      if (fallingPs != null && fallingPs.activeSelf) fallingPs.transform.position = transform.position;
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

      fallingPs = GameManager.Instance.PoolEffects.SpawnFromPool("FallingLoopEffect", transform.position, Quaternion.identity).gameObject;
    }

    #endregion

    protected void ApplyMovement() {
      if (!_ladderMovement.IsClimbing) _rb.linearVelocity = _frameVelocity;
    }
    
    public Vector2 GetVelocity() {
      return _rb.linearVelocity;
    }

/*#if UNITY_EDITOR
    private void OnValidate() {
      if (PlayerStats.StatsObject == null)
        Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
    }
#endif*/
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