using System;
using System.Collections.Generic;
using Scriptables.Inventory;

namespace Scriptables.Items {
  [Serializable]
  public class Item {
    [NonSerialized] public ItemObject info;
    public string id;
    public string name;
    public InventoryType InventoryType { get; private set; }

    //TODO: Add ContainerIndex to Item(for storages)
    public int ContainerIndex { get; private set; } = 0;

    public Item() {
      info = null;
      id = string.Empty;
      name = string.Empty;
      InventoryType = InventoryType.None;
    }

    public Item(ItemObject item, InventoryType type = InventoryType.None) {
      info = item;
      id = item.Id;
      name = item.Name;
      InventoryType = type;
    }

    public bool isEmpty => info == null || string.IsNullOrEmpty(id);
    public bool hasId => !string.IsNullOrEmpty(id);

    public void RestoreItemObject(List<ItemObject> itemDatabase) {
      info = itemDatabase.Find(x => x.Id == id);
    }

    public void SetInventoryType(InventoryType type) {
      InventoryType = type;
    }
  }
}