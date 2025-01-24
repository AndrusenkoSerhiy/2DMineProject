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
    [SerializeField] private Camera _uiCamera;
    [SerializeField] private Canvas _canvas;
    [SerializeField] protected Transform _tempDragParent;
    protected PlayerInventory _playerInventory;

    private void Awake() {
      _playerInventory = GameManager.instance.PlayerInventory;

      CreateSlots();

      _playerInventory.AddEvent(gameObject, EventTriggerType.PointerEnter, delegate { _playerInventory.OnEnterInterface(gameObject); });
      _playerInventory.AddEvent(gameObject, EventTriggerType.PointerExit, delegate { _playerInventory.OnExitInterface(gameObject); });
    }

    public abstract void CreateSlots();

    public abstract void UpdateSlotDisplayObject();

    public void OnEnable() {
      UpdateSlotDisplayObject();
      UpdateInventoryUI();
    }

    public void UpdateInventoryUI() {
      foreach (var slot in inventory.GetSlots) {
        _playerInventory.SlotUpdateHandler(slot); // Ensure each slot reflects the correct UI state
      }
    }
  }
}