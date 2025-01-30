using Inventory;
using Scriptables.Inventory;
using Settings;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuickSlotListener : MonoBehaviour {
  [SerializeField] private StaticInterface _staticInterface;
  [SerializeField] private InventorySlot[] slots;
  [SerializeField] private InventorySlot selectedSlot;
  private InventoryObject quickSlots;
  private void Start() {
    UserInput.instance.controls.GamePlay.QuickSlots.performed += ChooseSlot;
    slots = _staticInterface.inventory.GetSlots;
    selectedSlot = null;
    GameManager.instance.PlayerInventory.OnQuickSlotLoaded += UpdateQuickSlotsAfterLoad;
  }

  private void UpdateQuickSlotsAfterLoad() {
    quickSlots = GameManager.instance.PlayerInventory.quickSlots;
    // Manually instantiate equipped items after loading the equipment
    for (int i = 0; i < quickSlots.GetSlots.Length; i++) {
      if (quickSlots.GetSlots[i].IsSelected) {
        SelectSlot(i);
      }
    }
    GameManager.instance.PlayerInventory.OnQuickSlotLoaded -= UpdateQuickSlotsAfterLoad;
  }

  private void ChooseSlot(InputAction.CallbackContext obj) {
    //Debug.LogError($"chose slot {obj.control.name}");
    var index = int.Parse(obj.control.name) - 1;
    if (index == -1) {
      index = slots.Length - 1;
    }
    var slot = slots[index];
    if (slot.item != null && slot.item.Id >= 0) {
      //Debug.LogError($"select slot {index} item {slot.amount} {slot.item.Name}");
      GameManager.instance.ItemDatabaseObject.GetByID(slot.item.Id).Use();
      SelectSlot(index);
    }
    //else Debug.LogError($"slot {index} is empty");
  }

  private void SelectSlot(int index) {
    if (selectedSlot == slots[index]) {
      GameManager.instance.PlayerEquipment.OnRemoveItem(selectedSlot);
      selectedSlot.Unselect();
      selectedSlot = null;
    }
    else {
      if (selectedSlot != null && selectedSlot.item.Id >= 0) {
        GameManager.instance.PlayerEquipment.OnRemoveItem(selectedSlot);
        selectedSlot.Unselect();
      }
      //Debug.LogError($"select slot {index}");
      selectedSlot = slots[index];
      selectedSlot.Select();
      GameManager.instance.PlayerEquipment.OnEquipItem(selectedSlot);
    }
  }

  private void Update() {

  }
}