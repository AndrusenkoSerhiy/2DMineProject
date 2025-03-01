using System.Collections.Generic;
using Inventory;
using Scriptables.Craft;
using UnityEngine;
using UnityServiceLocator;

namespace Craft {
  public class FuelItems : MonoBehaviour, IFuelItems {
    [SerializeField] private UserInterface fuelInterface;
    [SerializeField] private List<FuelItem> items;

    private Workstation station;

    public void Awake() {
      station = ServiceLocator.For(this).Get<Workstation>();

      if (station.FuelInventory == null) {
        Debug.LogError("Set FuelInventory to the station");
        return;
      }

      fuelInterface.Setup(station.FuelInventory);
      ServiceLocator.For(this).Register<IFuelItems>(this);
    }

    public void ConsumeFuel(Recipe recipe, int count) {
      station.ConsumeFuel(recipe, count);

      BlockUnblockItems();
    }

    public void InitComponent() {
      BlockUnblockItems();
      station.OnCraftStarted += CraftStartedHandler;
      station.OnCraftStopped += CraftStoppedHandler;
    }

    public void ClearComponent() {
      station.OnCraftStarted -= CraftStartedHandler;
      station.OnCraftStopped -= CraftStoppedHandler;
    }

    private void CraftStartedHandler() {
      BlockUnblockItems();
    }

    private void CraftStoppedHandler() {
      BlockUnblockItems();
    }

    private void BlockUnblockItems() {
      var fuelAmountNeed = 0;
      var inputItems = station.Inputs;

      Debug.Log("BlockUnblockItems: " + inputItems.Count);

      foreach (var input in inputItems) {
        fuelAmountNeed += input.Count * input.Recipe.Fuel.Amount;
      }

      var setCurrent = false;
      for (var i = 0; i < items.Count; i++) {
        var slot = station.FuelInventory.GetSlots[i];
        var fuelItem = items[i];

        if (slot.isEmpty || fuelAmountNeed <= 0) {
          fuelItem.UnBlock();
          continue;
        }

        fuelItem.Block();
        if (!setCurrent) {
          setCurrent = true;
          fuelItem.SetCurrent();
        }

        fuelAmountNeed -= slot.amount;
      }
    }
  }
}