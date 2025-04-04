using UnityEngine;

namespace Player {
  public class MiningRobotAttack : BaseAttack {
    protected override void Awake() {
      base.Awake();
      LockHighlight(true);
    }

    protected override void TriggerAttack() {
      base.TriggerAttack();
      animator.SetTrigger("Attack");
      animator.SetInteger("WeaponID", attackID);
    }
    //get param from robot stats
    protected override void PrepareAttackParams() {
      attackLayer = stats.AttackLayer;
      blockDamage = stats.BlockDamage;
      entityDamage = stats.EntityDamage;
      attackRange = stats.Range;
      timeBtwAttacks = stats.TimeBtwAttacks;
      staminaUsage = stats.StaminaUsage;
      attackID = stats.AttackID;
    }
  }
}