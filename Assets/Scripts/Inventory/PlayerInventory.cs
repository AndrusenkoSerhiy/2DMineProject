using System.Collections.Generic;
using Windows;
using Items;
using Scriptables.Items;
using UnityEngine;
using SaveSystem;
using UnityEngine.Rendering;

namespace Inventory {
  [DefaultExecutionOrder(-1)]
  public class PlayerInventory : MonoBehaviour, IPlayerInventory, ISaveLoad {
    [SerializeField] private SerializedDictionary<InventoryType, int> inventoriesSizes = new() {
      { InventoryType.Inventory, 24 },
      { InventoryType.QuickSlots, 10 },
      { InventoryType.HandCraftOutput, 5 },
      { InventoryType.WorkbenchOutput, 5 },
      { InventoryType.StoneCutterOutput, 5 },
      { InventoryType.FoodStationOutput, 5 },
      { InventoryType.ChemicalStationOutput, 5 },
      { InventoryType.ForgeOutput, 5 },
      { InventoryType.ForgeFuel, 3 },
    };

    [SerializeField] private SerializedDictionary<StorageType, int> storagesSizes = new() {
      { StorageType.Small, 18 },
    };

    private PlayerInventoryWindow inventoryWindow;
    private Dictionary<InventoryType, InventoryObject> inventories = new();
    private Dictionary<string, InventoryObject> storages = new();

    public void Start() {
      inventoryWindow = GameManager.Instance.WindowsController.GetWindow<PlayerInventoryWindow>();
      GameManager.Instance.UserInput.controls.UI.Inventory.performed += ctx => ShowInventory();

      AddDefaultItemOnFirstStart();
    }

    /// <summary>
    /// Gets inventory object by type, if null - creates new and try to load data from file
    /// </summary>
    /// <param name="type">Inventory type</param>
    /// <returns>Inventory object</returns>
    public InventoryObject GetInventoryByType(InventoryType type) {
      if (type == InventoryType.None) {
        return null;
      }

      if (type == InventoryType.Storage) {
        Debug.LogError("Set storage to \"storage\"");
        return null;
      }

      if (inventories.ContainsKey(type)) {
        return inventories[type];
      }

      var inventory = new InventoryObject(type);
      inventory.LoadFromGameData();
      inventories.Add(type, inventory);

      return inventory;
    }

    public int GetInventorySizeByType(InventoryType type) => inventoriesSizes[type];

    public InventoryObject GetQuickSlots() => GetInventoryByType(InventoryType.QuickSlots);
    public InventoryObject GetInventory() => GetInventoryByType(InventoryType.Inventory);

    public InventoryObject GetStorageById(string id, StorageType storageType) {
      if (string.IsNullOrEmpty(id)) {
        return null;
      }

      if (storages.ContainsKey(id)) {
        return storages[id];
      }

      var inventory = new InventoryObject(InventoryType.Storage, id, storageType);
      inventory.LoadFromGameData();
      storages.Add(inventory.Id, inventory);

      return storages[id];
    }

    public int GetStorageSizeByType(StorageType type) => storagesSizes[type];

    private void AddDefaultItemOnFirstStart() {
      var itemAlreadyAdded = SaveLoadSystem.Instance.gameData.DefaultItemAdded;
      var defaultItems = GetInventory().database.DefaultItemsOnStart;

      if (itemAlreadyAdded || defaultItems.Count == 0) {
        return;
      }

      foreach (var item in defaultItems) {
        const int count = 1;
        GetInventory().AddItem(new Item(item), count);
        // GameManager.Instance.RecipesManager.DiscoverMaterial(item);
        // GameManager.Instance.MessagesManager.ShowAddResourceMessage(item, count);
      }

      SaveLoadSystem.Instance.gameData.DefaultItemAdded = true;
    }

    private void ShowInventory() {
      if (inventoryWindow.IsShow) {
        inventoryWindow.Hide();
      }
      else {
        inventoryWindow.Show();
      }
    }

    public void AddItemToInventory(ItemObject item, int count, Vector3 cellPos) {
      var addedAmount = AddItemToInventoryWithOverflowDrop(new Item(item), count);

      // GameManager.Instance.RecipesManager.DiscoverMaterial(item);
      GameManager.Instance.MessagesManager.ShowAddResourceMessage(item, addedAmount);

      //ObjectPooler.Instance.SpawnFlyEffect(item, cellPos);
      GameManager.Instance.PoolEffects.SpawnFlyEffect(item, cellPos);
      AddAdditionalItem(item, cellPos);
    }

    //get bonus resource when we are mining
    private void AddAdditionalItem(ItemObject item, Vector3 cellPos) {
      var resource = item as Resource;
      if (resource == null) {
        return;
      }

      var list = resource.GetBonusResources;
      for (var i = 0; i < list.Count; i++) {
        var currentResource = list[i];
        if (Random.value > currentResource.chance) {
          return;
        }

        var count = Random.Range((int)currentResource.rndCount.x, (int)currentResource.rndCount.y);
        var addedAmount = AddItemToInventoryWithOverflowDrop(new Item(currentResource.item), count);

        // GameManager.Instance.RecipesManager.DiscoverMaterial(currentResource.item);
        GameManager.Instance.MessagesManager.ShowAddResourceMessage(currentResource.item, addedAmount);

        //ObjectPooler.Instance.SpawnFlyEffect(currentResource.item, cellPos);
        GameManager.Instance.PoolEffects.SpawnFlyEffect(currentResource.item, cellPos);
      }
    }

    /// <summary>
    /// Adds item to inventory, if no space left then spawn this item on the ground
    /// </summary>
    /// <param name="item">Item to add</param>
    /// <param name="amount">Amount of this item</param>
    /// <returns>Amount of items that was added to inventory</returns>
    public int AddItemToInventoryWithOverflowDrop(Item item, int amount) {
      var overflow = GetInventory().AddItem(item, amount);
      if (overflow <= 0) {
        return amount;
      }

      SpawnItem(item, overflow);

      return amount - overflow;
    }

    //TODO remove after time, maybe add pool
    public void SpawnItem(Item item, int amount) {
      if (item.isEmpty || item.info.spawnPrefab == null) {
        return;
      }

      //spawn higher in y pos because need TO DO pick up on action not the trigger enter
      /*var newObj = Instantiate(item.info.spawnPrefab,
        GameManager.Instance.PlayerController.transform.position + new Vector3(0, 3, 0), Quaternion.identity);
      var groundObj = newObj.GetComponent<GroundItem>();*/
      var groundObj = GameManager.Instance.GroundItemPool.GetItem(item.info);
      groundObj.transform.position = GameManager.Instance.PlayerController.transform.position + new Vector3(0, 3, 0);
      groundObj.transform.rotation = Quaternion.identity;
      groundObj.Count = amount;

      GameManager.Instance.MessagesManager.ShowDroppedResourceMessage(item.info, amount);
    }

    #region Save/Load

    public void Load() {
      return;
    }

    public void Save() {
      foreach (var (_, inventory) in inventories) {
        inventory.SaveToGameData();
      }

      foreach (var (_, storage) in storages) {
        storage.SaveToGameData();
      }
    }

    #endregion
  }
}