// using Animation;
using System.Collections;
using System.Collections.Generic;
using Animation;
using UnityEngine;
using Settings;
using Items;
using Scriptables;
using Scriptables.Items;
using Tools;
using World;

namespace Player {
  public class PlayerAttack : AttackCollider {
    [SerializeField] private PlayerStats stats;
    [SerializeField] private Animator animator;
    [SerializeField] private ObjectHighlighter objectHighlighter;
    public bool shouldBeDamaging { get; private set; } = false;

    private List<IDamageable> iDamageables = new List<IDamageable>();

    private LayerMask attackLayer;
    private float blockDamage;
    private float entityDamage;
    private float attackRange;
    private float timeBtwAttacks;
    //TODO
    private float staminaUsage;
    private int attackID;
    private float attackTimeCounter;
    private Vector2 colliderSize;
    //private IDamageable currentTarget;
    private List<IDamageable> targets = new();
    
    private Renderer currentTargetRenderer;
    private PlayerEquipment playerEquipment;
    private void Awake() {
      AnimationEventManager.onAttackStarted += HandleAnimationStarted;
      AnimationEventManager.onAttackEnded += HandleAnimationEnded;
      playerEquipment = GetComponent<PlayerEquipment>();
      playerEquipment.OnEquippedWeapon += UpdateAttackParam;
      GameManager.instance.PlayerAttack = this;
    }
    //public IDamageable CurrentTarget => currentTarget;
    public float BlockDamage => blockDamage;
    private void OnDestroy() {
      AnimationEventManager.onAttackStarted -= HandleAnimationStarted;
      AnimationEventManager.onAttackEnded -= HandleAnimationEnded;
      playerEquipment.OnEquippedWeapon -= UpdateAttackParam;
    }

    private void Start() {
      PrepareAttackParams();
      attackTimeCounter = timeBtwAttacks;
    }

    private void UpdateAttackParam() {
      //Debug.LogError("UpdateAttackParam");
      SetAttackParamsFromEquipment();
      //try to activate tool
      ToolBase tool = playerEquipment.Weapon.GetComponent<ToolBase>();
      tool?.Activate();
      UpdateParams(.5f, attackRange, colliderSize.x, colliderSize.y);
    }

    private void PrepareAttackParams() {
      if (SetAttackParamsFromEquipment()) {
        return;
      }

      Debug.LogWarning("Could not set attack parameters from equipment", this);

      attackLayer = stats.AttackLayer;
      blockDamage = stats.BlockDamage;
      entityDamage = stats.EntityDamage;
      attackRange = stats.Range;
      timeBtwAttacks = stats.TimeBtwAttacks;
      staminaUsage = stats.StaminaUsage;
      attackID = stats.AttackID;
    }

    private bool SetAttackParamsFromEquipment() {
      if (playerEquipment == null) {
        Debug.LogWarning("Could not find Player Equipment", this);
        return false;
      }

      if (playerEquipment.Weapon == null) {
        Debug.LogWarning("Could not find equipped weapon", this);
        return false;
      }

      ItemObject weaponStats = playerEquipment.Weapon.GetComponent<GroundItem>().item;
      if (!(weaponStats is IAttackableItem attackableItem)) {
        Debug.LogWarning("Equipped item is not attackable", this);
        return false;
      }
      //Debug.LogError("SetAttackParamsFromEquipment");
      attackLayer = attackableItem.AttackLayer;
      blockDamage = attackableItem.BlockDamage;
      entityDamage = attackableItem.EntityDamage;
      attackRange = attackableItem.Range;
      timeBtwAttacks = attackableItem.TimeBtwAttacks;
      staminaUsage = attackableItem.StaminaUsage;
      attackID = attackableItem.AnimationAttackID;
      colliderSize = attackableItem.ColliderSize;
      return true;
    }

    protected override void Update() {
      base.Update();
      //HighlightTarget();
      HandleAttack();
    }

    private void HandleAttack() {
      if (UserInput.instance.IsAttacking() /*&& currentTarget != null*/
          && attackTimeCounter >= timeBtwAttacks) {
        TriggerAttack();
      }

      attackTimeCounter += Time.deltaTime;
    }

    private void TriggerAttack() {
      if(UserInput.instance.IsBuildMode)
        return;
      
      attackTimeCounter = 0f;
      animator.SetTrigger("Attack");
      animator.SetInteger("WeaponID", attackID);
    }

    private void ClearTarget() {
      targets.Clear();
    }
    
    private void Attack() {
      shouldBeDamaging = true;
      SetTargetsFromHighlight();
      foreach (var target in targets) {
        if (target == null || target.hasTakenDamage) continue;
        target.Damage(blockDamage);
        iDamageables.Add(target);
      }

      ReturnAttackableToDamageable();
    }
    
    private void ReturnAttackableToDamageable() {
      foreach (IDamageable damaged in iDamageables) {
        damaged.hasTakenDamage = false;
      }

      iDamageables.Clear();
    }
    
    private void SetTargetsFromHighlight() {
      foreach (var elem in objectHighlighter.Highlights) {
        var pos = CoordsTransformer.WorldToGrid(elem.transform.position);
        var test = GameManager.instance.ChunkController.GetCell(pos.X, pos.Y);
        if (test != null) targets.Add(test);
      }
    }

    private void DestroyTarget() {
      foreach (var t in targets) {
        if (t == null) continue;
        var getHp = t.GetHealth();
        if (getHp <= 0) {
          t.DestroyObject();
        }
      }

      ClearTarget();
    }

    private void ShouldBeDamagingToFalse() {
      shouldBeDamaging = false;
    }

    private void OnDrawGizmosSelected() {
      Gizmos.DrawWireSphere(attackTransform.position, attackRange);
      Gizmos.color = Color.red;
    }

    #region Animation Triggers

    private void HandleAnimationStarted(AnimationEvent animationEvent, GameObject go) {
      if(go != gameObject)
        return;
      Attack();
      // Debug.Log("Attack started");
      for (int i = 0; i < targets.Count; i++) {
        targets[i]?.AfterDamageReceived();
      }
    }

    private void HandleAnimationEnded(AnimationEvent animationEvent, GameObject go) {
      if(go != gameObject)
        return;
      ShouldBeDamagingToFalse();
      // Debug.Log("Attack ended");
      DestroyTarget();
    }

    #endregion
  }
}