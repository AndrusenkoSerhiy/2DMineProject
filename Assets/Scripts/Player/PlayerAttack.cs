// using Animation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using World;
using Utils;
using Settings;
using Game;

namespace Player {
  public class PlayerAttack : MonoBehaviour {
    [SerializeField] private Transform attackTransform;

    [SerializeField] private PlayerStats stats;

    [SerializeField] private Animator animator;

    public bool shouldBeDamaging { get; private set; } = false;

    private List<IDamageable> iDamageables = new List<IDamageable>();

    private float attackTimeCounter;

    private IDamageable currentTarget;
    private Renderer currentTargetRenderer;

    private void Awake() {
      AnimationEventManager.onAttackStarted += HandleAnimationStarted;
      AnimationEventManager.onAttackEnded += HandleAnimationEnded;
    }

    private void OnDestroy() {
      AnimationEventManager.onAttackStarted -= HandleAnimationStarted;
      AnimationEventManager.onAttackEnded -= HandleAnimationEnded;
    }

    private void Start() {
      attackTimeCounter = stats.TimeBtwAttacks;
    }

    private void Update() {
      HighlightTarget();

      // Handle attack
      if (UserInput.instance.IsAttacking() && currentTarget != null
        && attackTimeCounter >= stats.TimeBtwAttacks) {
        TriggerAttack();
      }

      attackTimeCounter += Time.deltaTime;
    }

    private void TriggerAttack() {
      attackTimeCounter = 0f;
      animator.SetTrigger("Attack");
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


      bool collision = Collisions.CheckCircleCollision(attackTransform.position, attackRange, hit);
      if (!collision) {
        ClearTarget();
        return;
      }

      ClearTarget();

      SetTarget(iDamageable);
      Highlight();
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
      if (currentTargetRenderer == null) {
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

      currentTarget.Damage(stats.AttackDamage);
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
      float attackRange = stats.AttackRange;
      Gizmos.DrawWireSphere(attackTransform.position, attackRange);

      Gizmos.color = Color.red;
      Gizmos.DrawWireSphere(attackTransform.position, attackRange);
    }

    #region Animation Triggers

    private void HandleAnimationStarted(AnimationEvent animationEvent) {
      StartCoroutine(DamageWhileSlashIsActive());
      // Debug.Log("Attack started");
      currentTarget?.AfterDamageReceived();
    }

    private void HandleAnimationEnded(AnimationEvent animationEvent) {
      ShouldBeDamagingToFalse();
      // Debug.Log("Attack ended");
      DestroyTarget();
    }

    #endregion
  }
}