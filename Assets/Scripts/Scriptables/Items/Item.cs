using System;
using System.Collections.Generic;
using Scriptables.Inventory;

namespace Scriptables.Items {
  [Serializable]
  public class Item {
    [NonSerialized] public ItemObject info;
    public string id;
    public string name;

    public Item() {
      info = null;
      id = string.Empty;
      name = string.Empty;
    }

    public Item(ItemObject item) {
      info = item;
      id = item.Id;
      name = item.Name;
    }

    public bool isEmpty => info == null || string.IsNullOrEmpty(id);
    public bool hasId => !string.IsNullOrEmpty(id);

    public void RestoreItemObject(List<ItemObject> itemDatabase) {
      info = itemDatabase.Find(x => x.Id == id);
    }
  }
}