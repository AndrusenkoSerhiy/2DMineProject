using System.Collections.Generic;
using Inventory;
using Scriptables.Craft;
using UnityEngine;
using UnityServiceLocator;

namespace Craft {
  public class FuelItems : MonoBehaviour {
    [SerializeField] private UserInterface fuelInterface;
    [SerializeField] private List<FuelItem> items;
    [SerializeField] private Color blinkBgColor;
    [SerializeField] private float blinkTime = 1.5f;

    private Workstation station;
    private Inventory.Inventory fuelInventory;

    private void Awake() {
      station = ServiceLocator.For(this).Get<Workstation>();
      fuelInventory = station.GetFuelInventory();

      if (fuelInventory == null) {
        return;
      }

      fuelInterface.Setup(station.FuelInventoryType, station.Id);

      InitItems();
    }

    private void InitItems() {
      foreach (var item in items) {
        item.Init();
      }
    }

    private void OnEnable() {
      fuelInterface.OnLoaded += OnFuelInterfaceLoadedHandler;
      station.OnCraftStarted += OnCraftStartedHandler;
      station.OnInputAllCrafted += OnInputAllCraftedHandler;
      station.OnFuelConsumed += OnFuelConsumedHandler;
      station.OnRecipeChanged += OnRecipeChangedHandler;
      station.OnAllInputsCanceled += OnAllInputsCanceledHandler;
      station.OnCraftPaused += OnCraftPausedHandler;
      AddFuelUpdateEvents();
    }

    private void OnDisable() {
      RemoveFuelUpdateEvents();
      station.OnRecipeChanged -= OnRecipeChangedHandler;
      station.OnFuelConsumed -= OnFuelConsumedHandler;
      station.OnInputAllCrafted -= OnInputAllCraftedHandler;
      station.OnCraftStarted -= OnCraftStartedHandler;
      fuelInterface.OnLoaded -= OnFuelInterfaceLoadedHandler;
      station.OnAllInputsCanceled -= OnAllInputsCanceledHandler;
      station.OnCraftPaused -= OnCraftPausedHandler;
    }

    private void OnCraftStartedHandler() => BlockUnblockItems();
    private void OnInputAllCraftedHandler() => BlockUnblockItems();
    private void OnFuelConsumedHandler() => BlockUnblockItems();
    private void OnAllInputsCanceledHandler() => BlockUnblockItems();
    private void OnCraftPausedHandler() => BlockUnblockItems();
    private void OnRecipeChangedHandler(Recipe recipe) => UpdateInterface(recipe);

    private void OnFuelInterfaceLoadedHandler() {
      UpdateInterface(station.CurrentRecipe);
      BlockUnblockItems();
    }

    private void AddFuelUpdateEvents() {
      foreach (var slot in fuelInventory.Slots) {
        slot.OnAfterUpdated += FuelSlotUpdateHandler;
      }
    }

    private void RemoveFuelUpdateEvents() {
      foreach (var slot in fuelInventory.Slots) {
        slot.OnAfterUpdated -= FuelSlotUpdateHandler;
      }
    }

    private void FuelSlotUpdateHandler(SlotUpdateEventData data) {
      RunFuelEffect(station.CurrentRecipe);

      station.StartCrafting();
    }

    private void RunFuelEffect(Recipe recipe) {
      if (station.HaveFuelForCraft(recipe)) {
        StopBlink();
        BlockUnblockItems();
      }
      else {
        StartBlink();
      }
    }

    private void UpdateInterface(Recipe recipe) {
      var fuelSlots = fuelInventory.Slots;
      var currentFuel = recipe.Fuel.Material;

      //check current fuel items, move them to inventory if they are not in the recipe
      if (fuelSlots[0].SlotDisplay.IsItemInAllowed(currentFuel)) {
        RunFuelEffect(recipe);
        return;
      }

      var firstNotEmpty = fuelInventory.FindFirstNotEmpty();
      if (firstNotEmpty != null && firstNotEmpty.Item.id != currentFuel.Id) {
        fuelInventory.MoveAllItemsTo(GameManager.Instance.PlayerInventory.GetInventory());
      }

      foreach (var slot in fuelSlots) {
        slot.SlotDisplay.AllowedItems.Clear();
        slot.SlotDisplay.AllowedItems.Add(currentFuel);
        slot.SlotDisplay.EmptySlotIcon = currentFuel.UiDisplay;
      }

      fuelInterface.UpdateInventoryUI();
      RunFuelEffect(recipe);
    }

    private void StartBlink() {
      foreach (var item in items) {
        item.StartBlink(blinkBgColor, blinkTime);
      }
    }

    private void StopBlink() {
      foreach (var item in items) {
        item.ClearBlinkEffect();
      }
    }

    private void BlockUnblockItems() {
      if (!station.CurrentProgress.IsCrafting) {
        foreach (var fuelItem in items) {
          fuelItem.UnBlock();
        }

        return;
      }

      var fuelAmountNeed = 0;
      var inputItems = station.Inputs;

      foreach (var input in inputItems) {
        fuelAmountNeed += input.Count * input.Recipe.Fuel.Amount;
      }

      var setCurrent = false;
      for (var i = 0; i < items.Count; i++) {
        var slot = fuelInventory.Slots[i];
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