using System.Collections.Generic;
using Windows;
using Scriptables.Items;
using UnityEngine;
using SaveSystem;
using UnityEngine.Rendering;

namespace Inventory {
  [DefaultExecutionOrder(-1)]
  public class PlayerInventory : MonoBehaviour, IPlayerInventory, ISaveLoad {
    [SerializeField] private SerializedDictionary<InventoryType, int> inventoriesSizes;

    [SerializeField] private SerializedDictionary<StorageType, int> storagesSizes;

    private PlayerInventoryWindow inventoryWindow;
    private Dictionary<string, InventoryObject> inventories = new();

    private SerializedDictionary<InventoryType, int> GetdefaultInventoriesSizes() {
      return new SerializedDictionary<InventoryType, int> {
        { InventoryType.Inventory, 24 },
        { InventoryType.QuickSlots, 10 },
        // { InventoryType.HandCraftOutput, 5 },
        { InventoryType.WorkbenchOutput, 5 },
        { InventoryType.StoneCutterOutput, 5 },
        { InventoryType.FoodStationOutput, 5 },
        { InventoryType.ChemicalStationOutput, 5 },
        { InventoryType.ForgeOutput, 5 },
        { InventoryType.ForgeFuel, 3 },
        { InventoryType.FoodStationFuel, 3 },
        { InventoryType.RobotRepair, 5 },
      };
    }

    private SerializedDictionary<StorageType, int> GetdefaultStoragesSizes() {
      return new SerializedDictionary<StorageType, int> {
        { StorageType.Small, 18 },
        { StorageType.Mid, 27 },
        { StorageType.Big, 36 },
      };
    }

    [ContextMenu("Set default inventories sizes")]
    private void SetDefaultMessagesSettings() {
      inventoriesSizes = GetdefaultInventoriesSizes();
      storagesSizes = GetdefaultStoragesSizes();
    }

    public void Start() {
      inventoryWindow = GameManager.Instance.WindowsController.GetWindow<PlayerInventoryWindow>();
      GameManager.Instance.UserInput.controls.UI.Inventory.performed += ctx => ShowInventory();

      AddDefaultItemOnFirstStart();
    }

    [ContextMenu("Clear inventories")]
    public void ClearInventories() {
      foreach (var (_, inventory) in inventories) {
        inventory.Clear();
      }
    }

    /// <summary>
    /// Gets inventory object by type and id, if null - creates new and try to load data from file
    /// </summary>
    /// <param name="type">Inventory type</param>
    /// <param name="entityId">Inventory id</param>
    /// <returns>Inventory object</returns>
    public InventoryObject GetInventoryByTypeAndId(InventoryType type, string entityId) {
      if (type == InventoryType.None) {
        return null;
      }

      if (type == InventoryType.Storage) {
        Debug.LogError("Set storage to \"storage\"");
        return null;
      }

      var fullId = InventoryObject.GenerateId(type, entityId);

      if (inventories.ContainsKey(fullId)) {
        return inventories[fullId];
      }

      var inventory = new InventoryObject(type, fullId);
      inventory.LoadFromGameData();
      inventories.Add(fullId, inventory);

      return inventory;
    }

    public int GetInventorySizeByType(InventoryType type) => inventoriesSizes[type];

    public InventoryObject GetQuickSlots() => GetInventoryByTypeAndId(InventoryType.QuickSlots, "");

    public InventoryObject GetInventory() => GetInventoryByTypeAndId(InventoryType.Inventory, "");

    public InventoryObject GetStorageById(StorageType storageType, string entityId) {
      if (string.IsNullOrEmpty(entityId)) {
        return null;
      }

      var fullId = InventoryObject.GenerateStorageId(storageType, entityId);

      if (inventories.ContainsKey(fullId)) {
        return inventories[fullId];
      }

      var inventory = new InventoryObject(InventoryType.Storage, fullId, storageType);
      inventory.LoadFromGameData();
      inventories.Add(fullId, inventory);

      return inventory;
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
      if (count != 0) {
        GameManager.Instance.PoolEffects.SpawnFlyEffect(item, cellPos);
      }
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
    /// Adds item to inventory(+quick slots), if no space left then spawn this item on the ground
    /// </summary>
    /// <param name="item">Item to add</param>
    /// <param name="amount">Amount of this item</param>
    /// <returns>Amount of items that was added to inventory</returns>
    public int AddItemToInventoryWithOverflowDrop(Item item, int amount) {
      var overflow = GetInventory().AddItem(item, amount);
      if (overflow <= 0) {
        return amount;
      }

      overflow = GetQuickSlots().AddItem(item, overflow);
      if (overflow <= 0) {
        return amount;
      }

      if (SpawnItem(item, overflow)) {
        return amount - overflow;
      }

      return amount;
    }

    public bool CanAddItemToInventory(ItemObject item) {
      return (GetInventory().CanAddItem(item) || GetQuickSlots().CanAddItem(item));
    }

    public bool SpawnItem(Item item, int amount) {
      if (item.isEmpty || item.info.spawnPrefab == null) {
        return false;
      }

      var groundObj = GameManager.Instance.GroundItemPool.GetItem(item.info);

      if (groundObj == null) {
        GameManager.Instance.MessagesManager.ShowSimpleMessage("Cant spawn item on the ground");
        return false;
      }

      groundObj.transform.position = GameManager.Instance.PlayerController.transform.position + new Vector3(0, 3, 0);
      groundObj.transform.rotation = Quaternion.identity;
      groundObj.Count = amount;

      GameManager.Instance.MessagesManager.ShowDroppedResourceMessage(item.info, amount);
      return true;
    }

    #region Save/Load

    public void Load() {
      return;
    }

    public void Save() {
      foreach (var (_, inventory) in inventories) {
        inventory.SaveToGameData();
      }
    }

    #endregion
  }
}