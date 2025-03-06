using Windows;
using Scriptables.Inventory;
using Scriptables.Items;
using UnityEngine;
using Pool;
using SaveSystem;

namespace Inventory {
  public class PlayerInventory : MonoBehaviour, IPlayerInventory, ISaveLoad {
    public InventoryObject inventory;
    public InventoryObject quickSlots;

    private PlayerInventoryWindow inventoryWindow;

    public void Awake() {
      Load();
    }

    public void Start() {
      inventoryWindow = GameManager.Instance.WindowsController.GetWindow<PlayerInventoryWindow>();
      GameManager.Instance.UserInput.controls.UI.Inventory.performed += ctx => ShowInventory();

      AddDefaultItemOnFirstStart();
    }

    private void AddDefaultItemOnFirstStart() {
      var itemAlreadyAdded = SaveLoadSystem.Instance.gameData.DefaultItemAdded;
      var defaultItems = inventory.database.DefaultItemsOnStart;

      if (itemAlreadyAdded || defaultItems.Count == 0) {
        return;
      }

      foreach (var item in defaultItems) {
        const int count = 1;
        inventory.AddItem(new Item(item), count);
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
      inventory.AddItem(new Item(item), count);
      
      GameManager.Instance.RecipesManager.DiscoverMaterial(item);
      GameManager.Instance.MessagesManager.ShowAddResourceMessage(item, count);
      
      ObjectPooler.Instance.SpawnFlyEffect(item, cellPos);
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
        inventory.AddItem(new Item(currentResource.item), count);
        
        GameManager.Instance.RecipesManager.DiscoverMaterial(currentResource.item);
        GameManager.Instance.MessagesManager.ShowAddResourceMessage(currentResource.item, count);
        
        ObjectPooler.Instance.SpawnFlyEffect(currentResource.item, cellPos);
      }
    }

    #region Save/Load

    public string Id => inventory.type.ToString();

    public void Load() {
      inventory.LoadFromGameData();
    }

    public void Save() {
      inventory.SaveToGameData();
    }

    #endregion
  }
}