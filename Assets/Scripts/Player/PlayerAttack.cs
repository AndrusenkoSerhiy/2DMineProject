using Inventory;
// using Scriptables;
using Scriptables.Items;
using Tools;
using UnityEngine;

namespace Player {
  public class PlayerAttack : BaseAttack {
    // [SerializeField] private AudioData audioData;
    // [SerializeField] protected PlayerEquipment playerEquipment;

    private HandItem handItem;

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
      /*//play sound when we hit something
      if (isHit && audioData) {
        GameManager.Instance.AudioController.PlayAudio(audioData);
      }*/

      if (playerEquipment.EquippedItem == null) {
        return;
      }

      playerEquipment.EquippedItem.ApplyDurabilityLoss(isHit);
    }

    protected override void RangeAttack() {
      if (!handItem) {
        return;
      }

      handItem.StartUse();
    }

    private void SetDefaultAttackParam() {
      UpdateParams(.5f, statsObject.attackRange, statsObject.attackColliderSize.x, statsObject.attackColliderSize.y);
    }

    protected override void TriggerAttack() {
      if (isRangedAttack && !playerEquipment.EquippedItem.CanShoot()) {
        return;
      }

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

      SetParamsFromPlayerStats();
    }

    private void SetParamsFromPlayerStats() {
      CancelAttack(null, null);
      attackLayer = statsObject.attackLayer;
      attackID = statsObject.attackID;
      colliderSize = statsObject.attackColliderSize;
      isRangedAttack = false;
      handItem = null;
      objectHighlighter.ChangeCrosshair(isRangedAttack);
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
      if (!handItem) {
        return;
      }

      handItem.Activate();
    }

    private bool SetAttackParamsFromEquipment() {
      if (!playerEquipment) {
        Debug.LogWarning("Could not find Player Equipment", this);
        return false;
      }

      if (!playerEquipment.ItemInHand) {
        return false;
      }

      handItem = playerEquipment.ItemInHand.GetComponent<HandItem>();
      var weaponStats = handItem.Item;
      if (!(weaponStats is IAttackableItem attackableItem)) {
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