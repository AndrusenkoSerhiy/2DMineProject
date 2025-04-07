using System;
using System.Collections.Generic;
using Animation;
using Scriptables;
using Scriptables.Stats;
using UnityEngine;
using World;

namespace Player {
  public class BaseAttack : MonoBehaviour {
    // [SerializeField] protected PlayerStats stats;
    [SerializeField] protected PlayerStatsObject statsObject;
    [SerializeField] protected Animator animator;
    [SerializeField] protected ObjectHighlighter objectHighlighter;
    [SerializeField] protected BoxCollider2D attackCollider;
    [SerializeField] protected Transform attackTransform;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 5f;

    private float attackTimeCounter;
    protected LayerMask attackLayer;
    protected int attackID;
    protected int maxTargets;

    protected Vector2 colliderSize;
    /*protected float timeBtwAttacks;
    protected float blockDamage;
    protected float entityDamage;
    protected float attackRange;
    protected float staminaUsage;*/

    private List<IDamageable> targets = new();
    private List<IDamageable> iDamageables = new();
    [SerializeField] private bool isHighlightLock;
    private Vector2 originalSize;
    private int lookDirection;
    private AnimatorParameters animParam;

    protected PlayerStats playerStats;
    protected bool firstAttack;
    public PlayerStats PlayerStats => playerStats ??= GameManager.Instance.CurrPlayerController.PlayerStats;

    public bool shouldBeDamaging { get; private set; } = false;

    protected virtual void Awake() {
      AnimationEventManager.onAttackStarted += HandleAnimationStarted;
      AnimationEventManager.onAttackEnded += HandleAnimationEnded;
      animParam = GameManager.Instance.AnimatorParameters;
    }

    protected virtual void Start() {
      attackTimeCounter = PlayerStats.TimeBtwAttacks;
      PrepareAttackParams();
      originalSize = attackCollider.size;
      GameManager.Instance.UserInput.OnAttackPerformed += PressAttack;
      GameManager.Instance.UserInput.OnAttackCanceled += CancelAttack;
    }

    private void PressAttack(object sender, EventArgs e) {
      animator.SetBool(animParam.IsAttacking, true);
    }

    private void CancelAttack(object sender, EventArgs e) {
      animator.SetBool(animParam.IsAttacking, false);
      firstAttack = false;
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

      objectHighlighter.EnableCrosshair(!state);
    }

    protected virtual void PrepareAttackParams() {
    }

    protected virtual void Update() {
      if (isHighlightLock)
        return;

      UpdateColliderPos();
      HandleAttack();
      GetDirection();
    }

    private void GetDirection() {
      Vector2 direction = attackCollider.transform.position - transform.position;
      //Debug.LogError($"directionY {direction.y}");

      //3f distance between player and mouse for top border 
      if (direction.y > 3f) {
        lookDirection = 1;
      }
      else if (direction.y < .3f) {
        lookDirection = -1;
      }
      else {
        lookDirection = 0;
      }

      animator.SetInteger(animParam.LookDirection, lookDirection);
    }

    protected virtual void TriggerAttack() {
      attackTimeCounter = 0f;
    }

    private void HandleAttack() {
      if (GameManager.Instance.UserInput.IsAttacking() /*&& currentTarget != null*/
          && attackTimeCounter >= PlayerStats.TimeBtwAttacks) {
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
      var mousePos =
        GameManager.Instance.MainCamera.ScreenToWorldPoint(GameManager.Instance.UserInput.GetMousePosition());
      mousePos.z = 0f;
      return mousePos;
    }

    private void Attack() {
      shouldBeDamaging = true;
      SetTargetsFromHighlight();
      foreach (var target in targets) {
        if (target == null || target.hasTakenDamage) continue;
        target.Damage(PlayerStats.BlockDamage);
        iDamageables.Add(target);
      }

      ReturnAttackableToDamageable();
    }

    private void SetTargetsFromHighlight() {
      foreach (var elem in objectHighlighter.Highlights) {
        var pos = CoordsTransformer.WorldToGrid(elem.transform.position);
        var cell = GameManager.Instance.ChunkController.GetCell(pos.X, pos.Y);
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
      var range = PlayerStats?.AttackRange ?? statsObject.attackRange;
      Gizmos.DrawWireSphere(attackTransform.position, range);
      Gizmos.color = Color.red;
    }

    protected virtual void OnDestroy() {
      AnimationEventManager.onAttackStarted -= HandleAnimationStarted;
      AnimationEventManager.onAttackEnded -= HandleAnimationEnded;
      if (GameManager.HasInstance) {
        GameManager.Instance.UserInput.OnAttackPerformed -= PressAttack;
        GameManager.Instance.UserInput.OnAttackCanceled -= CancelAttack;
      }
    }
  }
}