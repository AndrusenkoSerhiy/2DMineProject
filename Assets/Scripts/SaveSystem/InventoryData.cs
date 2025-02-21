using System;
using SaveSystem;
using UnityEngine;

namespace Inventory {
  [Serializable]
  public class InventoryData : ISaveable {
    [field: SerializeField] public string Id { get; set; }
    public InventorySlot[] Slots;
  }
}