using UnityEngine;

namespace Inventory {
  public class StaticInterface : UserInterface {
    public GameObject[] slots;

    public override void UpdateSlotsDisplayObject() {
      return;
    }

    public override void CreateSlots() {
      for (int i = 0; i < Inventory.GetSlots.Length; i++) {
        var obj = slots[i];
        var slot = Inventory.GetSlots[i];

        playerInventory.AddSlotEvents(obj, slot, tempDragParent);

        Inventory.GetSlots[i].parent = this;
        Inventory.GetSlots[i].slotDisplay = obj;

        slotsOnInterface.Add(obj, slot);
      }
    }
  }
}