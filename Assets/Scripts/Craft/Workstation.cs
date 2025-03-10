using System;
using System.Collections.Generic;
using Inventory;
using SaveSystem;
using Scriptables.Craft;
using Scriptables.Items;

namespace Craft {
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

  public class Workstation {
    private WorkstationObject workstationObject;
    private long craftStartTimestampMillis;
    private string id;

    //Current Progress
    private CurrentProgress CurrentProgress;

    //For loaded inputs
    private long millisecondsLeft;

    //For effects
    public List<CraftingTask> CraftingTasks = new();

    //For Save/Load
    public List<Input> Inputs = new();

    public event Action OnCraftStarted;
    public event Action OnCraftStopped;

    public WorkstationObject WorkstationObject => workstationObject;
    public long CraftStartTimestampMillis => craftStartTimestampMillis;
    public long MillisecondsLeft => millisecondsLeft;
    public string Id => id;
    public bool PlayEffectsWhenClosed => WorkstationObject.PlayEffectsWhenClosed;
    public RecipeType RecipeType => WorkstationObject.RecipeType;
    public InventoryType OutputInventoryType => WorkstationObject.OutputInventoryType;
    public InventoryType FuelInventoryType => WorkstationObject.FuelInventoryType;

    public Workstation(WorkstationObject workstationObject, string id) {
      this.workstationObject = workstationObject;
      this.id = id;
    }

    public void Load(WorkstationsData data) {
      if (data.Inputs.Count <= 0) {
        return;
      }

      var inputs = data.Inputs;
      var fuelInventory = GameManager.Instance.PlayerInventory.GetInventoryByTypeAndId(FuelInventoryType, Id);
      var totalFuel = fuelInventory?.GetTotalCount();

      craftStartTimestampMillis = Helper.GetCurrentTimestampMillis();
      var currentCraftStartTimestampMillis = craftStartTimestampMillis;
      //only for first input
      var millisecondsLeft = data.MillisecondsLeft;

      foreach (var input in inputs) {
        var recipe = WorkstationObject.RecipeDB.ItemsMap[input.RecipeId];

        Inputs.Add(new Input { Recipe = recipe, Count = input.Count });

        if (totalFuel.HasValue && totalFuel <= 0) {
          continue;
        }

        var inputTotalCraftMillis = (long)input.Count * recipe.CraftingTime * 1000;

        if (millisecondsLeft > 0) {
          var millisecondsPassed = inputTotalCraftMillis - millisecondsLeft;
          craftStartTimestampMillis = currentCraftStartTimestampMillis - millisecondsPassed;
          currentCraftStartTimestampMillis = craftStartTimestampMillis;
          millisecondsLeft = 0;
        }

        AddCraftingTask(recipe, input.Count, currentCraftStartTimestampMillis, ref totalFuel);

        currentCraftStartTimestampMillis += inputTotalCraftMillis;
      }
    }

    public void ProcessCraftedInputs() {
      if (Inputs.Count == 0) {
        return;
      }

      var currentTimeInMilliseconds = Helper.GetCurrentTimestampMillis();
      var currentCraftStartTimestampMillis = craftStartTimestampMillis;

      for (var i = 0; i < Inputs.Count; i++) {
        var input = Inputs[i];
        var recipe = input.Recipe;
        var craftedCount = 0;

        for (var j = 0; j < input.Count; j++) {
          var itemEndTimeInMilliseconds = currentCraftStartTimestampMillis + (recipe.CraftingTime * 1000);

          if (currentTimeInMilliseconds >= itemEndTimeInMilliseconds && HaveFuelForCraft(recipe)) {
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
          var outputInventory =
            GameManager.Instance.PlayerInventory.GetInventoryByTypeAndId(OutputInventoryType, Id);
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
      craftStartTimestampMillis = currentCraftStartTimestampMillis;

      if (Inputs.Count == 0) {
        ResetMillisecondsLeft();
      }
    }

    public bool HaveFuelForCraft(Recipe recipe) {
      if (recipe.Fuel == null) {
        return true;
      }

      var fuelInventory = GameManager.Instance.PlayerInventory.GetInventoryByTypeAndId(FuelInventoryType, Id);
      if (fuelInventory == null) {
        return true;
      }

      var total = fuelInventory.GetTotalCount() / recipe.Fuel.Amount;

      return total > 0;
    }

    public void ConsumeFuel(Recipe recipe, int count) {
      var fuelInventory = GameManager.Instance.PlayerInventory.GetInventoryByTypeAndId(FuelInventoryType, Id);
      if (fuelInventory == null) {
        return;
      }

      var totalCount = count * recipe.Fuel.Amount;

      fuelInventory.RemoveItem(recipe.Fuel.Material.Id, totalCount);
    }

    public long CalculateTimeLeftInMilliseconds(Recipe recipe, int count) {
      var currentTimeInMilliseconds = Helper.GetCurrentTimestampMillis();
      var elapsedTimeInMilliseconds = currentTimeInMilliseconds - CraftStartTimestampMillis;
      var totalTimeInMilliseconds = (long)count * recipe.CraftingTime * 1000;
      return Math.Clamp((totalTimeInMilliseconds - elapsedTimeInMilliseconds), 0, totalTimeInMilliseconds);
    }

    public void UpdateMillisecondsLeft(Recipe recipe, int count) {
      millisecondsLeft = CalculateTimeLeftInMilliseconds(recipe, count);
    }

    public void ResetMillisecondsLeft() {
      millisecondsLeft = 0;
    }

    public void UpdateMillisecondsLeft(long milliseconds) {
      millisecondsLeft = milliseconds;
    }
    
    public void UpdateCraftStartTimestampMillis(long milliseconds) {
      craftStartTimestampMillis = milliseconds;
    }

    public void UpdateCraftingTasks() {
      CraftingTasks.Clear();
      if (Inputs.Count <= 0) {
        return;
      }

      var fuelInventory = GameManager.Instance.PlayerInventory.GetInventoryByTypeAndId(FuelInventoryType, Id);
      var totalFuel = fuelInventory?.GetTotalCount();

      var currentDateTimestampMillis = CraftStartTimestampMillis;
      foreach (var input in Inputs) {
        if (totalFuel.HasValue && totalFuel <= 0) {
          break;
        }

        AddCraftingTask(input.Recipe, input.Count, currentDateTimestampMillis, ref totalFuel);
        currentDateTimestampMillis += input.Count * input.Recipe.CraftingTime * 1000;
      }
    }

    private void AddCraftingTask(Recipe recipe, int count, long startInMilliseconds, ref int? totalFuel) {
      for (var i = 1; i <= count; i++) {
        if (totalFuel.HasValue && (totalFuel - recipe.Fuel.Amount) < 0) {
          break;
        }

        var endTime = startInMilliseconds + (recipe.CraftingTime * i * 1000);
        CraftingTasks.Add(new CraftingTask(recipe.Result.Id, endTime));

        if (totalFuel.HasValue) {
          totalFuel -= recipe.Fuel.Amount;
        }
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
        ResetMillisecondsLeft();
        OnCraftStopped?.Invoke();
      }
    }

    public void RemoveInputFromInputs(int inputPosition) {
      if (!IsValidInputPosition(inputPosition)) {
        return;
      }

      Inputs.RemoveAt(inputPosition);

      if (Inputs.Count <= 0) {
        ResetMillisecondsLeft();
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