using System.Collections.Generic;
using Scriptables.Stats;
using UnityEngine;

namespace Stats {
  public class EntityStats {
    private readonly StatsMediator mediator;
    private readonly BaseStatsObject baseStatsObject;
    private readonly Dictionary<StatType, float> statValueCache = new();
    private readonly HashSet<StatType> dirtyStats = new();

    private float health;
    private float maxHealth;
    private float healthRegen;

    private float stamina;
    private float maxStamina;
    private float staminaDrain;
    private float staminaRecovery;
    public bool staminaInUse;

    private float attackRange;
    private float blockDamage;
    private float entityDamage;
    private float timeBtwAttacks;
    private float attackStaminaUsage;

    private float armor;

    public StatsMediator Mediator => mediator;
    public BaseStatsObject BaseStatsObject => baseStatsObject;

    public float Health => health;
    public float MaxHealth => GetStatValue(StatType.MaxHealth, maxHealth);
    public float HealthRegen => GetStatValue(StatType.HealthRegen, healthRegen);

    public float Stamina => stamina;
    public float MaxStamina => GetStatValue(StatType.MaxStamina, maxStamina);
    public float StaminaDrain => GetStatValue(StatType.StaminaDrain, staminaDrain);
    public float StaminaRecovery => GetStatValue(StatType.StaminaRecovery, staminaRecovery);

    public float AttackRange => GetStatValue(StatType.AttackRange, attackRange);
    public float BlockDamage => GetStatValue(StatType.BlockDamage, blockDamage);
    public float EntityDamage => GetStatValue(StatType.EntityDamage, entityDamage);
    public float TimeBtwAttacks => GetStatValue(StatType.TimeBtwAttacks, timeBtwAttacks);
    public float AttackStaminaUsage => GetStatValue(StatType.AttackStaminaUsage, attackStaminaUsage);

    public float Armor => GetStatValue(StatType.Armor, armor);

    public EntityStats(StatsMediator mediator, BaseStatsObject baseStatsObject) {
      this.mediator = mediator;
      this.baseStatsObject = baseStatsObject;
      this.mediator.SetStats(this);

      health = baseStatsObject.health;
      maxHealth = baseStatsObject.maxHealth;
      healthRegen = baseStatsObject.healthRegen;

      stamina = baseStatsObject.stamina;
      maxStamina = baseStatsObject.maxStamina;
      staminaDrain = baseStatsObject.staminaDrain;
      staminaRecovery = baseStatsObject.staminaRecovery;

      attackRange = baseStatsObject.attackRange;
      blockDamage = baseStatsObject.blockDamage;
      entityDamage = baseStatsObject.entityDamage;
      timeBtwAttacks = baseStatsObject.timeBtwAttacks;
      attackStaminaUsage = baseStatsObject.attackStaminaUsage;

      armor = baseStatsObject.armor;
    }

    public void UpdateStats(float deltaTime) {
      RecoverHp(deltaTime);
      RecoverStamina(deltaTime);
    }

    public bool IsValueReachMax(StatType type) {
      var maxValue = GetMaxValueByType(type);

      if (maxValue == null) {
        return false;
      }

      var value = GetValueByType(type);
      return value >= maxValue;
    }

    public void UpdateClearValueByModifier(StatModifier modifier) {
      switch (modifier.Type) {
        case StatType.Health:
          health = Mathf.Clamp(modifier.Strategy.Calculate(health), 0, MaxHealth);
          break;
        case StatType.MaxHealth:
          maxHealth = Mathf.Clamp(modifier.Strategy.Calculate(maxHealth), 0, MaxHealth);
          break;
        case StatType.HealthRegen:
          healthRegen = modifier.Strategy.Calculate(healthRegen);
          break;
        case StatType.Stamina:
          stamina = Mathf.Clamp(modifier.Strategy.Calculate(stamina), 0, MaxStamina);
          break;
        case StatType.MaxStamina:
          maxStamina = Mathf.Clamp(modifier.Strategy.Calculate(maxStamina), 0, MaxStamina);
          break;
        case StatType.StaminaRecovery:
          staminaRecovery = modifier.Strategy.Calculate(staminaRecovery);
          break;
        case StatType.StaminaDrain:
          staminaDrain = modifier.Strategy.Calculate(staminaDrain);
          break;
        case StatType.AttackRange:
          attackRange = modifier.Strategy.Calculate(attackRange);
          break;
        case StatType.BlockDamage:
          blockDamage = modifier.Strategy.Calculate(blockDamage);
          break;
        case StatType.EntityDamage:
          entityDamage = modifier.Strategy.Calculate(entityDamage);
          break;
        case StatType.TimeBtwAttacks:
          timeBtwAttacks = modifier.Strategy.Calculate(timeBtwAttacks);
          break;
        case StatType.AttackStaminaUsage:
          attackStaminaUsage = modifier.Strategy.Calculate(attackStaminaUsage);
          break;
        case StatType.Armor:
          armor = modifier.Strategy.Calculate(armor);
          break;
      }
    }

    public void UseStamina(float time) {
      staminaInUse = true;
      var amount = StaminaDrain * time;
      stamina = Mathf.Max(0, stamina - amount);
      staminaInUse = false;
    }

    public void AddHealth(float damage) {
      health = Mathf.Clamp(health + damage, 0, MaxHealth);
    }

    private void RecoverStamina(float time) {
      var max = MaxStamina;
      if (staminaInUse || stamina >= max) {
        return;
      }

      var amount = StaminaRecovery * time;
      stamina = Mathf.Clamp(stamina + amount, 0, max);
    }

    private void RecoverHp(float time) {
      var max = MaxHealth;
      if (health >= max) {
        return;
      }

      var amount = HealthRegen * time;
      health = Mathf.Clamp(health + amount, 0, max);
    }

    private float GetStatValue(StatType type, float baseValue) {
      if (!dirtyStats.Contains(type) && statValueCache.TryGetValue(type, out var cachedValue)) {
        return cachedValue;
      }

      var query = new Query(type, baseValue);
      mediator.PerformQuery(this, query);

      statValueCache[type] = query.Value;
      dirtyStats.Remove(type);

      return query.Value;
    }

    public void InvalidateStatCache(StatType type) {
      dirtyStats.Add(type);
    }

    private float GetValueByType(StatType type) {
      return type switch {
        StatType.Health => Health,
        StatType.MaxHealth => MaxHealth,
        StatType.HealthRegen => HealthRegen,
        StatType.Stamina => Stamina,
        StatType.MaxStamina => MaxStamina,
        StatType.StaminaRecovery => StaminaRecovery,
        StatType.StaminaDrain => StaminaDrain,
        StatType.AttackRange => AttackRange,
        StatType.BlockDamage => BlockDamage,
        StatType.EntityDamage => EntityDamage,
        StatType.TimeBtwAttacks => TimeBtwAttacks,
        StatType.AttackStaminaUsage => AttackStaminaUsage,
        StatType.Armor => Armor,
        _ => 0
      };
    }

    private float? GetMaxValueByType(StatType type) {
      return type switch {
        StatType.Health => MaxHealth,
        StatType.Stamina => MaxStamina,
        _ => null
      };
    }
  }
}