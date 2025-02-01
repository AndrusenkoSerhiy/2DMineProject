using UnityEngine;

namespace Inventory {
  public class StaticInterface : UserInterface {
    public GameObject[] slots;

    public override void UpdateSlotDisplayObject() {
      return;
    }

    public override void CreateSlots() {
      for (int i = 0; i < inventory.GetSlots.Length; i++) {
        var obj = slots[i];

        playerInventory.AddSlotEvents(obj, slotsOnInterface, tempDragParent);

        inventory.GetSlots[i].parent = this;
        inventory.GetSlots[i].slotDisplay = obj;

        slotsOnInterface.Add(obj, inventory.GetSlots[i]);
      }
    }
  }
}