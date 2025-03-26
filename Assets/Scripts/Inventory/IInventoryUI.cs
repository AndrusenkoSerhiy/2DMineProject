using System.Collections.Generic;
using UnityEngine;

namespace Inventory {
  public interface IInventoryUI : IInventoryDropZoneUI {
    public Inventory Inventory { get; }
    public Dictionary<GameObject, InventorySlot> SlotsOnInterface { get; }
    public void CreateSlots();
    public bool PreventDropOnGround { get; }
    public bool PreventSwapIn { get; }
    public bool PreventMergeIn { get; }
  }
}