using System;
using System.Collections.Generic;
using Craft;
using Inventory;
using SaveSystem;
using Scriptables.Items;
using UnityEngine;

namespace Scriptables.Craft {
  [Serializable]
  public struct Input {
    public int Count;
    public Recipe Recipe;
  }

  [Serializable]
  public struct CurrentProgress {
    public int CraftTimeInMilliseconds;
    public float CurrentTimeInMilliseconds;
  }

  [CreateAssetMenu(menuName = "Crafting System/Workstation", fileName = "New Workstation")]
  public class Workstation : BaseScriptableObject {
    public RecipeType RecipeType;
    public InventoryType OutputInventoryType;
    public InventoryType FuelInventoryType;
    public RecipesDatabaseObject RecipeDB;
    public string Title;
    [TextArea(15, 20)] public string Description;
    public bool PlayEffectsWhenOpened;
    public bool PlayEffectsWhenClosed = true;

    public long CraftStartTimestampMillis;

    //Current Progress
    private CurrentProgress CurrentProgress = new();
    public event Action OnCraftStarted;

    public event Action OnCraftStopped;

    //For loaded inputs
    public long MillisecondsLeft = 0;

    //For effects
    public List<CraftingTask> CraftingTasks = new();

    //For Save/Load
    public List<Input> Inputs = new();

    private InventoryObject outputInventory;
    private InventoryObject fuelInventory;
    private int outputSlotsAmount;
    private int fuelSlotsAmount;

    public void Clear() {
      Inputs.Clear();
      CraftingTasks.Clear();
      ResetMillisecondsLeft();
      CraftStartTimestampMillis = 0;
    }

    public void Load(WorkstationsData data) {
      Clear();

      if (data.Inputs.Count <= 0) {
        return;
      }

      var inputs = data.Inputs;

      CraftStartTimestampMillis = Helper.GetCurrentTimestampMillis();
      var currentCraftStartTimestampMillis = CraftStartTimestampMillis;
      //only for first input
      var millisecondsLeft = data.MillisecondsLeft;

      foreach (var input in inputs) {
        var recipe = RecipeDB.ItemsMap[input.RecipeId];
        var inputTotalCraftMillis = (long)input.Count * recipe.CraftingTime * 1000;

        if (millisecondsLeft > 0) {
          var millisecondsPassed = inputTotalCraftMillis - millisecondsLeft;
          CraftStartTimestampMillis = currentCraftStartTimestampMillis - millisecondsPassed;
          currentCraftStartTimestampMillis = CraftStartTimestampMillis;
          millisecondsLeft = 0;
        }

        Inputs.Add(new Input { Recipe = recipe, Count = input.Count });
        AddCraftingTask(recipe.Result.Id, recipe.CraftingTime, input.Count, currentCraftStartTimestampMillis);

        currentCraftStartTimestampMillis += inputTotalCraftMillis;
      }
    }

    public void ProcessCraftedInputs() {
      if (Inputs.Count == 0) {
        return;
      }

      var currentTimeInMilliseconds = Helper.GetCurrentTimestampMillis();
      var currentCraftStartTimestampMillis = CraftStartTimestampMillis;

      for (var i = 0; i < Inputs.Count; i++) {
        var input = Inputs[i];
        var recipe = input.Recipe;
        var craftedCount = 0;

        for (var j = 0; j < input.Count; j++) {
          var itemEndTimeInMilliseconds = currentCraftStartTimestampMillis + (recipe.CraftingTime * 1000);

          if (currentTimeInMilliseconds >= itemEndTimeInMilliseconds) {
            craftedCount++;
            currentCraftStartTimestampMillis = itemEndTimeInMilliseconds; // Move to next item's start
          }
          else {
            // Stop processing further — this item isn't done yet
            break;
          }
        }

        // Process fully crafted items
        if (craftedCount > 0) {
          var outputInventory = GameManager.Instance.PlayerInventory.GetInventoryByType(OutputInventoryType);
          outputInventory.AddItem(new Item(recipe.Result), craftedCount);
          ConsumeFuel(recipe, craftedCount);
        }

        // Fully crafted this input, remove it
        if (craftedCount >= input.Count) {
          Inputs.RemoveAt(i--);
          continue;
        }

        // Partial progress, update count
        input.Count -= craftedCount;
        Inputs[i] = input;

        // If we broke early (some items still crafting), the next item should start right after this one
        if (craftedCount < input.Count) {
          break;
        }
      }

      // Update CraftStartTime for next session
      CraftStartTimestampMillis = currentCraftStartTimestampMillis;
    }

    public void ConsumeFuel(Recipe recipe, int count) {
      var fuelInventory = GameManager.Instance.PlayerInventory.GetInventoryByType(FuelInventoryType);
      if (fuelInventory == null) {
        return;
      }

      var totalCount = count * recipe.Fuel.Amount;

      fuelInventory.RemoveItem(recipe.Fuel.Material.Id, totalCount);
    }

    public long CalculateTimeLeftInMilliseconds(Input input) {
      var currentTimeInMilliseconds = Helper.GetCurrentTimestampMillis();
      var elapsedTimeInMilliseconds = currentTimeInMilliseconds - CraftStartTimestampMillis;
      var totalTimeInMilliseconds = (long)input.Count * input.Recipe.CraftingTime * 1000;
      return Math.Clamp((totalTimeInMilliseconds - elapsedTimeInMilliseconds), 0, totalTimeInMilliseconds);
    }

    public void UpdateMillisecondsLeftByInput(Input input) {
      MillisecondsLeft = CalculateTimeLeftInMilliseconds(input);
      Debug.Log($"UpdateMillisecondsLeft MillisecondsLeft {MillisecondsLeft}");
    }

    public void ResetMillisecondsLeft() {
      MillisecondsLeft = 0;
    }

    public void UpdateCraftingTasks() {
      CraftingTasks.Clear();
      if (Inputs.Count <= 0) {
        return;
      }

      var currentDateTimestampMillis = CraftStartTimestampMillis;
      foreach (var input in Inputs) {
        AddCraftingTask(input.Recipe.Result.Id, input.Recipe.CraftingTime, input.Count, currentDateTimestampMillis);
        currentDateTimestampMillis += input.Count * input.Recipe.CraftingTime * 1000;
      }
    }

    private void AddCraftingTask(string id, int craftTime, int count, long startInMilliseconds) {
      for (var i = 1; i <= count; i++) {
        var endTime = startInMilliseconds + (craftTime * i * 1000);
        CraftingTasks.Add(new CraftingTask(id, endTime));
      }
    }

    public CraftingTask? RemoveFirstTaskIfEnded() {
      if (CraftingTasks.Count <= 0) {
        return null;
      }

      var currentDateTimestampMillis = Helper.GetCurrentTimestampMillis();
      var firstTask = CraftingTasks[0];
      if (firstTask.FinishTimeMilliseconds > currentDateTimestampMillis) {
        return null;
      }

      CraftingTasks.RemoveAt(0);
      return firstTask;
    }

    public void AddItemToInputs(Recipe recipe, int count) {
      Inputs.Add(new Input { Recipe = recipe, Count = count });
      if (Inputs.Count == 1) {
        OnCraftStarted?.Invoke();
      }
    }

    public void RemoveInputCountFromInputs(int count) {
      var inputPosition = 0;
      if (!IsValidInputPosition(inputPosition)) {
        return;
      }

      var input = Inputs[inputPosition];
      input.Count = Math.Max(0, input.Count - count);

      if (input.Count == 0) {
        Inputs.RemoveAt(inputPosition);
      }
      else {
        Inputs[inputPosition] = input;
      }

      if (Inputs.Count <= 0) {
        OnCraftStopped?.Invoke();
      }
    }

    public void RemoveInputFromInputs(int inputPosition) {
      if (!IsValidInputPosition(inputPosition)) {
        return;
      }

      Inputs.RemoveAt(inputPosition);

      if (Inputs.Count <= 0) {
        OnCraftStopped?.Invoke();
      }
    }

    private bool IsValidInputPosition(int position) {
      return position >= 0 && position < Inputs.Count;
    }

    public void SetProgress(int craftTimeInMilliseconds, float currentTimeInMilliseconds) {
      CurrentProgress.CraftTimeInMilliseconds = craftTimeInMilliseconds;
      CurrentProgress.CurrentTimeInMilliseconds = currentTimeInMilliseconds;
    }

    public void UpdateProgress(float currentTimeInMilliseconds) {
      CurrentProgress.CurrentTimeInMilliseconds = currentTimeInMilliseconds;
    }

    public void ResetProgress() {
      CurrentProgress.CraftTimeInMilliseconds = 0;
      CurrentProgress.CurrentTimeInMilliseconds = 0;
    }

    public float GetProgressTimeInMilliseconds() => CurrentProgress.CurrentTimeInMilliseconds;
    public int GetProgressCraftTimeInMilliseconds() => CurrentProgress.CraftTimeInMilliseconds;
  }
}