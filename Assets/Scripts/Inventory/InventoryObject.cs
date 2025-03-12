using System;
using System.Collections.Generic;
using System.Linq;
using SaveSystem;
using Scriptables.Items;
using UnityEngine;

namespace Inventory {
  public class ItemComparer : IEqualityComparer<Item> {
    public bool Equals(Item x, Item y) {
      return x != null && y != null && x.id == y.id;
    }

    public int GetHashCode(Item obj) {
      return obj.id.GetHashCode();
    }
  }

  public class InventoryObject {
    public ItemDatabaseObject database;
    public InventoryType type;
    public string Id => id;

    public event Action<SlotSwappedEventData> OnSlotSwapped;

    // public event Action OnLoaded;
    public event Action OnResorted;

    // private InventoryContainer Container = new InventoryContainer();
    private InventoryContainer Container;

    public InventorySlot[] GetSlots => Container.Slots;
    private bool loaded;
    private string id;
    private StorageType storageType;
    private GameManager gameManager;

    public InventoryObject(InventoryType type, string inventoryId, StorageType storageType) {
      Init(type);
      id = inventoryId;
      this.storageType = storageType;
    }

    public InventoryObject(InventoryType type, string inventoryId) {
      Init(type);
      id = inventoryId;
    }

    private void Init(InventoryType inventoryType) {
      var db = GameManager.Instance.ItemDatabaseObject;
      type = inventoryType;
      database = db;
      var size = type == InventoryType.Storage
        ? GameManager.Instance.PlayerInventory.GetStorageSizeByType(storageType)
        : GameManager.Instance.PlayerInventory.GetInventorySizeByType(type);
      Container = new InventoryContainer(size);
      gameManager = GameManager.Instance;
    }

    public void SortInventory(bool ascending = true) {
      var tmpList = new Dictionary<Item, int>(new ItemComparer());

      for (var i = 0; i < GetSlots.Length; i++) {
        var slot = GetSlots[i];
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
          var slot = GetSlots[slotIndex];
          remainAmount = slot.PreventEvents().AddItem(item, remainAmount);
          slotIndex++;
        }
      }

      OnResorted?.Invoke();
    }


    public void MoveAllItemsTo(InventoryObject destinationInventory) {
      if (destinationInventory == null) {
        return;
      }

      foreach (var slot in GetSlots) {
        if (slot.isEmpty || !slot.CanMoveToAnotherInventory) {
          continue; // Skip
        }

        var remainingAmount = destinationInventory.AddItemBySlot(slot);

        if (remainingAmount <= 0) {
          slot.RemoveItem();
        }
        else {
          slot.UpdateSlot(remainingAmount);
        }
      }
    }

    public void TakeSimilar(InventoryObject destinationInventory) {
      foreach (var slot in GetSlots) {
        if (slot.isEmpty || !slot.CanMoveToAnotherInventory) {
          continue;
        }

        foreach (var targetSlot in destinationInventory.GetSlots) {
          if (targetSlot.isEmpty) {
            continue;
          }

          if (slot.SlotsHasSameItems(targetSlot)) {
            var remainingAmount = destinationInventory.AddItemBySlot(slot);

            if (remainingAmount <= 0) {
              slot.RemoveItem();
            }
            else {
              slot.UpdateSlot(remainingAmount);
            }
          }
        }
      }
    }

    public void MergeItems(InventorySlot slot, InventorySlot targetSlot) {
      if (!slot.SlotsHasSameItems(targetSlot)) {
        return;
      }

      var remainingAmount = AddItemBySlot(slot, targetSlot);

      if (remainingAmount <= 0) {
        slot.RemoveItem();
      }
      else {
        slot.UpdateSlot(remainingAmount);
      }
    }

    public void SwapSlots(InventorySlot slot, InventorySlot targetSlot) {
      slot.SwapWith(targetSlot);
      OnSlotSwapped?.Invoke(new SlotSwappedEventData(slot, targetSlot));
    }

    public int RemoveItem(string id, int amount) {
      if (amount <= 0) {
        return 0;
      }

      var remainingAmount = amount;

      for (var i = 0; i < GetSlots.Length && remainingAmount > 0; i++) {
        var slot = GetSlots[i];

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
          slot.UpdateSlot(slotNewAmount);
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
        var overFlow = placeAt.isEmpty
          ? placeAt.UpdateSlot(amount, item, formSlot)
          : placeAt.AddAmount(amount, formSlot);
        return HandleOverflow(overFlow, item);
      }

      var slot = FindStackableItemOnInventory(item);
      // If no empty slots and no existing stackable slot
      if (emptySlotCount <= 0 && slot == null) {
        return amount;
      }

      // Add to a new slot if item is non-stackable or no existing stack
      if (!item.info.Stackable || slot == null) {
        var emptySlot = GetEmptySlot();
        var overFlow = emptySlot.UpdateSlot(amount, item, formSlot);
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
      var maxStackSize = item.info.MaxStackSize;

      var countRepeat = Mathf.CeilToInt((float)overflowAmount / maxStackSize);

      for (var i = 0; i < countRepeat; i++) {
        var emptySlot = GetEmptySlot();
        if (emptySlot != null) {
          emptySlot.UpdateSlot(overflowAmount, item);
          overflowAmount -= maxStackSize;
        }
        else {
          return overflowAmount;
        }
      }

      return 0;
    }

    public int FreeSpaceForItem(ItemObject itemObj) {
      var count = 0;
      foreach (var slot in GetSlots) {
        if (!slot.isEmpty && slot.Item.info.Id != itemObj.Id) {
          continue;
        }

        count += itemObj.MaxStackSize - slot.amount;
      }

      return count;
    }

    public (string[], int[], int) FreeSpaces() {
      var ids = new string[GetSlots.Length];
      var freeCounts = new int[GetSlots.Length];
      var emptySlots = 0;

      for (var i = 0; i < GetSlots.Length; i++) {
        var slot = GetSlots[i];

        if (slot.isEmpty) {
          emptySlots++;
          ids[i] = string.Empty;
          freeCounts[i] = 0;
        }
        else {
          var slotItemId = slot.Item.info.Id;
          var free = slot.Item.info.MaxStackSize - slot.amount;

          ids[i] = slotItemId;
          freeCounts[i] = free;
        }
      }

      return (ids, freeCounts, emptySlots);
    }

    public int GetTotalCount() {
      var count = 0;

      foreach (var slot in GetSlots) {
        if (slot.isEmpty) {
          continue;
        }

        count += slot.amount;
      }

      return count;
    }

    private int emptySlotCount {
      get {
        var counter = 0;
        for (var i = 0; i < GetSlots.Length; i++) {
          if (GetSlots[i].isEmpty) {
            counter++;
          }
        }

        return counter;
      }
    }

    public InventorySlot FindFirstNotEmpty() {
      for (var i = 0; i < GetSlots.Length; i++) {
        var slot = GetSlots[i];
        if (!slot.isEmpty) {
          return slot;
        }
      }

      return null;
    }

    public InventorySlot FindStackableItemOnInventory(Item item) {
      for (var i = 0; i < GetSlots.Length; i++) {
        var slot = GetSlots[i];
        if (slot.isEmpty) {
          continue;
        }

        if (slot.Item?.info.Id == item.info.Id && slot.amount < item.info.MaxStackSize) {
          return GetSlots[i];
        }
      }

      return null;
    }

    public InventorySlot GetEmptySlot() {
      for (var i = 0; i < GetSlots.Length; i++) {
        if (GetSlots[i].isEmpty) {
          return GetSlots[i];
        }
      }

      return null;
    }

    public void SaveToGameData() {
      SaveLoadSystem.Instance.gameData.Inventories[Id] = new InventoryData {
        Id = Id,
        Slots = GetSlots
      };
    }

    public void LoadFromGameData() {
      if (!SaveLoadSystem.Instance.gameData.Inventories.TryGetValue(Id, out var data)) {
        return;
      }

      var isNew = data.Slots == null || data.Slots.Length == 0;
      if (isNew) {
        return;
      }

      Load(data.Slots);
    }

    private void Load(InventorySlot[] slots) {
      if (loaded) {
        return;
      }

      for (var i = 0; i < slots.Length; i++) {
        var slotData = slots[i];
        if (slotData.Item.id == string.Empty) {
          continue;
        }

        slotData.Item.RestoreItemObject(database.ItemObjects);

        GetSlots[i].UpdateSlotBySlot(slotData);
      }

      loaded = true;
      // OnLoaded?.Invoke();
    }
  }
}