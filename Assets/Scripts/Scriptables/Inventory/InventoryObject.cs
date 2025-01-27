﻿using System.IO;
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
    public InterfaceType type;
    //public int MAX_ITEMS;
    [SerializeField]
    private InventoryContainer Container = new InventoryContainer();
    public InventorySlot[] GetSlots => Container.Slots;

    //use to get item from mining
    public bool AddItem(Item item, int amount, ItemObject itemObj, GroundItem groundItem) {
      Debug.Log("AddItem amount " + amount);
      InventorySlot slot = FindStackableItemOnInventory(item);//FindItemOnInventory(item);
      //don't have empty slot or existing item
      if (EmptySlotCount <= 0 && slot == null) {
        DropItemToGround(itemObj, groundItem, amount);
        return false;
      }

      //add to new slot
      int maxStackSize = database.ItemObjects[item.Id].MaxStackSize;
      if (!database.ItemObjects[item.Id].Stackable || slot == null) {
        var overFlow = GetEmptySlot().UpdateSlot(item, amount, maxStackSize);
        Debug.Log("AddItem overFlow " + overFlow);
        return HandleOverflow(overFlow, maxStackSize, itemObj, groundItem, item);
      }

      //add to exist slot
      var remainingAmount = slot.AddAmount(amount, maxStackSize);
      Debug.Log("AddItem remainingAmount " + remainingAmount);
      return HandleOverflow(remainingAmount, maxStackSize, itemObj, groundItem, item);
    }

    private void DropItemToGround(ItemObject itemObj, GroundItem groundItem, int amount) {
      if (itemObj != null) {
        Debug.LogError($"Need to spawn item on floor! Amount: {amount}");
        SpawnItem(itemObj, amount);
      }
      UpdateCount(groundItem, amount);
    }

    private bool HandleOverflow(int overflowAmount, int maxStackSize, ItemObject itemObj, GroundItem groundItem, Item item) {
      if (overflowAmount > 0) {
        //Debug.LogError($"lefted count {leftedCount}");
        var countRepeat = Mathf.CeilToInt((float)overflowAmount / maxStackSize);

        //Debug.LogError($"count repeat {countRepeat}!!!!!!!!!!!!!!!!!!!");
        for (int i = 0; i < countRepeat; i++) {
          var emptySlot = GetEmptySlot();
          if (emptySlot != null) {
            emptySlot.UpdateSlot(item, overflowAmount, maxStackSize);
            overflowAmount -= maxStackSize;
          }
          else {
            Debug.LogError($"Also need to spawn item on floor {overflowAmount}");
            DropItemToGround(itemObj, groundItem, overflowAmount);
            return false;
          }
        }
        return true;
      }
      return true;
    }
    private void SpawnItem(ItemObject item, int amount) {
      if (item == null)
        return;

      GameObject newObj = Instantiate(item.spawnPrefab, GameManager.instance.PlayerController.transform.position, Quaternion.identity);
      var groundObj = newObj.GetComponent<GroundItem>();
      groundObj.Count = amount;
    }
    //update count for ground item
    private void UpdateCount(GroundItem groundItem, int amount) {
      if (groundItem != null) groundItem.Count = amount;
    }

    public int EmptySlotCount {
      get {
        int counter = 0;
        for (int i = 0; i < GetSlots.Length; i++) {
          if (GetSlots[i].item.Id <= -1) {
            counter++;
          }
        }
        return counter;
      }
    }

    public InventorySlot FindItemOnInventory(Item item) {
      for (int i = 0; i < GetSlots.Length; i++) {
        if (GetSlots[i].item.Id == item.Id) {
          return GetSlots[i];
        }
      }
      return null;
    }

    public InventorySlot FindStackableItemOnInventory(Item item) {
      for (int i = 0; i < GetSlots.Length; i++) {
        if (GetSlots[i].item.Id == item.Id && GetSlots[i].amount < database.ItemObjects[item.Id].MaxStackSize) {
          return GetSlots[i];
        }
      }
      return null;
    }

    public bool IsItemInInventory(ItemObject item) {
      for (int i = 0; i < GetSlots.Length; i++) {
        if (GetSlots[i].item.Id == item.data.Id) {
          return true;
        }
      }
      return false;
    }

    public InventorySlot GetEmptySlot() {
      for (int i = 0; i < GetSlots.Length; i++) {
        if (GetSlots[i].item.Id <= -1) {
          return GetSlots[i];
        }
      }
      return null;
    }

    public void SwapItems(InventorySlot item1, InventorySlot item2) {
      if (item1 == item2) {
        return;
      }

      if (item2.CanPlaceInSlot(item1.GetItemObject()) && item1.CanPlaceInSlot(item2.GetItemObject())) {
        InventorySlot temp = new InventorySlot(item2.item, item2.amount, item2.amount);
        item2.UpdateSlot(item1.item, item1.amount, false);
        item1.UpdateSlot(temp.item, temp.amount, false);
      }
    }

    [ContextMenu("Save")]
    public void Save() {
      #region Optional Save
      //string saveData = JsonUtility.ToJson(Container, true);
      //BinaryFormatter bf = new BinaryFormatter();
      //FileStream file = File.Create(string.Concat(Application.persistentDataPath, savePath));
      //bf.Serialize(file, saveData);
      //file.Close();
      #endregion

      IFormatter formatter = new BinaryFormatter();
      Stream stream = new FileStream(string.Concat(Application.persistentDataPath, savePath), FileMode.Create, FileAccess.Write);
      formatter.Serialize(stream, Container);
      stream.Close();
    }

    [ContextMenu("Load")]
    public void Load() {
      //Debug.Log("Load " + Application.persistentDataPath);
      if (File.Exists(string.Concat(Application.persistentDataPath, savePath))) {
        #region Optional Load
        //BinaryFormatter bf = new BinaryFormatter();
        //FileStream file = File.Open(string.Concat(Application.persistentDataPath, savePath), FileMode.Open, FileAccess.Read);
        //JsonUtility.FromJsonOverwrite(bf.Deserialize(file).ToString(), Container);
        //file.Close();
        #endregion

        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream(string.Concat(Application.persistentDataPath, savePath), FileMode.Open, FileAccess.Read);
        InventoryContainer newContainer = (InventoryContainer)formatter.Deserialize(stream);

        for (int i = 0; i < GetSlots.Length; i++) {
          GetSlots[i].UpdateSlot(newContainer.Slots[i].item, newContainer.Slots[i].amount);
        }
        stream.Close();
      }
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