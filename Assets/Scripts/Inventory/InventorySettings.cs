using System;
using UnityEngine;

namespace Inventory {
  [Serializable]
  public struct InventorySettings {
    public InventoryType type;
    public int size;

    [Header("Settings for additional inventories, when we expand main inventory")]
    public Sprite slotIcon;
  }
}