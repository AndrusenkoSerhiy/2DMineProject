using System.Collections.Generic;
using Animation;
using Items;
using Scriptables;
using Scriptables.Items;
using Settings;
using UnityEngine;
using World;

namespace Player {
  public class MiningRobotAttack : MonoBehaviour {
    [SerializeField] private Transform attackTransform;
    [SerializeField] private BoxCollider2D attackCollider;
    [SerializeField] private PlayerStats stats;
    [SerializeField] private Animator animator;

    [SerializeField] private Transform colliderTR;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 5f;
    public bool shouldBeDamaging { get; private set; } = false;

    private List<IDamageable> iDamageables = new();
    private LayerMask attackLayer;
    private float blockDamage;
    private float entityDamage;
    private float attackRange;

    private float timeBtwAttacks;

    //TODO
    private float staminaUsage;
    private int attackID;
    private float attackTimeCounter;
    private PlayerEquipment playerEquipment;
    [SerializeField] private ObjectHighlighter objectHighlighter;
    private List<IDamageable> targets = new();

    private void Awake() {
      AnimationEventManager.onAttackStarted += HandleAnimationStarted;
      AnimationEventManager.onAttackEnded += HandleAnimationEnded;
      playerEquipment = GetComponent<PlayerEquipment>();
      attackCollider.enabled = false;
    }

    private void OnDestroy() {
      AnimationEventManager.onAttackStarted -= HandleAnimationStarted;
      AnimationEventManager.onAttackEnded -= HandleAnimationEnded;
    }

    private void Start() {
      PrepareAttackParams();
      attackTimeCounter = timeBtwAttacks;
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
      return true;
    }

    private void Update() {
      UpdateColliderPos();
      HandleAttack();
    }

    // Handle attack
    private void HandleAttack() {
      if (UserInput.instance.IsAttacking() /*&& currentTarget != null*/
          && attackTimeCounter >= timeBtwAttacks) {
        TriggerAttack();
      }

      attackTimeCounter += Time.deltaTime;
    }

    private void UpdateColliderPos() {
      var mousePos = GetMousePosition();
      // Calculate direction and distance from parent
      var parentPosition = transform.position;
      var direction = (mousePos - parentPosition).normalized;
      var distance = Vector3.Distance(parentPosition, mousePos);

      // Clamp the distance between minDistance and maxDistance
      var clampedDistance = Mathf.Clamp(distance, minDistance, maxDistance);

      // Set the new position of the child collider
      var newPosition = parentPosition + direction * clampedDistance;
      newPosition.z = 0f;
      colliderTR.position = newPosition;
    }

    private Vector3 GetMousePosition() {
      return GameManager.instance.MainCamera.ScreenToWorldPoint(UserInput.instance.GetMousePosition());
    }

    private void TriggerAttack() {
      if (UserInput.instance.IsBuildMode)
        return;
      
      attackTimeCounter = 0f;
      animator.SetTrigger("Attack");
      animator.SetInteger("WeaponID", attackID);
    }

    private void SetTargetsFromHighlight() {
      foreach (var elem in objectHighlighter.Highlights) {
        var pos = CoordsTransformer.WorldToGrid(elem.transform.position);
        var test = GameManager.instance.ChunkController.GetCell(pos.X, pos.Y);
        if (test != null) targets.Add(test);
      }
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

    private void ReturnAttackableToDamageable() {
      foreach (IDamageable damaged in iDamageables) {
        damaged.hasTakenDamage = false;
      }

      iDamageables.Clear();
    }

    private void ShouldBeDamagingToFalse() {
      shouldBeDamaging = false;
    }

    public void EnableAttackCollider(bool state) {
      attackCollider.enabled = state;
    }

    public void ClearHighlights() {
      objectHighlighter.ClearHighlights();
    }

    #region Animation Triggers

    private void HandleAnimationStarted(AnimationEvent animationEvent, GameObject go) {
      if (go != gameObject)
        return;
      
      Attack();

      for (int i = 0; i < targets.Count; i++) {
        targets[i]?.AfterDamageReceived();
      }
    }

    private void HandleAnimationEnded(AnimationEvent animationEvent, GameObject go) {
      if (go != gameObject)
        return;
      ShouldBeDamagingToFalse();
      DestroyTarget();
    }

    #endregion
  }
}