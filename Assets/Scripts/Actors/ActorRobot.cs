using System;

namespace Actors {
  public class ActorRobot : ActorBase {
    public static event Action OnRobotBroked;
    public override void Damage(float damage) {
      base.Damage(damage);
      if (IsDead) {
        OnRobotBroked?.Invoke();
      }
    }
  }
}