using System;
using UnityEngine;

[Serializable]
public class UnitHealth {
  private float _currentHealth;
  private readonly float _maxHealth;
  private bool _hasTakenDamage;

  public float health => _currentHealth;
  public float maxHealth => _maxHealth;

  public bool hasTakenDamage {
    get { return _hasTakenDamage; }
    set { _hasTakenDamage = value; }
  }

  //bool use for set info about attacke(is player)
  public event Action<float, bool> OnTakeDamage;

  public UnitHealth(float maxHealth) {
    _maxHealth = maxHealth;
    _currentHealth = maxHealth;
  }

  public void TakeDamage(float damage, bool isPlayer) {
    if (damage <= 0) {
      return;
    }

    _hasTakenDamage = true;
    //need to check how exactly damage we have, box can have 15 durability and pickaxe 6 damage
    if (_currentHealth < damage) {
      damage = _currentHealth;
    }

    _currentHealth -= damage;
    _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

    OnTakeDamage?.Invoke(damage, isPlayer);
  }

  public void SetCurrentHealth(float health) {
    _currentHealth = Mathf.Clamp(health, 0, _maxHealth);
  }
}