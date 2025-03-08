using System;
using System.Collections.Generic;
using System.Linq;
using Items;
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
    public string Id => !string.IsNullOrEmpty(id) ? id : type.ToString();
    public event Action<SlotSwappedEventData> OnSlotSwapped;
    // public event Action OnLoaded;
    public event Action OnResorted;

    // private InventoryContainer Container = new InventoryContainer();
    private InventoryContainer Container;

    public InventorySlot[] GetSlots => Container.Slots;
    private bool loaded;
    private string id;
    private StorageType storageType;

    public InventoryObject(InventoryType type, string inventoryId, StorageType storageType) {
      Init(type);
      id = inventoryId;
      this.storageType = storageType;
    }

    public InventoryObject(InventoryType type) => Init(type);

    private void Init(InventoryType inventoryType) {
      var db = GameManager.Instance.ItemDatabaseObject;
      type = inventoryType;
      database = db;
      var size = type == InventoryType.Storage
        ? GameManager.Instance.PlayerInventory.GetStorageSizeByType(storageType)
        : GameManager.Instance.PlayerInventory.GetInventorySizeByType(type);
      Container = new InventoryContainer(size);
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
      return AddItem(slot.Item, slot.amount, placeAt, null, slot);
    }

    public int AddItem(Item item, int amount, InventorySlot placeAt = null, GroundItem groundItem = null,
      InventorySlot formSlot = null) {
      if (placeAt != null) {
        var overFlow = placeAt.isEmpty
          ? placeAt.UpdateSlot(amount, item, formSlot)
          : placeAt.AddAmount(amount, formSlot);
        return HandleOverflow(overFlow, item, groundItem);
      }

      var slot = FindStackableItemOnInventory(item);
      //don't have empty slot or existing item
      if (emptySlotCount <= 0 && slot == null) {
        DropItemToGround(item, groundItem, amount);
        return amount;
      }

      //add to new slot
      if (!item.info.Stackable || slot == null) {
        var emptySlot = GetEmptySlot();
        var overFlow = emptySlot.UpdateSlot(amount, item, formSlot);
        return HandleOverflow(overFlow, item, groundItem);
      }

      //add to exist slot
      var remainingAmount = slot.AddAmount(amount, formSlot);
      return HandleOverflow(remainingAmount, item, groundItem);
    }

    private int HandleOverflow(int overflowAmount, Item item, GroundItem groundItem = null,
      InventorySlot formSlot = null) {
      if (overflowAmount <= 0) {
        return 0;
      }

      var maxStackSize = item.info.MaxStackSize;

      var countRepeat = Mathf.CeilToInt((float)overflowAmount / maxStackSize);

      for (var i = 0; i < countRepeat; i++) {
        var emptySlot = GetEmptySlot();
        if (emptySlot != null) {
          emptySlot.UpdateSlot(overflowAmount, item, formSlot);
          overflowAmount -= maxStackSize;
        }
        else {
          DropItemToGround(item, groundItem, overflowAmount);
          return overflowAmount;
        }
      }

      return 0;
    }

    public int GetFreeSlotsCount() {
      var count = 0;

      foreach (var slot in GetSlots) {
        //slot is empty
        if (slot.isEmpty) {
          count++;
        }
      }

      return count;
    }

    public Dictionary<string, int> CalculateTotalCounts() {
      var count = new Dictionary<string, int>();

      foreach (var slot in GetSlots) {
        if (slot.isEmpty) {
          continue;
        }

        var slotItemId = slot.Item.info.Id;

        if (!count.ContainsKey(slotItemId)) {
          count.Add(slotItemId, slot.amount);
        }
        else {
          count[slotItemId] += slot.amount;
        }
      }

      return count;
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

    public void AddDefaultItem(ItemObject defaultItem) {
      if (!isEmpty()) {
        return;
      }

      var item = new Item(defaultItem);
      // GetSlots[0].UpdateSlot(1, item);
      AddItem(item, 1);
    }

    private bool isEmpty() {
      for (var i = 0; i < GetSlots.Length; i++) {
        if (GetSlots[i].amount > 0) {
          return false;
        }
      }

      return true;
    }

    private void DropItemToGround(Item item, GroundItem groundItem, int amount) {
      if (!groundItem) {
        return;
      }

      if (item != null) {
        Debug.LogError($"Need to spawn item on floor! Amount: {amount}");
        GameManager.Instance.PlayerInventory.SpawnItem(item, amount);
      }

      // UpdateCount(groundItem, amount);
      groundItem.Count = amount;
      groundItem.isPicked = false;
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

    public InventorySlot FindItemOnInventory(Item item) {
      for (var i = 0; i < GetSlots.Length; i++) {
        if (GetSlots[i].Item?.info.Id == item.info.Id) {
          return GetSlots[i];
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

    public bool IsItemInInventory(ItemObject item) {
      for (var i = 0; i < GetSlots.Length; i++) {
        if (GetSlots[i].Item?.info.Id == item.Id) {
          return true;
        }
      }

      return false;
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