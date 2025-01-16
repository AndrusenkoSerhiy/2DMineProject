using System.Collections.Generic;
using System.Linq;
using Items;
using Scriptables.Inventory;
using Scriptables.Items;
using Settings;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(EventTrigger))]
public abstract class UserInterface : MonoBehaviour {
  public InventoryObject inventory;
  private InventoryObject _previousInventory;
  public Dictionary<GameObject, InventorySlot> slotsOnInterface = new Dictionary<GameObject, InventorySlot>();
  private bool _enabled = false;
  [SerializeField] private Camera _uiCamera;
  [SerializeField] private Canvas _canvas;
  [SerializeField] private Transform _tempDragParent;

  public void OnEnable() {
    if (_enabled) {
      return;
    }
    _enabled = true;
    // public void Start() {
    CreateSlots();

    for (int i = 0; i < inventory.GetSlots.Length; i++) {
      inventory.GetSlots[i].parent = this;
      inventory.GetSlots[i].onAfterUpdated += OnSlotUpdate;
    }
    AddEvent(gameObject, EventTriggerType.PointerEnter, delegate { OnEnterInterface(gameObject); });
    AddEvent(gameObject, EventTriggerType.PointerExit, delegate { OnExitInterface(gameObject); });

    // Update the entire UI when enabling the interface (e.g., after loading inventory)
    UpdateInventoryUI();
  }

  public abstract void CreateSlots();

  public void UpdateInventoryLinks() {
    int i = 0;
    foreach (var key in slotsOnInterface.Keys.ToList()) {
      slotsOnInterface[key] = inventory.GetSlots[i];
      i++;
    }
  }

  public void OnSlotUpdate(InventorySlot slot) {
    if (slot.item.Id <= -1) {
      slot.slotDisplay.transform.GetChild(0).GetComponent<Image>().sprite = null;
      slot.slotDisplay.transform.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 0);
      slot.slotDisplay.GetComponentInChildren<TextMeshProUGUI>().text = string.Empty;
    }
    else {
      slot.slotDisplay.transform.GetChild(0).GetComponent<Image>().sprite = slot.GetItemObject().UiDisplay;
      slot.slotDisplay.transform.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 1);
      slot.slotDisplay.GetComponentInChildren<TextMeshProUGUI>().text = slot.amount == 1 ? string.Empty : slot.amount.ToString("n0");
    }
  }

  // Updates all inventory slots at once
  public void UpdateInventoryUI() {
    foreach (var slot in inventory.GetSlots) {
      OnSlotUpdate(slot); // Ensure each slot reflects the correct UI state
    }
  }

  public void Update() {
    if (_previousInventory != inventory) {
      UpdateInventoryLinks();
    }
    _previousInventory = inventory;

  }

  protected void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action) {
    EventTrigger trigger = obj.GetComponent<EventTrigger>();
    if (!trigger) { Debug.LogWarning("No EventTrigger component found!"); return; }
    var eventTrigger = new EventTrigger.Entry { eventID = type };
    eventTrigger.callback.AddListener(action);
    trigger.triggers.Add(eventTrigger);
  }

  public void OnEnter(GameObject obj) {
    MouseData.slotHoveredOver = obj;
  }

  public void OnEnterInterface(GameObject obj) {
    MouseData.interfaceMouseIsOver = obj.GetComponent<UserInterface>();
  }

  public void OnExitInterface(GameObject obj) {
    MouseData.interfaceMouseIsOver = null;
  }

  public void OnExit(GameObject obj) {
    MouseData.slotHoveredOver = null;
  }

  public void OnDragStart(GameObject obj) {
    MouseData.tempItemBeingDragged = CreateTempItem(obj);
  }

  private GameObject CreateTempItem(GameObject obj) {
    GameObject tempItem = null;
    if (slotsOnInterface[obj].item.Id >= 0) {
      tempItem = new GameObject("TempItemBeingDragged");
      tempItem.layer = 5;
      var rt = tempItem.AddComponent<RectTransform>();

      rt.sizeDelta = new Vector2(80, 80);

      tempItem.transform.SetParent(_tempDragParent);//transform.parent.parent
      var img = tempItem.AddComponent<Image>();
      img.sprite = slotsOnInterface[obj].GetItemObject().UiDisplay;
      img.raycastTarget = false;

      tempItem.transform.localScale = Vector3.one;
    }
    return tempItem;
  }

  public void OnDragEnd(GameObject obj) {

    Destroy(MouseData.tempItemBeingDragged);

    if (MouseData.interfaceMouseIsOver == null) {
      SpawnItem(slotsOnInterface[obj]);
      slotsOnInterface[obj].RemoveItem();
      Debug.Log($"need to spawn item on ground");
      return;
    }
    if (MouseData.slotHoveredOver) {
      InventorySlot mouseHoverSlotData = MouseData.interfaceMouseIsOver.slotsOnInterface[MouseData.slotHoveredOver];
      inventory.SwapItems(slotsOnInterface[obj], mouseHoverSlotData);
    }
  }

  private void SpawnItem(InventorySlot slot){
    //spawn higher in y pos because need TO DO pick up on action not the trigger enter
    GameObject newObj = Instantiate(((Resource)GameManager.instance.ItemDatabaseObject.GetByID(slot.item.Id)).spawnPrefab, GameManager.instance.PlayerController.transform.position + new Vector3(0,5,0), Quaternion.identity);
    var groundObj = newObj.GetComponent<GroundItem>();
    groundObj.Count = slot.amount;
  }

  public void OnDrag(GameObject obj) {
    if (MouseData.tempItemBeingDragged != null) {
      var mousePos = UserInput.instance.GetMousePosition();//_uiCamera.ScreenToWorldPoint(Input.mousePosition);
      mousePos.z = 0;
      MouseData.tempItemBeingDragged.GetComponent<RectTransform>().position = mousePos;

    }
  }
}