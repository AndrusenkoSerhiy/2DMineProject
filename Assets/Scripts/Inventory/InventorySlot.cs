using System;
using Scriptables.Items;
using UnityEngine;

namespace Inventory {
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
    [NonSerialized]
    public Action<int, int> onAmountUpdate;

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

    public int AddAmount(int value, int maxStack = -1) => UpdateSlot(item, amount + value, maxStack);

    public int UpdateSlot(Item itemValue, int amountValue, int maxStack) {
      onBeforeUpdated?.Invoke(this);

      var oldAmount = amount;
      var newAmount = Mathf.Min(amountValue, maxStack); // Calculate the new amount
      var itemId = itemValue.Id != -1 ? itemValue.Id : (item?.Id ?? -1);

      item = itemValue;
      amount = newAmount; // Update the slot's amount after calculation

      onAfterUpdated?.Invoke(this);
      onAmountUpdate?.Invoke(itemId, newAmount - oldAmount);

      return Mathf.Max(0, amountValue - maxStack);
    }

    //use for load
    public void UpdateSlot(Item itemValue, int amountValue, bool triggerAmountEvent = true) {
      onBeforeUpdated?.Invoke(this);

      var itemId = itemValue.Id != -1 ? itemValue.Id : (item?.Id ?? -1);
      var oldAmount = amount;
      item = itemValue;
      amount = amountValue;

      onAfterUpdated?.Invoke(this);

      if (triggerAmountEvent) {
        onAmountUpdate?.Invoke(itemId, amount - oldAmount);
      }
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
