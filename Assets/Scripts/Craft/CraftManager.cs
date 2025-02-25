using System;
using System.Linq;
using Inventory;
using Scriptables.Craft;
using Scriptables.Items;
using UnityEngine;
using UnityEngine.UI;
using UnityServiceLocator;

namespace Craft {
  public class CraftManager : MonoBehaviour, IInventoryDropZoneUI {
    [SerializeField] private Workstation station;
    [SerializeField] private Button takeAllButton;
    [SerializeField] private bool preventItemDrop;

    private IRecipesManager recipesManager;
    private IRecipeDetail detail;
    private ICraftActions craftActions;
    private PlayerInventory playerInventory;
    private IInputItems inputItems;
    private ITotalAmount totalAmount;

    private bool started;

    public bool PreventItemDropIn => preventItemDrop;

    public void Awake() {
      ServiceLocator.For(this).Register<Workstation>(station);
    }

    public void Start() {
      Init();
      started = true;
    }

    public void OnEnable() {
      if (!started) {
        return;
      }

      Init();
    }

    private void OnDisable() {
      craftActions.ClearComponent();
      recipesManager.ClearComponent();
      totalAmount.ClearComponent();
      RemoveEvents();
    }

    private void Init() {
      InitReferences();
      AddEvents();
      totalAmount.InitComponent();
      recipesManager.InitComponent();
      craftActions.InitComponent();
    }

    private void InitReferences() {
      if (totalAmount != null) {
        return;
      }

      playerInventory = GameManager.Instance.PlayerInventory;
      totalAmount = ServiceLocator.For(this).Get<ITotalAmount>();
      detail = ServiceLocator.For(this).Get<IRecipeDetail>();
      craftActions = ServiceLocator.For(this).Get<ICraftActions>();
      inputItems = ServiceLocator.For(this).Get<IInputItems>();
      recipesManager = ServiceLocator.For(this).Get<IRecipesManager>();
    }

    private void AddEvents() {
      Debug.Log("CraftManager AddEvents");
      //recipes list
      recipesManager.OnSelected += OnRecipeSelectedHandler;
      //inventory slots
      AddSlotsUpdateEvents();
      //craft input actions
      craftActions.OnCraftRequested += OnCraftRequestedHandler;
      //craft input slots
      AddInputEvents();
      //craft output slots
      AddOutputUpdateEvents();
      takeAllButton.onClick.AddListener(OnTakeAllButtonClickHandler);
    }

    private void RemoveEvents() {
      Debug.Log("CraftManager RemoveEvents");
      //craft output slots
      takeAllButton.onClick.RemoveAllListeners();
      RemoveOutputUpdateEvents();
      //craft input slots
      RemoveInputEvents();
      //craft input actions
      craftActions.OnCraftRequested -= OnCraftRequestedHandler;
      //inventory slots
      RemoveSlotsUpdateEvents();
      //recipes list
      recipesManager.OnSelected -= OnRecipeSelectedHandler;
    }

    private void OnCraftRequestedHandler(int count) {
      var recipe = recipesManager.Recipe;

      //remove resources from inventory
      foreach (var item in recipe.RequiredMaterials) {
        var totalCount = count * item.Amount;
        //playerInventory.inventory.RemoveItem(item.Material.Id, totalCount);
        totalAmount.RemoveFromInventoriesPool(item.Material.Id, totalCount);
      }

      inputItems.SetRecipe(count, recipe);
      station.AddItemToCraftTotal(recipe.Result, count);
      station.AddToCraftInputsItemsIds(recipe.Result.Id);

      craftActions.UpdateAndPrintInputCount();
    }

    private void OnRecipeSelectedHandler(Recipe recipe) {
      detail.SetRecipeDetails(recipe);

      craftActions.SetRecipe(recipe);
      craftActions.UpdateAndPrintInputCount(true);
    }

    private void AddSlotsUpdateEvents() {
      totalAmount.onResourcesTotalUpdate += SlotAmountUpdateHandler;
    }

    private void RemoveSlotsUpdateEvents() {
      totalAmount.onResourcesTotalUpdate -= SlotAmountUpdateHandler;
    }

    private void SlotAmountUpdateHandler(string resourceId) {
      var recipeIngredientsIds = detail.GetRecipeIngredientsIds();
      if (recipeIngredientsIds.Length > 0 && recipeIngredientsIds.Contains(resourceId)) {
        UpdateResourcesListAndCount();
      }
    }

    private void UpdateResourcesListAndCount() {
      detail.PrintList();

      craftActions.UpdateAndPrintInputCount();
    }

    private void AddInputEvents() {
      foreach (var input in inputItems.Items) {
        input.OnItemCrafted += AddCraftedItemToOutput;
        input.OnInputAllCrafted += OnInputAllCraftedHandler;
        input.OnCanceled += OnInputCanceledHandler;
      }
    }

    private void RemoveInputEvents() {
      foreach (var input in inputItems.Items) {
        input.OnInputAllCrafted -= OnInputAllCraftedHandler;
        input.OnItemCrafted -= AddCraftedItemToOutput;
        input.OnCanceled -= OnInputCanceledHandler;
      }
    }

    private void AddCraftedItemToOutput(Recipe recipe, int count) {
      station.RemoveCountFromCraftTotal(recipe.Result, count);

      var outputInventory = station.OutputInventory;
      var item = new Item(recipe.Result);

      outputInventory.AddItem(item, count);
    }

    private void OnInputAllCraftedHandler() {
      station.RemoveFromCraftInputsItemsIds();
      inputItems.UpdateWaitInputs();
      craftActions.UpdateAndPrintInputCount();
    }

    private void OnInputCanceledHandler(InputItem inputItem) {
      station.RemoveCountFromCraftTotal(inputItem.Recipe.Result, inputItem.CountLeft);
      station.RemoveFromCraftInputsItemsIds(inputItem.Position);

      inputItems.UpdateTimersStartTimes(inputItem);
      inputItems.UpdateWaitInputs(inputItem.Position);

      //remove resources from inventory
      foreach (var item in inputItem.Recipe.RequiredMaterials) {
        var totalCount = inputItem.CountLeft * item.Amount;
        var addItem = new Item(item.Material);
        playerInventory.inventory.AddItem(addItem, totalCount);
      }
    }

    private void AddOutputUpdateEvents() {
      foreach (var output in station.OutputInventory.GetSlots) {
        output.OnAfterUpdated += OutputUpdateSlotHandler;
      }
    }

    private void RemoveOutputUpdateEvents() {
      foreach (var output in station.OutputInventory.GetSlots) {
        output.OnAfterUpdated -= OutputUpdateSlotHandler;
      }
    }

    private void OutputUpdateSlotHandler(SlotUpdateEventData data) {
      if (data.after.isEmpty) {
        craftActions.UpdateAndPrintInputCount();
      }
    }

    private void OnTakeAllButtonClickHandler() {
      station.OutputInventory.MoveAllItemsTo(playerInventory.inventory);
    }
  }
}