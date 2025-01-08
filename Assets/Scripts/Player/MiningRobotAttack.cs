using System.Collections;
using System.Collections.Generic;
using Animation;
using Items;
using Scriptables;
using Scriptables.Items;
using Settings;
using Tools;
using UnityEngine;
using World;

namespace Player {
  public class MiningRobotAttack : MonoBehaviour {
    [SerializeField] private Transform attackTransform;

    [SerializeField] private PlayerStats stats;

    [SerializeField] private Animator animator;

    [SerializeField] private Transform _colliderTR;
    [SerializeField] private BoxCollider2D _collider;
    [SerializeField] private float _minDistance = 2f;
    [SerializeField] private float _maxDistance = 5f;
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
    
    private Renderer currentTargetRenderer;
    private PlayerEquipment playerEquipment;
    private void Awake() {
      AnimationEventManager.onAttackStarted += HandleAnimationStarted;
      AnimationEventManager.onAttackEnded += HandleAnimationEnded;
      playerEquipment = GetComponent<PlayerEquipment>();
    }

    public float BlockDamage => blockDamage;
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
      HighlightTarget();
      UpdateColliderPos();
      
      // Handle attack
      if (UserInput.instance.IsAttacking() /*&& currentTarget != null*/
        && attackTimeCounter >= timeBtwAttacks) {
        TriggerAttack();
      }

      attackTimeCounter += Time.deltaTime;
    }

    private void UpdateColliderPos() {
      var mousePos = GetMousePosition();
      // Calculate direction and distance from parent
      Vector3 parentPosition = transform.position;
      Vector3 direction = (mousePos - parentPosition).normalized;
      float distance = Vector3.Distance(parentPosition, mousePos);

      // Clamp the distance between minDistance and maxDistance
      float clampedDistance = Mathf.Clamp(distance, _minDistance, _maxDistance);

      // Set the new position of the child collider
      Vector3 newPosition = parentPosition + direction * clampedDistance;
      _colliderTR.position = newPosition;
    }

    private Vector3 GetMousePosition() {
      return GameManager.instance.MainCamera.ScreenToWorldPoint(UserInput.instance.GetMousePosition());
    }

    private void TriggerAttack() {
      if(UserInput.instance.IsBuildMode)
        return;
      Debug.LogError("TriggerAttack");
      attackTimeCounter = 0f;
      animator.SetTrigger("Attack");
      animator.SetInteger("WeaponID", attackID);
    }

    private void HighlightTarget() {
      Vector3 playerPosition = attackTransform.position;
      //todo кешировать камеру
      Vector3 mousePoint = GetMousePosition();

      Debug.DrawLine(playerPosition, mousePoint, Color.red);  // Visualize ray

      RaycastHit2D hit = Physics2D.Raycast(playerPosition, mousePoint - playerPosition, attackRange, attackLayer);
      if (hit.collider != null && hit.collider.gameObject != null) {
          ClearTarget();
          Highlight();
      }
      else {
        ClearTarget();
      }
    }
    
    private List<IDamageable> additionalTargets = new List<IDamageable>();

    private void SetTargets(Vector3 pos) {
      //additionalTargets.Clear();
      //Debug.LogError("SetTargets");
      var currPos = CoordsTransformer.WorldToGrid(pos);
      var main = GameManager.instance.ChunkController.GetCell(currPos.X, currPos.Y);
      additionalTargets.Add(main);
      var test = GameManager.instance.ChunkController.GetCell(currPos.X + 1, currPos.Y);
      if (test != null){ additionalTargets.Add(test);}
      
      var test1 = GameManager.instance.ChunkController.GetCell(currPos.X - 1, currPos.Y);
      if (test1 != null){ additionalTargets.Add(test1);}
    }
    
    private void SetTargetsByCollider() {
      Collider2D[] hitColliders = Physics2D.OverlapBoxAll(_collider.bounds.center, _collider.bounds.size, 0);

      foreach (Collider2D hitCollider in hitColliders)
      {
        if (hitCollider != _collider && hitCollider.TryGetComponent(out IDamageable iDamageable)) // Avoid detecting itself
        {
          var pos = CoordsTransformer.WorldToGrid(hitCollider.gameObject.transform.position);
          
          var test = GameManager.instance.ChunkController.GetCell(pos.X, pos.Y);
          Debug.LogError($"Collision detected with: {hitCollider.name}");
          additionalTargets.Add(test);
        }
      }
    }

    private void RemoveTarget() {
      currentTargetRenderer = null;
      additionalTargets.Clear();
    }

    private void ClearTarget() {
      ResetHighlight();
      RemoveTarget();
    }

    private void Highlight() {
      if (currentTargetRenderer == null ) {
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
      /*if (currentTarget == null || currentTarget.hasTakenDamage) {
        return;
      }*/
      Debug.LogError("Attack");
      SetTargetsByCollider();
      //need to attack in radius
      //currentTarget.Damage(blockDamage);
      foreach (var target in additionalTargets) {
        if (target == null || target.hasTakenDamage) continue;
        target.Damage(blockDamage);
        
        iDamageables.Add(target);
      }
    }

    private void DestroyTarget() {
      foreach (var t in additionalTargets) {
        if (t == null) continue;
        var getHp = t.GetHealth();
        if (getHp <= 0) {
          t.DestroyObject();
        }
      }
      ClearTarget();
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
      for (int i = 0; i < additionalTargets.Count; i++) {
        additionalTargets[i]?.AfterDamageReceived();
      }
      
    }
    
    private void HandleAnimationEnded(AnimationEvent animationEvent, GameObject go) {
      if(go != gameObject)
        return;
      ShouldBeDamagingToFalse();
      DestroyTarget();
    }

    #endregion
  }
}