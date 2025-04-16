using System;
using System.Collections.Generic;

namespace Scriptables.Items {
  [Serializable]
  public class Item {
    [NonSerialized] private IDurableItem durableItemRef;
    [NonSerialized] private IRepairable repairableItemRef;
    private bool hasDurability;
    private bool canBeRepaired;
    private float durability;
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

    public event Action<float, float> OnDurabilityChanged;
    public event Action OnItemBroken;
    public event Action OnItemRepaired;

    public Item() {
      info = null;
      id = string.Empty;
      name = string.Empty;
    }

    public Item(ItemObject item) {
      info = item;
      id = item.Id;
      name = item.Name;

      if (item is IDurableItem durableItem) {
        durableItemRef = durableItem;
        hasDurability = true;
        durability = durableItem.MaxDurability;
        maxDurability = durableItem.MaxDurability;
      }

      if (item is IRepairable repairable) {
        repairableItemRef = repairable;
        canBeRepaired = true;
      }
    }

    public bool isEmpty => info == null || string.IsNullOrEmpty(id);
    public bool hasId => !string.IsNullOrEmpty(id);

    public void RestoreItemObject(List<ItemObject> itemDatabase) {
      info = itemDatabase.Find(x => x.Id == id);
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