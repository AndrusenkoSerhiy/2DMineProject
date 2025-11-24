using System.Collections.Generic;
using SaveSystem;
using Scriptables.Stats;
using Stats;
using UnityEngine;

public class PlayerStats : StatsBase {
  private bool staminaInUse;
  public bool StaminaInUse{set{staminaInUse = value;}}
  public PlayerStatsObject StatsObject => (PlayerStatsObject)statsObject;

  public float Stamina => Mathf.Min(baseValues[StatType.Stamina], MaxStamina);
  public float MaxStamina => GetStatValue(StatType.MaxStamina);
  public float StaminaDrain => GetStatValue(StatType.StaminaDrain);
  public float StaminaRecovery => GetStatValue(StatType.StaminaRecovery);
  public float AttackRange => GetStatValue(StatType.AttackRange);
  public float BlockDamage => GetStatValue(StatType.BlockDamage);
  public float EntityDamage => GetStatValue(StatType.EntityDamage);
  public float TimeBtwAttacks => GetStatValue(StatType.TimeBtwAttacks);
  public float AttackStaminaUsage => GetStatValue(StatType.AttackStaminaUsage);
  public float MaxSpeed => GetStatValue(StatType.MaxSpeed);
  public float MaxBackSpeed => GetStatValue(StatType.MaxBackSpeed);
  public float SprintSpeed => GetStatValue(StatType.SprintSpeed);
  public float SprintBackSpeed => GetStatValue(StatType.SprintBackSpeed);

  //TODO: refactor
  protected override void Awake() => Init();

  public void Init(PlayerStatsData data = null) {
    base.Init(data?.Health);
    staminaInUse = false;

    baseValues[StatType.Stamina] = data?.Stamina ?? StatsObject.stamina;
    baseValues[StatType.MaxStamina] = StatsObject.maxStamina;
    baseValues[StatType.StaminaDrain] = StatsObject.staminaDrain;
    baseValues[StatType.StaminaRecovery] = StatsObject.staminaRecovery;

    baseValues[StatType.AttackRange] = StatsObject.attackRange;
    baseValues[StatType.BlockDamage] = StatsObject.blockDamage;
    baseValues[StatType.EntityDamage] = StatsObject.entityDamage;
    baseValues[StatType.TimeBtwAttacks] = StatsObject.timeBtwAttacks;
    baseValues[StatType.AttackStaminaUsage] = StatsObject.attackStaminaUsage;

    baseValues[StatType.MaxSpeed] = StatsObject.maxSpeed;
    baseValues[StatType.MaxBackSpeed] = StatsObject.maxBackSpeed;
    baseValues[StatType.SprintSpeed] = StatsObject.sprintSpeed;
    baseValues[StatType.SprintBackSpeed] = StatsObject.sprintBackSpeed;

    if (data?.StatModifiersData?.Count > 0) {
      Mediator.Load(data.StatModifiersData);
    }
  }

  public void UpdateBaseValue(StatType type, float value) {
    if (baseValues.ContainsKey(type)) {
      baseValues[type] = value;
      InvalidateStatCache(type);
    }
  }

  protected override void UpdateStats(float deltaTime) {
    base.UpdateStats(deltaTime);
    RecoverStamina(deltaTime);
  }

  public void UseStamina(float time) {
    baseValues[StatType.Stamina] = Mathf.Max(0, Stamina - StaminaDrain * time);
  }

  private void RecoverStamina(float time) {
    if (GameManager.Instance.Paused) {
      return;
    }
    if (staminaInUse || Stamina >= MaxStamina) {
      return;
    }
    var recovery = StaminaRecovery * time;
    baseValues[StatType.Stamina] = Mathf.Clamp(Stamina + recovery, 0, MaxStamina);
  }

  protected override float? GetMaxValueByType(StatType type) => type switch {
    StatType.Stamina => MaxStamina,
    StatType.MaxStamina => StatsObject.staminaMaxPossibleValue,
    _ => base.GetMaxValueByType(type)
  };

  protected override float GetValueByType(StatType type) => type switch {
    StatType.Stamina => Stamina,
    StatType.MaxStamina => MaxStamina,
    StatType.StaminaRecovery => StaminaRecovery,
    StatType.StaminaDrain => StaminaDrain,
    StatType.AttackRange => AttackRange,
    StatType.BlockDamage => BlockDamage,
    StatType.EntityDamage => EntityDamage,
    StatType.TimeBtwAttacks => TimeBtwAttacks,
    StatType.AttackStaminaUsage => AttackStaminaUsage,
    StatType.MaxSpeed => MaxSpeed,
    StatType.MaxBackSpeed => MaxBackSpeed,
    StatType.SprintSpeed => SprintSpeed,
    _ => base.GetValueByType(type)
  };

  public PlayerStatsData PrepareSaveData() {
    var playerStatsData = new PlayerStatsData {
      Health = Health,
      Stamina = Stamina,
      StatModifiersData = new List<StatModifierData>()
    };

    foreach (var statModifier in Mediator.ListModifiers) {
      var modifier = statModifier.Modifier;
      if (statModifier.MarkedForRemoval || modifier.applyType == ApplyType.Equip) {
        continue;
      }

      var data = new StatModifierData {
        Type = modifier.type,
        ApplyType = modifier.applyType,
        OperatorType = modifier.operatorType,
        Value = modifier.value,
        Duration = statModifier.Duration,
        TimeLeft = statModifier.TimeLeft,
        ItemId = statModifier.ItemId,
        ModifierDisplayObjectId = statModifier.modifierDisplayObject
          ? statModifier.modifierDisplayObject.Id
          : string.Empty
      };

      playerStatsData.StatModifiersData.Add(data);
    }

    return playerStatsData;
  }
}