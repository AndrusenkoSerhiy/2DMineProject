using System.Collections;
using System.Collections.Generic;
using Animation;
using Enemy;
using NodeCanvas.BehaviourTrees;
using Scriptables.Siege;
using Stats;
using UnityEngine;
using Utils;

namespace Actors {
  public class ActorEnemy : ActorBase {
    public bool shouldBeDamaging { get; private set; } = false;
    private List<IDamageable> iDamageables = new List<IDamageable>();
    private IDamageable currentTarget;

    [SerializeField] private NPCMovement.NPCMovement npcMovement;
    [SerializeField] private EnemyCoords coords;
    [SerializeField] private LayerMask layerAfterDeath;
    [SerializeField] private BehaviourTreeOwner behaviourTreeOwner;

    [SerializeField] private ZombieDifficultyProfile difficulty;
    public Coords GetCoords => coords.GetCoords();
    public ZombieDifficultyProfile Difficulty => difficulty;

    public void SetBehaviour(BehaviourTree tree) {
      behaviourTreeOwner.behaviour = tree;
      behaviourTreeOwner.StartBehaviour();
    }

    public void SetDifficulty(ZombieDifficultyProfile difficulty) {
      this.difficulty = difficulty;
      ApplyStats();
    }

    private void ApplyStats() {
      stats.UpdateBaseValue(StatType.MaxHealth, stats.MaxHealth * difficulty.healthMultiplier);
      stats.UpdateBaseValue(StatType.Health, stats.Health * difficulty.healthMultiplier);
      stats.UpdateBaseValue(StatType.MaxSpeed, stats.MaxSpeed * difficulty.speedMultiplier);
      stats.UpdateBaseValue(StatType.BlockDamage, stats.BlockDamage * difficulty.attackMultiplier);
      stats.UpdateBaseValue(StatType.EntityDamage, stats.EntityDamage * difficulty.attackMultiplier);
      stats.UpdateBaseValue(StatType.Armor, stats.Armor * difficulty.armorMultiplier);
    }

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

      //TODO choose damage for player or cell
      currentTarget.Damage(stats.EntityDamage);
      iDamageables.Add(currentTarget);
    }

    public PlayerStats GetStats() {
      return stats;
    }

    public override void Damage(float damage) {
      base.Damage(damage);
      if (stats.Health > 0)
        return;
      //work only 1 layer selected in inspector
      var layerIndex = Mathf.RoundToInt(Mathf.Log(layerAfterDeath.value, 2));

      gameObject.layer = layerIndex;
      Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Character"), layerIndex);
    }

    protected override void DeathActions() {
      base.DeathActions();
      rigidbody.linearVelocity = Vector3.zero;
      SpawnDrop();
    }

    private void SpawnDrop() {
      GameManager.Instance.DropZombieData.DropItems(difficulty);
    }
  }
}