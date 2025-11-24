using Player;
using UnityEngine;

public class StaminaPlayer : StaminaBase {
  private PlayerController playerController;

  public override void Start() {
    base.Start();
    playerController = GameManager.Instance.PlayerController;
  }

  protected override void SetSprinting(bool value) {
    //block use stamina if she not enough 
    if (value && (stats.Stamina < minStamina) || /*Mathf.Sign(playerController.GetMoveForward()) < 0) ||*/
        /*(value && userInput.GetMovement().Equals(Vector2.zero)) ||*/
        !playerController.Grounded && !playerController.WasSprintingOnJump) {
      return;
    }

    base.SetSprinting(value);
  }
}