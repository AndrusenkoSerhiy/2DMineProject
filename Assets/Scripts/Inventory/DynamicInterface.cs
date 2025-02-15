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
    public bool reverseLayout = false;
    private List<GameObject> inventoryPrefabs = new List<GameObject>();

    public override void CreateSlots() {
      for (var i = 0; i < Inventory.GetSlots.Length; i++) {
        var obj = Instantiate(inventoryPrefab, Vector3.zero, Quaternion.identity, transform);
        obj.GetComponent<RectTransform>().localPosition = GetPosition(i);
        inventoryPrefabs.Add(obj);

        var slot = Inventory.GetSlots[i];

        AddSlotEvents(obj, slot, tempDragParent);

        slotsOnInterface.Add(obj, slot);
      }
    }

    public override void UpdateSlotsGameObjects() {
      for (var i = 0; i < Inventory.GetSlots.Length; i++) {
        UpdateSlotGameObject(Inventory.GetSlots[i], i);
      }
    }

    private void UpdateSlotGameObject(InventorySlot slot, int slotIndex) {
      slot.Parent = this;
      slot.SlotDisplay = inventoryPrefabs[slotIndex];
    }

    private Vector3 GetPosition(int i) {
      var column = i % NUMBER_OF_COLUMN;
      var row = i / NUMBER_OF_COLUMN;

      if (reverseLayout) {
        // If reversed, adjust the column's position to go from right to left
        column = NUMBER_OF_COLUMN - 1 - column;  // Flip the column index
      }

      return new Vector3(X_START + (X_SPACE_BETWEEN_ITEM * column), Y_START + (-Y_SPACE_BETWEEN_ITEMS * row), 0f);
    }
  }
}