using System.Collections.Generic;
using Animation;
using Scriptables;
using Settings;
using UnityEngine;
using World;

namespace Player {
  public class BaseAttack : MonoBehaviour {
    [SerializeField] protected PlayerStats stats;
    [SerializeField] protected Animator animator;
    [SerializeField] protected ObjectHighlighter objectHighlighter;
    [SerializeField] protected BoxCollider2D attackCollider;
    [SerializeField] protected Transform attackTransform;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 5f;
    
    private float attackTimeCounter;
    protected float timeBtwAttacks;
    protected float blockDamage;
    protected LayerMask attackLayer;
    protected float entityDamage;
    protected float attackRange;
    protected float staminaUsage;
    protected int attackID;
    protected int maxTargets;
    protected Vector2 colliderSize;
    
    private List<IDamageable> targets = new();
    private List<IDamageable> iDamageables = new();
    [SerializeField] private bool isHighlightLock;
    private Vector2 originalSize;
    
    public bool shouldBeDamaging { get; private set; } = false;
    
    protected virtual void Awake() {
      AnimationEventManager.onAttackStarted += HandleAnimationStarted;
      AnimationEventManager.onAttackEnded += HandleAnimationEnded;
    }
    
    protected virtual void Start() {
      attackTimeCounter = timeBtwAttacks;
      PrepareAttackParams();
      originalSize = attackCollider.size;
    }

    public void LockHighlight(bool state) {
      isHighlightLock = state;
      if (state) {
        attackCollider.transform.localPosition = new Vector3(0, 1, 0);
        originalSize = attackCollider.size;
        attackCollider.size = new Vector2(.2f, .2f);
      }
      else {
        attackCollider.size = originalSize;
      }
    }

    protected virtual void PrepareAttackParams() { }
    
    protected virtual void Update() {
      if(isHighlightLock)
        return;
      
      UpdateColliderPos();
      HandleAttack();
    }
    
    private void TriggerAttack() {
      attackTimeCounter = 0f;
      animator.SetTrigger("Attack");
      animator.SetInteger("WeaponID", attackID);
    }
    
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
      var parentPosition = attackTransform.position;
      var direction = (mousePos - parentPosition).normalized;
      var distance = Vector3.Distance(parentPosition, mousePos);
      // Clamp the distance between minDistance and maxDistance
      var clampedDistance = Mathf.Clamp(distance, minDistance, maxDistance);
      // Set the new position of the child collider
      var newPosition = parentPosition + direction * clampedDistance;
      newPosition.z = 0f;
      attackCollider.transform.position = newPosition;
    }

    protected void UpdateParams(float minDist, float maxDist, float sizeX, float sizeY) {
      minDistance = minDist;
      maxDistance = maxDist;
      attackCollider.size = new Vector2(sizeX, sizeY);
    }
    
    private Vector3 GetMousePosition() {
      var mousePos = GameManager.instance.MainCamera.ScreenToWorldPoint(UserInput.instance.GetMousePosition());
      mousePos.z = 0f;
      return mousePos;
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
    
    private void SetTargetsFromHighlight() {
      foreach (var elem in objectHighlighter.Highlights) {
        var pos = CoordsTransformer.WorldToGrid(elem.transform.position);
        var cell = GameManager.instance.ChunkController.GetCell(pos.X, pos.Y);
        if (cell != null) targets.Add(cell);
      }
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
    
    private void ClearTarget() {
      targets.Clear();
    }
    
    private void OnDrawGizmosSelected() {
      Gizmos.DrawWireSphere(attackTransform.position, attackRange);
      Gizmos.color = Color.red;
    }
    
    protected virtual void OnDestroy() {
      AnimationEventManager.onAttackStarted -= HandleAnimationStarted;
      AnimationEventManager.onAttackEnded -= HandleAnimationEnded;
    }
  }
}