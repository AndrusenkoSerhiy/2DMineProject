using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Inventory;
using Items;
using Scriptables.Items;
using UnityEngine;

namespace Scriptables.Inventory {
  [CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory System/Inventory")]
  public class InventoryObject : ScriptableObject {
    public string savePath;
    public ItemDatabaseObject database;
    public int slotsCount = 24;

    public InventoryType type;

    //public int MAX_ITEMS;
    [SerializeField]
    // private InventoryContainer Container = new InventoryContainer();
    private InventoryContainer Container;

    public InventorySlot[] GetSlots => Container.Slots;

    public InventoryObject() {
      Container = new InventoryContainer(slotsCount);
    }

    public void MoveAllItemsTo(InventoryObject destinationInventory) {
      if (destinationInventory == null) {
        return;
      }

      foreach (var slot in GetSlots) {
        if (slot.isEmpty) {
          continue; // Skip empty slots
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

    public static void SwapSlots(InventorySlot slot, InventorySlot targetSlot) {
      slot.SwapWith(targetSlot);
    }

    public int RemoveItem(string id, int amount) {
      if (amount <= 0) {
        return 0;
      }

      var remainingAmount = amount;

      for (var i = GetSlots.Length - 1; i >= 0 && remainingAmount > 0; i--) {
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
          ? placeAt.UpdateSlot(amount, item, null, formSlot)
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
        var overFlow = emptySlot.UpdateSlot(amount, item, null, formSlot);
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
          emptySlot.UpdateSlot(overflowAmount, item, null, formSlot);
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
        SpawnItem(item, amount);
      }

      // UpdateCount(groundItem, amount);
      groundItem.Count = amount;
      groundItem.isPicked = false;
    }


    public void SpawnItem(Item item, int amount) {
      if (item == null) {
        return;
      }

      //spawn higher in y pos because need TO DO pick up on action not the trigger enter
      var newObj = Instantiate(item.info.spawnPrefab,
        GameManager.instance.PlayerController.transform.position + new Vector3(0, 3, 0), Quaternion.identity);
      var groundObj = newObj.GetComponent<GroundItem>();
      groundObj.Count = amount;
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

    [ContextMenu("Save")]
    public void Save() {
      IFormatter formatter = new BinaryFormatter();
      Stream stream = new FileStream(string.Concat(Application.persistentDataPath, savePath), FileMode.Create,
        FileAccess.Write);
      formatter.Serialize(stream, Container);
      stream.Close();
    }

    [ContextMenu("Load")]
    public void Load() {
      //Debug.Log("Load " + Application.persistentDataPath);
      if (!File.Exists(string.Concat(Application.persistentDataPath, savePath))) {
        return;
      }

      IFormatter formatter = new BinaryFormatter();
      Stream stream = new FileStream(string.Concat(Application.persistentDataPath, savePath), FileMode.Open,
        FileAccess.Read);
      var newContainer = (InventoryContainer)formatter.Deserialize(stream);

      for (var i = 0; i < GetSlots.Length; i++) {
        var newSlot = newContainer.Slots[i];

        newSlot.Item.RestoreItemObject(database.ItemObjects);

        GetSlots[i].UpdateSlotBySlot(newSlot);
      }

      stream.Close();
    }

    [ContextMenu("Clear")]
    public void Clear() {
      Container.Clear();
    }

    [ContextMenu("Clear and Save", false, 0)]
    public void ClearAndSave() {
      Clear();
      Save();
    }
  }
}