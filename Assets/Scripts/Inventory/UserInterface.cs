using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Inventory {
  [RequireComponent(typeof(EventTrigger))]
  public class UserInterface : MonoBehaviour, IInventoryUI {
    [SerializeField] private InventoryType inventoryType;
    [SerializeField] private InventoryType fastDropInventoryType;

    // [SerializeField] private Color disabledSlotColor;
    [SerializeField] private bool preventItemDropIn;
    [SerializeField] private bool preventDropOnGround;
    [SerializeField] private bool preventSwapIn;
    [SerializeField] private bool preventMergeIn;
    [SerializeField] private bool preventSplit;
    [SerializeField] private bool showTooltips;

    private GameManager gameManager;
    private Inventory inventory;
    private Inventory fastDropInventory;
    private string inventoryId;
    private string fastDropInventoryId;

    private SplitItem splitItem;
    private GameObject tempDragItemObject;
    private TempDragItem tempDragItem;
    private Transform tempDragParent;
    private Dictionary<GameObject, InventorySlot> slotsOnInterface;
    private int activeSlotsCount;

    public event Action OnLoaded;
    public event Action OnDisabled;

    // public GameObject[] slotsPrefabs;
    public SlotDisplay[] slotsPrefabs;
    public Inventory Inventory => inventory;
    public Dictionary<GameObject, InventorySlot> SlotsOnInterface => slotsOnInterface;

    public bool PreventItemDropIn => preventItemDropIn;
    public bool PreventDropOnGround => preventDropOnGround;
    public bool PreventSwapIn => preventSwapIn;
    public bool PreventMergeIn => preventMergeIn;
    public string InventoryId => inventoryId;

    public void Setup(InventoryType type, string id) {
      inventoryType = type;
      inventoryId = id;
    }

    public void SetupFastDrop(InventoryType type, string id) {
      fastDropInventoryId = id;
      fastDropInventoryType = type;
    }

    public void Awake() {
      if (inventoryType == InventoryType.None) {
        Debug.LogError("Inventory type is none");
        return;
      }

      gameManager = GameManager.Instance;
      splitItem = gameManager.SplitItem;
      tempDragItemObject = gameManager.TempDragItem;
      tempDragItem = tempDragItemObject.GetComponent<TempDragItem>();
      tempDragParent = gameManager.Canvas.transform;
      slotsOnInterface = new Dictionary<GameObject, InventorySlot>();
    }

    private void Init() {
      inventory = gameManager.PlayerInventory.GetInventoryByTypeAndId(inventoryType, inventoryId);
      fastDropInventory =
        gameManager.PlayerInventory.GetInventoryByTypeAndId(fastDropInventoryType, fastDropInventoryId);
    }

    public void OnEnable() {
      Init();

      AddSlotsUpdateEvents();
      CreateSlots();
      // UpdateSlotsGameObjects();

      AddEvent(gameObject, EventTriggerType.PointerEnter, delegate { OnEnterInterface(gameObject); });
      AddEvent(gameObject, EventTriggerType.PointerExit, delegate { OnExitInterface(gameObject); });

      AddSlotsEvents();

      UpdateInventoryUI();

      OnLoaded?.Invoke();
      inventory.OnResorted += ResortHandler;
      inventory.OnSlotsCountChanged += SlotsCountChangedHandler;
    }

    public void OnDisable() {
      RemoveSlotsUpdateEvents();
      RemoveAllEvents(gameObject);
      RemoveAllEvents(gameObject);

      RemoveSlotsEvents();

      inventory.OnSlotsCountChanged -= SlotsCountChangedHandler;
      inventory.OnResorted -= ResortHandler;

      HideTooltip();
      OnDisabled?.Invoke();
    }

    public void CreateSlots() {
      if (slotsPrefabs.Length < Inventory.Slots.Length) {
        Debug.LogError("Not enough slots in the interface");
        return;
      }

      if (Inventory.Slots.Length == activeSlotsCount) {
        return;
      }

      activeSlotsCount = 0;

      for (var i = 0; i < slotsPrefabs.Length; i++) {
        var slotPrefab = slotsPrefabs[i];
        var obj = slotPrefab.gameObject;

        if (i > Inventory.Slots.Length - 1) {
          slotPrefab.Disable();
          slotsOnInterface.Remove(obj);
          continue;
        }

        if (!obj.activeSelf) {
          obj.SetActive(true);
          activeSlotsCount++;
        }

        var slot = Inventory.Slots[i];

        if (!slotsOnInterface.ContainsKey(obj)) {
          slotsOnInterface.Add(obj, slot);
        }

        if ((UserInterface)slot.Parent == this) {
          continue;
        }

        slot.SetParent(this);
        slot.SetSlotDisplay(slotPrefab);
        slotsOnInterface[obj] = slot;
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

    private void SlotsCountChangedHandler() {
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

    public void UpdateInventoryUI() {
      for (var i = 0; i < Inventory.Slots.Length; i++) {
        var slot = Inventory.Slots[i];
        UpdateSlotDisplay(slot);
      }
    }

    private void UpdateSlotHandler(SlotUpdateEventData data) {
      UpdateSlotDisplay(data.after);
    }

    public void UpdateSlotDisplay(InventorySlot slot) {
      var isMainInventorySlot = slot.InventoryId == inventory.Id;
      var slotDisplay = slot.SlotDisplay;

      slotDisplay.UpdateUI(slot);

      if (!isMainInventorySlot) {
        slotDisplay.SetTypeIcon(gameManager.PlayerInventory.GetInventoryIconByType(slot.InventoryObjectType));
      }
      else {
        slotDisplay.ClearTypeIcon();
      }

      /*if (slot.Item.info == null || slot.amount <= 0) {
        slotDisplay.ClearText();
        slotDisplay.ClearBackground();
      }
      else {
        var text = slot.amount == 1 ? string.Empty : slot.amount.ToString("n0");
        slotDisplay.SetBackground(slot.Item.info.UiDisplay);
        slotDisplay.SetText(text);
      }*/
    }

    private void AddSlotsUpdateEvents() {
      for (var i = 0; i < inventory.Slots.Length; i++) {
        inventory.Slots[i].OnAfterUpdated += UpdateSlotHandler;
      }
    }

    private void RemoveSlotsUpdateEvents() {
      for (var i = 0; i < inventory.Slots.Length; i++) {
        inventory.Slots[i].OnAfterUpdated -= UpdateSlotHandler;
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
      if (gameManager.UserInput.controls.UI.Ctrl.IsPressed() && fastDropInventory != null) {
        if (!slot.Item.info.CanMoveToAnotherInventory) {
          return;
        }

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
      if (gameManager.UserInput.controls.UI.Shift.IsPressed() && !preventSplit) {
        splitItem.Show(slot, obj, tempDragParent);
        HideTooltip();
      }
    }

    private void OnEnter(GameObject obj) {
      MouseData.slotHoveredOver = obj;

      ShowTooltip(obj);
    }

    private void OnEnterInterface(GameObject obj) {
      MouseData.interfaceMouseIsOver = obj.GetComponent<IInventoryDropZoneUI>();
    }

    private void OnExitInterface(GameObject obj) {
      MouseData.interfaceMouseIsOver = null;
    }

    private void OnExit(GameObject obj) {
      MouseData.slotHoveredOver = null;

      HideTooltip();
    }

    private void ShowTooltip(GameObject obj) {
      if (!showTooltips) {
        return;
      }

      var slot = slotsOnInterface[obj];
      if (slot.isEmpty || string.IsNullOrEmpty(slot.Item.info.Description)) {
        return;
      }

      gameManager.TooltipManager.Show(slot.Item.info.Description, slot.Item.info.Name);
    }

    private void HideTooltip() {
      if (showTooltips) {
        gameManager.TooltipManager.Hide();
      }
    }

    private void OnDrag(GameObject obj) {
      if (!tempDragItem.IsDrag) {
        return;
      }

      var mousePos = gameManager.UserInput.GetMousePosition();
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
        if (PreventDropOnGround || !slot.CanDrop) {
          gameManager.MessagesManager.ShowSimpleMessage("Can`t drop this item on the ground.");
          return false;
        }

        if (!gameManager.PlayerInventory.SpawnItem(slot.Item, slot.amount)) {
          return false;
        }

        slot.RemoveItem();
        Debug.Log("Dropping item on the ground");
        return true;
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

      if (!targetSlot.SlotDisplay.IsAllowedItem(slot.Item.info) ||
          !slot.SlotDisplay.IsAllowedItem(targetSlot.Item.info)) {
        gameManager.MessagesManager.ShowSimpleMessage("Item not allowed.");
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
        Debug.Log("Merging items");
        return targetInventory.MergeItems(slot, targetSlot);
      }

      // Handle swapping items
      // Check UI params
      var cantSwap = targetUI.PreventSwapIn || (PreventSwapIn && !targetSlot.isEmpty);
      if (cantSwap) {
        return false;
      }

      // Check item params for swap
      if (targetUI != slot.Parent && (!slot.CanMoveToAnotherInventory || !targetSlot.CanMoveToAnotherInventory)) {
        return false;
      }

      Debug.Log("SwapSlots");
      return inventory.SwapSlots(slot, targetSlot);
    }
  }
}