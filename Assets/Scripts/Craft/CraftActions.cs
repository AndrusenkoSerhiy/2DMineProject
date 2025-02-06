using System;
using System.Linq;
using Inventory;
using Scriptables.Craft;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Craft {
  public class CraftActions {
    private readonly TMP_InputField countInput;
    private readonly Button craftButton;
    private readonly Button incrementButton;
    private readonly Button decrementButton;
    private readonly Button maxCountButton;
    private readonly PlayerInventory playerInventory;
    private readonly Color buttonsActiveColor;
    private readonly Color buttonsDisabledColor;
    private Recipe recipe;
    private Workstation station;

    public Action<int> onCraftRequested;

    private int minCount = 1;
    private int maxCount = 0;
    private int currentCount = 1;

    // private Dictionary<ItemObject, int> outputSlotsInUse = new Dictionary<ItemObject, int>();
    // private int outputSlotsAmount;
    // private int slotsInUse = 0;

    public CraftActions(Workstation station, TMP_InputField countInput, Button craftButton, Button incrementButton,
      Button decrementButton, Button maxCountButton, Color buttonsActiveColor, Color buttonsDisabledColor) {
      this.station = station;
      this.countInput = countInput;
      this.craftButton = craftButton;
      this.incrementButton = incrementButton;
      this.decrementButton = decrementButton;
      this.maxCountButton = maxCountButton;
      this.buttonsActiveColor = buttonsActiveColor;
      this.buttonsDisabledColor = buttonsDisabledColor;
      playerInventory = GameManager.instance.PlayerInventory;

      this.station.CraftItemsTotal.Clear();
      this.station.CraftInputsItemsIds.Clear();
    }

    public void UpdateAndPrintInputCount() {
      Debug.Log("CraftActions UpdateAndPrintInputCount");
      ResetCurrentCount();
      CalculateMaxCount();
      SetCurrentCount();
      EnableButtons();
      PrintInputCount();
    }

    private void EnableButtons() {
      EnableButton(decrementButton, currentCount > minCount);
      EnableButton(incrementButton, currentCount < maxCount);
      EnableButton(maxCountButton, currentCount < maxCount);
      EnableButton(craftButton, currentCount > 0);
    }

    private void EnableButton(Button button, bool state) {
      button.enabled = state;
      button.image.color = state ? buttonsActiveColor : buttonsDisabledColor;
    }

    public void SetRecipe(Recipe recipe) {
      //Debug.Log("CraftActions SetRecipe: " + recipe.RecipeName);
      this.recipe = recipe;
    }

    public void AddCraftActionsEvents() {
      countInput.onValueChanged.AddListener(OnCountInputChangeHandler);
      craftButton.onClick.AddListener(OnCraftClickHandler);
      incrementButton.onClick.AddListener(OnIncrementClickHandler);
      decrementButton.onClick.AddListener(OnDecrementClickHandler);
      maxCountButton.onClick.AddListener(OnMaxCountButtonClickHandler);
    }

    public void RemoveCraftActionsEvents() {
      countInput.onValueChanged.RemoveAllListeners();
      craftButton.onClick.RemoveAllListeners();
      incrementButton.onClick.RemoveAllListeners();
      decrementButton.onClick.RemoveAllListeners();
      maxCountButton.onClick.RemoveAllListeners();
    }

    private void OnCountInputChangeHandler(string value) {
      //Debug.Log("CraftActions Value changed: " + value);
      if (value == string.Empty) {
        return;
      }

      var count = int.Parse(value);

      if (count == minCount || count == maxCount) {
        return;
      }

      count = Math.Clamp(count, minCount, maxCount);

      SetCurrentCount(count);
      PrintInputCount();
      EnableButtons();
    }

    private void OnCraftClickHandler() {
      //Debug.Log("CraftActions Craft Clicked");
      onCraftRequested?.Invoke(currentCount);
    }

    private void OnIncrementClickHandler() {
      //Debug.Log("CraftActions Increment Clicked");
      if (currentCount >= maxCount) {
        return;
      }
      currentCount++;
      PrintInputCount();
      EnableButtons();
    }

    private void OnDecrementClickHandler() {
      //Debug.Log("CraftActions Decrement Clicked");
      if (currentCount <= minCount) {
        return;
      }
      currentCount--;
      PrintInputCount();
      EnableButtons();
    }

    private void OnMaxCountButtonClickHandler() {
      //Debug.Log("CraftActions MaxCountButton Clicked");
      currentCount = maxCount;
      PrintInputCount();
      EnableButtons();
    }

    private void CalculateMaxCount() {
      var maxCountByInputOutput = CalculateMaxCountByCurrentCraftingAndOutput();
      var maxCountByResources = CalculateMaxCountByResources();
      maxCount = Math.Min(maxCountByInputOutput, maxCountByResources);
    }

    private int CalculateMaxCountByResources() {
      var max = int.MaxValue;

      foreach (var resource in recipe.RequiredMaterials) {
        var availableAmount = playerInventory.GetResourceTotalAmount(resource.Material.data.Id);
        var maxCraftable = availableAmount / resource.Amount;
        max = Math.Min(max, maxCraftable);
      }

      return max == int.MaxValue ? 0 : max;
    }

    private int CalculateMaxCountByCurrentCraftingAndOutput() {
      var freeOutputSlotsCount = station.OutputInventory.GetFreeSlotsCount();
      var freeInputSlotsCount = station.OutputSlotsAmount - station.CraftInputsItemsIds.Count;

      if (freeInputSlotsCount <= 0) {
        return 0;
      }

      var usedSlotsByCrafting = 0;
      var leftCount = 0;

      var maxStackSize = recipe.Result.MaxStackSize;
      var outputSlotsCount = station.OutputInventory.CalculateTotalCounts();
      var inputItemsIds = station.CraftItemsTotal.Keys.Select(x => x.data.Id).ToList();

      // Count used slots and check crafting progress
      foreach (var (item, count) in station.CraftItemsTotal) {
        var outputCount = outputSlotsCount.ContainsKey(item.data.Id) ? outputSlotsCount[item.data.Id] : 0;
        var inputOutputCount = count + outputCount;

        usedSlotsByCrafting += (inputOutputCount + item.MaxStackSize - 1) / item.MaxStackSize;

        if (outputCount > 0) {
          usedSlotsByCrafting -= (outputCount + item.MaxStackSize - 1) / item.MaxStackSize;
        }

        if (item.data.Id == recipe.Result.data.Id) {
          var left = inputOutputCount % maxStackSize;
          leftCount += (left == 0) ? 0 : maxStackSize - (left);
        }
      }

      foreach (var output in outputSlotsCount) {
        if (inputItemsIds.Contains(output.Key)) {
          continue;
        }

        if (output.Key == recipe.Result.data.Id) {
          var left = output.Value % maxStackSize;
          leftCount += (left == 0) ? 0 : maxStackSize - (left);
        }
      }

      // Calculate remaining space for new crafted items
      var availableStacks = freeOutputSlotsCount - usedSlotsByCrafting;
      var maxCraftable = availableStacks * maxStackSize + leftCount;

      return Mathf.Max(0, maxCraftable); // Ensure non-negative values
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