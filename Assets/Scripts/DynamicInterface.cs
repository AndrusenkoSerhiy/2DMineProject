using System.Collections.Generic;
using Scriptables.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;

public class DynamicInterface : UserInterface {
  public GameObject inventoryPrefab;
  public int X_START;
  public int Y_START;
  public int X_SPACE_BETWEEN_ITEM;
  public int NUMBER_OF_COLUMN;
  public int Y_SPACE_BETWEEN_ITEMS;
  private List<GameObject> inventoryPrefabs = new List<GameObject>();

  public override void CreateSlots() {
    Debug.Log("CreateSlots() on " + gameObject.transform.parent.parent.name);
    for (int i = 0; i < inventory.GetSlots.Length; i++) {
      var obj = Instantiate(inventoryPrefab, Vector3.zero, Quaternion.identity, transform);
      obj.GetComponent<RectTransform>().localPosition = GetPosition(i);
      inventoryPrefabs.Add(obj);

      AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnEnter(obj); });
      AddEvent(obj, EventTriggerType.PointerExit, delegate { OnExit(obj); });
      AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnDragStart(obj); });
      AddEvent(obj, EventTriggerType.EndDrag, delegate { OnDragEnd(obj); });
      AddEvent(obj, EventTriggerType.Drag, delegate { OnDrag(obj); });

      slotsOnInterface.Add(obj, inventory.GetSlots[i]);
    }
  }

  public override void UpdateSlotDisplayObject() {
    Debug.Log("UpdateSlotDisplayObject() on " + gameObject.transform.parent.parent.name);
    for (int i = 0; i < inventory.GetSlots.Length; i++) {
      inventory.GetSlots[i].slotDisplay = inventoryPrefabs[i];
    }
  }

  public Vector3 GetPosition(int i) {
    return new Vector3(X_START + (X_SPACE_BETWEEN_ITEM * (i % NUMBER_OF_COLUMN)), Y_START + (-Y_SPACE_BETWEEN_ITEMS * (i / NUMBER_OF_COLUMN)), 0f);
  }
}