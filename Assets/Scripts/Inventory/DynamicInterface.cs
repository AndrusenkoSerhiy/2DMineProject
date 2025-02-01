using System.Collections.Generic;
using UnityEngine;

namespace Inventory {
  public class DynamicInterface : UserInterface {
    public GameObject inventoryPrefab;
    public int X_START;
    public int Y_START;
    public int X_SPACE_BETWEEN_ITEM;
    public int NUMBER_OF_COLUMN;
    public int Y_SPACE_BETWEEN_ITEMS;
    private List<GameObject> inventoryPrefabs = new List<GameObject>();

    public override void CreateSlots() {
      Debug.Log("DynamicInterface CreateSlots");
      for (int i = 0; i < inventory.GetSlots.Length; i++) {
        var obj = Instantiate(inventoryPrefab, Vector3.zero, Quaternion.identity, transform);
        obj.GetComponent<RectTransform>().localPosition = GetPosition(i);
        inventoryPrefabs.Add(obj);

        playerInventory.AddSlotEvents(obj, slotsOnInterface, tempDragParent);

        slotsOnInterface.Add(obj, inventory.GetSlots[i]);
      }
    }

    public override void UpdateSlotDisplayObject() {
      for (int i = 0; i < inventory.GetSlots.Length; i++) {
        inventory.GetSlots[i].parent = this;
        inventory.GetSlots[i].slotDisplay = inventoryPrefabs[i];
      }
    }

    public Vector3 GetPosition(int i) {
      return new Vector3(X_START + (X_SPACE_BETWEEN_ITEM * (i % NUMBER_OF_COLUMN)), Y_START + (-Y_SPACE_BETWEEN_ITEMS * (i / NUMBER_OF_COLUMN)), 0f);
    }
  }
}