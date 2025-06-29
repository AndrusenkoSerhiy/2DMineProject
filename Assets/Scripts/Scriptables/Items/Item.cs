﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scriptables.Items {
  [Serializable]
  public class Item {
    [NonSerialized] private IDurableItem durableItemRef;
    [NonSerialized] private IRepairable repairableItemRef;
    [NonSerialized] private IAttackableItem attackableItemRef;
    [SerializeField] private float durability;
    [SerializeField] private int ammoCount;
    private bool hasDurability;
    private bool canBeRepaired;
    private float maxDurability;
    private bool isBroken;

    [NonSerialized] public ItemObject info;
    public string id;
    public string name;

    public float Durability => durability;
    public float MaxDurability => maxDurability;

    // public bool HasDurability => hasDurability;
    public bool IsBroken => isBroken;
    public bool CanBeRepaired => canBeRepaired && DurabilityNotFull();
    public int RepairCost => repairableItemRef?.RepairCost ?? 0;
    public int CurrentAmmoCount => ammoCount;
    public int MagazineSize => attackableItemRef?.MagazineSize ?? 0;
    public float ReloadTime => attackableItemRef?.ReloadTime ?? 0;
    public AudioData ReloadSound => attackableItemRef?.ReloadSound;

    public event Action<float, float> OnDurabilityChanged;
    public event Action OnItemBroken;
    public event Action OnItemRepaired;
    public event Action OnAmmoUsed;

    public Item() {
      info = null;
      id = string.Empty;
      name = string.Empty;
    }

    public Item(ItemObject item, float? restoreDurability = null) {
      info = item;
      id = item.Id;
      name = item.Name;

      if (item is IDurableItem durableItem) {
        durableItemRef = durableItem;
        hasDurability = true;
        durability = restoreDurability ?? durableItem.MaxDurability;
        maxDurability = durableItem.MaxDurability;
      }

      if (item is IRepairable repairable) {
        repairableItemRef = repairable;
        canBeRepaired = true;
      }

      if (item is IAttackableItem attackable) {
        attackableItemRef = attackable;
        ammoCount = 0;
      }
    }

    public bool isEmpty => info == null || string.IsNullOrEmpty(id);
    public bool hasId => !string.IsNullOrEmpty(id);

    public void RestoreItemObject(List<ItemObject> itemDatabase) {
      info = itemDatabase.Find(x => x.Id == id);

      if (info is IDurableItem durableItem) {
        durableItemRef = durableItem;
        hasDurability = true;
        maxDurability = durableItem.MaxDurability;

        durability = Mathf.Clamp(durability, 0, maxDurability);
        isBroken = durability == 0;
      }

      if (info is IRepairable repairable) {
        repairableItemRef = repairable;
        canBeRepaired = true;
      }

      if (info is IAttackableItem attackable) {
        attackableItemRef = attackable;
      }
    }

    public bool CanShoot() {
      if (attackableItemRef is not { WeaponType: WeaponType.Ranged } || isBroken) {
        return false;
      }

      return ammoCount > 0;
    }

    public bool ReloadNeeded() {
      if (attackableItemRef is not { WeaponType: WeaponType.Ranged }) {
        return false;
      }

      return ammoCount < attackableItemRef.MagazineSize;
    }

    public void Reload(int ammo) {
      if (attackableItemRef is not { WeaponType: WeaponType.Ranged }) {
        return;
      }

      ammoCount = Math.Min(attackableItemRef.MagazineSize, ammoCount + ammo);
    }

    public ItemObject GetAmmoItemObject() {
      return attackableItemRef is not { WeaponType: WeaponType.Ranged } ? null : attackableItemRef.Ammo;
    }

    public void ConsumeAmmo() {
      ammoCount--;
      OnAmmoUsed?.Invoke();
    }

    public bool DurabilityNotFull() {
      if (!hasDurability) {
        return false;
      }

      return durability < durableItemRef.MaxDurability;
    }

    public void ApplyDurabilityLoss(bool isHit) {
      if (!hasDurability) {
        return;
      }

      if (durableItemRef.DurabilityUsageType == DurabilityUsageType.OnHit && !isHit) {
        return;
      }

      var before = durability;

      durability = MathF.Max(0, durability - durableItemRef.DurabilityUse);

      if (durability == 0) {
        isBroken = true;
        OnItemBroken?.Invoke();
      }

      OnDurabilityChanged?.Invoke(before, durability);
    }

    public void AddDurability(int repairValue) {
      if (!hasDurability) {
        return;
      }

      var before = durability;

      durability = MathF.Min(maxDurability, durability + repairValue);

      if (durability > 0 && isBroken) {
        isBroken = false;
        OnItemRepaired?.Invoke();
      }

      OnDurabilityChanged?.Invoke(before, durability);
    }
  }
}