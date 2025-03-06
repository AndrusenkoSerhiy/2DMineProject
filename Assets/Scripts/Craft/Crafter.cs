using UnityEngine;
using Windows;
using Scriptables.Craft;

namespace Craft {
  public class Crafter : MonoBehaviour {
    [SerializeField] private GameObject overlayPrefab;
    [SerializeField] private GameObject interfacePrefab;
    [SerializeField] protected Workstation station;

    private CraftWindow craftWindow;
    private CraftManager craftManager;

    private void Init() {
      if (craftWindow != null) {
        return;
      }

      var craftWindowObj = Instantiate(interfacePrefab, overlayPrefab.transform);
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
        //TODO move to station plant
        GameManager.Instance.CraftTasks.SetStation(station);
        
        craftWindow.Show();
      }
    }
  }
}