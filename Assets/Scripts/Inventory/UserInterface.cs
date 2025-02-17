using System.Collections.Generic;
using Scriptables.Inventory;
using Settings;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Inventory {
  [RequireComponent(typeof(EventTrigger))]
  public class UserInterface : MonoBehaviour, IInventoryUI {
    [SerializeField] private InventoryObject inventory;
    [SerializeField] private Color disabledSlotColor;

    [SerializeField] private bool preventItemDropIn;
    [SerializeField] private bool preventDropOnGround;
    [SerializeField] private bool preventSwapIn;
    [SerializeField] private bool preventMergeIn;

    private Transform tempDragParent;
    private Dictionary<GameObject, InventorySlot> slotsOnInterface;

    public GameObject[] slotsPrefabs;
    public InventoryObject Inventory => inventory;
    public Dictionary<GameObject, InventorySlot> SlotsOnInterface => slotsOnInterface;

    public bool PreventItemDropIn => preventItemDropIn;
    public bool PreventDropOnGround => preventDropOnGround;
    public bool PreventSwapIn => preventSwapIn;
    public bool PreventMergeIn => preventMergeIn;

    public void Awake() {
      tempDragParent = GameManager.instance.Canvas.transform;
      slotsOnInterface = new Dictionary<GameObject, InventorySlot>();

      CheckSlotsUpdate();

      CreateSlots();

      AddEvent(gameObject, EventTriggerType.PointerEnter, delegate { OnEnterInterface(gameObject); });
      AddEvent(gameObject, EventTriggerType.PointerExit, delegate { OnExitInterface(gameObject); });
    }

    public void OnEnable() {
      UpdateSlotsGameObjects();
      UpdateInventoryUI();
    }

    private void UpdateSlotsGameObjects() {
      for (var i = 0; i < Inventory.GetSlots.Length; i++) {
        var slot = Inventory.GetSlots[i];
        slot.SetParent(this);
        slot.SetSlotDisplay(slotsPrefabs[i]);
        slotsOnInterface[slotsPrefabs[i]] = slot;
      }
    }

    public void CreateSlots() {
      if (slotsPrefabs.Length < Inventory.GetSlots.Length) {
        Debug.LogError("Not enough slots in the interface");
        return;
      }

      for (var i = 0; i < slotsPrefabs.Length; i++) {
        if (i > Inventory.GetSlots.Length - 1) {
          slotsPrefabs[i].GetComponent<Image>().color = disabledSlotColor;
          continue;
        }

        var obj = slotsPrefabs[i];
        var slot = Inventory.GetSlots[i];

        AddSlotEvents(obj, slot, tempDragParent);

        slotsOnInterface.Add(obj, slot);
      }
    }

    public void UpdateInventoryUI() {
      foreach (var slot in Inventory.GetSlots) {
        UpdateInventorySlotUI(slot);
      }
    }

    protected void UpdateInventorySlotUI(InventorySlot slot) {
      slot.ResetBackgroundAndText();
      UpdateSlotDisplay(slot); // Ensure each slot reflects the correct UI state
    }

    private void UpdateSlotHandler(SlotUpdateEventData data) {
      UpdateSlotDisplay(data.after);
    }

    public void UpdateSlotDisplay(InventorySlot slot) {
      var image = slot.Background;
      var text = slot.Text;
      if (slot.Item.info == null) {
        image.sprite = null;
        image.color = new Color(1, 1, 1, 0);
        text.text = string.Empty;
      }
      else {
        image.sprite = slot.Item.info.UiDisplay;
        image.color = new Color(1, 1, 1, 1);
        text.text = slot.amount == 1 ? string.Empty : slot.amount.ToString("n0");
      }
    }

    public void CheckSlotsUpdate() {
      for (var i = 0; i < inventory.GetSlots.Length; i++) {
        inventory.GetSlots[i].OnAfterUpdated += UpdateSlotHandler;
      }
    }

    public GameObject CreateTempItem(InventorySlot slot, Transform parent) {
      GameObject tempItem = null;
      if (slot.isEmpty) {
        return tempItem;
      }

      tempItem = new GameObject("TempItemBeingDragged");
      tempItem.layer = 5;
      var rt = tempItem.AddComponent<RectTransform>();

      rt.sizeDelta = new Vector2(80, 80);

      tempItem.transform.SetParent(parent);
      var img = tempItem.AddComponent<Image>();
      img.sprite = slot.GetItemObject().UiDisplay;
      img.raycastTarget = false;

      tempItem.transform.localScale = Vector3.one;

      return tempItem;
    }

    protected void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action) {
      EventTrigger trigger = obj.GetComponent<EventTrigger>();
      if (!trigger) {
        Debug.LogWarning("No EventTrigger component found!");
        return;
      }

      var eventTrigger = new EventTrigger.Entry { eventID = type };
      eventTrigger.callback.AddListener(action);
      trigger.triggers.Add(eventTrigger);
    }

    protected void AddSlotEvents(GameObject obj, InventorySlot slot, Transform parent) {
      AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnEnter(obj); });
      AddEvent(obj, EventTriggerType.PointerExit, delegate { OnExit(obj); });
      AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnDragStart(slot, parent); });
      AddEvent(obj, EventTriggerType.EndDrag, (data) => OnDragEnd(data, slot));
      AddEvent(obj, EventTriggerType.Drag, delegate { OnDrag(obj); });
    }

    protected void OnEnter(GameObject obj) {
      MouseData.slotHoveredOver = obj;
    }

    protected void OnEnterInterface(GameObject obj) {
      MouseData.interfaceMouseIsOver = obj.GetComponent<IInventoryDropZoneUI>();
    }

    protected void OnExitInterface(GameObject obj) {
      MouseData.interfaceMouseIsOver = null;
    }

    protected void OnExit(GameObject obj) {
      MouseData.slotHoveredOver = null;
    }

    protected void OnDrag(GameObject obj) {
      if (MouseData.tempItemBeingDragged != null) {
        var mousePos = UserInput.instance.GetMousePosition(); //_uiCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        MouseData.tempItemBeingDragged.GetComponent<RectTransform>().position = mousePos;
      }
    }

    protected void OnDragStart(InventorySlot slot, Transform parent) {
      if (slot.isEmpty) {
        Debug.Log("OnDragStart Slot is empty, cant drag");
        return;
      }

      MouseData.tempItemBeingDragged = CreateTempItem(slot, parent);
    }

    protected void OnDragEnd(BaseEventData data, InventorySlot slot) {
      var pointerData = data as PointerEventData;
      Destroy(MouseData.tempItemBeingDragged);

      if (slot.isEmpty) {
        Debug.Log("OnDragEnd Slot is empty, cant drag");
        return;
      }

      // Prevent dropping if the pointer is over a restricted drop zone
      if (pointerData?.pointerEnter != null) {
        var dropZone = pointerData.pointerEnter.GetComponentInParent<IInventoryDropZoneUI>();
        if (dropZone?.PreventItemDropIn == true) {
          Debug.Log("Preventing item drop on restricted drop zone");
          return;
        }
      }

      // Handle item drop on the ground
      if (MouseData.interfaceMouseIsOver == null) {
        if (!PreventDropOnGround) {
          inventory.SpawnItem(slot.Item, slot.amount);
          slot.RemoveItem();

          Debug.Log("Dropping item on the ground");
        }

        return;
      }

      if (!MouseData.slotHoveredOver) {
        Debug.Log("Slot hovered over is null");
        return;
      }

      // Retrieve the slot being hovered over
      var targetSlot = (MouseData.interfaceMouseIsOver as IInventoryUI)?.SlotsOnInterface[MouseData.slotHoveredOver];
      if (targetSlot == null) {
        Debug.Log("Target slot is null");
        return;
      }

      if (targetSlot == slot) {
        Debug.Log("Target slot is the same as the source slot");
        return;
      }

      var targetInventory = targetSlot.Parent.Inventory;
      var targetUI = targetSlot.Parent;

      // Prevent any item movement if drop is disabled
      if (targetUI.PreventItemDropIn) {
        Debug.Log("Preventing item drop");
        return;
      }

      // Handle merging items
      if (!targetUI.PreventMergeIn && slot.CanMerge(targetSlot)) {
        targetInventory.MergeItems(slot, targetSlot);

        Debug.Log("Merging items");
        return;
      }

      // Handle swapping items
      // var canSwap = !targetUI.PreventSwapIn && (PreventSwapIn || (!PreventSwapIn && slot.isEmpty));
      var cantSwap = targetUI.PreventSwapIn || (PreventSwapIn && !targetSlot.isEmpty);
      if (cantSwap) {
        return;
      }

      inventory.SwapSlots(slot, targetSlot);
    }
  }
}