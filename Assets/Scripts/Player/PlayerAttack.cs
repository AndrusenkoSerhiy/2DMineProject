using Animation;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TarodevController;
using UnityEngine;
using World;

public class PlayerAttack : MonoBehaviour {
  [SerializeField] private Transform attackTransform;

  [SerializeField] private CellObjectsPool pool;

  [SerializeField] private ScriptableStats stats;

  [SerializeField] private Animator animator;

  [SerializeField] private AnimatorEventReceiver eventReceiver;

  public bool shouldBeDamaging { get; private set; } = false;

  private List<IDamageable> iDamageables = new List<IDamageable>();

  private float attackTimeCounter;

  private IDamageable currentTarget;

  private void Awake() {
    eventReceiver.OnAnimationStarted += HandleAnimationStarted;
    eventReceiver.OnAnimationEnded += HandleAnimationEnded;
  }

  private void OnDestroy() {
    eventReceiver.OnAnimationStarted -= HandleAnimationStarted;
    eventReceiver.OnAnimationEnded -= HandleAnimationEnded;
  }

  private void Start() {
    attackTimeCounter = stats.TimeBtwAttacks;
  }

  private void Update() {
    HighlightTarget();

    if (UserInput.instance.controls.GamePlay.Attack.WasPressedThisFrame()
      && currentTarget != null && attackTimeCounter >= stats.TimeBtwAttacks) {
      attackTimeCounter = 0f;
      animator.SetTrigger("Attack");
    }

    attackTimeCounter += Time.deltaTime;
  }

  private void HighlightTarget() {
    float attackRange = stats.AttackRange;
    Vector3 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

    Collider2D hit = Physics2D.OverlapPoint(mousePoint, stats.AttackLayer);
    if (hit == null || !hit.TryGetComponent(out IDamageable iDamageable)) {
      ClearTarget();
      return;
    }

    if (iDamageable == currentTarget) {
      return;
    }

    bool collision = CheckAttackCollision(attackTransform.position, attackRange, hit);
    if (!collision) {
      ClearTarget();
      return;
    }

    ClearTarget();

    SetTarget(iDamageable);
    HighlightTarget(hit.gameObject);
  }

  private bool CheckAttackCollision(Vector2 circleCenter, float circleRadius, Collider2D collider) {
    // Get the collider's bounds
    Bounds bounds = collider.bounds;

    // Find the closest point on the collider to the center of the circle
    float closestX = Mathf.Clamp(circleCenter.x, bounds.min.x, bounds.max.x);
    float closestY = Mathf.Clamp(circleCenter.y, bounds.min.y, bounds.max.y);

    // Calculate the distance between the closest point and the circle's center
    float distanceX = circleCenter.x - closestX;
    float distanceY = circleCenter.y - closestY;

    // Calculate the distance squared (avoid square root for performance)
    float distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);

    // Check if the distance is less than or equal to the circle's radius squared
    return distanceSquared <= (circleRadius * circleRadius);
  }

  private void SetTarget(IDamageable target) {
    currentTarget = target;
  }

  private void RemoveTarget() {
    currentTarget = null;
  }

  private void ClearTarget() {
    if (currentTarget == null) {
      return;
    }

    ResetTargetHighlight();
    RemoveTarget();
  }

  private void HighlightTarget(GameObject obj) {
    Renderer renderer = obj.GetComponent<Renderer>();
    if (renderer != null) {
      renderer.material.color = Color.yellow; // Highlight color
    }
  }

  private void ResetTargetHighlight() {
    if (currentTarget is MonoBehaviour mb) {
      Renderer renderer = mb.GetComponent<Renderer>();
      if (renderer != null) {
        renderer.material.color = Color.white; // Reset to original color
      }
    }
  }

  private void Attack() {
    if (currentTarget == null || currentTarget.HasTakenDamage) {
      return;
    }

    currentTarget.Damage(stats.AttackDamage);
    iDamageables.Add(currentTarget);

    float hp = currentTarget.GetHealth();
    if (hp <= 0) {
      pool.ReturnObject(currentTarget as CellObject);
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
      damaged.HasTakenDamage = false;
    }

    iDamageables.Clear();
  }

  public void ShouldBeDamagingToFalse() {
    shouldBeDamaging = false;
  }

  private void OnDrawGizmosSelected() {
    float attackRange = stats.AttackRange;
    Gizmos.DrawWireSphere(attackTransform.position, attackRange);

    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(attackTransform.position, attackRange);
  }

  private void ShakeTarget() {
    if (currentTarget is MonoBehaviour mb) {
      mb.transform.DOShakePosition(0.5f, 0.1f, 10, 90, false, true);
    }
  }

  #region Animation Triggers

  private void HandleAnimationStarted(AnimationEvent animationEvent) {
    StartCoroutine(DamageWhileSlashIsActive());
    ShakeTarget();
  }

  private void HandleAnimationEnded(AnimationEvent animationEvent) {
    ShouldBeDamagingToFalse();
  }

  #endregion
}
