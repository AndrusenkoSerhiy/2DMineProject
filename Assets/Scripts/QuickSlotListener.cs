using System;
using Inventory;
using Scriptables.Items;
using Tools;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuickSlotListener : MonoBehaviour {
  [SerializeField] private InventorySlot selectedSlot;
  [SerializeField] private UserInterface userInterface;
  private Inventory.Inventory quickSlots;
  private InventorySlot[] slots;
  private Item selectedItem;
  private PlayerInventory playerInventory;
  [SerializeField] private int selectedSlotIndex = -1;
  private GameManager gameManager;
  public event Action OnActivate;
  public event Action OnDeactivate;

  private void Awake() {
    gameManager = GameManager.Instance;
    playerInventory = gameManager.PlayerInventory;
    quickSlots = playerInventory.GetQuickSlots();
    slots = quickSlots.Slots;

    //userInterface.OnLoaded += UpdateQuickSlotsAfterLoad;
    selectedSlot = null;
    selectedItem = null;
    //
    MiningRobotTool.OnPlayerEnteredRobot += UnequipSlot; //UnselectCurrSlot;
    PlaceCell.OnSlotReset += UnequipSlot;
  }

  public InventorySlot GetSelectedSlot() {
    return selectedSlot;
  }

  private void UnselectCurrSlot() {
    UnselectSlot(selectedSlot);
  }

  private void Start() {
    SubscribeToClickQuickSlots();
    SubscribeToMouseWheel();
    quickSlots.OnSlotSwapped += OnSlotUpdateHandler;
    playerInventory.GetInventory().OnSlotSwapped += OnSlotUpdateHandler;
    UpdateQuickSlotsAfterLoad();
    SelectFirstSlot();
  }

  private void SubscribeToMouseWheel() {
    gameManager.UserInput.controls.GamePlay.MouseScrollY.performed += ChooseSlotByMouseWheel;
  }

  private void UnsubscribeToMouseWheel() {
    if (gameManager != null)
      gameManager.UserInput.controls.GamePlay.MouseScrollY.performed -= ChooseSlotByMouseWheel;
  }

  private void SelectFirstSlot() {
    SelectSlotByIndex(0);
  }

  public void Activate() {
    gameObject.SetActive(true);
    SubscribeToClickQuickSlots();
    selectedSlot = slots[selectedSlotIndex];
    selectedItem = slots[selectedSlotIndex].Item;
    selectedSlot.Select();
    gameManager.PlayerEquipment.OnEquipItem(selectedSlot);
    selectedSlot.Item?.info?.Use(selectedSlot);
    OnActivate?.Invoke();
  }

  public void Deactivate() {
    UnsubscribeToClickQuickSlots();
    gameObject.SetActive(false);
    OnDeactivate?.Invoke();
  }

  private void SubscribeToClickQuickSlots() {
    gameManager.UserInput.controls.GamePlay.QuickSlots.performed += ChooseSlot;
  }

  private void UnsubscribeToClickQuickSlots() {
    gameManager.UserInput.controls.GamePlay.QuickSlots.performed -= ChooseSlot;
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
      otherSlot.Unselect();
      selectedSlotIndex = activeSlot.index;
      selectedSlot = activeSlot;
      selectedItem = activeSlot.Item;
      activeSlot.Select();
    }
    else {
      UnselectSlot(activeSlot);
    }

    //skip activate/deactivate when you swap items in the same inventory
    if (target.InventoryType == slot.InventoryType) {
      return;
    }

    //Debug.LogError($"active slot {activeSlot.InventoryType} | other slot {otherSlot.InventoryType}");
    if (target.InventoryType == InventoryType.QuickSlots) {
      DeactivateItem(slot);
      ActivateItem(slot, target);
    }

    if (target.InventoryType == InventoryType.Inventory) {
      DeactivateItem(target);
      //only when we swap not empty slots
      if (!slot.Item.isEmpty) ActivateItem(target, slot);
    }
  }

  //when you drop item to selected slot
  private void ActivateItem(InventorySlot slot, InventorySlot targetSlot) {
    if (!slot.isSelected)
      return;

    selectedSlot = targetSlot;
    selectedItem = targetSlot.Item;
    selectedSlot.Select();
    gameManager.PlayerEquipment.OnEquipItem(selectedSlot);
    selectedSlot.Item.info.Use(selectedSlot);
  }

  private void DeactivateItem(InventorySlot slot) {
    if (!slot.isSelected || slot.Item.isEmpty)
      return;

    gameManager.PlayerEquipment.OnRemoveItem(selectedItem, selectedSlot.InventoryType);
    selectedItem?.info?.Use(selectedSlot);
    selectedItem = null;
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

  private void ChooseSlotByMouseWheel(InputAction.CallbackContext obj) {
    var mouseScrollY = obj.ReadValue<float>();
    var index = (int)(selectedSlotIndex + mouseScrollY);

    if (index == selectedSlotIndex)
      return;

    if (index > slots.Length - 1) index = 0;
    if (index < 0) index = slots.Length - 1;

    SelectSlotByIndex(index);
  }

  private void SelectSlotByIndex(int index) {
    if (selectedSlotIndex == index)
      return;
    ResetSlot();
    selectedSlotIndex = index;
    SelectSlot(slots[index]);
  }

  private bool UnselectSlot(InventorySlot slot) {
    if (selectedSlot == null || selectedSlot != slot || selectedSlot.isEmpty) {
      return false;
    }

    ResetSlot();
    return true;
  }

  private void ResetSlot() {
    if (selectedSlot != null && !selectedSlot.Item.isEmpty) {
      //(selectedItem != null) {
      gameManager.PlayerEquipment.OnRemoveItem(selectedItem, selectedSlot.InventoryType);
    }

    selectedSlot?.Unselect();
    selectedItem?.info?.Use(selectedSlot);
    selectedSlot = null;
    selectedItem = null;
  }

  //use for building block. When block is run out we need just to unequip item
  private void UnequipSlot() {
    if (selectedSlot != null && !selectedSlot.Item.isEmpty) {
      //(selectedItem != null) {
      gameManager.PlayerEquipment.OnRemoveItem(selectedItem, selectedSlot.InventoryType);
    }

    selectedItem?.info?.Use(selectedSlot);
    selectedItem = null;
  }

  private void SelectSlot(InventorySlot slot) {
    //select empty slot
    if (slot.isEmpty) {
      if (selectedSlot != null && !selectedSlot.isEmpty) {
        ResetSlot();
      }

      selectedSlot = slot;
      selectedItem = slot.Item;
      selectedSlot.Select();

      return;
    }

    if (UnselectSlot(slot)) {
      return;
    }

    if (selectedSlot != null && selectedSlot.Item.info != null) {
      gameManager.PlayerEquipment.OnRemoveItem(selectedSlot);
      selectedSlot.Unselect();
      selectedItem?.info?.Use(selectedSlot);
    }

    selectedSlot = slot;
    selectedItem = slot.Item;
    selectedSlot.Select();
    gameManager.PlayerEquipment.OnEquipItem(selectedSlot);
    selectedSlot.Item.info.Use(selectedSlot);
  }

  private void OnDestroy() {
    UnsubscribeToMouseWheel();
    MiningRobotTool.OnPlayerEnteredRobot -= UnequipSlot; //UnselectCurrSlot;
    PlaceCell.OnSlotReset -= UnequipSlot;
  }
}