using System.Collections.Generic;
using Windows;
using Craft;
using Scriptables.Items;
using UnityEngine;
using SaveSystem;
using Scriptables;
using World;

namespace Inventory {
  [DefaultExecutionOrder(-1)]
  public class PlayerInventory : MonoBehaviour, IPlayerInventory, ISaveLoad {
    [SerializeField] private List<InventorySettings> inventoriesSettings;

    private PlayerInventoryWindow inventoryWindow;
    private Dictionary<string, Inventory> inventories = new();
    private Dictionary<InventoryType, InventorySettings> settings = new();
    private InventoriesPool inventoriesPool;
    private GameManager gameManager;
    private SaveLoadSystem saveLoadSystem;

    private List<string> weightItems = new();
    private float weight = 0f;

    //Inventories that are used in farm/craft
    public InventoriesPool InventoriesPool => inventoriesPool;
    public float Weight => weight;

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
      saveLoadSystem = SaveLoadSystem.Instance;
      saveLoadSystem.Register(this);
      gameManager = GameManager.Instance;

      foreach (var inventorySettings in inventoriesSettings) {
        settings.Add(inventorySettings.type, inventorySettings);
      }
    }

    private void Start() {
      inventoryWindow = gameManager.WindowsController.GetWindow<PlayerInventoryWindow>();
      gameManager.UserInput.controls.UI.Inventory.performed += ctx => ShowInventory();

      if (!GameManager.Instance.InitScriptsOnStart()) {
        return;
      }

      Init();
    }

    private void Init() {
      inventoriesPool = new InventoriesPool();
      AddDefaultItemOnFirstStart();
    }

    #region Save/Load

    public int Priority => LoadPriority.INVENTORIES;

    public void Load() {
      Clear();
      
      weight = saveLoadSystem.gameData.Weight;
      weightItems = saveLoadSystem.gameData.WeightItems;
      Init();
    }

    public void Save() {
      foreach (var (_, inventory) in inventories) {
        inventory.MainInventoryObject.SaveToGameData();
      }

      var data = saveLoadSystem.gameData;
      data.Weight = weight;
      data.WeightItems = weightItems;
    }

    public void Clear() {
      inventories.Clear();
      weightItems.Clear();
      weight = 0f;
      inventoriesPool = null;
    }

    #endregion

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
      var defaultItems = gameManager.ItemDatabaseObject.DefaultItemsOnStart;

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
        //TODO 
        //need to replace this condition
        var activeWindow = gameManager.WindowsController.WindowsList.Find(e => e.IsShow);
        if (activeWindow && activeWindow.name.Equals("RespawnWindow") ||
            gameManager.MenuController.ActiveMenu != Menu.Menu.None)
          return;

        inventoryWindow.Show();
      }
    }

    public void AddItemToInventory(ItemObject item, int count, Vector3 cellPos) {
      var addedAmount = AddItemToInventoryWithOverflowDrop(new Item(item), count);

      // gameManager.RecipesManager.DiscoverMaterial(item);
      gameManager.MessagesManager.ShowAddResourceMessage(item, addedAmount);

      //ObjectPooler.Instance.SpawnFlyEffect(item, cellPos);
      if (count != 0) {
        gameManager.PoolEffects.SpawnFlyEffect(item, cellPos);
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
          continue;
        }

        var count = Random.Range((int)currentResource.rndCount.x, (int)currentResource.rndCount.y);
        var addedAmount = AddItemToInventoryWithOverflowDrop(new Item(currentResource.item), count);

        // gameManager.RecipesManager.DiscoverMaterial(currentResource.item);
        gameManager.MessagesManager.ShowAddResourceMessage(currentResource.item, addedAmount);

        //ObjectPooler.Instance.SpawnFlyEffect(currentResource.item, cellPos);
        gameManager.PoolEffects.SpawnFlyEffect(currentResource.item, cellPos);
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

      AddWeight(item.info);

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

      var groundObj = gameManager.GroundItemPool.GetItem(item.info);

      if (groundObj == null) {
        gameManager.MessagesManager.ShowSimpleMessage("Cant spawn item on the ground");
        return false;
      }

      groundObj.transform.position = gameManager.PlayerController.transform.position + new Vector3(0, 3, 0);
      groundObj.transform.rotation = Quaternion.identity;
      groundObj.Count = amount;

      gameManager.MessagesManager.ShowDroppedResourceMessage(item.info, amount);
      return true;
    }

    public int Repair(float max, float currentValue, int repairCost) {
      if (!InventoriesPool.HasRepairKits()) {
        gameManager.MessagesManager.ShowSimpleMessage("You don't have repair kits.");
        return 0;
      }

      var valuePerKit = max / repairCost;
      var kitsNeeded = Mathf.CeilToInt((max - currentValue) / valuePerKit);

      var remainingKits = InventoriesPool.UseRepairKits(kitsNeeded);
      var kitsUsed = kitsNeeded - remainingKits;
      var repairValue = Mathf.CeilToInt(kitsUsed * valuePerKit);

      return repairValue;
    }

    public bool TakeBuildingToInventory(BuildingDataObject buildObject, ItemObject itemObject) {
      if (!itemObject || !buildObject) {
        return false;
      }

      if (!InventoriesPool.CanAddItem(itemObject)) {
        gameManager.MessagesManager.ShowSimpleMessage("Inventory is full");
        return false;
      }

      if (!gameManager.PlaceCell.RemoveBuilding(buildObject)) {
        return false;
      }

      var amountLeft = InventoriesPool.AddItemToInventoriesPool(new Item(itemObject), 1);

      return amountLeft == 0;
    }

    private void AddWeight(ItemObject itemObject) {
      if (!itemObject || itemObject.Weight <= 0f) {
        return;
      }

      if (weightItems.Contains(itemObject.Id)) {
        return;
      }

      weightItems.Add(itemObject.Id);
      weight += itemObject.Weight;
    }
  }
}