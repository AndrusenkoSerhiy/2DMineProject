using UnityEngine;
using Windows;
using Scriptables.Craft;
using World;

namespace Craft {
  public class Crafter : MonoBehaviour {
    [SerializeField] private GameObject interfacePrefab;
    [SerializeField] protected WorkstationObject stationObject;
    [SerializeField] private CellObject cellObject;

    private CraftWindow craftWindow;
    private CraftManager craftManager;
    private Workstation station;
    protected string id;

    public WorkstationObject StationObject => stationObject;
    public Workstation Station => station;
    public string Id => id;

    private void Init() {
      if (craftWindow != null) {
        return;
      }

      GenerateId();
      station = GameManager.Instance.CraftTasks.GetWorkstation(Id, stationObject.Id);
      var craftWindowObj = Instantiate(interfacePrefab, GameManager.Instance.Canvas.transform);
      craftManager = craftWindowObj.GetComponent<CraftManager>();
      craftManager.Setup(station);

      craftWindowObj.transform.SetSiblingIndex(0);

      craftWindow = craftWindowObj.GetComponent<CraftWindow>();
      GameManager.Instance.WindowsController.AddWindow(craftWindow);

      craftWindow.OnHide += OnHideWindowHandler;
      craftWindow.OnShow += OnShowWindowHandler;
    }

    private void OnShowWindowHandler(WindowBase window) {
      GameManager.Instance.CraftTasks.UpdateWindowState(station.Id, true);
    }

    private void OnHideWindowHandler(WindowBase window) {
      GameManager.Instance.CraftTasks.UpdateWindowState(station.Id, false);
    }

    protected void CheckInteract() {
      Init();

      if (!GameManager.Instance.RecipesManager.HasUnlockedRecipesForStation(station.RecipeType)) {
        GameManager.Instance.MessagesManager.ShowSimpleMessage("You don't have any recipes yet.");
        return;
      }

      if (craftWindow.IsShow) {
        craftWindow.Hide();
      }
      else {
        GameManager.Instance.CraftTasks.SetStation(station);
        craftWindow.Show();
      }
    }

    protected virtual void GenerateId() {
      if (cellObject == null) {
        Debug.LogError("Crafter: cellObject is null");
        return;
      }

      id = $"{stationObject.name}_{cellObject.CellData.x}_{cellObject.CellData.y}".ToLower();
    }
  }
}