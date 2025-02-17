using Inventory;
using Scriptables.Inventory;
using Scriptables.Items;
using Settings;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuickSlotListener : MonoBehaviour {
  [SerializeField] private InventorySlot selectedSlot;
  private InventoryObject quickSlots;
  private InventorySlot[] slots;
  private Item selectedItem;
  private PlayerInventory playerInventory;

  private void Awake() {
    playerInventory = GameManager.instance.PlayerInventory;
    quickSlots = playerInventory.quickSlots;
    slots = quickSlots.GetSlots;

    playerInventory.OnQuickSlotLoaded += UpdateQuickSlotsAfterLoad;
    selectedSlot = null;
    selectedItem = null;
  }

  private void Start() {
    UserInput.instance.controls.GamePlay.QuickSlots.performed += ChooseSlot;

    quickSlots.OnSlotSwapped += OnSlotUpdateHandler;
    playerInventory.inventory.OnSlotSwapped += OnSlotUpdateHandler;
  }

  private void OnSlotUpdateHandler(SlotSwappedEventData data) {
    var slot = data.slot;
    var target = data.target;

    // If neither slot is selected, ignore
    if (!slot.isSelected && !target.isSelected) {
      return;
    }

    var activeSlot = slot.isSelected ? slot : target;
    var otherSlot = !slot.isSelected ? slot : target;

    if (slot.InventoryType == target.InventoryType && activeSlot.InventoryType == InventoryType.QuickSlots) {
      //change selected
      activeSlot.Unselect();
      selectedSlot = otherSlot;
      selectedItem = otherSlot.Item;
      otherSlot.Select();
    }
    else {
      UnselectSlot(activeSlot);
    }
  }


  private void UpdateQuickSlotsAfterLoad() {
    // Manually instantiate equipped items after loading the equipment
    for (var i = 0; i < slots.Length; i++) {
      var slot = slots[i];
      if (slot.isEmpty || !slot.isSelected) {
        continue;
      }

      slot.Item.info.Use(slots[i]);
      SelectSlotByIndex(i);
    }

    playerInventory.OnQuickSlotLoaded -= UpdateQuickSlotsAfterLoad;
  }

  private void ChooseSlot(InputAction.CallbackContext obj) {
    //Debug.LogError($"chose slot {obj.control.name}");
    var index = int.Parse(obj.control.name) - 1;
    if (index == -1) {
      index = slots.Length - 1;
    }

    var slot = slots[index];
    if (slot.Item != null && slot.Item.info != null) {
      //Debug.LogError($"select slot {index} item {slot.amount} {slot.item.Name}");
      SelectSlotByIndex(index);
    }
  }

  private void SelectSlotByIndex(int index) => SelectSlot(slots[index]);

  private bool UnselectSlot(InventorySlot slot) {
    if (selectedSlot != slot) {
      return false;
    }

    GameManager.instance.PlayerEquipment.OnRemoveItem(selectedItem, selectedSlot.InventoryType);
    selectedSlot.Unselect();
    selectedItem?.info?.Use(selectedSlot);
    selectedSlot = null;
    selectedItem = null;

    return true;
  }


  private void SelectSlot(InventorySlot slot) {
    if (UnselectSlot(slot)) {
      return;
    }

    if (selectedSlot != null && selectedSlot.Item.info != null) {
      //Debug.LogError($"log1");
      GameManager.instance.PlayerEquipment.OnRemoveItem(selectedSlot);
      selectedSlot.Unselect();
    }

    selectedSlot = slot;
    selectedItem = slot.Item;
    selectedSlot.Select();
    GameManager.instance.PlayerEquipment.OnEquipItem(selectedSlot);
    selectedSlot.Item.info.Use(selectedSlot);
  }
}