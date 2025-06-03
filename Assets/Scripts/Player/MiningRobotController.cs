using Actors;
using Animation;
using Scriptables.Repair;
using UnityEngine;

namespace Player {
  public class MiningRobotController : PlayerControllerBase, IPlayerController{
    public RobotObject robotObject;
    [SerializeField] private MiningRobotAttack miningRobotAttack;

    protected override void Awake() {
      base.Awake();
      GameManager.Instance.MiningRobotController = this;
      EnableController(false);
      // stamina.SetStaminaBarRef();
      _ladderMovement = GameManager.Instance.PlayerLadderMovement;
      AnimationEventManager.onLeftStep += GameManager.Instance.AudioController.PlayRobotStep;
      AnimationEventManager.onRightStep += GameManager.Instance.AudioController.PlayRobotStep;
    }

    protected override void FixedUpdate() {
      CheckCollisions();
      HandleGravity();
      
      //if player exit from robot but under the cells
      if (GameManager.Instance.CurrPlayerController.Actor is ActorPlayer)
        return;
      
      HandleJump();
      HandleDirection();
      ApplyMovement();
    }

    protected override void LookAtMouse() {
      if (lockPlayer) {
        return;
      }

      base.LookAtMouse();
    }

    public override void SetLockHighlight(bool state, string reason = "") {
      miningRobotAttack.LockHighlight(state, reason);
    }

    public void ClearLockList() {
      miningRobotAttack.ClearLockList();
    }

    protected override void FlipX() {
      if (GameManager.Instance.CurrPlayerController.Actor is ActorPlayer)
        return;
      
      Vector2 mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
      var direction = (mousePosition - (Vector2)transform.position).normalized;

      if (Mathf.Abs(mousePosition.x - transform.position.x) > _flipDeadZone) {
        // Flip player
        var localScale = transform.localScale;
        localScale.x = Mathf.Sign(direction.x);
        transform.localScale = localScale;

        rotationCoef = isFlipped ? -1f : 1f;
        direction.x *= rotationCoef;
      }
    }
    
    /*protected override float GetMaxSpeed() {
      return (Mathf.Approximately(Mathf.Sign(_frameVelocity.x), Mathf.Sign(transform.localScale.x)))
        ? _stats.MaxSpeed : _stats.MaxBackSpeed;
    }*/
    protected override float GetMaxSpeed() {
      var isMovingForward = Mathf.Approximately(Mathf.Sign(_frameVelocity.x), Mathf.Sign(transform.localScale.x));
      return isMovingForward ? (stamina.IsSprinting) ? PlayerStats.SprintSpeed : PlayerStats.MaxSpeed
        : PlayerStats.MaxBackSpeed;
    }

    public override void EnableController(bool state) {
      ResetMovement();
      miningRobotAttack.enabled = state;
      enabled = state;
    }

    public void EnableAttack(bool state) {
      miningRobotAttack.LockAttack(!state);
    }
    
    public void SetMaxTargets(int value) {
      miningRobotAttack.SetMaxTargets(value);
      miningRobotAttack.LockHighlight(value == 0, "ChangeMode",false);
    }

    private void ResetMovement() {
      _rb.linearVelocity = Vector2.zero;
      _frameVelocity = Vector2.zero;
    }

    public void SetRBType(RigidbodyType2D bodyType) {
      _rb.bodyType = bodyType;
    }
    
    protected override void JumpSound() {
      GameManager.Instance.AudioController.PlayAudio(robotObject?.jumpAudioData);
    }

    protected override void JumpLandSound() {
      GameManager.Instance.AudioController.PlayAudio(robotObject?.jumpLandingAudioData);
    }
  }
}