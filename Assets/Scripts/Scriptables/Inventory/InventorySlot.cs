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
      return item.Id >= 0 ? parent.inventory.database.ItemObjects[item.Id] : null;
    }

    public InventorySlot() => UpdateSlot(new Item(), 0);

    public InventorySlot(Item item, int amount) => UpdateSlot(item, amount);

    public void RemoveItem() => UpdateSlot(new Item(), 0);

    public void AddAmount(int value) => UpdateSlot(item, amount += value);


    public void UpdateSlot(Item itemValue, int amountValue) {
      //Debug.Log("UpdateSlot itemValue: " + itemValue);
      //Debug.Log("UpdateSlot amountValue: " + amountValue);
      onBeforeUpdated?.Invoke(this);
      item = itemValue;
      amount = amountValue;
      onAfterUpdated?.Invoke(this);
    }

    public bool CanPlaceInSlot(ItemObject itemObject) {
      if (AllowedItems.Length <= 0 || itemObject == null || itemObject.data.Id < 0)
        return true;
      for (int i = 0; i < AllowedItems.Length; i++) {
        if (itemObject.Type == AllowedItems[i])
          return true;
      }
      return false;
    }

  }
}
