using System;
using System.Collections;
using System.Collections.Generic;
using Animation;
using Audio;
using Enemy;
using NodeCanvas.BehaviourTrees;
using ParadoxNotion;
using Scriptables;
using Scriptables.Siege;
using Stats;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;
using Random = UnityEngine.Random;
using DG.Tweening;

namespace Actors {
  public class ActorEnemy : ActorBase {
    public bool shouldBeDamaging { get; private set; } = false;
    private List<IDamageable> iDamageables = new List<IDamageable>();
    private IDamageable currentTarget;

    [SerializeField] private SpriteRenderer _spriteRenderer;

    [SerializeField] private NPCMovement.NPCMovement npcMovement;
    [SerializeField] private EnemyCoords coords;
    [SerializeField] private BehaviourTreeOwner behaviourTreeOwner;

    [SerializeField] private ZombieDifficultyProfile difficulty;

    [Header("Destroy after death")] [SerializeField]
    private bool destroyAfterDeath;

    [SerializeField] private float destroyAfter = 5f;
    [SerializeField] private LayerMask excludeLayerOnDeath;
    [SerializeField] private LayerMask excludeLayerOnAlive;
    
    private IEnumerator coroutine;
    public Coords GetCoords => coords.GetCoords();
    public Coords GetCoordsOutOfBounds => coords.GetCoordsOutOfBounds();
    public ZombieDifficultyProfile Difficulty => difficulty;
    public event Action OnEnemyDied;

    private AudioController audioController;
    private AudioData deathAudioData;
    private AudioData groanAudioData;
    private float groanInterval = 0f;
    private float timeSinceGroan = 0f;
    private bool paused;
    private bool isDead;

    private Vector3 currPosition;
    private void Update() {
      PlayGroan();
      ActivateZombieByDistance();
    }

    //zombie cant fall if chunk under them don't load
    private void ActivateZombieByDistance() {
      var playerCoords = GameManager.Instance.PlayerController.PlayerCoords.GetCoords();
      var upDownY = Mathf.Abs(playerCoords.Y - GetCoords.Y);
      if (upDownY < GameManager.Instance.GameConfig.PlayerAreaHeight / 3) {
        rigidbody.simulated = true;
      }
      else {
        rigidbody.simulated = false;
      }
    }

    private void PlayGroan() {
      if (!groanAudioData || paused || IsDead) {
        return;
      }

      if (timeSinceGroan == 0f || timeSinceGroan >= groanInterval) {
        audioController.PlayAudio(groanAudioData, transform);
        timeSinceGroan = 0f;
      }

      timeSinceGroan += Time.deltaTime;
    }

    public void SetBehaviour(BehaviourTree tree) {
      behaviourTreeOwner.behaviour = tree;
      behaviourTreeOwner.StartBehaviour();
      InitHealth();
    }

    public BehaviourTree GetBehaviourTree() {
      return behaviourTreeOwner.behaviour;
    }

    private void InitHealth() {
      stats.AddHealth(stats.MaxHealth);
    }

    public void PauseBehaviour() {
      behaviourTreeOwner.PauseBehaviour();
      npcMovement.StopAnimator();

      OnPauseAction();
    }

    public void UnpauseBehaviour() {
      behaviourTreeOwner.StartBehaviour();

      OnUnPauseAction();
    }

    private void OnPauseAction() {
      paused = true;
      audioController.StopAudio(groanAudioData);
      timeSinceGroan = 0f;
    }

    private void OnUnPauseAction() {
      paused = false;
    }

    public void SetDifficulty(ZombieDifficultyProfile difficulty) {
      this.difficulty = difficulty;
      SetAudioData();
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

    public void SetTargetTransform(Transform tr, float actorBoundsWidth) {
      npcMovement.SetTargetTransform(tr, actorBoundsWidth);
    }

    public bool HasArrived() {
      return npcMovement.HasArrived;
    }

    public void AttackPlayer() {
      npcMovement.AttackPlayer();
    }

    protected override void Awake() {
      base.Awake();

      _spriteRenderer.material.SetFloat("_Blink", 0f);
      _spriteRenderer.material.SetFloat("_Dissolve", 1f);

      audioController = GameManager.Instance.AudioController;
      DamageableType = DamageableType.Enemy;
      AnimationEventManager.onAttackStarted += HandleAnimationStarted;
      AnimationEventManager.onAttackEnded += HandleAnimationEnded;
      
      ActorPlayer.OnPlayerDeath += OnPauseAction;
      ActorPlayer.OnPlayerRespawn += OnUnPauseAction;
    }

    private void SetAudioData() {
      if (!difficulty) {
        return;
      }

      if (difficulty.OnTakeDamageAudioDatas is { Count: > 0 }) {
        OnTakeDamageAudioData =
          difficulty.OnTakeDamageAudioDatas[Random.Range(0, difficulty.OnTakeDamageAudioDatas.Count)];
      }

      if (difficulty.OnDeathAudioDatas is { Count: > 0 }) {
        deathAudioData =
          difficulty.OnDeathAudioDatas[Random.Range(0, difficulty.OnDeathAudioDatas.Count)];
      }

      if (difficulty.GroanAudioData) {
        groanAudioData = difficulty.GroanAudioData;
        groanInterval = Random.Range(difficulty.GroanInterval.x, difficulty.GroanInterval.y + 1);
      }
    }

    private void DeathAudio() {
      if (!deathAudioData) {
        return;
      }

      audioController.PlayAudio(deathAudioData, currPosition);
    }

    private void DamageAudio() {
      if (!OnTakeDamageAudioData) {
        return;
      }

      if (IsDead) {
        return;
      }

      currPosition = transform.position;
      audioController.PlayAudio(OnTakeDamageAudioData, currPosition);
    }

    private void OnDestroy() {
      AnimationEventManager.onAttackStarted -= HandleAnimationStarted;
      AnimationEventManager.onAttackEnded -= HandleAnimationEnded;
      
      ActorPlayer.OnPlayerDeath -= OnPauseAction;
      ActorPlayer.OnPlayerRespawn -= OnUnPauseAction;
    }

    public void TriggerAttack(IDamageable target) {
      if (_animator.GetBool(animParam.IsDeadHash))
        return;
      currentTarget = target;
      //currentTarget = GameManager.Instance.CurrPlayerController.Actor;
      if (currentTarget == null || currentTarget.GetHealth() <= 0)
        return;

      _animator.SetTrigger(animParam.AttackHash);
    }

    private void HandleAnimationStarted(AnimationEvent animationEvent, GameObject go) {
      if (go != gameObject)
        return;
      //StartCoroutine(DamageWhileSlashIsActive());
      Attack();
      // Debug.Log("Attack started");
      currentTarget?.AfterDamageReceived();
    }

    private void HandleAnimationEnded(AnimationEvent animationEvent, GameObject go) {
      if (go != gameObject)
        return;
      ShouldBeDamagingToFalse();
      DestroyTarget();
    }

    private void DestroyTarget() {
      if (currentTarget == null)
        return;

      var getHp = currentTarget.GetHealth();
      if (getHp <= 0) {
        currentTarget.DestroyObject();
      }

      ClearTarget();
    }

    private void ClearTarget() {
      currentTarget = null;
    }

    public void ShouldBeDamagingToFalse() {
      shouldBeDamaging = false;
    }

    private void Attack() {
      if (currentTarget == null || /*currentTarget.hasTakenDamage ||*/ currentTarget.GetHealth() <= 0) {
        return;
      }

      //TODO choose damage for player or cell
      currentTarget.Damage(stats.EntityDamage, false);
      iDamageables.Add(currentTarget);
    }

    public PlayerStats GetStats() {
      return stats;
    }

    public override void Damage(float damage, bool isPlayer) {
      //Debug.Log("Enemy Damage");
      _spriteRenderer.material.SetFloat("_Blink", 1f);
      DOTween.To(() => _spriteRenderer.material.GetFloat("_Blink"),
      x => _spriteRenderer.material.SetFloat("_Blink", x), 0f, 0.2f)
      .SetEase(Ease.InOutSine);
      Vector3 pos = transform.position;
      pos.y = pos.y + 1f;
      GameManager.Instance.PoolEffects.SpawnFromPool("ZombieDamageParticleEffect", 
        pos, Quaternion.identity);

      npcMovement.Knocked = true;

      Vector3 playerPos = GameManager.Instance.PlayerController.gameObject.transform.position;
      Vector2 direction = (transform.position - playerPos).normalized;
      direction.y = 0.3f;
      rigidbody.AddForce(direction * 15, ForceMode2D.Impulse);

      DOVirtual.DelayedCall(0.2f, () => {npcMovement.Knocked = false;});
      

      DamageAudio();
      base.Damage(damage, isPlayer);
      if (stats.Health > 0)
        return;
      
      if (isDead)//(stats.Health > 0)
        return;
      
      isDead = true;
      rigidbody.excludeLayers = excludeLayerOnDeath;
    }

    protected override void DeathActions() {
      Vector3 pos = transform.position;
      pos.y = pos.y + 1f;
      GameManager.Instance.PoolEffects.SpawnFromPool("ZombieDeathParticleEffect",
        pos, Quaternion.identity);

      npcMovement.Knocked = true;

      Vector3 playerPos = GameManager.Instance.PlayerController.gameObject.transform.position;
      Vector2 direction = (transform.position - playerPos).normalized;
      direction.y = 0.3f;
      rigidbody.AddForce(direction * 20, ForceMode2D.Impulse);

      DOVirtual.DelayedCall(0.2f, () => {npcMovement.Knocked = false;});

      DOVirtual.DelayedCall(destroyAfter -1f, () => {
        var psGo = GameManager.Instance.PoolEffects.SpawnFromPool("ZombieDissolveParticleEffect",
        pos, Quaternion.identity).gameObject;
        ParticleSystem ps = psGo.GetComponent<ParticleSystem>();
        var shape = ps.shape;
        shape.spriteRenderer = _spriteRenderer;

        DOTween.To(() => _spriteRenderer.material.GetFloat("_Dissolve"),
        x => _spriteRenderer.material.SetFloat("_Dissolve", x), 0f, .8f)
        .SetEase(Ease.InOutSine);

      });

      //Debug.LogError($"Death actions");
      base.DeathActions();
      DeathAudio();
      OnEnemyDied?.Invoke();
      rigidbody.linearVelocity = Vector3.zero;
      SpawnDrop();
      DestroyAfterDeath();
    }

    private void DestroyAfterDeath() {
      if (!destroyAfterDeath)
        return;
      //Debug.LogError("destroy after death");
      coroutine = WaitDestroy();
      StartCoroutine(coroutine);
    }

    private IEnumerator WaitDestroy() {
      GameManager.Instance.ActorBaseController.RemoveFromList(this);
      yield return new WaitForSeconds(destroyAfter);
      //return to pool
      Respawn();
      gameObject.SetActive(false);
    }

    public override void Respawn() {
      base.Respawn();
      isDead = false;
      _animator.Rebind();
      _animator.Update(0f);
      rigidbody.excludeLayers = excludeLayerOnAlive;
      InitHealth();
      _spriteRenderer.material.SetFloat("_Blink", 0f);
      _spriteRenderer.material.SetFloat("_Dissolve", 1f);
    }

    private void SpawnDrop() {
      GameManager.Instance.DropZombieData.DropItems(difficulty, currPosition);
    }
  }
}