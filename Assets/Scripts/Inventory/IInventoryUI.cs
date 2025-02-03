using System.Collections.Generic;
using Scriptables.Inventory;
using UnityEngine;

namespace Inventory {
  public interface IInventoryUI {
    public InventoryObject Inventory { get; }
    public Dictionary<GameObject, InventorySlot> SlotsOnInterface { get; }
    public void CreateSlots();
  }
}