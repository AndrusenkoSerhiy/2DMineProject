using System;
using System.Collections.Generic;
using Inventory;
using Menu;
using SaveSystem;
using Scriptables.Items;
using Tools;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuickSlotListener : MonoBehaviour, ISaveLoad {
  [SerializeField] private InventorySlot selectedSlot;
  [SerializeField] private UserInterface userInterface;
  private Inventory.Inventory quickSlots;
  private InventorySlot[] slots;
  [SerializeField] private Item selectedItem;
  private PlayerInventory playerInventory;
  private ItemConsumer itemConsumer;
  [SerializeField] private int selectedSlotIndex = -1;
  [SerializeField] private List<string> reasonsToBlock = new();
  private GameManager gameManager;
  public event Action OnActivate;
  public event Action OnDeactivate;

  #region Save/Load

  public int Priority => LoadPriority.QUICK_SLOTS;
  private bool loaded;

  public void Save() {
  }

  public void Load() {
    quickSlots = playerInventory.GetQuickSlots();
    slots = quickSlots.Slots;
    loaded = true;
    userInterface.OnLoaded += UpdateQuickSlotsAfterLoad;
  }

  public void Clear() {
    //userInterface.OnLoaded -= UpdateQuickSlotsAfterLoad;
    UnequipSlot();

    selectedSlot = null;
    SetSelectedItem(null);
    selectedSlotIndex = -1;
    itemConsumer = null;

    loaded = false;
  }

  #endregion

  private void Awake() {
    SaveLoadSystem.Instance.Register(this);
    gameManager = GameManager.Instance;
    playerInventory = gameManager.PlayerInventory;
    quickSlots = playerInventory.GetQuickSlots();
    slots = quickSlots.Slots;
    
    selectedSlot = null;
    SetSelectedItem(null);
    //
    MiningRobotTool.OnPlayerEnteredRobot += UnequipSlot; //UnselectCurrSlot;
    PlaceCell.OnSlotReset += UnequipSlot;
    MenuController.OnExitToMainMenu += ExitToMainMenu;
  }
  
  private void ExitToMainMenu() {
    UnsubscribeToAddItem();
  }

  private void SubscribeToAddItem() {
    //Debug.LogError("+++subscribe to add item");
    foreach (var slot in slots) {
      slot.OnAfterUpdated += TryActivateItem;
    }
  }
  
  private void UnsubscribeToAddItem() {
    //Debug.LogError("----unsubscribe to add item");
    foreach (var slot in slots) {
      slot.OnAfterUpdated -= TryActivateItem;
    }
  }
  
  private void TryActivateItem(SlotUpdateEventData obj) {
    DeactivateItem(obj.before);
    ActivateItem(obj.after);
  }

  private void Start() {
    SubscribeToClickQuickSlots();
    SubscribeToMouseWheel();
  }

  private void OnEnable() {
    if (reasonsToBlock.Count >= 1) {
      gameObject.SetActive(false);
    }
  }

  public InventorySlot GetSelectedSlot() {
    return selectedSlot;
  }

  private ItemConsumer GetConsumer() {
    return itemConsumer ??= new ItemConsumer();
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

  public void Activate(string reason, bool needActivate = true) {
    if (reasonsToBlock.Contains(reason)) {
      reasonsToBlock.Remove(reason);
    }
    gameObject.SetActive(true);
    
    SubscribeToClickQuickSlots();
    SubscribeToMouseWheel();
    
    if(!needActivate)
      return;
    
    ActivateItemInSelectedSlot(slots[selectedSlotIndex]);
    OnActivate?.Invoke();
  }

  public void Deactivate(string reason = "") {
    if (!string.IsNullOrEmpty(reason) && !reasonsToBlock.Contains(reason)) {
      reasonsToBlock.Add(reason);
    }
    
    UnsubscribeToClickQuickSlots();
    UnsubscribeToMouseWheel();
    gameObject.SetActive(false);
    OnDeactivate?.Invoke();
  }

  private void SubscribeToClickQuickSlots() {
    gameManager.UserInput.controls.GamePlay.QuickSlots.performed += ChooseSlot;
  }

  private void UnsubscribeToClickQuickSlots() {
    gameManager.UserInput.controls.GamePlay.QuickSlots.performed -= ChooseSlot;
  }

  //when you drop item to selected slot
  private void ActivateItem(/*InventorySlot slot, */InventorySlot targetSlot) {
    if (targetSlot != null && selectedSlot != null &&
        !targetSlot.index.Equals(selectedSlot.index))
      return;
    
    ActivateItemInSelectedSlot(targetSlot);
  }

  private void DeactivateItem(InventorySlot slot) {
    if (!slot.isSelected || slot.Item.isEmpty)
      return;

    gameManager.PlayerEquipment.UnEquipTool();
    slot.Unselect();
    GetConsumer().DeactivateItem(selectedItem);
    SetSelectedItem(null);
  }

  private void UpdateQuickSlotsAfterLoad() {
    //Debug.LogError("UpdateQuickSlotsAfterLoad");
    userInterface.OnLoaded -= UpdateQuickSlotsAfterLoad;
    SubscribeToAddItem();
    // Manually instantiate equipped items after loading the equipment
    for (var i = 0; i < slots.Length; i++) {
      var slot = slots[i];
      if (slot.isSelected) {
        //GetConsumer().SetActiveSlot(slot);
        SelectSlotByIndex(i);
      }
      else {
        slot.Unselect();
      }
    }

    if (selectedSlotIndex == -1) {
      SelectFirstSlot();
    }
  }

  private void ChooseSlot(InputAction.CallbackContext obj) {
    var index = int.Parse(obj.control.name) - 1;
    if (index == -1) {
      index = slots.Length - 1;
    }

    SelectSlotByIndex(index);
  }

  private void ChooseSlotByMouseWheel(InputAction.CallbackContext obj) {
    var mouseScrollY = obj.ReadValue<float>();
    var index = (int)(selectedSlotIndex - mouseScrollY);

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
      gameManager.PlayerEquipment.UnEquipTool();
    }

    selectedSlot?.Unselect();
    GetConsumer().DeactivateItem(selectedItem);
    selectedSlot = null;
    SetSelectedItem(null);
  }

  //use for building block. When block is run out we need just to unequip item
  private void UnequipSlot() {
    if (selectedSlot != null && !selectedSlot.Item.isEmpty) {
      gameManager.PlayerEquipment.UnEquipTool();
    }
    
    GetConsumer().DeactivateItem(selectedItem);
    SetSelectedItem(null);
  }

  private void SelectSlot(InventorySlot slot) {
    //select empty slot
    if (slot.isEmpty) {
      if (selectedSlot != null && !selectedSlot.isEmpty) {
        ResetSlot();
      }

      selectedSlot = slot;
      SetSelectedItem(slot.Item);
      selectedSlot.Select();
      GetConsumer().SetActiveSlot(selectedSlot);

      return;
    }

    if (UnselectSlot(slot)) {
      return;
    }

    if (selectedSlot != null && selectedSlot.Item.info != null) {
      gameManager.PlayerEquipment.UnEquipTool();
      selectedSlot.Unselect();
      GetConsumer().DeactivateItem(selectedItem);
    }

    ActivateItemInSelectedSlot(slot);
  }

  private void ActivateItemInSelectedSlot(InventorySlot slot) {
    selectedSlot = slot;
    SetSelectedItem(slot.Item);
    selectedSlot.Select();
    gameManager.PlayerEquipment.EquipTool(selectedSlot.Item);
    GetConsumer().SetActiveSlot(selectedSlot);
  }

  private void SetSelectedItem(Item item) {
    selectedItem = item;
  }

  private void OnDestroy() {
    UnsubscribeToAddItem();
    MiningRobotTool.OnPlayerEnteredRobot -= UnequipSlot;
    PlaceCell.OnSlotReset -= UnequipSlot;
  }
}