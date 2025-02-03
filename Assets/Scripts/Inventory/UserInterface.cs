using System.Collections.Generic;
using Scriptables.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Inventory {
  [RequireComponent(typeof(EventTrigger))]
  public abstract class UserInterface : MonoBehaviour, IInventoryUI {
    [SerializeField] protected InventoryObject inventory;
    public InventoryObject Inventory => inventory;
    protected Dictionary<GameObject, InventorySlot> slotsOnInterface;
    public Dictionary<GameObject, InventorySlot> SlotsOnInterface => slotsOnInterface;
    protected PlayerInventory playerInventory;
    [SerializeField] protected Transform tempDragParent;

    private void Awake() {
      slotsOnInterface = new Dictionary<GameObject, InventorySlot>();
      playerInventory = GameManager.instance.PlayerInventory;

      CreateSlots();

      playerInventory.AddEvent(gameObject, EventTriggerType.PointerEnter, delegate { playerInventory.OnEnterInterface(gameObject); });
      playerInventory.AddEvent(gameObject, EventTriggerType.PointerExit, delegate { playerInventory.OnExitInterface(gameObject); });
    }

    public abstract void CreateSlots();

    public abstract void UpdateSlotsDisplayObject();

    public void OnEnable() {
      UpdateUI();
    }

    public void UpdateUI() {
      UpdateSlotsDisplayObject();
      UpdateInventoryUI();
    }

    private void UpdateInventoryUI() {
      foreach (var slot in Inventory.GetSlots) {
        UpdateInventorySlotUI(slot);
      }
    }

    protected void UpdateInventorySlotUI(InventorySlot slot) {
      slot.ResetBackgroundAndText();
      playerInventory.SlotUpdateHandler(slot); // Ensure each slot reflects the correct UI state
    }

    void IInventoryUI.CreateSlots() {
      throw new System.NotImplementedException();
    }
  }
}