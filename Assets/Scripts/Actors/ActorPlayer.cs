using System;

namespace Actors {
  public class ActorPlayer : ActorBase {
    public static event Action OnPlayerDeath;
    public override void Damage(float damage) {
      base.Damage(damage);
      if(stats.Health <= 0) OnPlayerDeath?.Invoke();
    }
  }
}