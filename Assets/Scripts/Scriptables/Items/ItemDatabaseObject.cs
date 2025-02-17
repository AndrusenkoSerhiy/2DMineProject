using System.Collections.Generic;
using UnityEngine;

namespace Scriptables.Items {
  [CreateAssetMenu(fileName = "New Item Database", menuName = "Inventory System/Database")]
  public class ItemDatabaseObject : ScriptableObject {
    public List<ItemObject> ItemObjects = new();
    private Dictionary<string, ItemObject> itemLookup;

    public void OnValidate() => InitializeLookup();

    public ItemObject GetByID(string id) {
      if (itemLookup.TryGetValue(id, out var item)) {
        return item;
      }

      Debug.LogWarning($"Item with ID {id} not found.");
      return null;
    }

    private void InitializeLookup() {
      itemLookup = new Dictionary<string, ItemObject>();

      foreach (var item in ItemObjects) {
        if (item == null) {
          continue;
        }

        if (string.IsNullOrEmpty(item.Id)) {
          continue;
        }

        if (!itemLookup.ContainsKey(item.Id)) {
          itemLookup[item.Id] = item;
        }
        else {
          Debug.LogError($"Duplicate ItemObject ID found: {item.Id} in {item.name}");
        }
      }
    }
  }
}