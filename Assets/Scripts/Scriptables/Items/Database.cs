using System.Collections.Generic;
using UnityEngine;

namespace Scriptables.Items {
  public abstract class Database<T> : ScriptableObject where T : BaseScriptableObject {
    public List<T> ItemObjects = new();
    public Dictionary<string, T> ItemsMap;

    public void OnValidate() => InitializeLookup();

    private void InitializeLookup() {
      ItemsMap = new Dictionary<string, T>();

      foreach (var item in ItemObjects) {
        if (item == null) {
          Debug.LogError($"Database {name} has null item");
          continue;
        }

        if (string.IsNullOrEmpty(item.Id)) {
          Debug.LogError($"Database {name} item {item.name} has empty ID");
          continue;
        }

        if (!ItemsMap.TryGetValue(item.Id, out var find)) {
          ItemsMap[item.Id] = item;
        }
        else {
          Debug.LogError($"Database {name} item {item.name} has duplicate ID {item.Id} in {find.name}");
        }
      }
    }
  }
}