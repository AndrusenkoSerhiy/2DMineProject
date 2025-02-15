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

  private void Awake() {
    slots = _staticInterface.Inventory.GetSlots;
    selectedSlot = null;
    GameManager.instance.PlayerInventory.OnQuickSlotLoaded += UpdateQuickSlotsAfterLoad;
  }

  private void Start() {
    UserInput.instance.controls.GamePlay.QuickSlots.performed += ChooseSlot;
  }

  private void UpdateQuickSlotsAfterLoad() {
    quickSlots = GameManager.instance.PlayerInventory.quickSlots;
    // Manually instantiate equipped items after loading the equipment
    for (var i = 0; i < quickSlots.GetSlots.Length; i++) {
      var slot = quickSlots.GetSlots[i];
      if (slot.isEmpty || !slot.isSelected) {
        continue;
      }

      // GameManager.instance.ItemDatabaseObject.GetByID(quickSlots.GetSlots[i].Item.info.Id).Use(quickSlots.GetSlots[i]);
      slot.Item.info.Use(quickSlots.GetSlots[i]);
      SelectSlot(i);
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
    if (slot.Item != null && slot.Item.info != null) {
      //Debug.LogError($"select slot {index} item {slot.amount} {slot.item.Name}");
      //GameManager.instance.ItemDatabaseObject.GetByID(slot.item.Id).Use(slot);
      SelectSlot(index);
    }
    //else Debug.LogError($"slot {index} is empty");
  }

  private void SelectSlot(int index) {
    if (selectedSlot == slots[index]) {
      //Debug.LogError($"uselectSlot");
      GameManager.instance.PlayerEquipment.OnRemoveItem(selectedSlot);
      selectedSlot.Unselect();
      GameManager.instance.ItemDatabaseObject.GetByID(selectedSlot.Item.info.Id).Use(selectedSlot);
      selectedSlot = null;
    }
    else {
      if (selectedSlot != null && selectedSlot.Item.info != null) {
        //Debug.LogError($"log1");
        GameManager.instance.PlayerEquipment.OnRemoveItem(selectedSlot);
        selectedSlot.Unselect();
      }

      selectedSlot = slots[index];
      selectedSlot.Select();
      GameManager.instance.PlayerEquipment.OnEquipItem(selectedSlot);
      GameManager.instance.ItemDatabaseObject.GetByID(selectedSlot.Item.info.Id).Use(selectedSlot);
    }
  }
}