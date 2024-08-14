using System.Collections;
using System.Collections.Generic;
using Animation;
using TarodevController;
using UnityEngine;
using UnityEngine.Rendering;
using World;

public class PlayerAttack : MonoBehaviour {
  [SerializeField] private Transform _attackTransform;
  [SerializeField] private float _attackRange = 0.5f;
  [SerializeField] private float _timeBtwAttacks = 0.2f;
  [SerializeField] private LayerMask _attackLayer;
  [SerializeField] private CellObjectsPool _pool;
  [SerializeField] private ScriptableStats _stats;
  [SerializeField] private Animator _animator;
  [SerializeField] private AnimatorEventReceiver eventReceiver;
  public bool ShouldBeDamaging { get; private set; } = false;
  private List<IDamageable> iDamageables = new List<IDamageable>();

  private float _attackTimeCounter;

  private IDamageable _currentTarget;

  private void Awake() {
    eventReceiver.OnAnimationStarted += HandleAnimationStarted;
    eventReceiver.OnAnimationEnded += HandleAnimationEnded;
  }

  private void OnDestroy() {
    eventReceiver.OnAnimationStarted -= HandleAnimationStarted;
    eventReceiver.OnAnimationEnded -= HandleAnimationEnded;
  }

  private void Start() {
    _attackTimeCounter = _timeBtwAttacks;
  }

  private void Update() {
    HighlightTarget();

    if (UserInput.instance.controls.GamePlay.Attack.WasPressedThisFrame() && _attackTimeCounter >= _timeBtwAttacks) {
      _attackTimeCounter = 0f;
      _animator.SetTrigger("Attack");
    }

    _attackTimeCounter += Time.deltaTime;
  }

  private void HighlightTarget() {
    var mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

    var hit = Physics2D.OverlapPoint(mousePoint, _attackLayer);
    if (hit == null || !hit.TryGetComponent(out IDamageable iDamageable)) {
      ClearTarget();
      return;
    }

    if (iDamageable == _currentTarget) {
      return;
    }

    var collision = CheckAttackCollision(_attackTransform.position, _attackRange, hit);
    if (!collision) {
      ClearTarget();
      return;
    }

    ClearTarget();

    SetTarget(iDamageable);
    HighlightTarget(hit.gameObject);
  }

  bool CheckAttackCollision(Vector2 circleCenter, float circleRadius, Collider2D collider) {
    // Get the collider's bounds
    var bounds = collider.bounds;

    // Find the closest point on the collider to the center of the circle
    var closestX = Mathf.Clamp(circleCenter.x, bounds.min.x, bounds.max.x);
    var closestY = Mathf.Clamp(circleCenter.y, bounds.min.y, bounds.max.y);

    // Calculate the distance between the closest point and the circle's center
    var distanceX = circleCenter.x - closestX;
    var distanceY = circleCenter.y - closestY;

    // Calculate the distance squared (avoid square root for performance)
    var distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);

    // Check if the distance is less than or equal to the circle's radius squared
    return distanceSquared <= (circleRadius * circleRadius);
  }

  private void SetTarget(IDamageable target) {
    _currentTarget = target;
  }

  private void RemoveTarget() {
    _currentTarget = null;
  }

  private void ClearTarget() {
    if (_currentTarget == null) {
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
    if (_currentTarget is MonoBehaviour mb) {
      Renderer renderer = mb.GetComponent<Renderer>();
      if (renderer != null) {
        renderer.material.color = Color.white; // Reset to original color
      }
    }
  }

  private void Attack() {
    if (_currentTarget == null || _currentTarget.HasTakenDamage) {
      return;
    }

    _currentTarget.Damage(_stats.AttackDamage);
    iDamageables.Add(_currentTarget);

    float hp = _currentTarget.GetHealth();
    if (hp <= 0) {
      _pool.ReturnObject(_currentTarget as CellObject);
      ClearTarget();
    }
  }

  public IEnumerator DamageWhileSlashIsActive() {
    ShouldBeDamaging = true;

    while (ShouldBeDamaging) {
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
    ShouldBeDamaging = false;
  }

  private void OnDrawGizmosSelected() {
    Gizmos.DrawWireSphere(_attackTransform.position, _attackRange);

    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(_attackTransform.position, _attackRange);
  }

  #region Animation Triggers

  private void HandleAnimationStarted(AnimationEvent animationEvent) {
    StartCoroutine(DamageWhileSlashIsActive());
  }

  private void HandleAnimationEnded(AnimationEvent animationEvent) {
    ShouldBeDamagingToFalse();
  }

  #endregion
}
