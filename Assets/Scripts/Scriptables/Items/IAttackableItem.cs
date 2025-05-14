using UnityEngine;

namespace Scriptables.Items {
  public interface IAttackableItem {
    public WeaponType WeaponType { get; }
    public LayerMask AttackLayer { get; }
    //this moves to modifiers
    /*public float BlockDamage { get; }
    public float EntityDamage { get; }
    public float Range { get; }
    public float TimeBtwAttacks { get; }
    public float StaminaUsage { get; }*/
    public int AnimationAttackID { get; }
    public Vector2 ColliderSize { get; }
    public int MaxTargets { get; }
  }
}
