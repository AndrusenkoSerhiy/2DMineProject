using System;
using UnityEngine;

public class UnitHealth {
  private float _currentHealth;
  private float _maxHealth;
  private bool _hasTakenDamage;

  public float Health => _currentHealth;
  public float MaxHealth => _maxHealth;
  public bool HasTakenDamage { get { return _hasTakenDamage; } set { _hasTakenDamage = value; } }

  // Events for health changes
  public event Action<float> OnTakeDamage;
  public event Action<float> OnHeal;

  public UnitHealth(float maxHealth) {
    _maxHealth = maxHealth;
    _currentHealth = maxHealth;
  }

  public void TakeDamage(float damage) {
    if (damage <= 0) return;

    _hasTakenDamage = true;
    _currentHealth -= damage;
    _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

    OnTakeDamage?.Invoke(damage);
  }

  public void Heal(float healAmount) {
    if (healAmount <= 0) return;

    _currentHealth += healAmount;
    _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

    OnHeal?.Invoke(healAmount);
  }
}
