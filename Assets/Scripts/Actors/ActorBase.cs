using UnityEngine;

namespace Actors {
  public class ActorBase : MonoBehaviour, IDamageable {
    [SerializeField] protected float _currHP;
    [SerializeField] protected float _maxHP;
    [SerializeField] protected Animator _animator;
    [SerializeField] private bool _isDead;
    private UnitHealth unitHealth;

    public bool IsDead => _isDead;
    private void Awake() {
      _currHP = _maxHP;
      unitHealth = new UnitHealth(_currHP);
    }

    public bool hasTakenDamage {
      get { return unitHealth.hasTakenDamage; }
      set { unitHealth.hasTakenDamage = value; }
    }
    public void Damage(float damage) {
      unitHealth.TakeDamage(damage);
      _currHP -= damage;
      Debug.LogError($"Damage {_currHP}");
      if (_currHP <= 0) {
        _isDead = true;
        PlayDeathAnim();
      }
      else {
        PlayTakeDamage();
      }
    }

    private void PlayTakeDamage() {
      _animator.SetTrigger("TakeDamage");
    }
    
    private void PlayDeathAnim() {
      Debug.LogError($"PlayDeathAnim");
      _animator.SetLayerWeight(1, 0);
      _animator.SetTrigger("Die");
    }

    public float GetHealth() {
      return _currHP;
    }

    public float GetMaxHealth() {
      return _maxHP;
    }

    public void AfterDamageReceived() {
    }

    public void DestroyObject() {
      
    }
  }
}