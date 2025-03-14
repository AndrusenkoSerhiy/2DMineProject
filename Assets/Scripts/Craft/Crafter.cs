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
    private Window window;
    private Workstation station;
    private string id;

    public WorkstationObject StationObject => stationObject;
    public string Id => id;

    private void Init() {
      if (craftWindow != null) {
        return;
      }

      id = Workstation.GenerateId(cellObject, stationObject);
      station = GameManager.Instance.CraftManager.GetWorkstation(id, stationObject.Id);
      var craftWindowObj = Instantiate(interfacePrefab, GameManager.Instance.Canvas.transform);
      window = craftWindowObj.GetComponent<Window>();
      window.Setup(station);

      craftWindowObj.transform.SetSiblingIndex(0);

      craftWindow = craftWindowObj.GetComponent<CraftWindow>();
      GameManager.Instance.WindowsController.AddWindow(craftWindow);

      craftWindow.OnHide += OnHideWindowHandler;
      craftWindow.OnShow += OnShowWindowHandler;
    }

    private void OnShowWindowHandler(WindowBase window) {
      GameManager.Instance.CraftManager.UpdateWindowState(station.Id, true);
    }

    private void OnHideWindowHandler(WindowBase window) {
      GameManager.Instance.CraftManager.UpdateWindowState(station.Id, false);
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
        GameManager.Instance.CraftManager.SetStation(station);
        craftWindow.Show();
      }
    }
  }
}