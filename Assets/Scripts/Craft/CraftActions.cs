using System;
using System.Linq;
using Scriptables.Craft;
using Settings;
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
    [SerializeField] private Button minCountButton;
    [SerializeField] private Button maxCountButton;
    [SerializeField] private Image maxCountButtonSub;
    [SerializeField] private Image minCountButtonSub;
    private Color buttonsActiveColor;
    private Color buttonsDisabledColor;

    // private PlayerInventory playerInventory;
    private ITotalAmount totalAmount;
    private Recipe recipe;
    private Workstation station;

    private int minCount = 1;
    private int maxCount;
    private int currentCount = 1;

    public event Action<int> OnCraftRequested;

    public void Awake() {
      var uiSettings = GameManager.Instance.UISettings;
      buttonsActiveColor = uiSettings.buttonsActiveColor;
      buttonsDisabledColor = uiSettings.buttonsDisabledColor;

      ServiceLocator.For(this).Register<ICraftActions>(this);

      station = ServiceLocator.For(this).Get<Workstation>();
      totalAmount = ServiceLocator.For(this).Get<ITotalAmount>();
    }

    public void Start() {
      UserInput.instance.controls.UI.Craft.performed += ctx => OnCraftClickHandler();
    }

    public void InitComponent() => AddEvents();

    public void ClearComponent() => RemoveEvents();

    public void UpdateAndPrintInputCount(bool resetCurrentCount = false) {
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
      EnableButton(maxCountButton, currentCount < maxCount, maxCountButtonSub);
      EnableButton(minCountButton, currentCount > minCount, minCountButtonSub);
      EnableButton(craftButton, currentCount > 0);
    }

    private void EnableButton(Button button, bool state, Image subimage = null) {
      var color = state ? buttonsActiveColor : buttonsDisabledColor;
      button.enabled = state;
      button.image.color = color;
      if (subimage) {
        subimage.color = color;
      }
    }

    public void SetRecipe(Recipe recipe) {
      this.recipe = recipe;
    }

    private void AddEvents() {
      Debug.Log("CraftActions AddEvents");
      countInput.onValueChanged.AddListener(OnCountInputChangeHandler);
      craftButton.onClick.AddListener(OnCraftClickHandler);
      incrementButton.onClick.AddListener(OnIncrementClickHandler);
      decrementButton.onClick.AddListener(OnDecrementClickHandler);
      maxCountButton.onClick.AddListener(OnMaxCountButtonClickHandler);
      minCountButton.onClick.AddListener(OnMinCountButtonClickHandler);
    }

    private void RemoveEvents() {
      Debug.Log("CraftActions RemoveEvents");
      countInput.onValueChanged.RemoveAllListeners();
      craftButton.onClick.RemoveAllListeners();
      incrementButton.onClick.RemoveAllListeners();
      decrementButton.onClick.RemoveAllListeners();
      maxCountButton.onClick.RemoveAllListeners();
      minCountButton.onClick.RemoveAllListeners();
    }

    private void OnCountInputChangeHandler(string value) {
      if (value == string.Empty) {
        return;
      }

      var count = int.Parse(value);

      // if (count == minCount || count == maxCount) {
      //   return;
      // }

      count = Math.Clamp(count, 0, maxCount);

      SetCurrentCount(count);
      PrintInputCount();
      EnableButtons();
    }

    private void OnCraftClickHandler() {
      if (currentCount < minCount) {
        return;
      }

      OnCraftRequested?.Invoke(currentCount);
    }

    private void OnIncrementClickHandler() {
      if (currentCount >= maxCount) {
        return;
      }

      currentCount++;
      PrintInputCount();
    }

    private void OnDecrementClickHandler() {
      if (currentCount <= minCount) {
        return;
      }

      currentCount--;
      PrintInputCount();
    }

    private void OnMaxCountButtonClickHandler() {
      currentCount = maxCount;
      PrintInputCount();
    }

    private void OnMinCountButtonClickHandler() {
      currentCount = minCount;
      PrintInputCount();
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
      var freeInputSlotsCount = station.OutputSlotsAmount - station.Inputs.Count;

      if (freeInputSlotsCount <= 0) {
        return 0;
      }

      var usedSlotsByCrafting = 0;
      var leftCount = 0;

      var maxStackSize = recipe.Result.MaxStackSize;
      var outputSlotsCount = station.OutputInventory.CalculateTotalCounts();
      // var inputItemsIds = station.CraftItemsTotal.Keys.Select(x => x.Id).ToList();
      var inputItemsIds = station.Inputs.Select(x => x.Recipe.Result.Id).ToList();

      // Count used slots and check crafting progress
      foreach (var input in station.Inputs) {
        var id = input.Recipe.Result.Id;
        var inputMaxStackSize = input.Recipe.Result.MaxStackSize;
        var outputCount = outputSlotsCount.ContainsKey(id) ? outputSlotsCount[id] : 0;
        var inputOutputCount = input.Count + outputCount;

        usedSlotsByCrafting += (inputOutputCount + inputMaxStackSize - 1) / inputMaxStackSize;

        if (outputCount > 0) {
          usedSlotsByCrafting -= (outputCount + inputMaxStackSize - 1) / inputMaxStackSize;
        }

        if (id == recipe.Result.Id) {
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