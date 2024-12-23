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

namespace Player {
  public class PlayerAttack : MonoBehaviour {
    [SerializeField] private Transform attackTransform;

    [SerializeField] private PlayerStats stats;

    [SerializeField] private Animator animator;

    public bool shouldBeDamaging { get; private set; } = false;

    private List<IDamageable> iDamageables = new List<IDamageable>();

    private LayerMask attackLayer;
    private float blockDamage;
    private float entityDamage;
    private float attackRange;
    private float timeBtwAttacks;
    //TODO
    private float staminaUsage;
    private float attackTimeCounter;

    private IDamageable currentTarget;
    private Renderer currentTargetRenderer;
    [SerializeField] private bool _useToolAnimation = false;
    private PlayerEquipment playerEquipment;
    private void Awake() {
      AnimationEventManager.onAttackStarted += HandleAnimationStarted;
      AnimationEventManager.onAttackEnded += HandleAnimationEnded;
      playerEquipment = GetComponent<PlayerEquipment>();
      playerEquipment.OnEquippedWeapon += UpdateAttackParam;
    }

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
      Debug.LogError("UpdateAttackParam");
      SetAttackParamsFromEquipment();
      //try to activate tool
      ToolBase tool = playerEquipment.Weapon.GetComponent<ToolBase>();
      tool?.Activate();
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
      Debug.LogError("SetAttackParamsFromEquipment");
      _useToolAnimation = attackableItem.UseSelfAnim;
      attackLayer = attackableItem.AttackLayer;
      blockDamage = attackableItem.BlockDamage;
      entityDamage = attackableItem.EntityDamage;
      attackRange = attackableItem.Range;
      timeBtwAttacks = attackableItem.TimeBtwAttacks;
      staminaUsage = attackableItem.StaminaUsage;

      return true;
    }

    private void Update() {
      HighlightTarget();

      if (_useToolAnimation)
        return;
      
      // Handle attack
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
    }

    private void HighlightTarget() {
      Vector3 playerPosition = attackTransform.position;
      //todo кешировать камеру
      Vector3 mousePoint = Camera.main.ScreenToWorldPoint(UserInput.instance.GetMousePosition());//Input.mousePosition

      Debug.DrawLine(playerPosition, mousePoint, Color.red);  // Visualize ray

      RaycastHit2D hit = Physics2D.Raycast(playerPosition, mousePoint - playerPosition, attackRange, attackLayer);

      if (hit.collider != null && hit.collider.TryGetComponent(out IDamageable iDamageable)) {
        if (currentTarget != iDamageable) {
          ClearTarget();
          SetTarget(iDamageable);
          Highlight();
        }
      }
      else {
        ClearTarget();
      }
    }

    private void SetTarget(IDamageable target) {
      currentTarget = target;
      if (currentTarget is MonoBehaviour mb) {
        currentTargetRenderer = mb.GetComponentInChildren<Renderer>();
      }
    }

    private void RemoveTarget() {
      currentTarget = null;
      currentTargetRenderer = null;
    }

    private void ClearTarget() {
      if (currentTarget == null) {
        return;
      }

      ResetHighlight();
      RemoveTarget();
    }

    private void Highlight() {
      //                                     if current target is already dead
      if (currentTargetRenderer == null || currentTarget.GetHealth() <= 0) {
        return;
      }
      currentTargetRenderer.material.SetInt("_ShowOutline", 1);
    }

    private void ResetHighlight() {
      if (currentTargetRenderer == null) {
        return;
      }
      currentTargetRenderer.material.SetInt("_ShowOutline", 0);
    }

    private void Attack() {
      if (currentTarget == null || currentTarget.hasTakenDamage) {
        return;
      }

      currentTarget.Damage(blockDamage);
      iDamageables.Add(currentTarget);
    }

    private void DestroyTarget() {
      if (currentTarget == null) return;

      float hp = currentTarget.GetHealth();
      if (hp <= 0) {
        currentTarget.DestroyObject();
        ClearTarget();
      }
    }

    public IEnumerator DamageWhileSlashIsActive() {
      shouldBeDamaging = true;

      while (shouldBeDamaging) {
        Attack();

        yield return null;
      }

      ReturnAttackablesToDamageable();
    }

    private void ReturnAttackablesToDamageable() {
      foreach (IDamageable damaged in iDamageables) {
        damaged.hasTakenDamage = false;
      }

      iDamageables.Clear();
    }

    public void ShouldBeDamagingToFalse() {
      shouldBeDamaging = false;
    }

    private void OnDrawGizmosSelected() {
      Gizmos.DrawWireSphere(attackTransform.position, attackRange);

      Gizmos.color = Color.red;
      Gizmos.DrawWireSphere(attackTransform.position, attackRange);
    }

    #region Animation Triggers

    private void HandleAnimationStarted(AnimationEvent animationEvent, GameObject go) {
      if(go != gameObject)
        return;
      StartCoroutine(DamageWhileSlashIsActive());
      // Debug.Log("Attack started");
      currentTarget?.AfterDamageReceived();
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