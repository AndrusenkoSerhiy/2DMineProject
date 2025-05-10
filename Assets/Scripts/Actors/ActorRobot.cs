using System;

namespace Actors {
  public class ActorRobot : ActorBase {
    public static event Action OnRobotBroked;
    public override void Damage(float damage, bool isPlayer) {
      base.Damage(damage, isPlayer);
      if (IsDead) {
        OnRobotBroked?.Invoke();
      }
    }
  }
}