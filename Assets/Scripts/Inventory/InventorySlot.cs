using System;
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
    [field: NonSerialized] public event Action<SlotUpdateEventData> OnAfterUpdated;
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

    private bool preventEvents = false;

    public ItemObject GetItemObject() {
      return Item.info;
    }

    public bool isEmpty => Item.info == null || amount <= 0;

    public InventorySlot PreventEvents() {
      preventEvents = true;
      return this;
    }

    private void ResetPreventEvents() {
      preventEvents = false;
    }

    public void SetParent(IInventoryUI parent) {
      Parent = parent;
      InventoryType = parent.Inventory.type;
    }

    public void SetSlotDisplay(GameObject display) {
      if (display == null) {
        return;
      }

      SlotDisplay = display;
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

    public int AddItem(Item item, int amount) {
      return UpdateSlot(amount, item);
    }

    public void RemoveItem() {
      UpdateSlot(0, new Item());
    }

    public int AddAmount(int value, InventorySlot formSlot = null) {
      return UpdateSlot(amount + value, Item, formSlot);
    }

    public int RemoveAmount(int value, InventorySlot formSlot = null) {
      var newAmount = amount - value;
      var item = newAmount <= 0 ? new Item() : Item;

      return UpdateSlot(newAmount, item, formSlot);
    }

    public int UpdateSlotBySlot(InventorySlot slot) {
      return UpdateSlot(slot.amount, slot.Item, slot);
    }

    public int UpdateSlot(int amountValue, Item itemValue = null, InventorySlot formSlot = null) {
      var slotDataBefore = Clone(this);
      if (!preventEvents) {
        OnBeforeUpdated?.Invoke(slotDataBefore);
      }

      itemValue ??= Item;
      var maxStack = itemValue.info ? itemValue.info.MaxStackSize : 0;
      var newAmount = Mathf.Min(amountValue, maxStack);
      var overFlow = Mathf.Max(0, newAmount - maxStack);

      Item = itemValue;
      amount = newAmount;

      if (!preventEvents) {
        OnAfterUpdated?.Invoke(new SlotUpdateEventData(slotDataBefore, this, formSlot));
      }

      ResetPreventEvents();

      return overFlow;
    }

    //use when swap items
    public void SwapWith(InventorySlot targetSlot) {
      if (targetSlot == null || targetSlot == this) {
        return;
      }

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

    public void CheckSelectedUi() {
      if (isSelected) {
        ActivateOutline();
      }
      else {
        DeactivateOutline();
      }
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