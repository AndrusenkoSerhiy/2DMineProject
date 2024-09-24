using UnityEngine;

namespace Scriptables.Items {
  [CreateAssetMenu(fileName = "New Item Database", menuName = "Inventory System/Database")]
  public class ItemDatabaseObject : ScriptableObject {
    public ItemObject[] ItemObjects;

    public void OnValidate() {
      for (int i = 0; i < ItemObjects.Length; i++) {
        ItemObjects[i].data.Id = i;
      }
    }
  }
}

