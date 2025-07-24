using System;
using Scriptables;
using UnityEngine;

namespace Actors {
  public class ActorBase : MonoBehaviour, IDamageable {
    [SerializeField] protected Animator _animator;
    //[SerializeField] private bool isDead;
    [SerializeField] protected Rigidbody2D rigidbody;
    [SerializeField] private CapsuleCollider2D capsuleCollider;

    protected AnimatorParameters animParam;
    protected PlayerStats stats;
    //[SerializeField] private bool _hasTakenDamage;
    public AnimatorParameters AnimParam => animParam;
    public bool IsDead => _animator.GetBool(animParam.IsDeadHash);
    public DamageableType DamageableType { get; set; }
    public AudioData OnTakeDamageAudioData { get; set; }
    public bool CanGetDamage { get; set; }

    protected virtual void Awake() {
      stats = GetComponent<PlayerStats>();
    }

    protected virtual void Start() {
      animParam = GameManager.Instance.AnimatorParameters;
    }

    public float ActorBoundsWidth => capsuleCollider.size.x * .5f;

    /*public bool hasTakenDamage {
      get { return false; }
      set { _hasTakenDamage = value; }
    }*/

    public virtual void Damage(float damage, bool isPlayer) {
      //get param from animator
      if (IsDead)
        return;

      //hasTakenDamage = true;
      if (stats.TakeDamage(damage) <= 0) {
        DeathActions();
      }
      else {
        PlayTakeDamage();
      }
    }

    protected virtual void DeathActions() {
      //isDead = true;
      PlayDeathAnim();
    }

    private void PlayTakeDamage() {
      _animator.SetTrigger(animParam.TakeDamage);
    }

    private void PlayDeathAnim() {
      // Debug.LogError($"PlayDeathAnim");
      _animator.SetLayerWeight(1, 0);
      _animator.SetTrigger(animParam.Die);
    }

    public virtual void Respawn() {
      //_hasTakenDamage = false;
      _animator.SetLayerWeight(1, 1);
    }

    public float GetHealth() {
      return stats.Health;
    }

    public Vector3 GetPosition() {
      return transform.position;
    }

    public string GetName() {
      return name;
    }

    public float GetMaxHealth() {
      return stats.MaxHealth;
    }

    public void AfterDamageReceived() {
    }

    public void DestroyObject() {
    }

    public void SetAnimVelocityX(float val) {
      _animator.SetFloat(animParam.VelocityXHash, val);
      //Debug.LogError("stop zombie movement");
    }
  }
}