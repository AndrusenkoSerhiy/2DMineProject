using UnityEngine;

namespace Scriptables.Items {
  [CreateAssetMenu(menuName = "Inventory System/Items/Tool", fileName = "New Tool")]
  public class Tool : ItemObject {
    public LayerMask AttackLayer;
    public float BlockDamage = 5f;
    public float EntityDamage = 3f;
    public float Range = 1.8f;
    public float AttacksPerMinute = 65;
    public float StaminaUsage = 5f;

    public void Awake() {
      Type = ItemType.Tool;
    }
  }
}