using System;
using Scriptables.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory {
  [Serializable]
  public class InventorySlot {
    public ItemType[] AllowedItems = new ItemType[0];

    [NonSerialized]
    public IInventoryUI parent;
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
    [NonSerialized]
    private GameObject outline;
    [NonSerialized]
    private Image background;
    [NonSerialized]
    private TextMeshProUGUI text;

    public bool IsSelected;
    public Image Background => GetBackground();
    public TextMeshProUGUI Text => GetText();

    public ItemObject GetItemObject() {
      if (parent == null || parent.Inventory == null || item.Id < 0) {
        return null;
      }
      return parent.Inventory.database.ItemObjects[item.Id];
    }

    public InventorySlot() => UpdateSlot(new Item(), 0);

    //use for swap items in inventory
    public InventorySlot(Item item, int amount, int maxStack, bool selected = false) => UpdateSlot(item, amount, maxStack, selected);

    public void RemoveItem() => UpdateSlot(new Item(), 0);

    public int AddAmount(int value, int maxStack = -1) => UpdateSlot(item, amount + value, maxStack);
    
    public int UpdateSlot(Item itemValue, int amountValue, int maxStack, bool selected = false) {
      onBeforeUpdated?.Invoke(this);

      var oldAmount = amount;
      var newAmount = Mathf.Min(amountValue, maxStack); // Calculate the new amount
      var itemId = itemValue.Id != -1 ? itemValue.Id : (item?.Id ?? -1);

      item = itemValue;
      amount = newAmount; // Update the slot's amount after calculation
      IsSelected = selected;
      onAfterUpdated?.Invoke(this);

      var amountDelta = newAmount - oldAmount;

      if (amountDelta != 0) {
        onAmountUpdate?.Invoke(itemId, amountDelta);
      }

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

      var amountDelta = amount - oldAmount;

      if (triggerAmountEvent && amountDelta != 0) {
        onAmountUpdate?.Invoke(itemId, amountDelta);
      }
    }

    //use when swap items
    public void UpdateSlotAfterSwap(Item itemValue, int amountValue, bool isSelected) {
      onBeforeUpdated?.Invoke(this);
      var itemId = itemValue.Id != -1 ? itemValue.Id : (item?.Id ?? -1);
      var oldAmount = amount;
      item = itemValue;
      amount = amountValue;

      onAfterUpdated?.Invoke(this);

      if (isSelected) Select();
      else Unselect();
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

    public void Select() {
      GetOutline().SetActive(true);
      IsSelected = true;
    }

    public void Unselect() {
      GetOutline().SetActive(false);
      IsSelected = false;
    }

    private GameObject GetOutline() {
      if (outline == null) outline = slotDisplay.transform.GetChild(0).gameObject;
      return outline;
    }

    private Image GetBackground() {
      if (background == null) background = slotDisplay.transform.GetChild(1).GetComponent<Image>();
      return background;
    }

    private TextMeshProUGUI GetText() {
      if (text == null) text = slotDisplay.GetComponentInChildren<TextMeshProUGUI>();
      return text;
    }

    public void ResetBackgroundAndText() {
      background = null;
      text = null;
    }
  }
}
