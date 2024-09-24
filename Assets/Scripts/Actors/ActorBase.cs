using UnityEngine;

namespace Game.Actors {
  public class ActorBase : MonoBehaviour, IDamageable {
    [SerializeField] protected float _currHP;
    [SerializeField] protected float _maxHP;
    [SerializeField] private Animator _animator;
    private UnitHealth unitHealth;

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
      
      if (_currHP <= 0) {
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