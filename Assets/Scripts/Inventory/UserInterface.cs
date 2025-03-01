using System;
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

    [SerializeField] private InventoryObject fastDropInventory;
    [SerializeField] private bool preventItemDropIn;
    [SerializeField] private bool preventDropOnGround;
    [SerializeField] private bool preventSwapIn;
    [SerializeField] private bool preventMergeIn;
    [SerializeField] private bool preventSplit;

    private SplitItem splitItem;
    private GameObject tempDragItemObject;
    private TempDragItem tempDragItem;
    private Transform tempDragParent;
    private Dictionary<GameObject, InventorySlot> slotsOnInterface;

    public event Action OnLoaded;

    // public GameObject[] slotsPrefabs;
    public SlotDisplay[] slotsPrefabs;
    public InventoryObject Inventory => inventory;
    public Dictionary<GameObject, InventorySlot> SlotsOnInterface => slotsOnInterface;

    public bool PreventItemDropIn => preventItemDropIn;
    public bool PreventDropOnGround => preventDropOnGround;
    public bool PreventSwapIn => preventSwapIn;
    public bool PreventMergeIn => preventMergeIn;

    public void Setup(InventoryObject inventory) {
      this.inventory = inventory;
    }

    public void Awake() {
      splitItem = GameManager.Instance.SplitItem;
      tempDragItemObject = GameManager.Instance.TempDragItem;
      tempDragItem = tempDragItemObject.GetComponent<TempDragItem>();
      tempDragParent = GameManager.Instance.Canvas.transform;
      slotsOnInterface = new Dictionary<GameObject, InventorySlot>();

      CheckSlotsUpdate();
      CreateSlots();
    }

    public void OnEnable() {
      UpdateSlotsGameObjects();

      AddEvent(gameObject, EventTriggerType.PointerEnter, delegate { OnEnterInterface(gameObject); });
      AddEvent(gameObject, EventTriggerType.PointerExit, delegate { OnExitInterface(gameObject); });

      AddSlotsEvents();

      UpdateInventoryUI();

      OnLoaded?.Invoke();
      inventory.OnResorted += ResortHandler;
    }

    public void OnDisable() {
      RemoveAllEvents(gameObject);
      RemoveAllEvents(gameObject);

      RemoveSlotsEvents();

      inventory.OnResorted -= ResortHandler;
    }

    public void CreateSlots() {
      if (slotsPrefabs.Length < Inventory.GetSlots.Length) {
        Debug.LogError("Not enough slots in the interface");
        return;
      }

      for (var i = 0; i < slotsPrefabs.Length; i++) {
        if (i > Inventory.GetSlots.Length - 1) {
          // slotsPrefabs[i].GetComponent<Image>().color = disabledSlotColor;
          slotsPrefabs[i].Background.color = disabledSlotColor;
          continue;
        }

        var obj = slotsPrefabs[i].gameObject;
        var slot = Inventory.GetSlots[i];

        slotsOnInterface.Add(obj, slot);
      }
    }

    private void AddSlotsEvents() {
      foreach (var slot in slotsOnInterface) {
        AddSlotEvents(slot.Key, slot.Value);
      }
    }

    private void RemoveSlotsEvents() {
      foreach (var slot in slotsOnInterface) {
        RemoveAllEvents(slot.Key);
      }
    }

    private void ResortHandler() {
      UpdateInventoryUI();
    }

    private void RemoveAllEvents(GameObject obj) {
      if (!obj.TryGetComponent(out EventTrigger trigger)) {
        Debug.LogWarning($"No EventTrigger component found on {obj.name}!");
        return;
      }

      trigger.triggers.Clear();
    }

    private void AddSlotEvents(GameObject obj, InventorySlot slot) {
      AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnEnter(obj); });
      AddEvent(obj, EventTriggerType.PointerExit, delegate { OnExit(obj); });
      AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnDragStart(slot); });
      AddEvent(obj, EventTriggerType.EndDrag, (data) => SlotDrop(data, slot));
      AddEvent(obj, EventTriggerType.Drag, delegate { OnDrag(obj); });
      AddEvent(obj, EventTriggerType.PointerClick, (data) => OnSlotClick(data, slot, obj));
    }

    private void UpdateSlotsGameObjects() {
      for (var i = 0; i < Inventory.GetSlots.Length; i++) {
        var slot = Inventory.GetSlots[i];
        slot.SetParent(this);
        slot.SetSlotDisplay(slotsPrefabs[i]);
        slotsOnInterface[slotsPrefabs[i].gameObject] = slot;
      }
    }

    public void UpdateInventoryUI() {
      foreach (var slot in Inventory.GetSlots) {
        UpdateInventorySlotUI(slot);
      }
    }

    private void UpdateInventorySlotUI(InventorySlot slot) {
      // slot.ResetBackgroundAndText();
      UpdateSlotDisplay(slot); // Ensure each slot reflects the correct UI state
    }

    private void UpdateSlotHandler(SlotUpdateEventData data) {
      UpdateSlotDisplay(data.after);
    }

    public void UpdateSlotDisplay(InventorySlot slot) {
      var image = slot.SlotDisplay.Background;
      var text = slot.SlotDisplay.Text;
      if (slot.Item.info == null || slot.amount <= 0) {
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

    public void CreateTempItem(InventorySlot slot) {
      if (slot.isEmpty) {
        return;
      }

      tempDragItem.Enable(slot.Item, slot.amount, tempDragParent);
    }

    private void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action) {
      if (!obj.TryGetComponent(out EventTrigger trigger)) {
        Debug.LogWarning($"No EventTrigger component found on {obj.name}!");
        return;
      }

      var entry = trigger.triggers.Find(e => e.eventID == type);

      if (entry == null) {
        entry = new EventTrigger.Entry { eventID = type };
        trigger.triggers.Add(entry);
      }

      entry.callback.AddListener(action);
    }

    private void OnSlotClick(BaseEventData data, InventorySlot slot, GameObject obj) {
      if (data is not PointerEventData { button: PointerEventData.InputButton.Left }) {
        return;
      }

      //drop splited
      if (splitItem.Active) {
        var dropResult = SlotDrop(data, splitItem.Slot);
        splitItem.End(dropResult);
        return;
      }

      if (slot.isEmpty) {
        return;
      }

      //fast drop item to another inventory
      if (UserInput.instance.controls.UI.Ctrl.IsPressed() && fastDropInventory) {
        var overFlow = fastDropInventory.AddItem(slot.Item, slot.amount);
        if (overFlow > 0) {
          slot.RemoveAmount(slot.amount - overFlow);
        }
        else {
          slot.RemoveItem();
        }

        return;
      }

      //split
      if (UserInput.instance.controls.UI.Shift.IsPressed() && !preventSplit) {
        splitItem.Show(slot, obj, tempDragParent);
      }
    }

    private void OnEnter(GameObject obj) {
      MouseData.slotHoveredOver = obj;
    }

    private void OnEnterInterface(GameObject obj) {
      MouseData.interfaceMouseIsOver = obj.GetComponent<IInventoryDropZoneUI>();
    }

    private void OnExitInterface(GameObject obj) {
      MouseData.interfaceMouseIsOver = null;
    }

    private void OnExit(GameObject obj) {
      MouseData.slotHoveredOver = null;
    }

    private void OnDrag(GameObject obj) {
      if (!tempDragItem.IsDrag) {
        return;
      }

      var mousePos = UserInput.instance.GetMousePosition();
      mousePos.z = 0;
      tempDragItem.UpdatePosition(mousePos);
    }

    private void OnDragStart(InventorySlot slot) {
      if (slot.isEmpty) {
        Debug.Log("OnDragStart Slot is empty, cant drag");
        return;
      }

      CreateTempItem(slot);
    }

    private bool SlotDrop(BaseEventData data, InventorySlot slot) {
      var pointerData = data as PointerEventData;
      var dragFull = tempDragItem.DragFull;
      var dragAmount = tempDragItem.Amount;
      tempDragItem.Disable();

      if (slot.isEmpty) {
        Debug.Log("OnDragEnd Slot is empty, cant drag");
        return false;
      }

      // Prevent dropping if the pointer is over a restricted drop zone
      if (pointerData?.pointerEnter != null) {
        var dropZone = pointerData.pointerEnter.GetComponentInParent<IInventoryDropZoneUI>();
        if (dropZone?.PreventItemDropIn == true) {
          Debug.Log("Preventing item drop on restricted drop zone");
          return false;
        }
      }

      // Handle item drop on the ground
      if (MouseData.interfaceMouseIsOver == null) {
        if (!PreventDropOnGround) {
          inventory.SpawnItem(slot.Item, slot.amount);
          slot.RemoveItem();

          Debug.Log("Dropping item on the ground");
          return true;
        }

        return false;
      }

      if (!MouseData.slotHoveredOver) {
        Debug.Log("Slot hovered over is null");
        return false;
      }

      // Retrieve the slot being hovered over
      var targetSlot = (MouseData.interfaceMouseIsOver as IInventoryUI)?.SlotsOnInterface[MouseData.slotHoveredOver];
      if (targetSlot == null) {
        Debug.Log("Target slot is null");
        return false;
      }

      if (targetSlot == slot) {
        Debug.Log("Target slot is the same as the source slot");
        return false;
      }

      var targetInventory = targetSlot.Parent.Inventory;
      var targetUI = targetSlot.Parent;

      // Prevent any item movement if drop is disabled
      if (targetUI.PreventItemDropIn) {
        Debug.Log("Preventing item drop");
        return false;
      }

      //Add split item
      if (!dragFull) {
        inventory.AddItem(slot.Item, dragAmount, targetSlot);
        Debug.Log("Add split item");
        return true;
      }
      
      // Handle merging items
      if (!targetUI.PreventMergeIn && slot.CanMerge(targetSlot)) {
        targetInventory.MergeItems(slot, targetSlot);

        Debug.Log("Merging items");
        return true;
      }

      // Handle swapping items
      // var canSwap = !targetUI.PreventSwapIn && (PreventSwapIn || (!PreventSwapIn && slot.isEmpty));
      var cantSwap = targetUI.PreventSwapIn || (PreventSwapIn && !targetSlot.isEmpty);
      if (cantSwap) {
        return false;
      }

      inventory.SwapSlots(slot, targetSlot);
      Debug.Log("SwapSlots");
      return true;
    }
  }
}