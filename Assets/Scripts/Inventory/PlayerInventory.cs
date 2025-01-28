using Windows;
using Scriptables.Inventory;
using Scriptables.Items;
using Settings;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Items;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System;

namespace Inventory {
  public class PlayerInventory : MonoBehaviour {
    public InventoryObject inventory;
    public InventoryObject equipment;
    public InventoryObject quickSlots;
    private int defaultItemId = 0;
    private WindowsController windowsController;
    private PlayerInventoryWindow inventoryWindow;
    private SerializedDictionary<int, int> resourcesTotal = new SerializedDictionary<int, int>();
    [NonSerialized]
    public Action<int> onResourcesTotalUpdate;
    public Dictionary<int, int> ResourcesTotal => resourcesTotal;

    private void Start() {
      CheckSlotsUpdate(inventory, true);
      CheckSlotsUpdate(equipment);
      CheckSlotsUpdate(quickSlots);

      inventory.Load();
      equipment.Load();
      quickSlots.Load();

      Item defaultItem = new Item(inventory.database.ItemObjects[defaultItemId]);
      if (!inventory.IsItemInInventory(inventory.database.ItemObjects[defaultItemId])
          && !equipment.IsItemInInventory(inventory.database.ItemObjects[defaultItemId])) {
        Debug.Log("Adding default item to inventory.");
        inventory.AddItem(defaultItem, 1, null, null);
      }

      windowsController = GameManager.instance.WindowsController;
      inventoryWindow = windowsController.GetWindow<PlayerInventoryWindow>();
    }

    private void CheckSlotsUpdate(InventoryObject inventory, bool checkAmount = false) {
      for (int i = 0; i < inventory.GetSlots.Length; i++) {
        inventory.GetSlots[i].onAfterUpdated += SlotUpdateHandler;
        if (checkAmount) {
          inventory.GetSlots[i].onAmountUpdate += SlotAmountUpdateHandler;
        }
      }
    }

    private void SlotAmountUpdateHandler(int resourceId, int amountDelta) {
      UpdateResourceTotal(resourceId, amountDelta);
    }

    private void UpdateResourceTotal(int resourceId, int amount) {
      if (resourcesTotal.ContainsKey(resourceId)) {
        resourcesTotal[resourceId] += amount;

        if (resourcesTotal[resourceId] <= 0) {
          resourcesTotal.Remove(resourceId);
        }
      }
      else if (amount > 0) {
        resourcesTotal[resourceId] = amount;
      }

      onResourcesTotalUpdate?.Invoke(resourceId);

      Debug.Log("PlayerInventory UpdateResourceTotal amount " + amount);
    }

    public int GetResourceTotalAmount(int resourceId) {
      return resourcesTotal.ContainsKey(resourceId) ? resourcesTotal[resourceId] : 0;
    }

    public void AddSlotEvents(GameObject obj, Dictionary<GameObject, InventorySlot> slotsOnInterface, Transform parent) {
      AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnEnter(obj); });
      AddEvent(obj, EventTriggerType.PointerExit, delegate { OnExit(obj); });
      AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnDragStart(obj, slotsOnInterface, parent); });
      AddEvent(obj, EventTriggerType.EndDrag, delegate { OnDragEnd(obj, slotsOnInterface); });
      AddEvent(obj, EventTriggerType.Drag, delegate { OnDrag(obj); });
    }

    public void SlotUpdateHandler(InventorySlot slot) {
      var image = slot.slotDisplay.transform.GetChild(0).GetComponent<Image>();
      var text = slot.slotDisplay.GetComponentInChildren<TextMeshProUGUI>();
      if (slot.item.Id <= -1) {
        image.sprite = null;
        image.color = new Color(1, 1, 1, 0);
        text.text = string.Empty;
      }
      else {
        image.sprite = slot.GetItemObject().UiDisplay;
        image.color = new Color(1, 1, 1, 1);
        text.text = slot.amount == 1 ? string.Empty : slot.amount.ToString("n0");
      }
    }

    public void AddItemToInventory(ItemObject item, int count) {
      inventory.AddItem(new Item(item), count, item, null);
      AddAdditionalItem(item);
    }

    //get bonus resource when we are mining
    private void AddAdditionalItem(ItemObject item) {
      var resource = item as Resource;
      if (resource == null)
        return;

      var list = resource.GetBonusResources;
      for (int i = 0; i < list.Count; i++) {
        if (UnityEngine.Random.value > list[i].chance)
          return;

        var count = UnityEngine.Random.Range((int)list[i].rndCount.x, (int)list[i].rndCount.y);
        //Debug.LogError($"spawn {list[i].item.name} | count {count} ");
        inventory.AddItem(new Item(list[i].item), count, list[i].item, null);
      }
    }

    private void Update() {
      if (UserInput.instance.controls.UI.Inventory.triggered /*&& inventoryPrefab != null*/) {
        UserInput.instance.EnableUIControls(!inventoryWindow.IsShow);
        if (inventoryWindow.IsShow)
          inventoryWindow.Hide();
        else inventoryWindow.Show();
      }
    }

    public void OnApplicationQuit() {
      inventory.Save();
      equipment.Save();
      quickSlots.Save();
      inventory.Clear();
      equipment.Clear();
      quickSlots.Clear();
    }

    public void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action) {
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

    public void SpawnItem(InventorySlot slot) {
      //spawn higher in y pos because need TO DO pick up on action not the trigger enter
      GameObject newObj = Instantiate(GameManager.instance.ItemDatabaseObject.GetByID(slot.item.Id).spawnPrefab, GameManager.instance.PlayerController.transform.position + new Vector3(0, 3, 0), Quaternion.identity);
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

    public void OnDragStart(GameObject obj, Dictionary<GameObject, InventorySlot> slotsOnInterface, Transform parent) {
      MouseData.tempItemBeingDragged = CreateTempItem(obj, slotsOnInterface, parent);
    }

    private GameObject CreateTempItem(GameObject obj, Dictionary<GameObject, InventorySlot> slotsOnInterface, Transform parent) {
      GameObject tempItem = null;
      if (slotsOnInterface[obj].item.Id >= 0) {
        tempItem = new GameObject("TempItemBeingDragged");
        tempItem.layer = 5;
        var rt = tempItem.AddComponent<RectTransform>();

        rt.sizeDelta = new Vector2(80, 80);

        tempItem.transform.SetParent(parent);
        var img = tempItem.AddComponent<Image>();
        img.sprite = slotsOnInterface[obj].GetItemObject().UiDisplay;
        img.raycastTarget = false;

        tempItem.transform.localScale = Vector3.one;
      }
      return tempItem;
    }

    public void OnDragEnd(GameObject obj, Dictionary<GameObject, InventorySlot> slotsOnInterface) {

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
  }
}