#if UNITY_EDITOR
using Craft;
using Scriptables.Items;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Window))]
public class CraftManagerEditor : Editor {
  private int[] variants = { 5, 10, 100, 1000 };

  public override void OnInspectorGUI() {
    DrawDefaultInspector();

    if (!Application.isPlaying) {
      return;
    }

    var station = target?.GetComponent<Window>()?.Station;
    if (station == null) {
      return;
    }

    var inventory =
      GameManager.Instance.PlayerInventory.GetInventoryByTypeAndId(station.OutputInventoryType, station.Id);

    var db = GameManager.Instance.ItemDatabaseObject;

    // Generate buttons dynamically for each item in the database
    foreach (var itemObject in db.ItemObjects) {
      EditorGUILayout.LabelField(itemObject.name, EditorStyles.boldLabel);
      var item = new Item(itemObject);
      var maxAmount = itemObject.MaxStackSize;

      EditorGUILayout.BeginHorizontal();
      foreach (var amount in variants) {
        if (maxAmount > amount && GUILayout.Button($"+{amount} {itemObject.name}")) {
          AddItemToInventory(inventory, item, amount);
        }
      }

      if (GUILayout.Button($"+{maxAmount} {itemObject.name}")) {
        AddItemToInventory(inventory, item, maxAmount);
      }

      EditorGUILayout.EndHorizontal();
    }
  }

  private void AddItemToInventory(Inventory.Inventory inventory, Item item, int amount) {
    Undo.RecordObject(target, "Add Item to Inventory");
    inventory.AddItem(item, amount, inventory.GetEmptySlot(item.info));
    EditorUtility.SetDirty(target);
  }
}
#endif