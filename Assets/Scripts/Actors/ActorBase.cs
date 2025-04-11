using Scriptables;
using UnityEngine;

namespace Actors {
  public class ActorBase : MonoBehaviour, IDamageable {
    // [SerializeField] protected float _currHP;
    // [SerializeField] protected float _maxHP;
    [SerializeField] protected Animator _animator;

    [SerializeField] private bool _isDead;

    // private UnitHealth unitHealth;
    protected AnimatorParameters animParam;
    protected StatsBase stats;
    private bool _hasTakenDamage;

    public AnimatorParameters AnimParam => animParam;
    public bool IsDead => _isDead;

    private void Awake() {
      // _currHP = _maxHP;
      // unitHealth = new UnitHealth(_currHP);
      stats = GetComponent<StatsBase>();
    }

    private void Start() {
      animParam = GameManager.Instance.AnimatorParameters;
    }
    
    public bool hasTakenDamage {
      get { return _hasTakenDamage; }
      set { _hasTakenDamage = value; }
    }

    public virtual void Damage(float damage) {
      hasTakenDamage = true;
      if (stats.TakeDamage(damage) <= 0) {
        _isDead = true;
        PlayDeathAnim();
      }
      else {
        PlayTakeDamage();
      }
    }

    private void PlayTakeDamage() {
      _animator.SetTrigger(animParam.TakeDamage);
    }

    private void PlayDeathAnim() {
      // Debug.LogError($"PlayDeathAnim");
      _animator.SetLayerWeight(1, 0);
      _animator.SetTrigger(animParam.Die);
    }

    public float GetHealth() {
      return stats.Health;
    }

    public float GetMaxHealth() {
      return stats.MaxHealth;
    }

    public void AfterDamageReceived() {
    }

    public void DestroyObject() {
    }
  }
}