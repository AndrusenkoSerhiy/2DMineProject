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
      // Update the entire UI when enabling the interface (e.g., after loading inventory)
      UpdateSlotDisplayObject();
      UpdateInventoryUI();
    }

    public void UpdateInventoryUI() {
      foreach (var slot in inventory.GetSlots) {
        _playerInventory.SlotUpdateHandler(slot); // Ensure each slot reflects the correct UI state
      }
    }


    // public void UpdateInventoryLinks() {
    //   int i = 0;
    //   foreach (var key in slotsOnInterface.Keys.ToList()) {
    //     slotsOnInterface[key] = inventory.GetSlots[i];
    //     i++;
    //   }
    // }

    // public void OnSlotUpdate(InventorySlot slot) {
    //   var image = slot.slotDisplay.transform.GetChild(0).GetComponent<Image>();
    //   var text = slot.slotDisplay.GetComponentInChildren<TextMeshProUGUI>();
    //   if (slot.item.Id <= -1) {
    //     image.sprite = null;
    //     image.color = new Color(1, 1, 1, 0);
    //     text.text = string.Empty;
    //   }
    //   else {
    //     image.sprite = slot.GetItemObject().UiDisplay;
    //     image.color = new Color(1, 1, 1, 1);
    //     text.text = slot.amount == 1 ? string.Empty : slot.amount.ToString("n0");
    //   }

    //   Debug.Log($"OnSlotUpdate slot amount: {slot.amount}");
    // }

    // Updates all inventory slots at once


    // public void Update() {
    //   if (_previousInventory != inventory) {
    //     UpdateInventoryLinks();
    //     Debug.Log("UpdateInventoryLinks()");
    //   }
    //   _previousInventory = inventory;
    // }
  }
}