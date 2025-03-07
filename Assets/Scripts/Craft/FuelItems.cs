using System.Collections.Generic;
using Inventory;
using Scriptables.Craft;
using UnityEngine;
using UnityServiceLocator;

namespace Craft {
  public class FuelItems : MonoBehaviour, IFuelItems {
    [SerializeField] private UserInterface fuelInterface;
    [SerializeField] private List<FuelItem> items;
    [SerializeField] private Color blinkBgColor;
    [SerializeField] private float blinkTime = 1.5f;

    private Workstation station;
    private InventoryObject fuelInventory;

    public void Awake() {
      station = ServiceLocator.For(this).Get<Workstation>();
      fuelInventory = GameManager.Instance.PlayerInventory.GetInventoryByType(station.FuelInventoryType);

      if (fuelInventory == null) {
        Debug.LogError("Set FuelInventory to the station");
        return;
      }

      fuelInterface.Setup(InventoryType.ForgeFuel);
      ServiceLocator.For(this).Register<IFuelItems>(this);
    }

    public void UpdateInterface(Recipe recipe) {
      var fuelInventory = fuelInterface.Inventory;
      var fuelSlots = fuelInventory.GetSlots;
      var currentFuel = recipe.Fuel.Material;

      //check current fuel items, move them to inventory if they are not in the recipe
      if (fuelSlots[0].AllowedItem == currentFuel) {
        return;
      }

      fuelInventory.MoveAllItemsTo(GameManager.Instance.PlayerInventory.GetInventory());

      foreach (var slot in fuelSlots) {
        slot.AllowedItem = currentFuel;
      }

      fuelInterface.UpdateInventoryUI();
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

    public void StartBlink() {
      foreach (var item in items) {
        item.StartBlink(blinkBgColor, blinkTime);
      }
    }

    public void StopBlink() {
      foreach (var item in items) {
        item.ClearBlinkEffect();
      }
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

      foreach (var input in inputItems) {
        fuelAmountNeed += input.Count * input.Recipe.Fuel.Amount;
      }

      var setCurrent = false;
      for (var i = 0; i < items.Count; i++) {
        var slot = fuelInventory.GetSlots[i];
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