using UnityEngine;

namespace Scriptables.Items {
  [CreateAssetMenu(menuName = "Inventory System/Items/Item", fileName = "New Item")]
  public class Resource : ItemObject {
    public GameObject spawnPrefab;

    public void Awake() {
      Type = ItemType.Tool;
    }
  }
}