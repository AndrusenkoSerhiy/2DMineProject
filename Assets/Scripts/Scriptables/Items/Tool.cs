using UnityEngine;

namespace Scriptables.Items {
  [CreateAssetMenu(menuName = "Inventory System/Items/Tool", fileName = "New Tool")]
  public class Tool : ItemObject, IAttackableItem, IRepairable, IDurableItem {
    public WeaponType weaponType;
    
    [SerializeField] private bool useSelfAnim = false;
    [SerializeField] private LayerMask attackLayer;
    [SerializeField] private int animationattackID = 0;
    [SerializeField] private Vector2 colliderSize = new Vector2(1f, 1f);
    [SerializeField] private int maxTargets = 1;
    [Tooltip("Count of repair kits")] 
    [SerializeField] private int repairCost;
    [SerializeField] private float maxDurability = 100f;
    [SerializeField] private float durabilityUse = 0.5f;
    [SerializeField] private DurabilityUsageType durabilityUsageType = DurabilityUsageType.OnHit;
    [Tooltip("When you need to use tool animation for attacking")]
    public WeaponType WeaponType => weaponType;
    public LayerMask AttackLayer => attackLayer;
    public int AnimationAttackID => animationattackID;
    public Vector2 ColliderSize => colliderSize;
    public int MaxTargets => maxTargets;
    public int RepairCost => repairCost;
    public float MaxDurability => maxDurability;
    public float DurabilityUse => durabilityUse;
    public DurabilityUsageType DurabilityUsageType => durabilityUsageType;

    public void Awake() {
      Type = ItemType.Tool;
    }
  }
}