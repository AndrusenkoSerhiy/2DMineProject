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

    public void Awake() {
      Debug.Log("Player inventory awake");

      SetInventoryByType(InventoryType.Inventory, new InventoryObject(InventoryType.Inventory));
      SetInventoryByType(InventoryType.QuickSlots, new InventoryObject(InventoryType.QuickSlots));

      Load();
    }

    public void Start() {
      inventoryWindow = GameManager.Instance.WindowsController.GetWindow<PlayerInventoryWindow>();
      GameManager.Instance.UserInput.controls.UI.Inventory.performed += ctx => ShowInventory();

      AddDefaultItemOnFirstStart();
    }

    public InventoryObject GetInventoryByType(InventoryType type) {
      if (type == InventoryType.None || !inventories.ContainsKey(type)) {
        return null;
      }

      return inventories[type];
    }

    public int GetInventorySizeByType(InventoryType type) => inventoriesSizes[type];

    public InventoryObject GetQuickSlots() => inventories[InventoryType.QuickSlots];
    public InventoryObject GetInventory() => inventories[InventoryType.Inventory];

    public void SetInventoryByType(InventoryType type, InventoryObject inventory) {
      if (type == InventoryType.None) {
        Debug.LogError("Inventory type is none");
        return;
      }

      if (type == InventoryType.Storage) {
        Debug.LogError("Set storage to \"storage\"");
        return;
      }

      if (inventories.ContainsKey(type)) {
        return;
      }

      inventories.Add(type, inventory);
    }

    public InventoryObject GetStorageById(string id) {
      if (string.IsNullOrEmpty(id) || !storages.ContainsKey(id)) {
        return null;
      }

      return storages[id];
    }

    public int GetStorageSizeByType(StorageType type) => storagesSizes[type];

    public void SetStorage(InventoryObject inventory) {
      if (inventory == null || string.IsNullOrEmpty(inventory.Id)) {
        Debug.LogError("Storage id is empty");
        return;
      }

      if (storages.ContainsKey(inventory.Id)) {
        return;
      }

      storages.Add(inventory.Id, inventory);
    }

    private void AddDefaultItemOnFirstStart() {
      var itemAlreadyAdded = SaveLoadSystem.Instance.gameData.DefaultItemAdded;
      var defaultItems = GetInventory().database.DefaultItemsOnStart;

      if (itemAlreadyAdded || defaultItems.Count == 0) {
        return;
      }

      foreach (var item in defaultItems) {
        const int count = 1;
        GetInventory().AddItem(new Item(item), count);
        GameManager.Instance.RecipesManager.DiscoverMaterial(item);
        GameManager.Instance.MessagesManager.ShowAddResourceMessage(item, count);
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
      GetInventory().AddItem(new Item(item), count);

      GameManager.Instance.RecipesManager.DiscoverMaterial(item);
      GameManager.Instance.MessagesManager.ShowAddResourceMessage(item, count);

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
        GetInventory().AddItem(new Item(currentResource.item), count);

        GameManager.Instance.RecipesManager.DiscoverMaterial(currentResource.item);
        GameManager.Instance.MessagesManager.ShowAddResourceMessage(currentResource.item, count);

        //ObjectPooler.Instance.SpawnFlyEffect(currentResource.item, cellPos);
        GameManager.Instance.PoolEffects.SpawnFlyEffect(currentResource.item, cellPos);
      }
    }

    public void SpawnItem(Item item, int amount) {
      if (item == null) {
        return;
      }

      //spawn higher in y pos because need TO DO pick up on action not the trigger enter
      var newObj = Instantiate(item.info.spawnPrefab,
        GameManager.Instance.PlayerController.transform.position + new Vector3(0, 3, 0), Quaternion.identity);
      var groundObj = newObj.GetComponent<GroundItem>();
      groundObj.Count = amount;
    }

    #region Save/Load

    public string Id => GetInventory().type.ToString();

    public void Load() {
      GetInventory().LoadFromGameData();
    }

    public void Save() {
      GetInventory().SaveToGameData();
    }

    #endregion
  }
}