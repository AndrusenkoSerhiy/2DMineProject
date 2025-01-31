using System.Collections.Generic;
using Scriptables.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Inventory {
  [RequireComponent(typeof(EventTrigger))]
  public abstract class UserInterface : MonoBehaviour {
    public InventoryObject inventory;
    // private InventoryObject _previousInventory;
    public Dictionary<GameObject, InventorySlot> slotsOnInterface = new Dictionary<GameObject, InventorySlot>();
    // private Camera uiCamera;
    // private Canvas canvas;
    protected PlayerInventory playerInventory;
    [SerializeField] protected Transform tempDragParent;

    private void Awake() {
      playerInventory = GameManager.instance.PlayerInventory;
      // uiCamera = GameManager.instance.UiCamera;
      // canvas = GameManager.instance.OverlayCanvas;

      CreateSlots();

      playerInventory.AddEvent(gameObject, EventTriggerType.PointerEnter, delegate { playerInventory.OnEnterInterface(gameObject); });
      playerInventory.AddEvent(gameObject, EventTriggerType.PointerExit, delegate { playerInventory.OnExitInterface(gameObject); });
    }

    public abstract void CreateSlots();

    public abstract void UpdateSlotDisplayObject();

    public void OnEnable() {
      UpdateSlotDisplayObject();
      UpdateInventoryUI();
    }

    public void UpdateInventoryUI() {
      foreach (var slot in inventory.GetSlots) {
        slot.ResetBackgroundAndText();
        playerInventory.SlotUpdateHandler(slot); // Ensure each slot reflects the correct UI state
      }
    }
  }
}