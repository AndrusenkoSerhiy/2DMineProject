using UnityEngine;

namespace Inventory {
  public class StaticInterface : UserInterface {
    public GameObject[] slots;

    public override void UpdateSlotsGameObjects() {
      return;
    }

    public override void CreateSlots() {
      for (int i = 0; i < Inventory.GetSlots.Length; i++) {
        var obj = slots[i];
        var slot = Inventory.GetSlots[i];

        AddSlotEvents(obj, slot, tempDragParent);

        Inventory.GetSlots[i].Parent = this;
        Inventory.GetSlots[i].SlotDisplay = obj;

        slotsOnInterface.Add(obj, slot);
      }
    }
  }
}