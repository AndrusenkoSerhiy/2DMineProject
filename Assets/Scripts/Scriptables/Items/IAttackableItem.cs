using UnityEngine;

namespace Scriptables.Items {
  public interface IAttackableItem {
    public WeaponType WeaponType { get; }
    public int MagazineSize { get; }
    public ItemObject Ammo { get; }
    public float AmmoSpeed { get; }
    public float ReloadTime { get; }
    public AudioData ReloadSound { get; }
    public LayerMask AttackLayer { get; }
    public int AnimationAttackID { get; }
    public Vector2 ColliderSize { get; }
    public int MaxTargets { get; }
  }
}
