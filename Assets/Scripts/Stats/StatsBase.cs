using System.Collections.Generic;
using Scriptables.Stats;
using Stats;
using UnityEngine;
using StatModifier = Stats.StatModifier;

public class StatsBase : MonoBehaviour {
  [SerializeField] protected BaseStatsObject statsObject;

  private StatsMediator mediator;
  private readonly Dictionary<StatType, float> statValueCache = new();
  private readonly HashSet<StatType> dirtyStats = new();

  protected readonly Dictionary<StatType, float> baseValues = new();

  public StatsMediator Mediator => mediator;
  public BaseStatsObject StatsObject => statsObject;

  public float Health => Mathf.Min(baseValues[StatType.Health], MaxHealth);
  public float MaxHealth => GetStatValue(StatType.MaxHealth);
  public float HealthRegen => GetStatValue(StatType.HealthRegen);

  protected virtual void Awake() {
    mediator = new StatsMediator();
    mediator.SetStats(this);

    baseValues[StatType.Health] = statsObject.health;
    baseValues[StatType.MaxHealth] = statsObject.maxHealth;
    baseValues[StatType.HealthRegen] = statsObject.healthRegen;
  }

  private void Update() {
    UpdateStats(Time.deltaTime);
    mediator.Update(Time.deltaTime);
  }

  public void AddHealth(float amount) {
    baseValues[StatType.Health] = Mathf.Clamp(baseValues[StatType.Health] + amount, 0, MaxHealth);
  }

  protected virtual void UpdateStats(float deltaTime) {
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
    var max = GetMaxValueByType(type);
    if (max.HasValue) {
      result = Mathf.Min(result, max.Value);
    }

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
    var max = GetMaxValueByType(modifier.Type);

    var newValue = modifier.Strategy.Calculate(currentValue);

    if (max.HasValue) {
      newValue = Mathf.Min(newValue, max.Value);
    }

    baseValues[modifier.Type] = newValue;
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
      StatType.MaxHealth => statsObject.healthMaxPossibleValue,
      _ => null
    };
  }
}