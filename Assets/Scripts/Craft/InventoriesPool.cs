using System.Collections.Generic;
using Inventory;
using Scriptables.Items;

namespace Craft {
  public class InventoriesPool {
    private PlayerInventory playerInventory;
    private List<Inventory.Inventory> pool = new();
    public List<Inventory.Inventory> Inventories => pool;

    public InventoriesPool() {
      playerInventory = GameManager.Instance.PlayerInventory;
      InitInventories();
      AddEvents();
    }

    public int GetResourceTotalAmount(string resourceId) {
      var totalAmount = 0;
      foreach (var inventory in pool) {
        totalAmount += inventory.GetTotalCount(resourceId);
      }

      return totalAmount;
    }

    public int RemoveFromInventoriesPool(string id, int amount) {
      if (amount <= 0) {
        return amount;
      }

      var remainingAmount = amount;
      foreach (var inventory in pool) {
        remainingAmount = inventory.RemoveItem(id, remainingAmount);
        if (remainingAmount <= 0) {
          break;
        }
      }

      return remainingAmount;
    }

    public int AddItemToInventoriesPool(Item item, int amount) {
      if (amount <= 0) {
        return amount;
      }

      var remainingAmount = amount;
      foreach (var inventory in pool) {
        remainingAmount = inventory.AddItem(item, remainingAmount);
        if (remainingAmount <= 0) {
          break;
        }
      }

      return remainingAmount;
    }

    public bool CanAddItem(ItemObject item) {
      foreach (var inventory in pool) {
        if (inventory.CanAddItem(item)) {
          return true;
        }
      }

      return false;
    }

    private void InitInventories() {
      pool.Add(playerInventory.GetInventory());
      pool.Add(playerInventory.GetQuickSlots());
    }

    private void AddEvents() {
      GameManager.Instance.QuickSlotListener.OnActivate += QuickSlotActivateHandler;
      GameManager.Instance.QuickSlotListener.OnDeactivate += QuickSlotDeactivateHandler;
    }

    private void QuickSlotDeactivateHandler() {
      pool.Remove(playerInventory.GetQuickSlots());
    }

    private void QuickSlotActivateHandler() {
      pool.Add(playerInventory.GetQuickSlots());
    }
  }
}