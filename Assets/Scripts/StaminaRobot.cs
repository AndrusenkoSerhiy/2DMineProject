using Player;
using UnityEngine;

public class StaminaRobot : StaminaBase {
  private MiningRobotController robotController;
  
  public override void Start() {
    base.Start();
    robotController = GameManager.Instance.MiningRobotController;
  }
  
  public override void SetSprinting(bool value) {
    //block use stamina if she not enough 
    if (value && (stats.Stamina < minStamina || Mathf.Sign(robotController.GetMoveForward()) < 0) ||
        (value && userInput.GetMovement().Equals(Vector2.zero)) ||
        !robotController.Grounded && !robotController.WasSprintingOnJump) {
      return;
    }

    isSprinting = value;
  }
}