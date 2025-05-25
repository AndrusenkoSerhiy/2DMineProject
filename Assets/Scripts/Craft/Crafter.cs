using UnityEngine;
using Windows;
using Inventory;
using SaveSystem;
using Scriptables.Craft;
using World;

namespace Craft {
  public class Crafter : MonoBehaviour, ISaveLoad {
    [SerializeField] private GameObject interfacePrefab;
    [SerializeField] protected WorkstationObject stationObject;
    [SerializeField] private BuildingDataObject buildObject;

    protected GameManager gameManager;
    private CraftWindow craftWindow;
    private GameObject craftWindowObj;
    private Window window;
    private Workstation station;
    private string id;

    public WorkstationObject StationObject => stationObject;
    public string Id => id;

    #region Save/Load

    public int Priority => LoadPriority.CRAFT_WINDOWS;

    public void Save() {
    }

    public void Load() {
    }

    public void Clear() {
      craftWindow = null;
      Destroy(craftWindowObj);
    }

    #endregion

    private void Awake() {
      SaveLoadSystem.Instance.Register(this);
      gameManager = GameManager.Instance;
    }

    private void Init() {
      if (craftWindow != null) {
        return;
      }

      station = gameManager.CraftManager.GetWorkstation(GetId(), stationObject.Id);
      craftWindowObj = Instantiate(interfacePrefab, gameManager.Canvas.transform);
      window = craftWindowObj.GetComponent<Window>();
      window.Setup(station);

      craftWindowObj.transform.SetSiblingIndex(0);

      craftWindow = craftWindowObj.GetComponent<CraftWindow>();
      gameManager.WindowsController.AddWindow(craftWindow);

      craftWindow.OnHide += OnHideWindowHandler;
      craftWindow.OnShow += OnShowWindowHandler;
    }

    private void OnShowWindowHandler(WindowBase window) {
      gameManager.CraftManager.UpdateWindowState(station.Id, true);
    }

    private void OnHideWindowHandler(WindowBase window) {
      gameManager.CraftManager.UpdateWindowState(station.Id, false);
    }

    protected void CheckInteract() {
      if (gameManager.MenuController.ActiveMenu != Menu.Menu.None)
        return;
      
      Init();

      if (!gameManager.RecipesManager.HasUnlockedRecipesForStation(station.RecipeType)) {
        gameManager.MessagesManager.ShowSimpleMessage("You don't have any recipes yet.");
        return;
      }

      if (craftWindow.IsShow) {
        craftWindow.Hide();
      }
      else {
        gameManager.CraftManager.SetStation(station);
        craftWindow.Show();
      }
    }

    protected void CheckHoldInteract() {
      var craftingInProgress = station?.CurrentProgress?.IsCrafting ?? false;
      if (craftingInProgress) {
        gameManager.MessagesManager.ShowSimpleMessage("Crafting in progress.");
        return;
      }

      var outputInventory =
        gameManager.PlayerInventory.GetInventoryByTypeAndId(stationObject.OutputInventoryType, GetId());

      if (!outputInventory.IsEmpty()) {
        gameManager.MessagesManager.ShowSimpleMessage("Station output is not empty.");
        return;
      }

      if (stationObject.FuelInventoryType != InventoryType.None) {
        var fuelInventory =
          gameManager.PlayerInventory.GetInventoryByTypeAndId(stationObject.FuelInventoryType, GetId());

        if (!fuelInventory.IsEmpty()) {
          gameManager.MessagesManager.ShowSimpleMessage("Station fuel is not empty.");
          return;
        }
      }

      gameManager.PlayerInventory.TakeBuildingToInventory(buildObject, stationObject.InventoryItem);
    }

    private string GetId() {
      if (string.IsNullOrEmpty(id)) {
        id = Workstation.GenerateId(buildObject, stationObject);
      }

      return id;
    }
  }
}