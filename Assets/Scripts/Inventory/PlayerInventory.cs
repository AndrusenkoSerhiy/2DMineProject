using System.Collections.Generic;
using Windows;
using Craft;
using Scriptables.Items;
using UnityEngine;
using SaveSystem;
using Scriptables;

namespace Inventory {
  [DefaultExecutionOrder(-1)]
  public class PlayerInventory : MonoBehaviour, IPlayerInventory, ISaveLoad {
    [SerializeField] private List<InventorySettings> inventoriesSettings;

    private PlayerInventoryWindow inventoryWindow;
    private Dictionary<string, Inventory> inventories = new();
    private Dictionary<InventoryType, InventorySettings> settings = new();
    private InventoriesPool inventoriesPool;

    //Inventories that are used in farm/craft
    public InventoriesPool InventoriesPool => inventoriesPool;

    private List<InventorySettings> GetDefaultInventoriesSettings() {
      return new List<InventorySettings> {
        new() { type = InventoryType.Inventory, size = 24 },
        new() { type = InventoryType.QuickSlots, size = 10 },

        new() { type = InventoryType.StorageSmall, size = 18 },
        new() { type = InventoryType.StorageMid, size = 27 },
        new() { type = InventoryType.StorageBig, size = 36 },

        new() { type = InventoryType.WorkstationOutput, size = 5 },
        new() { type = InventoryType.WorkstationFuel, size = 3 },
        new() { type = InventoryType.RobotInventory, size = 24 },
        new() { type = InventoryType.RobotRepair, size = 5 },
      };
    }

    [ContextMenu("Set default inventories settings")]
    private void SetDefaultInventoriesSettings() {
      inventoriesSettings = GetDefaultInventoriesSettings();
    }

    private void Awake() {
      foreach (var inventorySettings in inventoriesSettings) {
        settings.Add(inventorySettings.type, inventorySettings);
      }

      inventoriesPool = new InventoriesPool();
    }

    private void Start() {
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
    /// <returns>Inventory</returns>
    public Inventory GetInventoryByTypeAndId(InventoryType type, string entityId) {
      if (type == InventoryType.None) {
        return null;
      }

      var fullId = InventoryObject.GenerateId(type, entityId);

      if (inventories.ContainsKey(fullId)) {
        return inventories[fullId];
      }

      var inventoryObject = new InventoryObject(type, fullId);
      inventoryObject.LoadFromGameData();

      var inventory = new Inventory(inventoryObject);
      inventories.Add(fullId, inventory);

      return inventory;
    }

    public int GetInventorySizeByType(InventoryType type) => settings[type].size;
    public Sprite GetInventoryIconByType(InventoryType type) => settings[type].slotIcon;

    public Inventory GetQuickSlots() => GetInventoryByTypeAndId(InventoryType.QuickSlots, "");
    public Inventory GetEquipment() => GetInventoryByTypeAndId(InventoryType.Equipment, "");

    public Inventory GetInventory() => GetInventoryByTypeAndId(InventoryType.Inventory, "");

    private void AddDefaultItemOnFirstStart() {
      var itemAlreadyAdded = SaveLoadSystem.Instance.gameData.DefaultItemAdded;
      var defaultItems = GameManager.Instance.ItemDatabaseObject.DefaultItemsOnStart;

      if (itemAlreadyAdded || defaultItems.Count == 0) {
        return;
      }

      foreach (var item in defaultItems) {
        const int count = 1;
        inventoriesPool.AddItemToInventoriesPool(new Item(item), count);
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

      //if (withAdditional) AddAdditionalItem(item, cellPos);
    }

    //get bonus resource when we are mining
    public void AddAdditionalItem(ResourceData resourceData, Vector3 cellPos) {
      if (resourceData == null) {
        return;
      }

      var list = resourceData.GetBonusResources;
      for (var i = 0; i < list.Count; i++) {
        var currentResource = list[i];
        var rand = Random.value;
        if (rand > currentResource.chance) {
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
      var overflow = inventoriesPool.AddItemToInventoriesPool(item, amount);
      if (overflow <= 0) {
        return amount;
      }

      if (SpawnItem(item, overflow)) {
        return amount - overflow;
      }

      return amount;
    }

    public bool CanAddItemToInventory(ItemObject item) {
      return inventoriesPool.CanAddItem(item);
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
        inventory.MainInventoryObject.SaveToGameData();
      }
    }

    #endregion
  }
}