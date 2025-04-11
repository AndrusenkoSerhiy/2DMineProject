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
    [field: NonSerialized] public event Action<InventorySlot> OnAfterItemAdd;
    [field: NonSerialized] public event Action<InventorySlot> OnAfterItemRemoved;

    [field: NonSerialized] public event Action<InventorySlot> OnAfterAmountChanged;
    // public ItemObject AllowedItem;
    // public int MaxAllowedAmount = -1;

    public Item Item;
    public int amount;
    public int index;

    public InventoryType InventoryType { get; private set; }
    public InventoryType InventoryObjectType { get; private set; }
    public string InventoryId { get; private set; }

    public bool isSelected;

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
      InventoryType = parent.Inventory.Type;
    }

    public bool IsItemAllowed(ItemObject item) {
      return SlotDisplay == null || SlotDisplay.IsAllowedItem(item);
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

    public InventorySlot(int index, InventoryType type, string inventoryId) {
      Item = new Item();
      amount = 0;
      this.index = index;
      InventoryObjectType = type;
      InventoryId = inventoryId;
    }

    public InventorySlot Clone(InventorySlot formSlot = null) {
      return new InventorySlot {
        Item = Item,
        amount = amount,
        isSelected = isSelected,
        index = index,
        InventoryObjectType = formSlot?.InventoryObjectType ?? InventoryObjectType,
        InventoryType = formSlot?.InventoryType ?? InventoryType,
      };
    }

    public int GetMaxSize(ItemObject itemObject = null) {
      itemObject ??= Item.info;

      if (!itemObject) {
        return SlotDisplay.MaxAllowedAmount == -1 ? 0 : SlotDisplay.MaxAllowedAmount;
      }

      return !SlotDisplay || SlotDisplay.MaxAllowedAmount == -1
        ? itemObject.MaxStackSize
        : Math.Min(itemObject.MaxStackSize, SlotDisplay.MaxAllowedAmount);
    }

    public int AddItem(int addAmount, Item item, InventorySlot formSlot = null) {
      var amountBefore = amount;
      var overFlow = UpdateSlot(addAmount, item, formSlot);

      if (!preventEvents && amountBefore != amount) {
        OnAfterItemAdd?.Invoke(this);
      }

      return overFlow;
    }

    public void RemoveItem() {
      var slotDataBefore = Clone(this);
      UpdateSlot(0, new Item());

      if (!preventEvents) {
        OnAfterItemRemoved?.Invoke(slotDataBefore);
      }
    }

    public int AddAmount(int value, InventorySlot formSlot = null) {
      var amountBefore = amount;
      var overFlow = UpdateSlot(amount + value, Item, formSlot);

      if (!preventEvents && amountBefore != amount) {
        OnAfterAmountChanged?.Invoke(this);
      }

      return overFlow;
    }

    public int RemoveAmount(int value, InventorySlot formSlot = null) {
      var newAmount = amount - value;
      var item = newAmount <= 0 ? new Item() : Item;
      var amountBefore = amount;
      var overFlow = UpdateSlot(newAmount, item, formSlot);

      if (!preventEvents && amountBefore != amount) {
        OnAfterAmountChanged?.Invoke(this);
      }

      return overFlow;
    }

    public int SetAmount(int value) {
      var amountBefore = amount;
      var overFlow = UpdateSlot(value);

      if (!preventEvents && amountBefore != amount) {
        OnAfterAmountChanged?.Invoke(this);
      }

      return overFlow;
    }

    public int UpdateSlotBySlot(InventorySlot slot) {
      return UpdateSlot(slot.amount, slot.Item, slot);
    }

    private int UpdateSlot(int amountValue, Item itemValue = null, InventorySlot formSlot = null) {
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
  }
}