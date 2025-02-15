using System;
using System.Linq;
using Inventory;
using Scriptables.Craft;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityServiceLocator;

namespace Craft {
  public class CraftActions : MonoBehaviour, ICraftActions {
    [SerializeField] private TMP_InputField countInput;
    [SerializeField] private Button craftButton;
    [SerializeField] private Button incrementButton;
    [SerializeField] private Button decrementButton;
    [SerializeField] private Button maxCountButton;
    [SerializeField] private Color buttonsActiveColor;
    [SerializeField] private Color buttonsDisabledColor;

    // private PlayerInventory playerInventory;
    private ITotalAmount totalAmount;
    private Recipe recipe;
    private Workstation station;

    private int minCount = 1;
    private int maxCount = 0;
    private int currentCount = 1;

    public event Action<int> OnCraftRequested;

    public void Awake() {
      Debug.Log("CraftActions Awake");
      // playerInventory = GameManager.instance.PlayerInventory;
      ServiceLocator.For(this).Register<ICraftActions>(this);

      station = ServiceLocator.For(this).Get<Workstation>();
      station.CraftItemsTotal.Clear();
      station.CraftInputsItemsIds.Clear();

      totalAmount = ServiceLocator.For(this).Get<ITotalAmount>();
    }

    public void InitComponent() => AddEvents();

    public void ClearComponent() => RemoveEvents();

    public void UpdateAndPrintInputCount(bool resetCurrentCount = false) {
      Debug.Log("CraftActions UpdateAndPrintInputCount");
      if (resetCurrentCount) {
        ResetCurrentCount();
      }

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

    private void AddEvents() {
      Debug.Log("CraftActions AddEvents");
      countInput.onValueChanged.AddListener(OnCountInputChangeHandler);
      craftButton.onClick.AddListener(OnCraftClickHandler);
      incrementButton.onClick.AddListener(OnIncrementClickHandler);
      decrementButton.onClick.AddListener(OnDecrementClickHandler);
      maxCountButton.onClick.AddListener(OnMaxCountButtonClickHandler);
    }

    private void RemoveEvents() {
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
      OnCraftRequested?.Invoke(currentCount);
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
        var availableAmount = totalAmount.GetResourceTotalAmount(resource.Material.Id);
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
      var inputItemsIds = station.CraftItemsTotal.Keys.Select(x => x.Id).ToList();

      // Count used slots and check crafting progress
      foreach (var (item, count) in station.CraftItemsTotal) {
        var outputCount = outputSlotsCount.ContainsKey(item.Id) ? outputSlotsCount[item.Id] : 0;
        var inputOutputCount = count + outputCount;

        usedSlotsByCrafting += (inputOutputCount + item.MaxStackSize - 1) / item.MaxStackSize;

        if (outputCount > 0) {
          usedSlotsByCrafting -= (outputCount + item.MaxStackSize - 1) / item.MaxStackSize;
        }

        if (item.Id == recipe.Result.Id) {
          var left = inputOutputCount % maxStackSize;
          leftCount += (left == 0) ? 0 : maxStackSize - (left);
        }
      }

      foreach (var output in outputSlotsCount) {
        if (inputItemsIds.Contains(output.Key)) {
          continue;
        }

        if (output.Key == recipe.Result.Id) {
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