using System;
using Inventory;
using UnityEngine;

namespace SaveSystem {
  [Serializable]
  public class InventoryData {
    [field: SerializeField] public string Id { get; set; }
    public InventorySlot[] Slots;
  }
}