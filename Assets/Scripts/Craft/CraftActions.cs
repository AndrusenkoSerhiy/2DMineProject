using System;
using System.Collections.Generic;
using Inventory;
using Scriptables.Craft;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityServiceLocator;

namespace Craft {
  public class CraftActions : MonoBehaviour {
    [SerializeField] private TMP_InputField countInput;
    [SerializeField] private Button craftButton;
    [SerializeField] private Button incrementButton;
    [SerializeField] private Button decrementButton;
    [SerializeField] private Button minCountButton;
    [SerializeField] private Button maxCountButton;
    [SerializeField] private Image maxCountButtonSub;
    [SerializeField] private Image minCountButtonSub;
    private Color buttonsActiveColor;
    private Color buttonsDisabledColor;

    private Workstation station;
    private GameManager gameManager;
    private InventoriesPool inventoriesPool;
    private List<Inventory.Inventory> outputInventories;

    private int minCount = 1;
    private int maxCount;
    private int currentCount = 1;

    private void Awake() {
      gameManager = GameManager.Instance;
      var uiSettings = gameManager.UISettings;
      buttonsActiveColor = uiSettings.buttonsActiveColor;
      buttonsDisabledColor = uiSettings.buttonsDisabledColor;

      station = ServiceLocator.For(this).Get<Workstation>();
      inventoriesPool = gameManager.PlayerInventory.InventoriesPool;
      outputInventories = station.GetOutputInventories();
    }

    private void OnEnable() {
      UpdateAndPrintInputCount();
      AddEvents();
    }

    private void OnDisable() {
      RemoveEvents();
    }

    private void UpdateAndPrintInputCount(bool resetCurrentCount = false) {
      if (resetCurrentCount) {
        ResetCurrentCount();
      }

      CalculateMaxCount();
      SetCurrentCount();
      EnableButtons();
      PrintInputCount();
    }

    private void EnableButtons() {
      //EnableButton(decrementButton, currentCount > minCount);
      //EnableButton(incrementButton, currentCount < maxCount);
      //EnableButton(maxCountButton, currentCount < maxCount, maxCountButtonSub);
      //EnableButton(minCountButton, currentCount > minCount, minCountButtonSub);
      //EnableButton(craftButton, currentCount > 0);
    }

    private void EnableButton(Button button, bool state, Image subimage = null) {
      var color = state ? buttonsActiveColor : buttonsDisabledColor;
      button.enabled = state;
      button.image.color = color;
      if (subimage) {
        subimage.color = color;
      }
    }

    private void AddEvents() {
      gameManager.UserInput.controls.UI.Craft.performed += OnCraftPerformed;

      countInput.onValueChanged.AddListener(OnCountInputChangeHandler);
      craftButton.onClick.AddListener(OnCraftClickHandler);
      incrementButton.onClick.AddListener(OnIncrementClickHandler);
      decrementButton.onClick.AddListener(OnDecrementClickHandler);
      maxCountButton.onClick.AddListener(OnMaxCountButtonClickHandler);
      minCountButton.onClick.AddListener(OnMinCountButtonClickHandler);

      station.OnRecipeChanged += OnRecipeChangedHandler;
      station.OnAfterAddItemToInputs += OnAfterCraftRequestedHandler;
      station.OnInputAllCrafted += OnInputAllCraftedHandler;
      // inventoriesPool.OnResourcesTotalUpdate += OnResourcesTotalUpdateHandler;

      AddInventoryPoolEvents();

      //for InventoryType.Inventory handles inside station
      if (station.OutputInventoryType != InventoryType.Inventory) {
        AddOutputUpdateEvents();
      }
    }

    private void RemoveEvents() {
      //for InventoryType.Inventory handles inside station
      if (station.OutputInventoryType != InventoryType.Inventory) {
        RemoveOutputUpdateEvents();
      }

      RemoveInventoryPoolEvents();

      station.OnInputAllCrafted -= OnInputAllCraftedHandler;
      station.OnAfterAddItemToInputs -= OnAfterCraftRequestedHandler;
      station.OnRecipeChanged -= OnRecipeChangedHandler;
      // inventoriesPool.OnResourcesTotalUpdate -= OnResourcesTotalUpdateHandler;

      countInput.onValueChanged.RemoveAllListeners();
      craftButton.onClick.RemoveAllListeners();
      incrementButton.onClick.RemoveAllListeners();
      decrementButton.onClick.RemoveAllListeners();
      maxCountButton.onClick.RemoveAllListeners();
      minCountButton.onClick.RemoveAllListeners();

      gameManager.UserInput.controls.UI.Craft.performed -= OnCraftPerformed;
    }

    private void OnCraftPerformed(InputAction.CallbackContext ctx) => OnCraftClickHandler();
    private void OnRecipeChangedHandler(Recipe recipe) => UpdateAndPrintInputCount(true);
    private void OnAfterCraftRequestedHandler(Input input) => UpdateAndPrintInputCount();
    private void OnInputAllCraftedHandler() => UpdateAndPrintInputCount();

    private void OnResourcesTotalUpdateHandler(string resourceId) {
      var recipeIngredientsIds = station.GetRecipeIngredientsIds();
      if (Array.IndexOf(recipeIngredientsIds, resourceId) != -1) {
        UpdateAndPrintInputCount();
      }
    }

    private void AddOutputUpdateEvents() {
      foreach (var outputInventory in outputInventories) {
        foreach (var output in outputInventory.Slots) {
          output.OnAfterUpdated += OutputUpdateSlotHandler;
        }
      }
    }

    private void RemoveOutputUpdateEvents() {
      foreach (var outputInventory in outputInventories) {
        foreach (var output in outputInventory.Slots) {
          output.OnAfterUpdated -= OutputUpdateSlotHandler;
        }
      }
    }

    private void OutputUpdateSlotHandler(SlotUpdateEventData data) {
      if (data.after.isEmpty) {
        UpdateAndPrintInputCount();
      }

      station.StartCrafting();
    }

    private void AddInventoryPoolEvents() {
      foreach (var inventory in inventoriesPool.Inventories) {
        foreach (var slot in inventory.Slots) {
          slot.OnAfterUpdated += OnAfterUpdatedHandler;
        }
      }
    }

    private void RemoveInventoryPoolEvents() {
      foreach (var inventory in inventoriesPool.Inventories) {
        foreach (var slot in inventory.Slots) {
          slot.OnAfterUpdated -= OnAfterUpdatedHandler;
        }
      }
    }

    private void OnAfterUpdatedHandler(SlotUpdateEventData obj) {
      if (obj.after.isEmpty && obj.before.isEmpty) {
        return;
      }

      var id = obj.after.isEmpty ? obj.before.Item.info.Id : obj.after.Item.info.Id;

      OnResourcesTotalUpdateHandler(id);
    }

    private void OnCountInputChangeHandler(string value) {
      if (value == string.Empty) {
        return;
      }

      var count = int.Parse(value);

      count = Math.Clamp(count, 0, maxCount);

      SetCurrentCount(count);
      PrintInputCount();
      EnableButtons();
    }

    private void OnCraftClickHandler() {
      if (currentCount < minCount) {
        return;
      }

      gameManager.AudioController.PlayCraftClick();
      station.CraftRequested(currentCount);
    }

    private void OnIncrementClickHandler() {
      if (currentCount >= maxCount) {
        return;
      }

      gameManager.AudioController.PlayUIClick();
      currentCount++;
      PrintInputCount();
    }

    private void OnDecrementClickHandler() {
      if (currentCount <= minCount) {
        return;
      }

      gameManager.AudioController.PlayUIClick();
      currentCount--;
      PrintInputCount();
    }

    private void OnMaxCountButtonClickHandler() {
      currentCount = maxCount;
      gameManager.AudioController.PlayUIClick();
      PrintInputCount();
    }

    private void OnMinCountButtonClickHandler() {
      currentCount = minCount;
      gameManager.AudioController.PlayUIClick();
      PrintInputCount();
    }

    private void CalculateMaxCount() {
      var maxCountByInput = CalculateMaxCountByCurrentCrafting();
      var maxCountByResources = CalculateMaxCountByResources();

      maxCount = Math.Min(maxCountByInput, maxCountByResources);
    }

    private int CalculateMaxCountByResources() {
      var max = int.MaxValue;

      foreach (var resource in station.CurrentRecipe.RequiredMaterials) {
        var availableAmount = inventoriesPool.GetResourceTotalAmount(resource.Material.Id);
        var maxCraftable = availableAmount / resource.Amount;
        max = Math.Min(max, maxCraftable);
      }

      return max == int.MaxValue ? 0 : max;
    }

    private int CalculateMaxCountByCurrentCrafting() {
      var slotsCount = station.CraftSlotsCount;
      var freeInputSlotsCount = slotsCount - station.Inputs.Count;

      if (freeInputSlotsCount <= 0) {
        return 0;
      }

      var maxStackSize = station.CurrentRecipe.Result.MaxStackSize;
      var maxByFreeSlots = freeInputSlotsCount * maxStackSize;

      return Math.Max(0, maxByFreeSlots);
    }

    private void SetCurrentCount() {
      if (currentCount <= 0 && maxCount > 0) {
        currentCount = 1;
        return;
      }

      currentCount = currentCount > maxCount ? maxCount : currentCount;
    }

    private void SetCurrentCount(int count) {
      currentCount = count;
    }

    private void ResetCurrentCount() {
      currentCount = 1;
      maxCount = 0;
    }

    private void PrintInputCount() {
      countInput.text = currentCount.ToString();
    }
  }
}