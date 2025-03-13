#if UNITY_EDITOR
using Craft;
using Inventory;
using Scriptables.Items;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CraftManager))]
public class CraftManagerEditor : Editor {
  private int[] variants = { 5, 10, 100, 1000 };

  public override void OnInspectorGUI() {
    DrawDefaultInspector();

    if (!Application.isPlaying) {
      return;
    }

    var station = target?.GetComponent<CraftManager>()?.Station;
    if (station == null) {
      return;
    }

    var inventory =
      GameManager.Instance.PlayerInventory.GetInventoryByTypeAndId(station.OutputInventoryType, station.Id);

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
    Undo.RecordObject(target, "Add Item to Inventory");
    inventory.AddItem(item, amount, inventory.GetEmptySlot());
    EditorUtility.SetDirty(target);
  }
}
#endif