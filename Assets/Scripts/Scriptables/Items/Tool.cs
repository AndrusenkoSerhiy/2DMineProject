using UnityEngine;

namespace Scriptables.Items {
  [CreateAssetMenu(menuName = "Inventory System/Items/Tool", fileName = "New Tool")]
  public class Tool : ItemObject, IAttackableItem {
    public LayerMask attackLayer;
    public float blockDamage = 5f;
    public float entityDamage = 3f;
    public float range = 1.8f;
    public float timeBtwAttacks = 0.4f;
    public float staminaUsage = 5f;

    public LayerMask AttackLayer => attackLayer;
    public float BlockDamage => blockDamage;
    public float EntityDamage => entityDamage;
    public float Range => range;
    public float TimeBtwAttacks => timeBtwAttacks;
    public float StaminaUsage => staminaUsage;

    public void Awake() {
      Type = ItemType.Tool;
    }
  }
}