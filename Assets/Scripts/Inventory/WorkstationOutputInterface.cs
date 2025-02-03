using System.Collections.Generic;
using Scriptables.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Inventory {
  public class WorkstationOutputInterface : MonoBehaviour, IInventoryUI {
    [SerializeField] private Transform tempDragParent;
    [SerializeField] private InventoryObject inventory;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private GameObject inventoryPrefab;
    [SerializeField] private int X_START;
    [SerializeField] private int Y_START;
    [SerializeField] private int X_SPACE_BETWEEN_ITEM;
    [SerializeField] private int NUMBER_OF_COLUMN;
    [SerializeField] private int Y_SPACE_BETWEEN_ITEMS;
    [SerializeField] private bool reverseLayout = true;

    public InventoryObject Inventory => inventory;
    private List<GameObject> inventoryPrefabs = new List<GameObject>();

    private Dictionary<GameObject, InventorySlot> slotsOnInterface;
    public Dictionary<GameObject, InventorySlot> SlotsOnInterface => slotsOnInterface;

    private void Awake() {
      slotsOnInterface = new Dictionary<GameObject, InventorySlot>();
      playerInventory = GameManager.instance.PlayerInventory;

      CreateSlots();

      playerInventory.AddEvent(gameObject, EventTriggerType.PointerEnter, delegate { playerInventory.OnEnterInterface(gameObject); });
      playerInventory.AddEvent(gameObject, EventTriggerType.PointerExit, delegate { playerInventory.OnExitInterface(gameObject); });
    }

    private void CreateSlots() {
      Debug.Log("WorkstationOutputInterface CreateSlots");
      for (int i = 0; i < inventory.GetSlots.Length; i++) {
        var obj = Instantiate(inventoryPrefab, Vector3.zero, Quaternion.identity, transform);
        obj.GetComponent<RectTransform>().localPosition = GetPosition(i);
        inventoryPrefabs.Add(obj);

        UpdateSlotDisplayObject(inventory.GetSlots[i], i);

        var slot = Inventory.GetSlots[i];

        playerInventory.AddSlotEvents(obj, slot, tempDragParent);

        slotsOnInterface.Add(obj, slot);
      }
    }

    public void UpdateSlotsDisplayObject() {
      for (int i = 0; i < inventory.GetSlots.Length; i++) {
        UpdateSlotDisplayObject(inventory.GetSlots[i], i);
      }
    }

    private void UpdateSlotDisplayObject(InventorySlot slot, int slotIndex) {
      slot.parent = this;
      slot.slotDisplay = inventoryPrefabs[slotIndex];
    }

    private Vector3 GetPosition(int i) {
      int column = i % NUMBER_OF_COLUMN;
      int row = i / NUMBER_OF_COLUMN;

      if (reverseLayout) {
        // If reversed, adjust the column's position to go from right to left
        column = NUMBER_OF_COLUMN - 1 - column;  // Flip the column index
      }

      return new Vector3(X_START + (X_SPACE_BETWEEN_ITEM * column), Y_START + (-Y_SPACE_BETWEEN_ITEMS * row), 0f);
    }

    void IInventoryUI.CreateSlots() {
      CreateSlots();
    }
  }
}