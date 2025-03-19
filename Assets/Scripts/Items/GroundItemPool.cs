using System.Collections.Generic;
using Scriptables.Items;
using UnityEngine;
using UnityEngine.Rendering;

namespace Items {
  //TODO add rigidbody to items
  public class GroundItemPool : MonoBehaviour {
    public float autoDestroyDelay = 60f;
    private SerializedDictionary<string, List<GroundItem>> pool = new();

    private GroundItem CreateNewItem(ItemObject item) {
      if (item.spawnPrefab == null) {
        Debug.LogError($"Item spawnPrefab is null, id - {item.Id}, name - {item.Name}");
        return null;
      }

      if (!item.spawnPrefab.TryGetComponent<GroundItem>(out var prefabGroundItem)) {
        Debug.LogError($"Item spawnPrefab does not contain a GroundItem component, id - {item.Id}, name - {item.Name}");
        return null;
      }

      var newObj = Instantiate(item.spawnPrefab, gameObject.transform);
      if (!newObj.TryGetComponent<GroundItem>(out var groundItem)) {
        return null;
      }

      if (!pool.ContainsKey(item.Id)) {
        pool[item.Id] = new List<GroundItem>();
      }

      pool[item.Id].Add(groundItem);
      return groundItem;
    }

    public GroundItem GetItem(ItemObject itemObject) {
      if (itemObject == null) {
        Debug.LogError("Item not found");
        return null;
      }

      if (!pool.TryGetValue(itemObject.Id, out var poolItems)) {
        return CreateNewItem(itemObject);
      }

      foreach (var item in poolItems) {
        if (item.gameObject.activeInHierarchy) {
          continue;
        }

        item.gameObject.SetActive(true);
        item.ResetState();
        return item;
      }

      return CreateNewItem(itemObject);
    }

    public void ReturnItem(GroundItem item) {
      item.gameObject.SetActive(false);
    }
  }
}