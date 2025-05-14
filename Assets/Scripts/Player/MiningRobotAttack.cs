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
      animator.SetTrigger("Attack");
      animator.SetInteger("WeaponID", attackID);
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