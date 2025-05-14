using Inventory;
using Scriptables;
using Scriptables.Items;
using Tools;
using UnityEngine;

namespace Player {
  public class PlayerAttack : BaseAttack {
    [SerializeField] private AudioData audioData;
    [SerializeField] protected PlayerEquipment playerEquipment;

    protected override void Awake() {
      base.Awake();
      SetDefaultAttackParam();
      playerEquipment.OnEquippedWeapon += UpdateAttackParam;
      playerEquipment.OnUnequippedWeapon += SetParamsFromPlayerStats;
      playerEquipment = GetComponent<PlayerEquipment>();
    }

    protected override void Start() {
      //equipped item init before this awake, and we need to update param from this item
      UpdateAttackParam();
      //init max target count to 1
      objectHighlighter.SetMaxHighlights(1);
      base.Start();
    }

    protected override void AfterTargetsTakenDamage(int targetsCount) {
      var isHit = targetsCount > 0;
      //play sound when we hit something
      if (isHit && audioData) {
        GameManager.Instance.AudioController.PlayAudio(audioData);
      }

      if (playerEquipment.EquippedItem == null) {
        return;
      }

      playerEquipment.EquippedItem.ApplyDurabilityLoss(isHit);
    }
    
    protected override void RangeAttack() {
      playerEquipment.EquippedItem.ApplyDurabilityLoss(false);
    }

    private void SetDefaultAttackParam() {
      UpdateParams(.5f, statsObject.attackRange, statsObject.attackColliderSize.x, statsObject.attackColliderSize.y);
    }

    protected override void TriggerAttack() {
      base.TriggerAttack();
      if (!firstAttack) {
        firstAttack = true;
        animator.SetTrigger("Attack");
        animator.SetInteger("WeaponID", attackID);
      }
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
      CancelAttack(null, null);
      attackLayer = statsObject.attackLayer;
      attackID = statsObject.attackID;
      colliderSize = statsObject.attackColliderSize;
      isRangedAttack = false;
      SetDefaultAttackParam();
    }

    private void UpdateAttackParam() {
      SetAttackParamsFromEquipment();
      //try to activate tool
      TryActivateTool();
      // UpdateParams(.5f, GetEntityStats().AttackRange, colliderSize.x, colliderSize.y);
      UpdateParams(.5f, PlayerStats.AttackRange, colliderSize.x, colliderSize.y);
      objectHighlighter.SetMaxHighlights(maxTargets);
    }

    private void TryActivateTool() {
      if (playerEquipment.ItemInHand == null)
        return;

      var tool = playerEquipment.ItemInHand.GetComponent<HandItem>();
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

      var weaponStats = playerEquipment.ItemInHand.GetComponent<HandItem>().Item;
      if (!(weaponStats is IAttackableItem attackableItem)) {
        Debug.LogWarning("Equipped item is not attackable", this);
        return false;
      }

      //Debug.LogError("SetAttackParamsFromEquipment");
      attackLayer = attackableItem.AttackLayer;

      attackID = attackableItem.AnimationAttackID;
      colliderSize = attackableItem.ColliderSize;
      maxTargets = attackableItem.MaxTargets;

      isRangedAttack = attackableItem.WeaponType == WeaponType.Ranged;
      objectHighlighter.ChangeCrosshair(isRangedAttack);

      return true;
    }

    protected override void OnDestroy() {
      base.OnDestroy();
      playerEquipment.OnEquippedWeapon -= UpdateAttackParam;
      playerEquipment.OnUnequippedWeapon -= SetParamsFromPlayerStats;
    }
  }
}