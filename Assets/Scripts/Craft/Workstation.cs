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

  [Serializable]
  public class Workstation {
    private WorkstationObject workstationObject;
    private long craftStartTimestampMillis;
    private string id;
    private InventoryObject outputInventory;
    private InventoryObject fuelInventory;
    private InventoryObject inventory;

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
    public bool ShowSuccessCraftMessages => WorkstationObject.ShowSuccessCraftMessages;
    public RecipeType RecipeType => WorkstationObject.RecipeType;
    public InventoryType OutputInventoryType => WorkstationObject.OutputInventoryType;
    public InventoryType FuelInventoryType => WorkstationObject.FuelInventoryType;
    public int CraftSlotsCount => WorkstationObject.CraftSlotsCount;

    #region Tmp fields for tasks

    private int? totalFuel;
    private string[] outputSlotsIds;
    private int[] outputSlotsFreeCounts;
    private int outputFreeSlotsCount;

    private void InitTmpFuel() {
      totalFuel = GetFuelInventory()?.GetTotalCount();
    }

    private void InitTmpOutput(bool mainInventory = false) {
      (outputSlotsIds, outputSlotsFreeCounts, outputFreeSlotsCount) = OutputSpaces(mainInventory);
    }

    private void InitTmpFieldsForLoad() {
      InitTmpFuel();
      InitTmpOutput();
    }

    private void ClearTmpFuel() {
      totalFuel = null;
    }

    private void ClearTmpOutput() {
      outputSlotsIds = null;
      outputSlotsFreeCounts = null;
      outputFreeSlotsCount = 0;
    }

    private void ClearTmpFieldsForLoad() {
      ClearTmpFuel();
      ClearTmpOutput();
    }

    private bool HaveTmpFuel() {
      if (!totalFuel.HasValue) {
        return true;
      }

      return totalFuel > 0;
    }

    private bool CanConsumeTmpFuel(int fuelCount) {
      if (!totalFuel.HasValue) {
        return true;
      }

      return (totalFuel - fuelCount) >= 0;
    }

    private void ConsumeTmpFuel(int count) {
      if (!totalFuel.HasValue) {
        return;
      }

      totalFuel -= count;
    }

    private bool HaveTmpOutputSpace(ItemObject itemObj, int count = 1) {
      for (var i = 0; i < outputSlotsIds.Length; i++) {
        var slotItemId = outputSlotsIds[i];

        if (slotItemId != itemObj.Id) {
          continue;
        }

        var freeCount = outputSlotsFreeCounts[i];
        if (freeCount >= count) {
          return true;
        }

        count = Math.Abs(freeCount - count);
      }

      return outputFreeSlotsCount * itemObj.MaxStackSize >= count;
    }

    private void AddToTmpOutput(ItemObject itemObj, int count = 1) {
      for (var i = 0; i < outputSlotsIds.Length; i++) {
        var slotItemId = outputSlotsIds[i];

        if (slotItemId != itemObj.Id) {
          continue;
        }

        var freeCount = outputSlotsFreeCounts[i];
        if (freeCount >= count) {
          outputSlotsFreeCounts[i] -= count;
          return;
        }

        outputSlotsFreeCounts[i] = 0;
        count -= freeCount;
      }

      if (outputFreeSlotsCount == 0) {
        return;
      }

      while (count > 0 && outputFreeSlotsCount > 0) {
        var firstFreeCount = outputSlotsIds.Length - outputFreeSlotsCount;
        outputSlotsIds[firstFreeCount] = itemObj.Id;
        outputSlotsFreeCounts[firstFreeCount] = itemObj.MaxStackSize - count;
        outputFreeSlotsCount--;
      }
    }

    #endregion

    public Workstation(WorkstationObject workstationObject, string id) {
      this.workstationObject = workstationObject;
      this.id = id;
    }

    public InventoryObject GetOutputInventory() {
      if (outputInventory == null) {
        outputInventory =
          GameManager.Instance.PlayerInventory.GetInventoryByTypeAndId(OutputInventoryType, Id);
      }

      return outputInventory;
    }

    public InventoryObject GetFuelInventory() {
      if (fuelInventory == null) {
        fuelInventory =
          GameManager.Instance.PlayerInventory.GetInventoryByTypeAndId(FuelInventoryType, Id);
      }

      return fuelInventory;
    }

    public InventoryObject GetInventory() {
      if (inventory == null) {
        inventory =
          GameManager.Instance.PlayerInventory.GetInventory();
      }

      return inventory;
    }

    public void Load(WorkstationsData data) {
      if (data.Inputs.Count <= 0) {
        return;
      }

      var inputs = data.Inputs;
      InitTmpFieldsForLoad();

      craftStartTimestampMillis = Helper.GetCurrentTimestampMillis();
      var currentCraftStartTimestampMillis = craftStartTimestampMillis;
      //only for first input
      var millisecondsLeft = data.MillisecondsLeft;

      foreach (var input in inputs) {
        var recipe = WorkstationObject.RecipeDB.ItemsMap[input.RecipeId];

        Inputs.Add(new Input { Recipe = recipe, Count = input.Count });

        if (!ShowSuccessCraftMessages || !HaveTmpFuel() || !HaveTmpOutputSpace(recipe.Result)) {
          continue;
        }

        var inputTotalCraftMillis = (long)input.Count * recipe.CraftingTime * 1000;

        if (millisecondsLeft > 0) {
          var millisecondsPassed = inputTotalCraftMillis - millisecondsLeft;
          craftStartTimestampMillis = currentCraftStartTimestampMillis - millisecondsPassed;
          currentCraftStartTimestampMillis = craftStartTimestampMillis;
          millisecondsLeft = 0;
        }

        AddCraftingTask(recipe, input.Count, currentCraftStartTimestampMillis);

        currentCraftStartTimestampMillis += inputTotalCraftMillis;
      }

      ClearTmpFieldsForLoad();
    }

    public void ProcessCraftedInputs() {
      if (Inputs.Count == 0) {
        return;
      }

      var currentTimeInMilliseconds = Helper.GetCurrentTimestampMillis();
      var currentCraftStartTimestampMillis = craftStartTimestampMillis;
      InitTmpFieldsForLoad();

      for (var i = 0; i < Inputs.Count; i++) {
        var input = Inputs[i];
        var recipe = input.Recipe;
        var craftedCount = 0;

        for (var j = 0; j < input.Count; j++) {
          var itemEndTimeInMilliseconds = currentCraftStartTimestampMillis + (recipe.CraftingTime * 1000);

          if (currentTimeInMilliseconds >= itemEndTimeInMilliseconds
              && HaveTmpFuel() && HaveTmpOutputSpace(recipe.Result)) {
            craftedCount++;
            currentCraftStartTimestampMillis = itemEndTimeInMilliseconds; // Move to next item's start

            ConsumeTmpFuel(recipe.Fuel.Amount);
            AddToTmpOutput(recipe.Result);
          }
          else {
            // Stop processing further — this item isn't done yet
            break;
          }
        }

        // Process fully crafted items
        if (craftedCount > 0) {
          GetOutputInventory().AddItem(new Item(recipe.Result), craftedCount);
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

      ClearTmpFieldsForLoad();
    }

    public bool CanCraft(Recipe recipe) {
      return HaveFuelForCraft(recipe) && OutputSpaceForItem(recipe.Result) > 0;
    }

    public bool CanCancelCraft(Recipe recipe, int count) {
      InitTmpOutput(true);

      foreach (var material in recipe.RequiredMaterials) {
        if (!HaveTmpOutputSpace(material.Material, count)) {
          return false;
        }

        AddToTmpOutput(material.Material, count);
      }

      ClearTmpOutput();
      return true;
    }

    private int GetFuelCount(Recipe recipe) {
      if (recipe.Fuel == null) {
        return -1;
      }

      GetFuelInventory();
      if (fuelInventory == null) {
        return -1;
      }

      var total = fuelInventory.GetTotalCount() / recipe.Fuel.Amount;

      return total;
    }

    public bool HaveFuelForCraft(Recipe recipe) {
      var count = GetFuelCount(recipe);

      return count == -1 || count > 0;
    }

    public int OutputSpaceForItem(ItemObject itemObj) {
      return GetOutputInventory().FreeSpaceForItem(itemObj);
    }

    private (string[], int[], int) OutputSpaces(bool mainInventory = false) {
      return mainInventory ? GetInventory().FreeSpaces() : GetOutputInventory().FreeSpaces();
    }

    public void ConsumeFuel(Recipe recipe, int count) {
      GetFuelInventory();
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

    public void UpdateMillisecondsLeftByCurrentTime(Recipe recipe, int count) {
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
      if (!ShowSuccessCraftMessages || Inputs.Count <= 0) {
        return;
      }

      InitTmpFieldsForLoad();

      var currentDateTimestampMillis = CraftStartTimestampMillis;
      foreach (var input in Inputs) {
        if (!HaveTmpFuel() || !HaveTmpOutputSpace(input.Recipe.Result)) {
          break;
        }

        AddCraftingTask(input.Recipe, input.Count, currentDateTimestampMillis);
        currentDateTimestampMillis += input.Count * input.Recipe.CraftingTime * 1000;
      }

      ClearTmpFieldsForLoad();
    }

    private void AddCraftingTask(Recipe recipe, int count, long startInMilliseconds) {
      for (var i = 1; i <= count; i++) {
        if (!CanConsumeTmpFuel(recipe.Fuel.Amount) || !HaveTmpOutputSpace(recipe.Result)) {
          break;
        }

        var endTime = startInMilliseconds + (recipe.CraftingTime * i * 1000);
        CraftingTasks.Add(new CraftingTask(recipe.Result.Id, endTime));

        ConsumeTmpFuel(recipe.Fuel.Amount);
        AddToTmpOutput(recipe.Result);
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
      var maxStack = recipe.Result.MaxStackSize;

      while (count > 0) {
        var addCount = count > maxStack ? maxStack : count;
        Inputs.Add(new Input { Recipe = recipe, Count = addCount });
        count -= addCount;
      }

      if (Inputs.Count >= 1) {
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
        ResetMillisecondsLeft();
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
      if (inputPosition == 0) {
        ResetMillisecondsLeft();
      }

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