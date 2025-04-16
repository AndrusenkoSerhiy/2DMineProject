using System.Collections;
using System.Collections.Generic;
using Animation;
using Enemy;
using UnityEngine;
using Utils;

namespace Actors {
  public class ActorEnemy : ActorBase {
    [SerializeField] private int _damage = 3;
    public bool shouldBeDamaging { get; private set; } = false;
    private List<IDamageable> iDamageables = new List<IDamageable>();
    private IDamageable currentTarget;
    
    [SerializeField] private NPCMovement.NPCMovement npcMovement;
    [SerializeField] private EnemyCoords coords;
    [SerializeField] private LayerMask layerAfterDeath;
    public Coords GetCoords => coords.GetCoords();

    public void SetPatrolPosition(Vector3 pos) {
      npcMovement.SetTarget(pos);
    }
    
    public void SetTargetTransform(Transform tr) {
      npcMovement.SetTargetTransform(tr);
    }

    public bool HasArrived() {
      return npcMovement.HasArrived;
    }
    
    public void AttackPlayer() {
      npcMovement.AttackPlayer();
    }
    
    protected override void Awake() {
      base.Awake();
      AnimationEventManager.onAttackStarted += HandleAnimationStarted;
      AnimationEventManager.onAttackEnded += HandleAnimationEnded;
    }

    private void OnDestroy() {
      AnimationEventManager.onAttackStarted -= HandleAnimationStarted;
      AnimationEventManager.onAttackEnded -= HandleAnimationEnded;
    }
    public void TriggerAttack() {
      if (_animator.GetBool(animParam.IsDeadHash))
        return;
      
      currentTarget = GameManager.Instance.PlayerController.GetComponent<IDamageable>();
      //Debug.LogError($"trigger attack {currentTarget.GetHealth()}");
      if (currentTarget.GetHealth() <= 0)
        return;

      _animator.SetTrigger(animParam.AttackHash);
    }

    private void HandleAnimationStarted(AnimationEvent animationEvent, GameObject go) {
      if (go != gameObject)
        return;
      StartCoroutine(DamageWhileSlashIsActive());
      // Debug.Log("Attack started");
      currentTarget?.AfterDamageReceived();
    }

    private void HandleAnimationEnded(AnimationEvent animationEvent, GameObject go) {
      if (go != gameObject)
        return;
      ShouldBeDamagingToFalse();
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

    private void Attack() {
      if (currentTarget == null || currentTarget.hasTakenDamage || currentTarget.GetHealth() <= 0) {

        return;
      }
      
      currentTarget.Damage(_damage);
      iDamageables.Add(currentTarget);
    }

    public override void Damage(float damage) {
      base.Damage(damage);
      if(stats.Health > 0)
        return;
      //work only 1 layer selected in inspector
      var layerIndex = Mathf.RoundToInt(Mathf.Log(layerAfterDeath.value, 2));
      
      gameObject.layer = layerIndex;
      Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Character"), layerIndex);
    }
  }
}