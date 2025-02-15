using System.Collections.Generic;
using Scriptables.Inventory;
using UnityEngine;

namespace Inventory {
  public interface IInventoryUI : IInventoryDropZoneUI {
    public InventoryObject Inventory { get; }
    public Dictionary<GameObject, InventorySlot> SlotsOnInterface { get; }
    public void CreateSlots();
    public bool PreventDropOnGround { get; }
    public bool PreventSwapIn { get; }
    public bool PreventMergeIn { get; }
  }
}