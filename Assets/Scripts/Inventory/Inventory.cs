using System;
using System.Collections.Generic;
using System.Linq;
using Scriptables.Items;

namespace Inventory {
  public class ItemComparer : IEqualityComparer<Item> {
    public bool Equals(Item x, Item y) {
      return x != null && y != null && x.id == y.id;
    }

    public int GetHashCode(Item obj) {
      return obj.id.GetHashCode();
    }
  }

  public class Inventory {
    private string id;
    private GameManager gameManager;
    private List<InventoryObject> inventoriesObjects = new();

    public string Id => id;
    public InventorySlot[] Slots;
    public event Action OnResorted;
    public event Action<SlotSwappedEventData> OnSlotSwapped;
    public event Action OnSlotsCountChanged;
    public InventoryObject MainInventoryObject => inventoriesObjects[0];
    public InventoryType Type => MainInventoryObject.Type;
    public List<InventoryObject> InventoriesObjects => inventoriesObjects;

    public Inventory(InventoryObject mainInventory) {
      gameManager = GameManager.Instance;
      AddInventoryObject(mainInventory);
      id = mainInventory.Id;
    }

    public void AddInventoryObject(InventoryObject inventory) {
      inventoriesObjects.Add(inventory);
      Slots = Slots == null ? inventory.Slots : Slots.Concat(inventory.Slots).ToArray();
      OnSlotsCountChanged?.Invoke();
    }

    public void RemoveInventoryObject(InventoryObject inventory) {
      if (inventory.Id == MainInventoryObject.Id) {
        return;
      }

      inventoriesObjects.Remove(inventory);
      Slots = inventoriesObjects.SelectMany(inv => inv.Slots).ToArray();
      OnSlotsCountChanged?.Invoke();
    }

    public void SortInventory(bool ascending = true) {
      var tmpList = new Dictionary<Item, int>(new ItemComparer());

      for (var i = 0; i < Slots.Length; i++) {
        var slot = Slots[i];
        if (slot.isEmpty) {
          continue;
        }

        if (tmpList.ContainsKey(slot.Item)) {
          tmpList[slot.Item] += slot.amount;
        }
        else {
          tmpList[slot.Item] = slot.amount;
        }

        slot.PreventEvents()
          .RemoveItem();
      }

      // Sort items by name
      var sortedItems = ascending
        ? tmpList.OrderBy(pair => pair.Key.info.Name, StringComparer.Ordinal)
        : tmpList.OrderByDescending(pair => pair.Key.info.Name, StringComparer.Ordinal);

      var slotIndex = 0;
      foreach (var (item, totalAmount) in sortedItems) {
        var remainAmount = totalAmount;
        while (remainAmount > 0) {
          var slot = Slots[slotIndex];
          remainAmount = slot.PreventEvents().AddItem(remainAmount, item);
          slotIndex++;
        }
      }

      OnResorted?.Invoke();
    }


    public void MoveAllItemsTo(Inventory destinationInventory) {
      if (destinationInventory == null) {
        return;
      }

      foreach (var slot in Slots) {
        if (slot.isEmpty || !slot.CanMoveToAnotherInventory) {
          continue; // Skip
        }

        var remainingAmount = destinationInventory.AddItemBySlot(slot);

        if (remainingAmount <= 0) {
          slot.RemoveItem();
        }
        else {
          slot.SetAmount(remainingAmount);
        }
      }
    }

    /// <summary>
    /// Completes stacks in the destination inventory with items from this inventory.
    /// </summary>
    /// <param name="destinationInventory">The inventory to fill stacks in.</param>
    public void CompleteStacksIfExist(Inventory destinationInventory) {
      if (destinationInventory == null) {
        return;
      }

      foreach (var slot in Slots.Where(slot => !slot.isEmpty)) {
        while (slot.amount > 0) {
          var stackableSlot = destinationInventory.FindStackableItemOnInventory(slot.Item);
          if (stackableSlot == null) {
            break;
          }

          var canMoveAmount = Math.Min(stackableSlot.GetMaxSize() - stackableSlot.amount, slot.amount);
          slot.RemoveAmount(canMoveAmount);
          stackableSlot.AddAmount(canMoveAmount);
        }
      }
    }

    public void TakeSimilar(Inventory destinationInventory) {
      foreach (var slot in Slots) {
        if (slot.isEmpty || !slot.CanMoveToAnotherInventory) {
          continue;
        }

        foreach (var targetSlot in destinationInventory.Slots) {
          if (targetSlot.isEmpty) {
            continue;
          }

          if (slot.SlotsHasSameItems(targetSlot)) {
            var remainingAmount = destinationInventory.AddItemBySlot(slot);

            if (remainingAmount <= 0) {
              slot.RemoveItem();
            }
            else {
              slot.SetAmount(remainingAmount);
            }
          }
        }
      }
    }

    public bool MergeItems(InventorySlot slot, InventorySlot targetSlot) {
      var remainingAmount = AddItemBySlot(slot, targetSlot);

      if (remainingAmount <= 0) {
        slot.RemoveItem();
      }
      else {
        slot.SetAmount(remainingAmount);
      }

      return true;
    }

    public bool SwapSlots(InventorySlot slot, InventorySlot targetSlot) {
      var result = slot.SwapWith(targetSlot);
      OnSlotSwapped?.Invoke(new SlotSwappedEventData(slot, targetSlot));
      return result;
    }

    public int RemoveItem(string id, int amount) {
      if (amount <= 0) {
        return 0;
      }

      var remainingAmount = amount;

      for (var i = 0; i < Slots.Length && remainingAmount > 0; i++) {
        var slot = Slots[i];

        if (slot.isEmpty || slot.Item.info.Id != id) {
          continue;
        }

        var removeAmount = Math.Min(slot.amount, remainingAmount);
        remainingAmount -= removeAmount;
        var slotNewAmount = slot.amount - removeAmount;

        if (slotNewAmount <= 0) {
          slot.RemoveItem();
        }
        else {
          slot.SetAmount(slotNewAmount);
        }
      }

      return remainingAmount;
    }

    public int AddItemBySlot(InventorySlot slot, InventorySlot placeAt = null) {
      return AddItem(slot.Item, slot.amount, placeAt, slot);
    }

    /// <summary>
    /// Adds an item to the inventory, handling overflow and stacking where applicable.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <param name="amount">The amount of the item to add.</param>
    /// <param name="placeAt">Optional slot to place the item in.</param>
    /// <param name="formSlot">Optional slot form which the item is coming from.</param>
    /// <returns>The amount that couldn't be added due to lack of space.</returns>
    public int AddItem(Item item, int amount, InventorySlot placeAt = null, InventorySlot formSlot = null) {
      gameManager.RecipesManager.DiscoverMaterial(item.info);
      if (placeAt != null) {
        if (!placeAt.IsItemAllowed(item.info)) {
          return amount;
        }

        var overFlow = placeAt.isEmpty
          ? placeAt.AddItem(amount, item, formSlot)
          : placeAt.AddAmount(amount, formSlot);
        return HandleOverflow(overFlow, item);
      }

      var slot = FindStackableItemOnInventory(item);
      var emptySlotCount = GetEmptySlotCount(item.info);
      // If no empty slots and no existing stackable slot
      if (emptySlotCount <= 0 && slot == null) {
        return amount;
      }

      // Add to a new slot if item is non-stackable or no existing stack
      if (!item.info.Stackable || slot == null) {
        var emptySlot = GetEmptySlot(item.info);
        var overFlow = emptySlot.AddItem(amount, item, formSlot);
        return HandleOverflow(overFlow, item);
      }

      // Add to an existing stackable slot
      var remainingAmount = slot.AddAmount(amount, formSlot);
      return HandleOverflow(remainingAmount, item);
    }

    /// <summary>
    /// Handles overflow of items when adding to inventory, distributing overflow into new slots.
    /// </summary>
    /// <param name="overflowAmount">The amount of overflow to handle.</param>
    /// <param name="item">The item causing the overflow.</param>
    /// <returns>The remaining overflow amount if slots are unavailable, otherwise 0.</returns>
    private int HandleOverflow(int overflowAmount, Item item) {
      while (overflowAmount > 0) {
        var emptySlot = GetEmptySlot(item.info);
        if (emptySlot == null) {
          break;
        }

        overflowAmount = emptySlot.AddItem(overflowAmount, item);
      }

      return overflowAmount;
    }

    public void Clear() {
      foreach (var slot in Slots) {
        if (slot.isEmpty) {
          continue;
        }

        slot.RemoveItem();
      }
    }

    public int FreeSpaceForItem(ItemObject itemObj) {
      var count = 0;
      foreach (var slot in Slots) {
        if (!slot.isEmpty && slot.Item.info.Id != itemObj.Id) {
          continue;
        }

        count += slot.GetMaxSize(itemObj) - slot.amount;
      }

      return count;
    }

    public (List<string>, List<int>, int) FreeSpaces() {
      var ids = new List<string>();
      var freeCounts = new List<int>();
      var emptySlots = 0;

      for (var i = 0; i < Slots.Length; i++) {
        var slot = Slots[i];

        if (slot.isEmpty) {
          emptySlots++;
        }
        else {
          var slotItemId = slot.Item.info.Id;
          var free = slot.GetMaxSize() - slot.amount;

          ids.Add(slotItemId);
          freeCounts.Add(free);
        }
      }

      return (ids, freeCounts, emptySlots);
    }

    public bool CanAddItem(ItemObject item) {
      foreach (var slot in Slots) {
        if (slot.isEmpty || (slot.Item.info.Id == item.Id && slot.amount < slot.GetMaxSize(item))) {
          return true;
        }
      }

      return false;
    }

    public int GetTotalCount() {
      var count = 0;

      foreach (var slot in Slots) {
        if (slot.isEmpty) {
          continue;
        }

        count += slot.amount;
      }

      return count;
    }

    public int GetTotalCount(string itemId) {
      var count = 0;

      foreach (var slot in Slots) {
        if (slot.isEmpty || slot.Item.info.Id != itemId) {
          continue;
        }

        count += slot.amount;
      }

      return count;
    }

    private int GetEmptySlotCount(ItemObject itemObject) {
      var counter = 0;
      foreach (var slot in Slots) {
        if (slot.isEmpty && slot.IsItemAllowed(itemObject)) {
          counter++;
        }
      }

      return counter;
    }

    public InventorySlot FindFirstNotEmpty() {
      for (var i = 0; i < Slots.Length; i++) {
        var slot = Slots[i];
        if (!slot.isEmpty) {
          return slot;
        }
      }

      return null;
    }

    private InventorySlot FindStackableItemOnInventory(Item item) {
      foreach (var slot in Slots) {
        if (slot.isEmpty) {
          continue;
        }

        if (slot.Item.info.Id == item.info.Id && slot.amount < slot.GetMaxSize(item.info)) {
          return slot;
        }
      }

      return null;
    }

    public InventorySlot GetEmptySlot(ItemObject itemObject) {
      foreach (var slot in Slots) {
        if (slot.isEmpty && slot.IsItemAllowed(itemObject)) {
          return slot;
        }
      }

      return null;
    }

    public bool IsItemInInventory(ItemObject itemObject) {
      if (itemObject == null) {
        return false;
      }

      foreach (var slot in Slots) {
        if (!slot.isEmpty && slot.Item.info.Id == itemObject.Id) {
          return true;
        }
      }

      return false;
    }

    #region Repair

    public bool HasRepairKits() {
      var repairKitObject = gameManager?.ItemDatabaseObject?.RepairKit;
      return IsItemInInventory(repairKitObject);
    }

    /// <summary>
    /// Uses repair kits in the inventory. Returns the amount that couldn't be used.
    /// </summary>
    /// <param name="amount">The amount of repair kits to use.</param>
    /// <returns>The amount that couldn't be used due to not enough repair kits.</returns>
    public int UseRepairKits(int amount) {
      var repairKitObject = gameManager?.ItemDatabaseObject?.RepairKit;
      return repairKitObject == null ? amount : RemoveItem(repairKitObject.Id, amount);
    }

    #endregion
  }
}