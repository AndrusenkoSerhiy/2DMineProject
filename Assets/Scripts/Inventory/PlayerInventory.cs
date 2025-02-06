using Windows;
using Scriptables.Inventory;
using Scriptables.Items;
using Settings;
using UnityEngine;
using Items;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System;

namespace Inventory {
  public class PlayerInventory : MonoBehaviour {
    public InventoryObject inventory;
    //public InventoryObject equipment;
    public InventoryObject quickSlots;
    [SerializeField] private ItemObject defaultItem;
    // private WindowsController windowsController;
    private SerializedDictionary<int, int> resourcesTotal = new SerializedDictionary<int, int>();
    [NonSerialized]
    public Action<int> onResourcesTotalUpdate;

    public Action OnQuickSlotLoaded;
    public Dictionary<int, int> ResourcesTotal => resourcesTotal;

    [SerializeField] private GameObject inventoryOverlayPrefab;
    [SerializeField] private GameObject inventoryInterfacePrefab;
    private PlayerInventoryWindow inventoryWindow;

    public void Start() {
      CheckSlotsAmountUpdate(inventory);

      inventory.Load();

      quickSlots.Load();
      OnQuickSlotLoaded?.Invoke();
    }

    public void Update() {
      if (UserInput.instance.controls.UI.Inventory.triggered /*&& inventoryPrefab != null*/) {
        InitInventoryWindow();
        UserInput.instance.EnableUIControls(!inventoryWindow.IsShow);
        if (inventoryWindow.IsShow) {
          inventoryWindow.Hide();
        }
        else {
          inventoryWindow.Show();
        }
      }
    }

    public void OnApplicationQuit() {
      inventory.Save();
      //equipment.Save();
      quickSlots.Save();
      inventory.Clear();
      //equipment.Clear();
      quickSlots.Clear();
    }

    public void CheckSlotsAmountUpdate(InventoryObject inventory) {
      for (int i = 0; i < inventory.GetSlots.Length; i++) {
        inventory.GetSlots[i].onAmountUpdate += SlotAmountUpdateHandler;
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

    private void AddDefaultItem() {
      if (defaultItem == null) {
        return;
      }

      inventory.AddDefaultItem(defaultItem);
    }

    private void InitInventoryWindow() {
      if (inventoryWindow != null) {
        return;
      }

      AddDefaultItem();
      // CheckSlotsUpdate(inventory);

      inventoryWindow = Instantiate(inventoryInterfacePrefab, inventoryOverlayPrefab.transform).GetComponent<PlayerInventoryWindow>();
      GameManager.instance.WindowsController.AddWindow(inventoryWindow);
    }

    public void SpawnItem(InventorySlot slot) {
      //spawn higher in y pos because need TO DO pick up on action not the trigger enter
      GameObject newObj = Instantiate(GameManager.instance.ItemDatabaseObject.GetByID(slot.item.Id).spawnPrefab, GameManager.instance.PlayerController.transform.position + new Vector3(0, 3, 0), Quaternion.identity);
      var groundObj = newObj.GetComponent<GroundItem>();
      groundObj.Count = slot.amount;
    }
  }
}