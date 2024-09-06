using System;
using UnityEngine;

public class UnitHealth {
  private float currentHealth;
  private float maxHealth;
  private bool hasTakenDamage;

  public float Health => currentHealth;
  public float MaxHealth => maxHealth;
  public bool HasTakenDamage { get { return hasTakenDamage; } set { hasTakenDamage = value; } }

  // Events for health changes
  public event Action<float> OnTakeDamage;
  public event Action<float> OnHeal;

  public UnitHealth(float maxHealth) {
    this.maxHealth = maxHealth;
    currentHealth = maxHealth;
  }

  public void TakeDamage(float damage) {
    if (damage <= 0) return;

    hasTakenDamage = true;
    currentHealth -= damage;
    currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

    OnTakeDamage?.Invoke(damage);
  }

  public void Heal(float healAmount) {
    if (healAmount <= 0) return;

    currentHealth += healAmount;
    currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

    OnHeal?.Invoke(healAmount);
  }
}
