using Inventory;
using Settings;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuickSlotListener : MonoBehaviour {
  [SerializeField] private StaticInterface _staticInterface;
  private void Start() {
    UserInput.instance.controls.GamePlay.QuickSlots.performed += ChooseSlot;
  }

  private void ChooseSlot(InputAction.CallbackContext obj) {
    //Debug.LogError($"chose slot {obj.control.name}");
    var slots = _staticInterface.inventory.GetSlots;
    var index = int.Parse(obj.control.name) - 1;
    if (index == -1) {
      index = slots.Length - 1;
    }
    var slot = slots[index];
    if (slot.item != null) Debug.LogError($"select slot {index} item {slot.amount} {slot.item.Name}");
    else Debug.LogError($"slot {index} is empty");
  }

  private void Update() {

  }
}