using UnityEngine;

namespace Scriptables.Items {
  [CreateAssetMenu(menuName = "Inventory System/Items/Tool", fileName = "New Tool")]
  public class Tool : ItemObject, IAttackableItem {
    [SerializeField] private bool useSelfAnim = false;
    [SerializeField] private LayerMask attackLayer;
    [SerializeField] private float blockDamage = 5f;
    [SerializeField] private float entityDamage = 3f;
    [SerializeField] private float range = 1.8f;
    [SerializeField] private float timeBtwAttacks = 0.4f;
    [SerializeField] private float staminaUsage = 5f;
    [SerializeField] private int animationattackID = 0;
    [SerializeField] private Vector2 colliderSize = new Vector2(1f, 1f);
    [SerializeField] private int maxTargets = 1;

    [Tooltip("When you need to use tool animation for attacking")]
    public bool UseSelfAnim => useSelfAnim;
    public LayerMask AttackLayer => attackLayer;
    public float BlockDamage => blockDamage;
    public float EntityDamage => entityDamage;
    public float Range => range;
    public float TimeBtwAttacks => timeBtwAttacks;
    public float StaminaUsage => staminaUsage;
    public int AnimationAttackID => animationattackID;
    public Vector2 ColliderSize => colliderSize;
    public int MaxTargets => maxTargets;

    public void Awake() {
      Type = ItemType.Tool;
    }
  }
}