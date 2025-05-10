using System;
using Inventory;
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
  private Item selectedItem;
  private PlayerInventory playerInventory;
  private ItemConsumer itemConsumer;
  [SerializeField] private int selectedSlotIndex = -1;
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
  }

  public void Clear() {
    userInterface.OnLoaded -= UpdateQuickSlotsAfterLoad;
    UnequipSlot();

    selectedSlot = null;
    selectedItem = null;
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

    //userInterface.OnLoaded += UpdateQuickSlotsAfterLoad;
    selectedSlot = null;
    selectedItem = null;
    //
    MiningRobotTool.OnPlayerEnteredRobot += UnequipSlot; //UnselectCurrSlot;
    PlaceCell.OnSlotReset += UnequipSlot;
  }

  private void Start() {
    SubscribeToClickQuickSlots();
    SubscribeToMouseWheel();
    quickSlots.OnSlotSwapped += OnSlotUpdateHandler;
    playerInventory.GetInventory().OnSlotSwapped += OnSlotUpdateHandler;
    UpdateQuickSlotsAfterLoad();
    // SelectFirstSlot();
  }

  private void OnEnable() {
    if (!loaded) {
      return;
    }

    quickSlots.OnSlotSwapped += OnSlotUpdateHandler;
    playerInventory.GetInventory().OnSlotSwapped += OnSlotUpdateHandler;
    userInterface.OnLoaded += UpdateQuickSlotsAfterLoad;
  }

  public InventorySlot GetSelectedSlot() {
    return selectedSlot;
  }

  private void UnselectCurrSlot() {
    UnselectSlot(selectedSlot);
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

  public void Activate() {
    gameObject.SetActive(true);
    SubscribeToClickQuickSlots();

    UpdateQuickSlotsAfterLoad();

    selectedSlot = slots[selectedSlotIndex];
    selectedItem = slots[selectedSlotIndex].Item;
    selectedSlot.Select();
    // gameManager.PlayerEquipment.OnEquipItem(selectedSlot);
    gameManager.PlayerEquipment.EquipTool(selectedSlot.Item);
    // selectedSlot.Item?.info?.Use();
    GetConsumer().SetActiveSlot(selectedSlot);
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
    //Always quickslot
    var slot = data.slot.InventoryType == InventoryType.QuickSlots ? data.slot : data.target;
    //Quickslot or inventory slot
    var target = slot == data.slot ? data.target : data.slot;

    if (slot.InventoryType == target.InventoryType && slot.InventoryType != InventoryType.QuickSlots) {
      return;
    }

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
      if (!slot.isEmpty) {
        ActivateItem(target, slot);
      }
    }
  }

  //when you drop item to selected slot
  private void ActivateItem(InventorySlot slot, InventorySlot targetSlot) {
    if (!slot.isSelected)
      return;

    selectedSlot = targetSlot;
    selectedItem = targetSlot.Item;
    selectedSlot.Select();
    // gameManager.PlayerEquipment.OnEquipItem(selectedSlot);
    gameManager.PlayerEquipment.EquipTool(selectedSlot.Item);
    // selectedSlot.Item.info.Use();
    GetConsumer().SetActiveSlot(selectedSlot);
  }

  private void DeactivateItem(InventorySlot slot) {
    if (!slot.isSelected || slot.Item.isEmpty)
      return;

    // gameManager.PlayerEquipment.OnRemoveItem(selectedItem, selectedSlot.InventoryType);
    gameManager.PlayerEquipment.UnEquipTool();
    slot.Unselect();
    // selectedItem?.info?.Use();
    GetConsumer().DeactivateItem(selectedItem);
    selectedItem = null;
  }

  private void UpdateQuickSlotsAfterLoad() {
    // Manually instantiate equipped items after loading the equipment
    for (var i = 0; i < slots.Length; i++) {
      var slot = slots[i];
      if (slot.isSelected) {
        GetConsumer().SetActiveSlot(slot);
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
    //Debug.LogError($"chose slot {obj.control.name}");
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
      //(selectedItem != null) {
      // gameManager.PlayerEquipment.OnRemoveItem(selectedItem, selectedSlot.InventoryType);
      gameManager.PlayerEquipment.UnEquipTool();
    }

    selectedSlot?.Unselect();
    // selectedItem?.info?.Use();
    GetConsumer().DeactivateItem(selectedItem);
    selectedSlot = null;
    selectedItem = null;
  }

  //use for building block. When block is run out we need just to unequip item
  private void UnequipSlot() {
    if (selectedSlot != null && !selectedSlot.Item.isEmpty) {
      //(selectedItem != null) {
      // gameManager.PlayerEquipment.OnRemoveItem(selectedItem, selectedSlot.InventoryType);
      gameManager.PlayerEquipment.UnEquipTool();
    }

    // selectedItem?.info?.Use();
    GetConsumer().DeactivateItem(selectedItem);
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
      GetConsumer().SetActiveSlot(selectedSlot);

      return;
    }

    if (UnselectSlot(slot)) {
      return;
    }

    if (selectedSlot != null && selectedSlot.Item.info != null) {
      // gameManager.PlayerEquipment.OnRemoveItem(selectedSlot);
      gameManager.PlayerEquipment.UnEquipTool();
      selectedSlot.Unselect();
      // selectedItem?.info?.Use();
      GetConsumer().DeactivateItem(selectedItem);
    }

    selectedSlot = slot;
    selectedItem = slot.Item;
    selectedSlot.Select();
    // gameManager.PlayerEquipment.OnEquipItem(selectedSlot);
    gameManager.PlayerEquipment.EquipTool(selectedSlot.Item);
    // selectedSlot.Item.info.Use();
    GetConsumer().SetActiveSlot(selectedSlot);
  }

  private void OnDestroy() {
    UnsubscribeToMouseWheel();
    MiningRobotTool.OnPlayerEnteredRobot -= UnequipSlot; //UnselectCurrSlot;
    PlaceCell.OnSlotReset -= UnequipSlot;
  }
}