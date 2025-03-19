using System;
using UnityEngine;

[Serializable]
public class UnitHealth {
  private float _currentHealth;
  private readonly float _maxHealth;
  private bool _hasTakenDamage;

  public float health => _currentHealth;
  public float maxHealth => _maxHealth;
  public bool hasTakenDamage { get { return _hasTakenDamage; } set { _hasTakenDamage = value; } }

  // Events for health changes
  public event Action<float> OnTakeDamage;
  public event Action<float> OnHeal;

  public UnitHealth(float maxHealth) {
    this._maxHealth = maxHealth;
    _currentHealth = maxHealth;
  }

  public void TakeDamage(float damage) {
    if (damage <= 0) return;

    _hasTakenDamage = true;
    //need to check how exactly damage we have, box can have 15 durability and pickaxe 6 damage
    if(_currentHealth < damage) damage = _currentHealth;
    _currentHealth -= damage;
    _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

    OnTakeDamage?.Invoke(damage);
  }

  public void SetCurrentHealth(float health) {
    _currentHealth = Mathf.Clamp(health, 0, _maxHealth);
  }

  public void Heal(float healAmount) {
    if (healAmount <= 0) return;

    _currentHealth += healAmount;
    _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

    OnHeal?.Invoke(healAmount);
  }
}
