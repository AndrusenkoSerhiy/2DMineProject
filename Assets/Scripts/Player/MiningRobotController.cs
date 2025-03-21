using UnityEngine;

namespace Player {
  public class MiningRobotController : PlayerControllerBase, IPlayerController{
    [SerializeField] private MiningRobotAttack miningRobotAttack;

    protected override void Awake() {
      base.Awake();
      GameManager.Instance.MiningRobotController = this;
      EnableController(false);
      _stamina.SetStaminaBarRef();
      _ladderMovement = GameManager.Instance.PlayerLadderMovement;
    }

    protected override void LookAtMouse() {
      if (lockPlayer) {
        return;
      }

      base.LookAtMouse();
    }

    public override void SetLockHighlight(bool state) {
      miningRobotAttack.LockHighlight(state);
    }

    protected override void FlipX() {
      Vector2 mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
      var direction = (mousePosition - (Vector2)Head.position).normalized;

      if (Mathf.Abs(mousePosition.x - Head.transform.position.x) > _flipDeadZone) {
        // Flip player
        var localScale = transform.localScale;
        localScale.x = Mathf.Sign(direction.x);
        transform.localScale = localScale;

        rotationCoef = isFlipped ? -1f : 1f;
        direction.x *= rotationCoef;
      }
    }
    
    protected override float GetMaxSpeed() {
      return (Mathf.Approximately(Mathf.Sign(_frameVelocity.x), Mathf.Sign(transform.localScale.x)))
        ? _stats.MaxSpeed : _stats.MaxBackSpeed;
    }

    public override void EnableController(bool state) {
      miningRobotAttack.enabled = state;
      enabled = state;
    }
  }
}