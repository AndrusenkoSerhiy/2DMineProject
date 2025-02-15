using System;
using Scriptables.Items;

namespace Inventory {
  [Serializable]
  public class InventoryContainer {
    public InventorySlot[] Slots;

    public InventoryContainer(int size) {
      Slots = new InventorySlot[size];
    }

    public void Clear() {
      foreach (var slot in Slots) {
        slot.Item = new Item();
        slot.amount = 0;
        slot.isSelected = false;
      }
    }
  }
}