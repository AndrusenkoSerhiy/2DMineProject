using System.Collections.Generic;
using UnityEngine;

namespace Scriptables.Items {
  [CreateAssetMenu(fileName = "New Item Database", menuName = "Inventory System/Database")]
  public class ItemDatabaseObject : Database<ItemObject> {
    public List<ItemObject> DefaultItemsOnStart;
  }
}