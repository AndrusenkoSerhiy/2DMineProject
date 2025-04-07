using Scriptables.Stats;
using Stats;
using UnityEngine;

public class PlayerStats : StatsBase {
  private bool staminaInUse;

  public float Stamina => Mathf.Min(baseValues[StatType.Stamina], MaxStamina);
  public float MaxStamina => GetStatValue(StatType.MaxStamina);
  public float StaminaDrain => GetStatValue(StatType.StaminaDrain);
  public float StaminaRecovery => GetStatValue(StatType.StaminaRecovery);
  public float AttackRange => GetStatValue(StatType.AttackRange);
  public float BlockDamage => GetStatValue(StatType.BlockDamage);
  public float EntityDamage => GetStatValue(StatType.EntityDamage);
  public float TimeBtwAttacks => GetStatValue(StatType.TimeBtwAttacks);
  public float AttackStaminaUsage => GetStatValue(StatType.AttackStaminaUsage);
  public float Armor => GetStatValue(StatType.Armor);

  public PlayerStats(StatsMediator mediator, PlayerStatsObject statsObject) : base(mediator, statsObject) {
    baseValues[StatType.Stamina] = statsObject.stamina;
    baseValues[StatType.MaxStamina] = statsObject.maxStamina;
    baseValues[StatType.StaminaDrain] = statsObject.staminaDrain;
    baseValues[StatType.StaminaRecovery] = statsObject.staminaRecovery;

    baseValues[StatType.AttackRange] = statsObject.attackRange;
    baseValues[StatType.BlockDamage] = statsObject.blockDamage;
    baseValues[StatType.EntityDamage] = statsObject.entityDamage;
    baseValues[StatType.TimeBtwAttacks] = statsObject.timeBtwAttacks;
    baseValues[StatType.AttackStaminaUsage] = statsObject.attackStaminaUsage;
    baseValues[StatType.Armor] = statsObject.armor;
  }

  public override void UpdateStats(float deltaTime) {
    base.UpdateStats(deltaTime);
    RecoverStamina(deltaTime);
  }

  public void UseStamina(float time) {
    staminaInUse = true;
    baseValues[StatType.Stamina] = Mathf.Max(0, Stamina - StaminaDrain * time);
    staminaInUse = false;
  }

  public void RecoverStamina(float time) {
    if (staminaInUse || Stamina >= MaxStamina) {
      return;
    }

    var recovery = StaminaRecovery * time;
    baseValues[StatType.Stamina] = Mathf.Clamp(Stamina + recovery, 0, MaxStamina);
  }

  protected override float? GetMaxValueByType(StatType type) => type switch {
    StatType.Stamina => MaxStamina,
    _ => base.GetMaxValueByType(type)
  };

  public override float GetValueByType(StatType type) => type switch {
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
    _ => base.GetValueByType(type)
  };
}