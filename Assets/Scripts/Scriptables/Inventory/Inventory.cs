﻿using System;
using System.Linq;
using Scriptables.Items;

namespace Scriptables.Inventory {
  [Serializable]
  public class Inventory {
    //TODO
    public InventorySlot[] Slots = new InventorySlot[24];

    public void Clear() {
      for (int i = 0; i < Slots.Length; i++) {
        Slots[i].item = new Item();
        Slots[i].amount = 0;
      }
    }

    public bool ContainsItem(ItemObject itemObject) {
      return Array.Find(Slots, i => i.item.Id == itemObject.data.Id) != null;
    }

    public bool ContainsItem(int id) {
      return Slots.FirstOrDefault(i => i.item.Id == id) != null;
    }
  }
}
