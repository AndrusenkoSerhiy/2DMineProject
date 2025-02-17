#if UNITY_EDITOR
using Scriptables.Inventory;
using Scriptables.Items;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InventoryObject))]
public class InventoryObjectEditor : Editor {
  private int[] variants = { 5, 10, 100, 1000 };

  public override void OnInspectorGUI() {
    DrawDefaultInspector();

    var inventory = (InventoryObject)target;
    if (inventory.type != InventoryType.Inventory) {
      return;
    }

    if (inventory.database == null || inventory.database.ItemObjects == null) {
      EditorGUILayout.HelpBox("Item Database is missing or empty!", MessageType.Warning);
      return;
    }

    // Generate buttons dynamically for each item in the database
    foreach (var itemObject in inventory.database.ItemObjects) {
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

  private void AddItemToInventory(InventoryObject inventory, Item item, int amount) {
    Undo.RecordObject(inventory, "Add Item to Inventory");
    inventory.AddItem(item, amount, inventory.GetEmptySlot());
    EditorUtility.SetDirty(inventory);
  }
}
#endif