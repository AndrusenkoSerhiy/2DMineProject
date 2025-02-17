using System;
using JetBrains.Annotations;
using Scriptables.Inventory;
using Scriptables.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory {
  [Serializable]
  public class InventorySlot {
    [NonSerialized] public IInventoryUI Parent;
    [NonSerialized] public GameObject SlotDisplay;
    [field: NonSerialized] public event Action<InventorySlot, InventorySlot, InventorySlot> OnAfterUpdated;
    [field: NonSerialized] public event Action<InventorySlot> OnBeforeUpdated;

    // [NonSerialized] public Action<string, int> OnAmountUpdate;
    [NonSerialized] private GameObject outline;
    [NonSerialized] private Image background;
    [NonSerialized] private TextMeshProUGUI text;

    public Item Item;
    public int amount;

    public InventoryType InventoryType { get; private set; }

    //TODO: Add ContainerIndex to Item(for storages)
    public int ContainerIndex { get; private set; } = 0;

    public bool isSelected;
    public Image Background => GetBackground();
    public TextMeshProUGUI Text => GetText();

    public ItemObject GetItemObject() {
      return Item.info;
    }

    public bool isEmpty => Item.info == null || amount <= 0;

    public void SetParent(IInventoryUI parent) {
      Parent = parent;
      InventoryType = parent.Inventory.type;
    }

    public InventorySlot() {
      Item = new Item();
      amount = 0;
    }

    public InventorySlot Clone(InventorySlot formSlot = null) {
      return new InventorySlot {
        Item = Item,
        amount = amount,
        isSelected = isSelected,
        InventoryType = formSlot?.InventoryType ?? InventoryType,
      };
    }

    public void RemoveItem() {
      UpdateSlot(0, new Item());
    }

    public int AddAmount(int value, InventorySlot formSlot = null) {
      return UpdateSlot(amount + value, Item, null, formSlot);
    }

    public int UpdateSlotBySlot(InventorySlot slot) {
      return UpdateSlot(slot.amount, slot.Item, slot.isSelected, slot);
    }

    public int UpdateSlot(int amountValue, Item itemValue = null, bool? selected = null,
      InventorySlot formSlot = null) {
      var slotDataBefore = Clone(this);
      OnBeforeUpdated?.Invoke(slotDataBefore);
      itemValue ??= Item;
      var maxStack = itemValue.info ? itemValue.info.MaxStackSize : 0;
      var newAmount = Mathf.Min(amountValue, maxStack);
      var overFlow = Mathf.Max(0, newAmount - maxStack);

      Item = itemValue;
      amount = newAmount;

      if (selected != null) {
        isSelected = (bool)selected;
      }

      OnAfterUpdated?.Invoke(slotDataBefore, this, formSlot);

      return overFlow;
    }

    //use when swap items
    public void SwapWith(InventorySlot targetSlot) {
      if (targetSlot == null || targetSlot == this) {
        return;
      }

      // Save current slot data
      // var tempItem = Item;
      // var tempAmount = amount;
      // var tempSelected = isSelected;
      var tempSlot = Clone();

      // Swap data
      UpdateSlotBySlot(targetSlot);
      targetSlot.UpdateSlotBySlot(tempSlot);
    }

    public bool SlotsHasSameItems(InventorySlot targetSlot) {
      if (isEmpty || targetSlot.isEmpty || Item.isEmpty || targetSlot.Item.isEmpty) {
        return false;
      }

      return Item.info.Id == targetSlot.Item.info.Id;
    }

    public bool CanMerge(InventorySlot targetSlot) {
      if (!SlotsHasSameItems(targetSlot)) {
        return false;
      }

      return targetSlot.Item.info.MaxStackSize >= amount + targetSlot.amount;
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