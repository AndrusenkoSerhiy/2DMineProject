using System;
using Scriptables.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory {
  [Serializable]
  public class InventorySlot {
    [NonSerialized] public IInventoryUI Parent;
    [NonSerialized] public GameObject SlotDisplay;
    [NonSerialized] public Action<InventorySlot, InventorySlot> OnAfterUpdated;

    [NonSerialized] public Action<InventorySlot> OnBeforeUpdated;

    // [NonSerialized] public Action<string, int> OnAmountUpdate;
    [NonSerialized] private GameObject outline;
    [NonSerialized] private Image background;
    [NonSerialized] private TextMeshProUGUI text;
    [NonSerialized] private const bool PREVENT_AMOUNT_EVENT_DEFAULT = false;
    // [NonSerialized] private bool preventAmountEvent;

    public Item Item;
    public int amount;

    public bool isSelected;
    public Image Background => GetBackground();
    public TextMeshProUGUI Text => GetText();

    public ItemObject GetItemObject() {
      return Item.info;
    }

    public bool isEmpty => Item.info == null || amount <= 0;

    public InventorySlot() {
      Item = new Item();
      amount = 0;
    }

    public InventorySlot Clone() {
      return new InventorySlot {
        Item = Item,
        amount = amount,
        isSelected = isSelected
      };
    }

    public void RemoveItem() {
      UpdateSlot(0, new Item());
    }

    public int AddAmount(int value) {
      return UpdateSlot(amount + value);
    }

    public int UpdateSlot(int amountValue, InventorySlot slot, bool? selected = null) {
      return UpdateSlot(amountValue, slot.Item, selected);
    }

    public int UpdateSlot(int amountValue, Item itemValue = null, bool? selected = null) {
      var slotDataBefore = Clone();
      OnBeforeUpdated?.Invoke(slotDataBefore);
      itemValue ??= Item;
      var maxStack = itemValue.info ? itemValue.info.MaxStackSize : 0;
      var newAmount = Mathf.Min(amountValue, maxStack);
      var overFlow = Mathf.Max(0, newAmount - maxStack);

      Item = itemValue;
      amount = newAmount;

      if (Parent?.Inventory) {
        Item.SetInventoryType(Parent.Inventory.type);
      }

      if (selected != null) {
        isSelected = (bool)selected;
        CheckSelectDisplay();
      }

      OnAfterUpdated?.Invoke(slotDataBefore, this);

      return overFlow;
    }

    //use when swap items
    public void SwapWith(InventorySlot targetSlot) {
      if (targetSlot == null || targetSlot == this) {
        return;
      }

      // Save current slot data
      var tempItem = Item;
      var tempAmount = amount;
      var tempSelected = isSelected;

      // Swap data
      UpdateSlot(targetSlot.amount, targetSlot.Item, targetSlot.isSelected);
      targetSlot.UpdateSlot(tempAmount, tempItem, tempSelected);
    }

    private void CheckSelectDisplay() {
      if (isSelected) {
        ActivateOutline();
      }
      else {
        DeactivateOutline();
      }
    }

    public bool SlotsHasSameItems(InventorySlot targetSlot) {
      if (isEmpty || targetSlot.isEmpty || Item.isEmpty || targetSlot.Item.isEmpty) {
        return false;
      }

      return Item.info.Id == targetSlot.Item.info.Id;
    }

    public void Select() {
      ActivateOutline();
      isSelected = true;
    }

    public void Unselect() {
      DeactivateOutline();
      isSelected = false;
    }

    private void ActivateOutline() {
      GetOutline().SetActive(true);
    }

    private void DeactivateOutline() {
      GetOutline().SetActive(false);
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
}