/*using System;
using System.Collections.Generic;
using Inventory;
using Scriptables.Items;
using UnityEngine;
using UnityEngine.Rendering;

namespace Craft {
  [Serializable]
  public class InventoriesPool {
    private List<Inventory.Inventory> checkInventories = new();

    private SerializedDictionary<string, int> resourcesTotal = new();
    public event Action<string> OnResourcesTotalUpdate;
    public List<Inventory.Inventory> Inventories => checkInventories;

    public void Init() {
      InitInventories();
      CalculateResourcesTotal();
      AddEvents();
    }

    public int GetResourceTotalAmount(string resourceId) {
      return resourcesTotal.GetValueOrDefault(resourceId, 0);
    }

    public void RemoveFromInventoriesPool(string id, int amount) {
      if (amount <= 0) {
        return;
      }

      var remainingAmount = amount;
      foreach (var inventory in checkInventories) {
        remainingAmount = inventory.RemoveItem(id, remainingAmount);
        if (remainingAmount <= 0) {
          break;
        }
      }
    }

    public void AddItemToInventoriesPool(Item item, int amount) {
      if (amount <= 0) {
        return;
      }

      var remainingAmount = amount;
      foreach (var inventory in checkInventories) {
        remainingAmount = inventory.AddItem(item, remainingAmount);
        if (remainingAmount <= 0) {
          break;
        }
      }
    }

    private void InitInventories() {
      var playerInventory = GameManager.Instance.PlayerInventory;
      checkInventories.Add(playerInventory.GetInventory());
      checkInventories.Add(playerInventory.GetQuickSlots());
    }

    private void CalculateResourcesTotal() {
      resourcesTotal.Clear();

      foreach (var inventory in checkInventories) {
        foreach (var inventorySlot in inventory.Slots) {
          if (inventorySlot.isEmpty) {
            continue;
          }

          var item = inventorySlot.Item;

          UpdateResourceTotal(item.info.Id, inventorySlot.amount, false);
        }
      }
    }

    private void AddEvents() {
      GameManager.Instance.QuickSlotListener.OnActivate += QuickSlotActivateHandler;
      GameManager.Instance.QuickSlotListener.OnDeactivate += QuickSlotDeactivateHandler;

      foreach (var inventory in checkInventories) {
        inventory.OnSlotsCountChanged += SlotsCountChangedHandler;
      }

      AddSlotsUpdateEvents();
    }

    private void QuickSlotDeactivateHandler() {
      var playerInventory = GameManager.Instance.PlayerInventory;
      RemoveSlotsUpdateEvents();
      checkInventories.Remove(playerInventory.GetQuickSlots());
      CalculateResourcesTotal();
      AddSlotsUpdateEvents();
    }

    private void QuickSlotActivateHandler() {
      var playerInventory = GameManager.Instance.PlayerInventory;
      RemoveSlotsUpdateEvents();
      checkInventories.Add(playerInventory.GetQuickSlots());
      CalculateResourcesTotal();
      AddSlotsUpdateEvents();
    }

    private void SlotsCountChangedHandler() {
      RemoveSlotsUpdateEvents();
      CalculateResourcesTotal();
      AddSlotsUpdateEvents();
    }

    private void AddSlotsUpdateEvents() {
      foreach (var inventory in checkInventories) {
        foreach (var inventorySlot in inventory.Slots) {
          inventorySlot.OnAfterUpdated += SlotAmountUpdateHandler;
        }
      }
    }

    private void RemoveSlotsUpdateEvents() {
      foreach (var inventory in checkInventories) {
        foreach (var inventorySlot in inventory.Slots) {
          inventorySlot.OnAfterUpdated -= SlotAmountUpdateHandler;
        }
      }
    }

    private bool IsTypeInCheckInventories(InventoryType? inventoryType) {
      if (inventoryType == null) {
        return false;
      }

      foreach (var inventory in checkInventories) {
        if (inventory.Type == inventoryType) {
          return true;
        }
      }

      return false;
    }

    private void SlotAmountUpdateHandler(SlotUpdateEventData data) {
      var before = data.before;
      var after = data.after;
      var from = data.from;
      var itemBefore = before?.Item?.info;
      var itemAfter = after?.Item?.info;

      // Swap empty slots
      if (!itemBefore && !itemAfter) {
        return;
      }

      var itemsInSameInventoriesPool = IsTypeInCheckInventories(from?.InventoryType)
                                       && IsTypeInCheckInventories(after?.InventoryType);
      // Swap different in same inventory pool
      if (itemsInSameInventoriesPool && itemBefore != itemAfter) {
        return;
      }

      //Item amount changed
      if (before?.amount == after?.amount) {
        return;
      }

      var id = itemBefore?.Id ?? itemAfter?.Id;

      var amountDelta = after.amount - before.amount;

      UpdateResourceTotal(id, amountDelta);
    }

    private void UpdateResourceTotal(string resourceId, int amount, bool triggerEvent = true) {
      if (resourcesTotal.ContainsKey(resourceId)) {
        resourcesTotal[resourceId] += amount;

        if (resourcesTotal[resourceId] <= 0) {
          resourcesTotal.Remove(resourceId);
        }
      }
      else if (amount > 0) {
        resourcesTotal[resourceId] = amount;
      }

      if (triggerEvent) {
        OnResourcesTotalUpdate?.Invoke(resourceId);
      }
    }
  }
}*/