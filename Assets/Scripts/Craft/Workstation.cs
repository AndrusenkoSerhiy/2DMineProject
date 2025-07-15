using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Inventory;
using Messages;
using SaveSystem;
using Scriptables.Craft;
using Scriptables.Items;
using UnityEngine;
using World;

namespace Craft {
  [Serializable]
  public class Input {
    public int Count;
    public Recipe Recipe;
  }

  [Serializable]
  public class CurrentProgress {
    public bool IsCrafting;
    public bool Finished;
    public int CraftTimeForOneInMilliseconds;
    public float CurrentTimeInMilliseconds;
    public float MillisecondsLeft;

    public CurrentProgress() {
      Reset();
    }

    public void Reset() {
      IsCrafting = false;
      Finished = true;
      CraftTimeForOneInMilliseconds = 0;
      CurrentTimeInMilliseconds = 0f;
      MillisecondsLeft = 0f;
    }
  }

  [Serializable]
  public class Workstation {
    private readonly WorkstationObject workstationObject;
    private readonly string id;
    private readonly GameManager gameManager;
    private readonly MessagesManager messageManager;
    private readonly CraftManager craftManager;
    private readonly InventoriesPool inventoriesPool;
    private List<Inventory.Inventory> outputInventories;
    private Inventory.Inventory fuelInventory;
    private Inventory.Inventory inventory;
    private int tickDelay = 200;
    private bool outputEventsAdded;

    private Recipe currentRecipe;
    private string[] recipeIngredientsIds;

    public CurrentProgress CurrentProgress;
    public List<Input> Inputs = new();
    public WorkstationObject WorkstationObject => workstationObject;
    public string Id => id;
    public Recipe CurrentRecipe => currentRecipe;
    public bool ShowSuccessCraftMessages => WorkstationObject.ShowSuccessCraftMessages;
    public RecipeType RecipeType => WorkstationObject.RecipeType;
    public InventoryType OutputInventoryType => WorkstationObject.OutputInventoryType;
    public InventoryType FuelInventoryType => WorkstationObject.FuelInventoryType;
    public int CraftSlotsCount => WorkstationObject.CraftSlotsCount;

    #region Events

    public event Action<Recipe> OnRecipeChanged;
    public event Action<Input> OnAfterAddItemToInputs;
    public event Action OnCraftStarted;
    public event Action<Input, int> OnCraftCanceled;
    public event Action OnCraftPaused;
    public event Action OnAllInputsCanceled;
    public event Action OnInputAllCrafted;
    public event Action OnItemCrafted;
    public event Action OnFuelConsumed;

    #endregion

    public Workstation(WorkstationObject workstationObject, string id) {
      this.workstationObject = workstationObject;
      this.id = id;
      gameManager = GameManager.Instance;
      messageManager = gameManager.MessagesManager;
      craftManager = gameManager.CraftManager;
      inventoriesPool = gameManager.PlayerInventory.InventoriesPool;
      CurrentProgress = new CurrentProgress();

      gameManager.OnGamePaused += PauseCraft;
      gameManager.OnGameResumed += ResumeCrafting;
    }

    public static string GenerateId(BuildingDataObject buildObject, WorkstationObject stationObject) {
      return buildObject != null
        ? $"{stationObject.name}_{buildObject.transform.position.x}_{buildObject.transform.position.y}".ToLower()
        : stationObject.name.ToLower();
    }

    #region Stop and Drop

    public void StopAndDropItems(Vector3 spawnPosition) {
      PauseCraft();

      var playerInventory = gameManager.PlayerInventory;

      //spawn resources from inputs
      foreach (var input in Inputs) {
        foreach (var material in input.Recipe.RequiredMaterials) {
          var totalCount = input.Count * material.Amount;
          var item = new Item(material.Material);
          playerInventory.SpawnItem(item, totalCount, spawnPosition);
        }
      }

      Inputs.Clear();

      //spawn resources from output
      var outputInventory = gameManager.PlayerInventory.GetInventoryByTypeAndId(OutputInventoryType, Id);
      if (outputInventory != null) {
        foreach (var slot in outputInventory.Slots) {
          if (slot.isEmpty) {
            continue;
          }

          playerInventory.SpawnItem(slot.Item, slot.amount, spawnPosition);
        }

        outputInventory.Clear();
      }

      //spawn resources from fuel
      var fuelInventoryTmp = GetFuelInventory();
      if (fuelInventoryTmp != null) {
        foreach (var slot in fuelInventoryTmp.Slots) {
          if (slot.isEmpty) {
            continue;
          }

          playerInventory.SpawnItem(slot.Item, slot.amount, spawnPosition);
        }

        fuelInventoryTmp.Clear();
      }

      CancelCraftProcess();
      CurrentProgress.Reset();
    }

    #endregion

    #region Inventories

    public List<Inventory.Inventory> GetOutputInventories() {
      if (outputInventories == null) {
        outputInventories = OutputInventoryType == InventoryType.Inventory
          ? inventoriesPool.Inventories
          : new List<Inventory.Inventory>()
            { gameManager.PlayerInventory.GetInventoryByTypeAndId(OutputInventoryType, Id) };
      }

      return outputInventories;
    }

    public Inventory.Inventory GetFuelInventory() {
      if (fuelInventory == null) {
        fuelInventory =
          gameManager.PlayerInventory.GetInventoryByTypeAndId(FuelInventoryType, Id);
      }

      return fuelInventory;
    }

    private void AddItemToOutput(Item item, int count) {
      if (count <= 0) {
        return;
      }

      var remainingAmount = count;
      foreach (var inventory in GetOutputInventories()) {
        if (inventory.Type != InventoryType.QuickSlots) {
          continue;
        }

        if (inventory.SlotWithNotFinishedStack(item.info) != null) {
          remainingAmount = inventory.AddItem(item, remainingAmount);
        }

        break;
      }

      if (remainingAmount <= 0) {
        return;
      }

      foreach (var inventory in GetOutputInventories()) {
        remainingAmount = inventory.AddItem(item, remainingAmount);
        if (remainingAmount <= 0) {
          break;
        }
      }
    }

    private void RemoveRecipeResources(string materialId, int totalCount) {
      inventoriesPool.RemoveFromInventoriesPool(materialId, totalCount);
    }

    private void ReturnCraftResourcesToInventory(Item addItem, int totalCount) {
      inventoriesPool.AddItemToInventoriesPool(addItem, totalCount);
    }

    /// <summary>
    /// Moves all items from the output inventory to the inventories in the pool.
    /// Prioritizes completing stacks in the QuickSlots inventory first.
    /// </summary>
    public void MoveAllFromOutput() {
      var outputInventory = outputInventories[0];

      // Complete stacks in QuickSlots inventory first
      foreach (var inventory in inventoriesPool.Inventories) {
        if (inventory.Type != InventoryType.QuickSlots) {
          continue;
        }

        outputInventory.CompleteStacksIfExist(inventory);
        break;
      }

      // Move all remaining items to other inventories
      foreach (var inventory in inventoriesPool.Inventories) {
        outputInventory.MoveAllItemsTo(inventory);
      }
    }

    public bool IsOutputEmpty() {
      return outputInventories[0].IsEmpty();
    }

    #endregion

    #region Handle output events even if craft window is closed, for output type = inventory

    private void AddOutputUpdateEvents() {
      foreach (var outputInventory in GetOutputInventories()) {
        foreach (var output in outputInventory.Slots) {
          output.OnAfterUpdated += OutputUpdateSlotHandler;
        }
      }

      outputEventsAdded = true;
    }

    private void RemoveOutputUpdateEvents() {
      foreach (var outputInventory in outputInventories) {
        foreach (var output in outputInventory.Slots) {
          output.OnAfterUpdated -= OutputUpdateSlotHandler;
        }
      }

      outputEventsAdded = false;
    }

    private void OutputUpdateSlotHandler(SlotUpdateEventData data) {
      StartCrafting();
    }

    private void TryAddOutputSlotsEvents() {
      if (OutputInventoryType != InventoryType.Inventory) {
        return;
      }

      if (outputEventsAdded) {
        return;
      }

      if (Inputs.Count == 0) {
        return;
      }

      AddOutputUpdateEvents();
    }

    private void TryRemoveOutputSlotsEvents() {
      if (OutputInventoryType != InventoryType.Inventory) {
        return;
      }

      if (!outputEventsAdded) {
        return;
      }

      if (Inputs.Count > 0) {
        return;
      }

      RemoveOutputUpdateEvents();
    }

    #endregion

    #region Tmp fields for checks

    private int? totalFuel;
    private List<string> tmpSlotsIds;
    private List<int> tmpSlotsFreeCounts;
    private int tmpFreeSlotsCount;

    private void InitTmpFuel() {
      totalFuel = GetFuelInventory()?.GetTotalCount();
    }

    private void InitTmpOutput() {
      (tmpSlotsIds, tmpSlotsFreeCounts, tmpFreeSlotsCount) = GetSpaces(GetOutputInventories());
    }

    private void InitTmpPool() {
      (tmpSlotsIds, tmpSlotsFreeCounts, tmpFreeSlotsCount) = GetSpaces(inventoriesPool.Inventories);
    }

    private (List<string>, List<int>, int) GetSpaces(List<Inventory.Inventory> inventories) {
      var ids = new List<string>();
      var free = new List<int>();
      var totalFreeSlots = 0;

      foreach (var inventory in inventories) {
        var (invIds, invFree, freeSlots) = inventory.FreeSpaces();

        ids.AddRange(invIds);
        free.AddRange(invFree);
        totalFreeSlots += freeSlots;
      }

      return (ids, free, totalFreeSlots);
    }

    public bool HaveFuelForCraft(Recipe recipe) {
      var count = GetFuelCount(recipe);

      return count == -1 || count > 0;
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

    public int OutputSpaceForItem(ItemObject itemObj) {
      var total = 0;

      foreach (var inventory in GetOutputInventories()) {
        total += inventory.FreeSpaceForItem(itemObj);
      }

      return total;
    }

    private void InitTmpFieldsForLoad() {
      InitTmpFuel();
      InitTmpOutput();
    }

    private void ClearTmpFuel() {
      totalFuel = null;
    }

    private void ClearTmpOutput() {
      tmpSlotsIds = null;
      tmpSlotsFreeCounts = null;
      tmpFreeSlotsCount = 0;
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
      for (var i = 0; i < tmpSlotsIds.Count; i++) {
        var slotItemId = tmpSlotsIds[i];

        if (slotItemId != itemObj.Id) {
          continue;
        }

        var freeCount = tmpSlotsFreeCounts[i];
        if (freeCount >= count) {
          return true;
        }

        count = Math.Abs(freeCount - count);
      }

      return tmpFreeSlotsCount * itemObj.MaxStackSize >= count;
    }

    private void AddToTmpOutput(ItemObject itemObj, int count = 1) {
      for (var i = 0; i < tmpSlotsIds.Count; i++) {
        var slotItemId = tmpSlotsIds[i];

        if (slotItemId != itemObj.Id) {
          continue;
        }

        var freeCount = tmpSlotsFreeCounts[i];
        if (freeCount >= count) {
          tmpSlotsFreeCounts[i] -= count;
          return;
        }

        tmpSlotsFreeCounts[i] = 0;
        count -= freeCount;
      }

      if (tmpFreeSlotsCount == 0) {
        return;
      }

      while (count > 0 && tmpFreeSlotsCount > 0) {
        var canAdd = count > itemObj.MaxStackSize ? itemObj.MaxStackSize : count;

        tmpSlotsIds.Add(itemObj.Id);
        tmpSlotsFreeCounts.Add((itemObj.MaxStackSize - canAdd));
        tmpFreeSlotsCount--;

        count -= canAdd;
      }
    }

    #endregion

    #region Timer

    private CancellationTokenSource cancellationTokenSource;

    public async void StartCrafting() {
      if (CurrentProgress.IsCrafting
          || Inputs.Count == 0
          || !HaveFuelForCraft(CurrentRecipe)
          || (!CurrentProgress.Finished && OutputSpaceForItem(CurrentRecipe.Result) <= 0)
         ) {
        return;
      }

      if (CurrentProgress.Finished) {
        var input = Inputs[0];
        var oneItemCraftTimeMillis = input.Recipe.CraftingTime * 1000;
        var totalCraftTimeMillis = oneItemCraftTimeMillis * input.Count;

        CurrentProgress = new CurrentProgress {
          IsCrafting = true,
          Finished = false,
          CraftTimeForOneInMilliseconds = oneItemCraftTimeMillis,
          CurrentTimeInMilliseconds = oneItemCraftTimeMillis,
          MillisecondsLeft = totalCraftTimeMillis
        };
      }
      else {
        CurrentProgress.IsCrafting = true;
      }

      cancellationTokenSource = new CancellationTokenSource();

      try {
        await CraftingAsync(cancellationTokenSource.Token);
      }
      catch (TaskCanceledException) {
        CancelCraft(true);
        return;
      }

      FinishInputCrafting();
    }

    private async Task CraftingAsync(CancellationToken token) {
      OnCraftStarted?.Invoke();

      var input = Inputs[0];
      var stopwatch = new Stopwatch();
      stopwatch.Start();

      while (input.Count > 0 && !token.IsCancellationRequested) {
        var startTime = stopwatch.ElapsedMilliseconds;

        await Task.Delay(tickDelay, token);

        var elapsed = stopwatch.ElapsedMilliseconds - startTime;
        CurrentProgress.MillisecondsLeft -= elapsed;
        CurrentProgress.CurrentTimeInMilliseconds -= elapsed;

        var timeLeftWithoutCurrent = (input.Count - 1) * CurrentProgress.CraftTimeForOneInMilliseconds;
        if (CurrentProgress.MillisecondsLeft > timeLeftWithoutCurrent) {
          continue;
        }

        if (!CanCraft(input.Recipe)) {
          CurrentProgress.MillisecondsLeft = timeLeftWithoutCurrent;
          CancelCraft(true);
          return;
        }

        CurrentProgress.CurrentTimeInMilliseconds = CurrentProgress.CraftTimeForOneInMilliseconds;
        ItemCrafted(input.Recipe, 1);
      }

      stopwatch.Stop();
    }

    private void FinishInputCrafting() {
      if (!CurrentProgress.Finished) {
        return;
      }

      CancelCraft();

      OnInputAllCrafted?.Invoke();

      if (Inputs.Count > 0) {
        StartCrafting();
      }
    }

    public void CancelCraft(bool isPaused = false) {
      CancelCraftProcess();

      if (isPaused) {
        CurrentProgress.IsCrafting = false;
        OnCraftPaused?.Invoke();
      }
      else {
        CurrentProgress.Reset();
      }
    }

    public void PauseCraft() {
      CancelCraft(true);
    }

    public void ResumeCrafting() {
      if (CurrentProgress == null || CurrentProgress.Finished || CurrentProgress.IsCrafting) {
        return;
      }

      StartCrafting();
    }

    private void CancelCraftProcess() {
      if (cancellationTokenSource == null) {
        return;
      }

      cancellationTokenSource?.Cancel();
      cancellationTokenSource?.Dispose();
      cancellationTokenSource = null;
    }

    #endregion

    #region Load

    public void Load(WorkstationsData data) {
      if (data.Inputs.Count <= 0) {
        return;
      }

      var inputs = data.Inputs;

      foreach (var input in inputs) {
        var recipe = WorkstationObject.RecipeDB.ItemsMap[input.RecipeId];

        AddItemToInputs(recipe, input.Count);
      }

      CurrentProgress = data.CurrentProgress;
      SetRecipe(Inputs[0].Recipe);

      StartCrafting();
    }

    #endregion

    #region Actions

    public void SetRecipe(Recipe recipe) {
      currentRecipe = recipe;
      FillRecipeIngredientsIds();
      OnRecipeChanged?.Invoke(recipe);
    }

    public void CraftRequested(int currentCount) {
      //remove resources from inventory pool
      foreach (var item in CurrentRecipe.RequiredMaterials) {
        var totalCount = currentCount * item.Amount;
        RemoveRecipeResources(item.Material.Id, totalCount);
      }

      AddItemToInputs(CurrentRecipe, currentCount);

      StartCrafting();
    }

    private void ItemCrafted(Recipe inputRecipe, int count) {
      RemoveInputCountFromInputs(count);

      var item = new Item(inputRecipe.Result);

      ConsumeFuel(inputRecipe, count);
      AddItemToOutput(item, count);

      OnItemCrafted?.Invoke();

      if (WorkstationObject.ShowSuccessCraftMessages && !craftManager.IsWindowOpen(Id)) {
        messageManager.ShowCraftMessage(inputRecipe.Result, 1);
      }
    }

    public bool CancelInput(Input inputData, int position) {
      if (!CanCancelCraft(inputData)) {
        messageManager.ShowSimpleMessage("You can't cancel craft. Not enough space in inventory.");
        return false;
      }

      if (!RemoveInputFromInputs(position)) {
        return false;
      }

      foreach (var item in inputData.Recipe.RequiredMaterials) {
        var totalCount = inputData.Count * item.Amount;
        var addItem = new Item(item.Material);
        ReturnCraftResourcesToInventory(addItem, totalCount);
      }

      //Stop input crafting
      if (position == 0) {
        CancelCraft();
        StartCrafting();
      }

      OnCraftCanceled?.Invoke(inputData, position);

      if (Inputs.Count == 0) {
        OnAllInputsCanceled?.Invoke();
      }

      return true;
    }

    public void ConsumeFuel(Recipe recipe, int count) {
      GetFuelInventory();
      if (fuelInventory == null) {
        return;
      }

      var totalCount = count * recipe.Fuel.Amount;

      fuelInventory.RemoveItem(recipe.Fuel.Material.Id, totalCount);
      OnFuelConsumed?.Invoke();
    }

    #endregion

    #region Inputs

    private void AddItemToInputs(Recipe recipe, int count) {
      var maxStack = recipe.Result.MaxStackSize;
      while (count > 0) {
        var addCount = count > maxStack ? maxStack : count;

        var input = new Input {
          Count = addCount,
          Recipe = recipe
        };
        Inputs.Add(input);

        OnAfterAddItemToInputs?.Invoke(input);

        count -= addCount;
      }

      TryAddOutputSlotsEvents();
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
        CurrentProgress.Finished = true;
      }
      else {
        Inputs[inputPosition] = input;
      }

      TryRemoveOutputSlotsEvents();
    }

    public bool RemoveInputFromInputs(int inputPosition) {
      if (!IsValidInputPosition(inputPosition)) {
        return false;
      }

      Inputs.RemoveAt(inputPosition);

      TryRemoveOutputSlotsEvents();

      return true;
    }

    private bool IsValidInputPosition(int position) {
      return position >= 0 && position < Inputs.Count;
    }

    #endregion

    public string[] GetRecipeIngredientsIds() => recipeIngredientsIds;

    private void FillRecipeIngredientsIds() {
      var materials = CurrentRecipe.RequiredMaterials;
      recipeIngredientsIds = new string[materials.Count];

      for (var i = 0; i < materials.Count; i++) {
        var resource = materials[i];
        recipeIngredientsIds[i] = resource.Material.Id;
      }
    }

    public bool CanCraft(Recipe recipe) {
      return HaveFuelForCraft(recipe) && OutputSpaceForItem(recipe.Result) > 0;
    }

    public bool CanCancelCraft(Input inputData) {
      InitTmpPool();

      foreach (var material in inputData.Recipe.RequiredMaterials) {
        if (!HaveTmpOutputSpace(material.Material, inputData.Count)) {
          return false;
        }

        AddToTmpOutput(material.Material, inputData.Count);
      }

      ClearTmpOutput();
      return true;
    }
  }
}