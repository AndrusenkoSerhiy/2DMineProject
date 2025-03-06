using System;
using System.Collections.Generic;
using Inventory;
using UnityEngine;
using UnityEngine.Rendering;
using UnityServiceLocator;

namespace Craft {
  public class TotalAmount : MonoBehaviour, ITotalAmount {
    private List<InventoryObject> checkInventories = new();

    private SerializedDictionary<string, int> resourcesTotal = new();
    public event Action<string> onResourcesTotalUpdate;

    public void Awake() {
      ServiceLocator.For(this).Register<ITotalAmount>(this);

      InitInventories();
    }

    public void InitComponent() {
      CalculateResourcesTotal();
      AddEvents();
    }

    public void ClearComponent() {
      RemoveEvents();
      resourcesTotal.Clear();
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

    private void InitInventories() {
      var playerInventory = GameManager.Instance.PlayerInventory;
      checkInventories.Add(playerInventory.GetInventory());
      checkInventories.Add(playerInventory.GetQuickSlots());
    }

    private void CalculateResourcesTotal() {
      resourcesTotal.Clear();

      foreach (var inventory in checkInventories) {
        foreach (var inventorySlot in inventory.GetSlots) {
          var item = inventorySlot.Item;

          if (item.isEmpty) {
            continue;
          }

          UpdateResourceTotal(item.info.Id, inventorySlot.amount, false);
        }
      }
    }

    private void AddEvents() {
      if (checkInventories.Count == 0) {
        Debug.LogError("Craft TotalAmount: No inventories to check");
        return;
      }

      foreach (var inventory in checkInventories) {
        foreach (var inventorySlot in inventory.GetSlots) {
          inventorySlot.OnAfterUpdated += SlotAmountUpdateHandler;
        }
      }
    }

    private void RemoveEvents() {
      if (checkInventories.Count == 0) {
        Debug.LogError("Craft TotalAmount: No inventories to check");
        return;
      }

      foreach (var inventory in checkInventories) {
        foreach (var inventorySlot in inventory.GetSlots) {
          inventorySlot.OnAfterUpdated -= SlotAmountUpdateHandler;
        }
      }
    }

    private bool IsTypeInCheckInventories(InventoryType? inventoryType) {
      if (inventoryType == null) {
        return false;
      }

      foreach (var inventory in checkInventories) {
        if (inventory.type == inventoryType) {
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
        onResourcesTotalUpdate?.Invoke(resourceId);
      }
    }
  }
}