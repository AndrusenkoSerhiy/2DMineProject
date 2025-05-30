using UnityEngine;

namespace Player {
  public class MiningRobotAttack : BaseAttack {
    private bool lockAttack;
    protected override void Awake() {
      base.Awake();
      LockHighlight(true);
    }

    protected override void TriggerAttack() {
      if(lockAttack)
        return;
        
      base.TriggerAttack();
      if (!firstAttack) {
        firstAttack = true;
        animator.SetTrigger("Attack");
        animator.SetInteger("WeaponID", attackID);
      }
    }
    
    public override void GetDirection() {
      Vector2 direction = attackCollider.transform.position - transform.position;
      //Debug.LogError($"directionY {direction.y}");

      //6f distance between player and mouse for top border 
      if (direction.y > 6f) {
        lookDirection = 1;
        
        if (Mathf.Abs(direction.x) > 1.5f) {
          lookDirection = 2;
        }
      }
      else if (direction.y < .3f) {
        lookDirection = -1;
        if (Mathf.Abs(direction.x) > 1.5f) {
          lookDirection = -2;
        }
      }
      else {
        lookDirection = 0;
      }

      animator.SetInteger(animParam.LookDirection, lookDirection);
    }

    //get param from robot stats
    protected override void PrepareAttackParams() {
      attackLayer = statsObject.attackLayer;
      attackID = statsObject.attackID;
    }
    //use when build mode is enabled, we don't want attack
    //and we can get attack collider position for placing cells
    public void LockAttack(bool state) {
      lockAttack = state;
    }

    public void SetMaxTargets(int value) {
      objectHighlighter.SetMaxHighlights(value);
    }
  }
}