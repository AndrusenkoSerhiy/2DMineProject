/*using System;
using Scriptables.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory {
  [Serializable]
  public class InventorySlot {
    [NonSerialized] public IInventoryUI Parent;
    [NonSerialized] public GameObject SlotDisplay;
    [NonSerialized] public Action<InventorySlot> OnAfterUpdated;
    [NonSerialized] public Action<InventorySlot> OnBeforeUpdated;
    [NonSerialized] public Action<string, int> OnAmountUpdate;
    [NonSerialized] private GameObject outline;
    [NonSerialized] private Image background;
    [NonSerialized] private TextMeshProUGUI text;

    public Item Item;
    public int amount;

    public bool isSelected;
    public Image Background => GetBackground();
    public TextMeshProUGUI Text => GetText();

    public ItemObject GetItemObject() {
      return Item.info;
    }

    public bool isEmpty => Item.info == null || amount <= 0;

    // public InventorySlot() => UpdateSlot(null, 0);
    public InventorySlot() {
      Item = new Item();
      amount = 0;
    }

    //use for swap items in inventory
    public InventorySlot(InventorySlot slot) {
      UpdateSlot(slot.Item, slot.amount, slot.Item.info.MaxStackSize, slot.isSelected);
    }

    public void RemoveItem() {
      UpdateSlot(new Item(), 0);
    }

    public int AddAmount(int value, int maxStack = -1) {
      return UpdateSlot(Item, amount + value, maxStack);
    }

    public int UpdateSlot(Item itemValue, int amountValue, int maxStack, bool selected = false) {
      var amountDelta = ApplySlotUpdate(itemValue, amountValue, maxStack, selected);

      if (!itemValue.isEmpty && amountDelta != 0) {
        OnAmountUpdate?.Invoke(itemValue.info.Id, amountDelta);
      }

      return Mathf.Max(0, amountValue - maxStack);
    }

    //use for load
    public void UpdateSlot(Item itemValue, int amountValue, bool triggerAmountEvent = true) {
      var amountDelta = ApplySlotUpdate(itemValue, amountValue);

      if (triggerAmountEvent && amountDelta != 0 && !itemValue.isEmpty) {
        OnAmountUpdate?.Invoke(itemValue.info.Id, amountDelta);
      }
    }

    //use when swap items
    // public void UpdateSlotAfterSwap(Item itemValue, int amountValue, bool isSelected, bool triggerAmountEvent = false) {
    public void UpdateSlotAfterSwap(InventorySlot slot) {
      var itemValue = slot.Item;
      var amountValue = slot.amount;

      var amountDelta = ApplySlotUpdate(itemValue, amountValue);

      if (slot.Parent != null
          && !itemValue.isEmpty
          && slot.Parent.Inventory.UpdateInventoryAmountOnSwap
          && amountDelta != 0) {
        OnAmountUpdate?.Invoke(itemValue.info.Id, amountDelta);
      }

      if (slot.isSelected) {
        Select();
      }
      else {
        Unselect();
      }
    }

    public int ApplySlotUpdate(Item itemValue, int amountValue, int maxStack = -1, bool selected = false) {
      OnBeforeUpdated?.Invoke(this);
      var oldAmount = amount;
      var newAmount = maxStack == -1 ? amountValue : Mathf.Min(amountValue, maxStack);

      Item = itemValue;
      amount = newAmount;
      isSelected = selected;
      OnAfterUpdated?.Invoke(this);

      return amount - oldAmount;
    }

    public bool SlotsHasSameItems(InventorySlot targetSlot) {
      if (isEmpty || targetSlot.isEmpty || Item.isEmpty || targetSlot.Item.isEmpty) {
        return false;
      }

      return Item.info.Id == targetSlot.Item.info.Id;
    }

    public void Select() {
      GetOutline().SetActive(true);
      isSelected = true;
    }

    public void Unselect() {
      GetOutline().SetActive(false);
      isSelected = false;
    }

    private GameObject GetOutline() {
      if (!outline) {
        outline = SlotDisplay.transform.GetChild(0).gameObject;
      }

      return outline;
    }

    private Image GetBackground() {
      if (background == null) {
        background = SlotDisplay.transform.GetChild(1).GetComponent<Image>();
      }

      return background;
    }

    private TextMeshProUGUI GetText() {
      if (text == null) {
        text = SlotDisplay.GetComponentInChildren<TextMeshProUGUI>();
      }

      return text;
    }

    public void ResetBackgroundAndText() {
      background = null;
      text = null;
    }
  }
}*/