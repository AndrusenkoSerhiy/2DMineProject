using Inventory;
using Scriptables.Items;
using Tools;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuickSlotListener : MonoBehaviour {
  [SerializeField] private InventorySlot selectedSlot;
  [SerializeField] private UserInterface userInterface;
  private InventoryObject quickSlots;
  private InventorySlot[] slots;
  private Item selectedItem;
  private PlayerInventory playerInventory;

  private void Awake() {
    playerInventory = GameManager.Instance.PlayerInventory;
    quickSlots = playerInventory.GetQuickSlots();
    slots = quickSlots.GetSlots;

    //userInterface.OnLoaded += UpdateQuickSlotsAfterLoad;
    selectedSlot = null;
    selectedItem = null;
    //
    MiningRobotTool.OnPlayerEnteredRobot += UnselectCurrSlot;
  }

  private void UnselectCurrSlot() {
    UnselectSlot(selectedSlot);
  }

  private void Start() {
    GameManager.Instance.UserInput.controls.GamePlay.QuickSlots.performed += ChooseSlot;

    quickSlots.OnSlotSwapped += OnSlotUpdateHandler;
    playerInventory.GetInventory().OnSlotSwapped += OnSlotUpdateHandler;
    UpdateQuickSlotsAfterLoad();
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

    //userInterface.OnLoaded -= UpdateQuickSlotsAfterLoad;
  }

  private void ChooseSlot(InputAction.CallbackContext obj) {
    //Debug.LogError($"chose slot {obj.control.name}");
    var index = int.Parse(obj.control.name) - 1;
    if (index == -1) {
      index = slots.Length - 1;
    }

    SelectSlotByIndex(index);
  }

  private void SelectSlotByIndex(int index) {
    SelectSlot(slots[index]);
  }

  private bool UnselectSlot(InventorySlot slot) {
    if (selectedSlot == null || selectedSlot != slot) {
      return false;
    }

    ResetSlot();
    return true;
  }

  private void ResetSlot() {
    GameManager.Instance.PlayerEquipment.OnRemoveItem(selectedItem, selectedSlot.InventoryType);
    selectedSlot.Unselect();
    selectedItem?.info?.Use(selectedSlot);
    selectedSlot = null;
    selectedItem = null;
  }

  private void SelectSlot(InventorySlot slot) {
    //select empty slot
    if (slot == null || slot.Item.info == null) {
      if (selectedSlot != null && selectedSlot.Item.info != null) {
        ResetSlot();
      }

      return;
    }

    if (UnselectSlot(slot)) {
      return;
    }

    if (selectedSlot != null && selectedSlot.Item.info != null) {
      GameManager.Instance.PlayerEquipment.OnRemoveItem(selectedSlot);
      selectedSlot.Unselect();
      selectedItem?.info?.Use(selectedSlot);
    }

    selectedSlot = slot;
    selectedItem = slot.Item;
    selectedSlot.Select();
    GameManager.Instance.PlayerEquipment.OnEquipItem(selectedSlot);
    selectedSlot.Item.info.Use(selectedSlot);
  }

  private void OnDestroy() {
    MiningRobotTool.OnPlayerEnteredRobot -= UnselectCurrSlot;
  }
}