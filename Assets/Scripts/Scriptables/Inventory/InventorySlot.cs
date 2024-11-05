using System;
using Interface;
using Scriptables.Items;
using UnityEngine;

namespace Scriptables.Inventory {
  [Serializable]
  public class InventorySlot {
    public ItemType[] AllowedItems = new ItemType[0];

    [NonSerialized]
    public UserInterface parent;
    [NonSerialized]
    public GameObject slotDisplay;


    [NonSerialized]
    public Action<InventorySlot> onAfterUpdated;
    [NonSerialized]
    public Action<InventorySlot> onBeforeUpdated;

    public Item item;
    public int amount;

    public ItemObject GetItemObject() {
      if (parent == null || parent.inventory == null || item.Id < 0) {
        return null;
      }

      return parent.inventory.database.ItemObjects[item.Id];
    }

    public InventorySlot() => UpdateSlot(new Item(), 0);

    //use for swap items in inventory
    public InventorySlot(Item item, int amount, int maxStack) => UpdateSlot(item, amount, maxStack);

    public void RemoveItem() => UpdateSlot(new Item(), 0);

    public int AddAmount(int value, int maxStack = -1) => UpdateSlot(item, amount += value, maxStack);

    //use to add item, and check maxStack
    public int UpdateSlot(Item itemValue, int amountValue, int maxStack) {
      onBeforeUpdated?.Invoke(this);

      item = itemValue;
      amount = Mathf.Min(amountValue, maxStack);
      
      onAfterUpdated?.Invoke(this);

      return Mathf.Max(0, amountValue - maxStack);
    }

    //use for load
    public void UpdateSlot(Item itemValue, int amountValue) {
      onBeforeUpdated?.Invoke(this);
      item = itemValue;
      amount = amountValue;
      onAfterUpdated?.Invoke(this);
    }

    public bool CanPlaceInSlot(ItemObject itemObject) {
      if (AllowedItems.Length <= 0 || itemObject == null || itemObject.data.Id < 0) {
        return true;
      }

      for (int i = 0; i < AllowedItems.Length; i++) {
        if (itemObject.Type == AllowedItems[i]) {
          return true;
        }
      }

      return false;
    }
  }
}
