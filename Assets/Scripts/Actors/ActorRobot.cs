using System;

namespace Actors {
  public class ActorRobot : ActorBase {
    public static event Action OnRobotBroked;
    public static event Action OnRobotRepaired;

    protected override void Awake() {
      base.Awake();
      DamageableType = DamageableType.Robot;
    }

    public override void Damage(float damage, bool isPlayer) {
      base.Damage(damage, isPlayer);
      if (IsDead) {
        OnRobotBroked?.Invoke();
      }
    }

    public override void Respawn() {
      base.Respawn();
      hasTakenDamage = false;
      OnRobotRepaired?.Invoke();
    }
  }
}