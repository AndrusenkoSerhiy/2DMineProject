using System.Collections.Generic;
using Scriptables.Stats;
using Stats;
using UnityEngine;
using StatModifier = Stats.StatModifier;

public class StatsBase {
  private readonly StatsMediator mediator;
  private readonly BaseStatsObject statsObject;
  private readonly Dictionary<StatType, float> statValueCache = new();
  private readonly HashSet<StatType> dirtyStats = new();

  protected readonly Dictionary<StatType, float> baseValues = new();

  public StatsMediator Mediator => mediator;
  public BaseStatsObject StatsObject => statsObject;

  public float Health => Mathf.Min(baseValues[StatType.Health], MaxHealth);
  public float MaxHealth => GetStatValue(StatType.MaxHealth);
  public float HealthRegen => GetStatValue(StatType.HealthRegen);

  public StatsBase(StatsMediator mediator, BaseStatsObject statsObject) {
    this.mediator = mediator;
    this.statsObject = statsObject;
    this.mediator.SetStats(this);

    baseValues[StatType.Health] = statsObject.health;
    baseValues[StatType.MaxHealth] = statsObject.maxHealth;
    baseValues[StatType.HealthRegen] = statsObject.healthRegen;
  }

  public void AddHealth(float amount) {
    baseValues[StatType.Health] = Mathf.Clamp(baseValues[StatType.Health] + amount, 0, MaxHealth);
  }

  public virtual void UpdateStats(float deltaTime) {
    RecoverHp(deltaTime);
  }

  public virtual bool IsValueReachMax(StatType type) {
    var max = GetMaxValueByType(type);
    return max.HasValue && GetValueByType(type) >= max.Value;
  }

  protected virtual void RecoverHp(float deltaTime) {
    var max = MaxHealth;
    var current = baseValues[StatType.Health];
    if (current >= max) {
      return;
    }

    var recovered = HealthRegen * deltaTime;
    baseValues[StatType.Health] = Mathf.Clamp(current + recovered, 0, max);
  }

  protected float GetStatValue(StatType type) {
    if (!dirtyStats.Contains(type) && statValueCache.TryGetValue(type, out var cached)) {
      return cached;
    }

    var baseValue = baseValues.ContainsKey(type) ? baseValues[type] : 0f;
    var result = mediator.Calculate(type, baseValue);

    statValueCache[type] = result;
    dirtyStats.Remove(type);

    return result;
  }

  public void InvalidateStatCache(StatType type) {
    dirtyStats.Add(type);
  }

  public virtual void UpdateClearValueByModifier(StatModifier modifier) {
    if (!baseValues.ContainsKey(modifier.Type)) {
      return;
    }

    var currentValue = baseValues[modifier.Type];
    var max = GetMaxValueByType(modifier.Type) ?? float.MaxValue;

    var newValue = modifier.Strategy.Calculate(currentValue);
    baseValues[modifier.Type] = Mathf.Clamp(newValue, 0, max);
  }

  public virtual float GetValueByType(StatType type) {
    return type switch {
      StatType.Health => Health,
      StatType.MaxHealth => MaxHealth,
      StatType.HealthRegen => HealthRegen,
      _ => baseValues.GetValueOrDefault(type, 0f)
    };
  }

  protected virtual float? GetMaxValueByType(StatType type) {
    return type switch {
      StatType.Health => MaxHealth,
      _ => null
    };
  }
}