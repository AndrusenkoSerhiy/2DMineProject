using System.Collections;
using System.Collections.Generic;
using Animation;
using Scriptables;
using UnityEngine;

namespace Actors {
  public class ActorEnemy : ActorBase {
    [SerializeField] private int _damage = 3;
    public bool shouldBeDamaging { get; private set; } = false;
    private List<IDamageable> iDamageables = new List<IDamageable>();
    private IDamageable currentTarget;

    private void Awake() {
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

      currentTarget = GameManager.instance.PlayerController.GetComponent<IDamageable>();
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
      // Debug.Log("Attack ended");
      //DestroyTarget();
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
      Debug.LogError("ATTACK");
      currentTarget.Damage(_damage);
      iDamageables.Add(currentTarget);
    }
  }
}