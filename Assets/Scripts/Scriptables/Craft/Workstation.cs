using System;
using System.Collections.Generic;
using System.Linq;
using Craft;
using SaveSystem;
using Scriptables.Inventory;
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
    public int CraftTime;
    public float CurrentTime;
  }

  [CreateAssetMenu(menuName = "Crafting System/Workstation", fileName = "New Workstation")]
  public class Workstation : BaseScriptableObject {
    public RecipeType RecipeType;
    public string Title;
    [TextArea(15, 20)] public string Description;
    public int OutputSlotsAmount;
    public List<Recipe> recipes = new();
    public InventoryObject OutputInventory;
    public InventoryObject FuelInventory;
    public bool PlayEffectsWhenOpened;
    public bool PlayEffectsWhenClosed = true;

    [SerializeField] private string CraftStartTimeString;

    public DateTime CraftStartTime {
      get => string.IsNullOrEmpty(CraftStartTimeString)
        ? DateTime.MinValue
        : Helper.ParseDate(CraftStartTimeString);
      set => CraftStartTimeString = Helper.FormatDate(value);
    }

    //Current Progress
    private CurrentProgress CurrentProgress = new();
    public event Action OnCraftStarted;
    public event Action OnCraftStopped;

    //For loaded inputs
    public float SecondsLeft = 0;

    //For effects
    public List<CraftingTask> CraftingTasks = new();

    //For Save/Load
    public List<Input> Inputs = new();

    public void Clear() {
      Inputs.Clear();
      CraftingTasks.Clear();
    }

    public void Load(WorkstationsData data) {
      if (data.Inputs.Count <= 0) {
        return;
      }

      var inputs = data.Inputs;

      CraftStartTime = Helper.GetCurrentTime();
      var currentDateTime = CraftStartTime;
      //only for first input
      var secondsLeft = data.SecondsLeft;

      foreach (var input in inputs) {
        //TODO refactor, need function to get by ID
        var recipe = recipes.First(x => x.Result.Id == input.ItemId);
        var inputTotalCraftTime = input.Count * recipe.CraftingTime;

        if (secondsLeft > 0) {
          var timePassed = inputTotalCraftTime - secondsLeft;
          CraftStartTime = currentDateTime.AddSeconds(-timePassed);
          currentDateTime = CraftStartTime;
          secondsLeft = 0;
        }

        Inputs.Add(new Input { Recipe = recipe, Count = input.Count });
        AddCraftingTask(input.ItemId, recipe.CraftingTime, input.Count, currentDateTime);

        currentDateTime = currentDateTime.AddSeconds(input.Count * recipe.CraftingTime);
      }
    }

    public void ProcessCraftedInputs() {
      var inputs = Inputs;
      if (inputs.Count == 0) {
        return;
      }

      var haveCraftingItems = false;
      var startTime = CraftStartTime;

      for (var i = 0; i < inputs.Count; i++) {
        var input = inputs[i];
        var craftedCount = 0;

        while (!haveCraftingItems && craftedCount < input.Count) {
          var endCraftTime = startTime.AddSeconds(input.Recipe.CraftingTime);
          startTime = endCraftTime;
          if (Helper.GetCurrentTime() >= endCraftTime) {
            craftedCount++;
          }
          else {
            haveCraftingItems = true;
            break;
          }
        }

        if (craftedCount > 0) {
          OutputInventory.AddItem(new Item(input.Recipe.Result), craftedCount);
          ConsumeFuel(input.Recipe, craftedCount);
        }

        if (craftedCount >= input.Count) {
          inputs.RemoveAt(i--);
          continue;
        }

        input.Count -= craftedCount;
        inputs[i] = input;
      }
    }

    public void ConsumeFuel(Recipe recipe, int count) {
      if (FuelInventory == null) {
        return;
      }

      var totalCount = count * recipe.Fuel.Amount;

      FuelInventory.RemoveItem(recipe.Fuel.Material.Id, totalCount);
    }

    public float CalculateTimeLeft(Input input) {
      var currentTime = Helper.GetCurrentTime();
      var elapsedTime = (float)(currentTime - CraftStartTime).TotalSeconds;
      var totalTime = input.Count * input.Recipe.CraftingTime;
      return Mathf.Clamp((totalTime - elapsedTime), 0, totalTime);
    }

    public void UpdateCraftingTasks() {
      CraftingTasks.Clear();
      if (Inputs.Count <= 0) {
        return;
      }

      var currentDateTime = CraftStartTime;
      foreach (var input in Inputs) {
        AddCraftingTask(input.Recipe.Result.Id, input.Recipe.CraftingTime, input.Count, currentDateTime);
        currentDateTime = currentDateTime.AddSeconds(input.Count * input.Recipe.CraftingTime);
      }
    }

    private void AddCraftingTask(string id, int craftTime, int count, DateTime start) {
      for (var i = 1; i <= count; i++) {
        var endTime = start.AddSeconds(craftTime * i);
        CraftingTasks.Add(new CraftingTask(id, endTime));
      }
    }

    public CraftingTask? RemoveFirstTaskIfEnded() {
      if (CraftingTasks.Count <= 0) {
        return null;
      }

      var currentTime = Helper.GetCurrentTime();
      var firstTask = CraftingTasks[0];
      if (firstTask.FinishTime > currentTime) {
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

    public void SetProgress(int craftTime, float currentTime) {
      CurrentProgress.CraftTime = craftTime;
      CurrentProgress.CurrentTime = currentTime;
    }

    public void UpdateProgress(float currentTime) {
      CurrentProgress.CurrentTime = currentTime;
    }

    public void ResetProgress() {
      CurrentProgress.CraftTime = 0;
      CurrentProgress.CurrentTime = 0;
    }

    public float GetProgressTime() => CurrentProgress.CurrentTime;
    public int GetProgressCraftTime() => CurrentProgress.CraftTime;
  }
}