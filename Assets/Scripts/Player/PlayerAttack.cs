using Inventory;
using Items;
using Scriptables.Items;
using Tools;
using UnityEngine;

namespace Player {
  public class PlayerAttack : BaseAttack {
    [SerializeField] protected PlayerEquipment playerEquipment;

    protected override void Awake() {
      base.Awake();
      SetDefaultAttackParam();
      playerEquipment.OnEquippedWeapon += UpdateAttackParam;
      playerEquipment.OnUnequippedWeapon += SetParamsFromPlayerStats;
      playerEquipment = GetComponent<PlayerEquipment>();
      //equipped item init before this awake, and we need to update param from this item
      UpdateAttackParam();
      //init max target count to 1
      objectHighlighter.SetMaxHighlights(1);
    }

    private void SetDefaultAttackParam() {
      UpdateParams(.5f, stats.AttackRange, stats.AttackColliderSize.x, stats.AttackColliderSize.y);
    }

    //get param from equipped tool
    protected override void PrepareAttackParams() {
      if (SetAttackParamsFromEquipment()) {
        return;
      }

      Debug.LogWarning("Could not set attack parameters from equipment", this);
      SetParamsFromPlayerStats();
    }

    private void SetParamsFromPlayerStats() {
      attackLayer = stats.AttackLayer;
      blockDamage = stats.BlockDamage;
      entityDamage = stats.EntityDamage;
      attackRange = stats.Range;
      timeBtwAttacks = stats.TimeBtwAttacks;
      staminaUsage = stats.StaminaUsage;
      attackID = stats.AttackID;
      colliderSize = stats.AttackColliderSize;
      SetDefaultAttackParam();
    }

    private void UpdateAttackParam() {
      SetAttackParamsFromEquipment();
      //try to activate tool
      TryActivateTool();
      UpdateParams(.5f, attackRange, colliderSize.x, colliderSize.y);
      objectHighlighter.SetMaxHighlights(maxTargets);
    }

    private void TryActivateTool() {
      if(playerEquipment.ItemInHand == null)
        return;
      
      var tool = playerEquipment.ItemInHand.GetComponent<ToolBase>();
      tool?.Activate();
    }

    private bool SetAttackParamsFromEquipment() {
      if (playerEquipment == null) {
        Debug.LogWarning("Could not find Player Equipment", this);
        return false;
      }

      if (playerEquipment.ItemInHand == null) {
        Debug.LogWarning("Could not find equipped weapon", this);
        return false;
      }

      var weaponStats = playerEquipment.ItemInHand.GetComponent<GroundItem>().Item;
      if (!(weaponStats is IAttackableItem attackableItem)) {
        Debug.LogWarning("Equipped item is not attackable", this);
        return false;
      }

      //Debug.LogError("SetAttackParamsFromEquipment");
      attackLayer = attackableItem.AttackLayer;
      blockDamage = attackableItem.BlockDamage;
      entityDamage = attackableItem.EntityDamage;
      attackRange = attackableItem.Range;
      timeBtwAttacks = attackableItem.TimeBtwAttacks;
      staminaUsage = attackableItem.StaminaUsage;
      attackID = attackableItem.AnimationAttackID;
      colliderSize = attackableItem.ColliderSize;
      maxTargets = attackableItem.MaxTargets;
      return true;
    }

    protected override void OnDestroy() {
      base.OnDestroy();
      playerEquipment.OnEquippedWeapon -= UpdateAttackParam;
      playerEquipment.OnUnequippedWeapon -= SetParamsFromPlayerStats;
    }
  }
}