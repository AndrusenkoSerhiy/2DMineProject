using System;
using Scriptables.Items;
using UnityEngine;

namespace Inventory {
  [Serializable]
  public class InventorySlot {
    [NonSerialized] public IInventoryUI Parent;
    [NonSerialized] public SlotDisplay SlotDisplay;
    [field: NonSerialized] public event Action<SlotUpdateEventData> OnAfterUpdated;
    [field: NonSerialized] public event Action<InventorySlot> OnBeforeUpdated;
    public ItemObject AllowedItem;
    public int MaxAllowedAmount = -1;

    // [NonSerialized] public Action<string, int> OnAmountUpdate;
    // [NonSerialized] private GameObject outline;
    // [NonSerialized] private Image background;
    // [NonSerialized] private TextMeshProUGUI text;

    public Item Item;
    public int amount;
    public int index;

    public InventoryType InventoryType { get; private set; }

    public bool isSelected;
    /*public Image Background => GetBackground();
    public TextMeshProUGUI Text => GetText();*/

    private bool preventEvents = false;

    public ItemObject GetItemObject() {
      return Item.info;
    }

    public bool isEmpty => Item.info == null || amount <= 0;
    public bool CanMoveToAnotherInventory => Item?.info?.CanMoveToAnotherInventory ?? true;
    public bool CanDrop => Item?.info?.CanDrop ?? true;

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

    public bool IsItemAllowed(ItemObject item) {
      return AllowedItem == null || AllowedItem == item || item == null;
    }

    public void SetSlotDisplay(SlotDisplay display) {
      if (display == null) {
        return;
      }

      SlotDisplay = display;
    }

    public InventorySlot() {
      Item = new Item();
      amount = 0;
    }

    public InventorySlot(int index) {
      Item = new Item();
      amount = 0;
      this.index = index;
    }

    public InventorySlot Clone(InventorySlot formSlot = null) {
      return new InventorySlot {
        Item = Item,
        amount = amount,
        isSelected = isSelected,
        index = index,
        InventoryType = formSlot?.InventoryType ?? InventoryType,
      };
    }

    public int GetMaxSize(ItemObject itemObject = null) {
      itemObject ??= Item.info;

      if (!itemObject) {
        return MaxAllowedAmount == -1 ? 0 : MaxAllowedAmount;
      }

      return MaxAllowedAmount == -1
        ? itemObject.MaxStackSize
        : Math.Min(itemObject.MaxStackSize, MaxAllowedAmount);
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
      // var maxStack = itemValue.info ? itemValue.info.MaxStackSize : 0;
      var maxStack = GetMaxSize(itemValue.info);
      var newAmount = Mathf.Min(amountValue, maxStack);
      var overFlow = Mathf.Max(0, amountValue - maxStack);
      isSelected = formSlot?.isSelected ?? isSelected;

      Item = itemValue;
      amount = newAmount;

      if (!preventEvents) {
        OnAfterUpdated?.Invoke(new SlotUpdateEventData(slotDataBefore, this, formSlot));
      }

      ResetPreventEvents();

      return overFlow;
    }

    //use when swap items
    public bool SwapWith(InventorySlot targetSlot) {
      if (targetSlot == null || targetSlot == this) {
        return false;
      }

      var tempSlot = Clone();

      // Swap data
      var slotLeftAmount = UpdateSlotBySlot(targetSlot);
      var targetLeftAmount = targetSlot.UpdateSlotBySlot(tempSlot);

      if (slotLeftAmount > 0 && targetSlot.isEmpty) {
        targetSlot.UpdateSlot(slotLeftAmount, Item);
      }

      if (targetLeftAmount > 0 && isEmpty) {
        UpdateSlot(targetLeftAmount, targetSlot.Item);
      }

      return true;
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

      if (targetSlot.isEmpty || isEmpty) {
        return true;
      }

      var targetMax = targetSlot.GetMaxSize();
      var slotMax = GetMaxSize();

      return targetMax == slotMax
        ? targetSlot.amount < targetSlot.GetMaxSize() && amount < GetMaxSize()
        : targetSlot.amount < targetSlot.GetMaxSize();
    }

    public void Select() {
      SlotDisplay.ActivateOutline();
      isSelected = true;
    }

    public void Unselect() {
      SlotDisplay?.DeactivateOutline();
      isSelected = false;
    }

    public void CheckSelectedUi() {
      if (isSelected) {
        SlotDisplay.ActivateOutline();
      }
      else {
        SlotDisplay.DeactivateOutline();
      }
    }

    /*private void ActivateOutline() {
      GetOutline().SetActive(true);
    }

    private void DeactivateOutline() {
      GetOutline().SetActive(false);
    }*/

    /*private GameObject GetOutline() {
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
    }*/
  }
}