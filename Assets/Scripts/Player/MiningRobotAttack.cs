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
      attackLayer = statsObject.attackLayer;
      /*var entityStats = GetEntityStats();
      blockDamage = entityStats.BlockDamage;
      entityDamage = entityStats.EntityDamage;
      attackRange = entityStats.AttackRange;
      timeBtwAttacks = entityStats.TimeBtwAttacks;
      staminaUsage = entityStats.AttackStaminaUsage;*/
      
      attackID = statsObject.attackID;
    }
  }
}