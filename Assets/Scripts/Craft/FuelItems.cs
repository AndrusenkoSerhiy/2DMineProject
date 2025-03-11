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
    public InventoryObject Inventory => fuelInventory;

    public void Awake() {
      station = ServiceLocator.For(this).Get<Workstation>();
      fuelInventory =
        GameManager.Instance.PlayerInventory.GetInventoryByTypeAndId(station.FuelInventoryType, station.Id);

      if (fuelInventory == null) {
        Debug.LogError("Set FuelInventory to the station");
        return;
      }

      fuelInterface.Setup(station.FuelInventoryType, station.Id);
      ServiceLocator.For(this).Register<IFuelItems>(this);
    }

    public void RunFuelEffect(Recipe recipe) {
      if (station.HaveFuelForCraft(recipe)) {
        StopBlink();
        BlockUnblockItems();
      }
      else {
        StartBlink();
      }
    }

    public void UpdateInterface(Recipe recipe) {
      var fuelSlots = fuelInventory.GetSlots;
      var currentFuel = recipe.Fuel.Material;

      //check current fuel items, move them to inventory if they are not in the recipe
      if (fuelSlots[0].AllowedItem == currentFuel) {
        RunFuelEffect(recipe);
        return;
      }

      var firstNotEmpty = fuelInventory.FindFirstNotEmpty();
      if (firstNotEmpty != null && firstNotEmpty.Item.id != currentFuel.Id) {
        fuelInventory.MoveAllItemsTo(GameManager.Instance.PlayerInventory.GetInventory());
      }

      foreach (var slot in fuelSlots) {
        slot.AllowedItem = currentFuel;
      }

      fuelInterface.UpdateInventoryUI();
      RunFuelEffect(recipe);
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