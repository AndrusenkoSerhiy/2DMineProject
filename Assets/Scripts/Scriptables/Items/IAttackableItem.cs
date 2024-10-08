using UnityEngine;

namespace Scriptables.Items {
  public interface IAttackableItem {
    public LayerMask AttackLayer { get; }
    public float BlockDamage { get; }
    public float EntityDamage { get; }
    public float Range { get; }
    public float TimeBtwAttacks { get; }
    public float StaminaUsage { get; }
  }
}
