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

    protected override void Attack() {
      targets = LockByWall();
      
      var lookDir = GetLookDirection();

      if ((targets == null || targets.Count == 0)
          && !HasNearbyZombie()) {
        return;
      }

      IDamageable target = null;
      var hasZombieTarget = false;
      if (targets != null && targets.Count > 0) {
        target = targets[0];
      }
      float closestDist = float.MaxValue;
      //try to find nearest zombie
      foreach (var npc in GameManager.Instance.ActorBaseController.Enemies) {
        Vector2 toNpc = npc.transform.position - transform.position;
        var distance = toNpc.magnitude;

        if (distance > playerStats.AttackRange)
          continue;

        var dot = Vector2.Dot(lookDir.normalized, toNpc.normalized);

        // NPC має бути попереду
        if (dot < 0.3f)
          continue;
        if (distance < closestDist) {
          closestDist = distance;
          target = npc;
          hasZombieTarget = true;
        }
      }

      var damage = hasZombieTarget ? playerStats.EntityDamage : playerStats.BlockDamage;
      target?.Damage(damage, true);
      var targetsCount = targets != null && targets.Count > 0 || hasZombieTarget ? 1 : 0;
      AfterTargetsTakenDamage(targetsCount);
      
      //attack blocks
      if (targets != null && targetsCount > 0) {
        for (int i = 0; i < targets.Count; i++) {
          targets[i]?.AfterDamageReceived();
        }
      }
      //attack zombie
      if (hasZombieTarget) {
        target.AfterDamageReceived();
      }
    }

    private Vector2 GetLookDirection() {
      return transform.localScale.x > 0
        ? Vector2.right
        : Vector2.left;
    }
    
    //If targets is empty try to check are we have zombie in attack range
    private bool HasNearbyZombie() {
      foreach (var npc in GameManager.Instance.ActorBaseController.Enemies){
        Vector2 toNpc = npc.transform.position - transform.position;
        var distance = toNpc.magnitude;
        if (distance <= playerStats.AttackRange)
          return true;
      }
      return false;
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
      //if we use last item, then we block attack first time
      if (!GameManager.Instance.IsConsumeItem) {
        if (!firstAttack) {
          firstAttack = true;
          animator.SetTrigger("Attack");
          animator.SetInteger("WeaponID", attackID);
        }
      }
      else {
        GameManager.Instance.IsConsumeItem = false;
      }
    }
    
    public override void GetDirection() {
      Vector2 direction = attackCollider.transform.position - transform.position;
      //Debug.LogError($"directionY {direction.y}");

      //3f distance between player and mouse for top border 
      if (direction.y > 3f) {
        lookDirection = 1;
        
        //use only for drill upside attack
        if (animator.GetInteger("WeaponID") == 1 && Mathf.Abs(direction.x) > 1) {
          lookDirection = 2;
        }
      }
      else if (direction.y < .3f) {
        lookDirection = -1;
        //use only for drill upside attack
        if (animator.GetInteger("WeaponID") == 1 && Mathf.Abs(direction.x) > 1) {
          lookDirection = -2;
        }
      }
      else {
        lookDirection = 0;
      }

      animator.SetInteger(animParam.LookDirection, lookDirection);
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