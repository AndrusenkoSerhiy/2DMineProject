using System;
using Scriptables.Items;

namespace Inventory {
  [Serializable]
  public class InventoryContainer {
    public InventorySlot[] Slots;

    public InventoryContainer(int size, InventoryType type, string inventoryId) {
      Slots = new InventorySlot[size];

      for (var i = 0; i < Slots.Length; i++) {
        Slots[i] = new InventorySlot(i, type, inventoryId);
      }
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