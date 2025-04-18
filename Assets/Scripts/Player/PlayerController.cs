using UnityEngine;

namespace Player {
  [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
  public class PlayerController : PlayerControllerBase, IPlayerController {
    [SerializeField] private float topAngleLimit = 20;
    [SerializeField] private float bottomAngleLimit = -20;
    [SerializeField] private PlayerAttack playerAttack;
    //[SerializeField] private ActorPlayer actor;

    protected override void Awake() {
      base.Awake();
      GameManager.Instance.PlayerController = this;
      GameManager.Instance.CurrPlayerController = this;
    }
    
    public override void SetLockHighlight(bool state) {
      playerAttack.LockHighlight(state);
    }
    
    protected override void FlipX() {
      if(GameManager.Instance.WindowsController.IsAnyWindowOpen)
        return;
      
      Vector2 mousePosition = _camera.ScreenToWorldPoint(GameManager.Instance.UserInput.GetMousePosition());
      var direction = (mousePosition - (Vector2)transform.position).normalized;

      if (Mathf.Abs(mousePosition.x - transform.position.x) > _flipDeadZone) {
        // Flip player
        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Sign(direction.x);
        transform.localScale = localScale;

        rotationCoef = isFlipped ? -1f : 1f;
        direction.x *= rotationCoef;
      }
    }
    
    /*protected override void LookAtMouse() {
      if (lockPlayer) {
        return;
      }
      base.LookAtMouse();
      
      var dir = ((Vector2)_camera.ScreenToWorldPoint(GameManager.Instance.UserInput.GetMousePosition()) - (Vector2)Head.position);
      dir.x *= Mathf.Sign(transform.localScale.x);
      // Calculate the target angle based on the direction
      float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

      // Clamp the target angle within the specified limits
      float clampedAngle = Mathf.Clamp(targetAngle, bottomAngleLimit, topAngleLimit) *
                           Mathf.Sign(transform.localScale.x);

      // Apply the clamped angle to the head
      Head.rotation = Quaternion.Euler(0, 0, clampedAngle + Mathf.Sign(transform.localScale.x) * 90);
    }*/
  }
}