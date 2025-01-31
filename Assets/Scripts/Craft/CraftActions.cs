using System;
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

    public Action<int> onCraftRequested;

    private int minCount = 1;
    private int maxCount = -1;
    private int currentCount = 1;
    private int outputSlotsAmount;
    private int slotsInUse = 0;

    public CraftActions(TMP_InputField countInput, Button craftButton, Button incrementButton,
      Button decrementButton, Button maxCountButton, Color buttonsActiveColor, Color buttonsDisabledColor, int outputSlotsAmount) {
      this.countInput = countInput;
      this.craftButton = craftButton;
      this.incrementButton = incrementButton;
      this.decrementButton = decrementButton;
      this.maxCountButton = maxCountButton;
      this.buttonsActiveColor = buttonsActiveColor;
      this.buttonsDisabledColor = buttonsDisabledColor;
      this.outputSlotsAmount = outputSlotsAmount;
      playerInventory = GameManager.instance.PlayerInventory;
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
      Debug.Log("CraftActions SetRecipe: " + recipe.RecipeName);
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
      Debug.Log("CraftActions Value changed: " + value);
      if (value == string.Empty) {
        return;
      }

      var count = int.Parse(value);

      if (count == minCount || count == maxCount) {
        return;
      }

      if (count > maxCount) {
        count = maxCount;
      }
      else if (count < minCount) {
        count = minCount;
      }

      SetCurrentCount(count);
      PrintInputCount();
      EnableButtons();
    }

    private void OnCraftClickHandler() {
      Debug.Log("CraftActions Craft Clicked");
      onCraftRequested?.Invoke(currentCount);
    }

    private void OnIncrementClickHandler() {
      Debug.Log("CraftActions Increment Clicked");
      if (currentCount >= maxCount) {
        return;
      }
      currentCount++;
      PrintInputCount();
      EnableButtons();
    }

    private void OnDecrementClickHandler() {
      Debug.Log("CraftActions Decrement Clicked");
      if (currentCount <= minCount) {
        return;
      }
      currentCount--;
      PrintInputCount();
      EnableButtons();
    }

    private void OnMaxCountButtonClickHandler() {
      Debug.Log("CraftActions MaxCountButton Clicked");
      currentCount = maxCount;
      PrintInputCount();
      EnableButtons();
    }

    private void CalculateMaxCount() {
      var maxSlotsCapacity = (outputSlotsAmount - slotsInUse) * recipe.Result.MaxStackSize;
      Debug.Log("CalculateMaxCount maxSlotsCapacity " + maxSlotsCapacity);

      foreach (var resource in recipe.RequiredMaterials) {
        var amount = playerInventory.GetResourceTotalAmount(resource.Material.data.Id);
        var max = amount / resource.Amount;
        Debug.Log("CalculateMaxCount max " + max);

        max = Math.Min(max, maxSlotsCapacity);

        maxCount = maxCount != -1 ? Math.Min(maxCount, max) : max;
        Debug.Log("CalculateMaxCount maxCount " + maxCount);
      }
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
      maxCount = -1;
    }

    private void PrintInputCount() {
      countInput.text = currentCount.ToString();
    }
  }
}