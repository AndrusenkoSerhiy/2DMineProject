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
    private bool isInitialized;

    public bool PreventItemDrop => preventItemDrop;

    public void Awake() {
      Debug.Log("CraftManager Awake");
      ServiceLocator.For(this).Register<Workstation>(station);
    }

    public void Start() {
      Debug.Log("CraftManager Start");
      Init();
      AddEvents();
    }

    public void OnEnable() {
      if (!isInitialized) {
        return;
      }

      Debug.Log("CraftManager OnEnable");

      AddEvents();
    }

    private void OnDisable() => RemoveEvents();

    private void Init() {
      Debug.Log("CraftManager Init");

      playerInventory = GameManager.instance.PlayerInventory;
      detail = ServiceLocator.For(this).Get<IRecipeDetail>();
      craftActions = ServiceLocator.For(this).Get<ICraftActions>();
      inputItems = ServiceLocator.For(this).Get<IInputItems>();
      recipesManager = ServiceLocator.For(this).Get<IRecipesManager>();

      isInitialized = true;
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
      Debug.Log("CraftManager OnCraftRequestedHandler: " + count);

      var recipe = recipesManager.Recipe;

      //remove resources from inventory
      foreach (var item in recipe.RequiredMaterials) {
        var totalCount = count * item.Amount;
        playerInventory.inventory.RemoveItem(item.Material.data, totalCount);
      }

      inputItems.SetRecipe(count, recipe);
      station.AddItemToCraftTotal(recipe.Result, count);
      station.AddToCraftInputsItemsIds(recipe.Result.data.Id);

      craftActions.UpdateAndPrintInputCount();
    }

    private void OnRecipeSelectedHandler(Recipe recipe) {
      Debug.Log("Recipe selected OnRecipeSelectedHandler: " + recipe.RecipeName);
      detail.SetRecipeDetails(recipe);

      craftActions.SetRecipe(recipe);
      craftActions.UpdateAndPrintInputCount(true);
    }

    private void AddSlotsUpdateEvents() {
      playerInventory.onResourcesTotalUpdate += SlotAmountUpdateHandler;
    }

    private void RemoveSlotsUpdateEvents() {
      playerInventory.onResourcesTotalUpdate -= SlotAmountUpdateHandler;
    }

    private void SlotAmountUpdateHandler(int resourceId) {
      var recipeIngredientsIds = detail.GetRecipeIngredientsIds();
      if (recipeIngredientsIds.Length > 0 && recipeIngredientsIds.Contains(resourceId)) {
        UpdateResourcesListAndCount();
      }
    }

    private void UpdateResourcesListAndCount() {
      Debug.Log("CraftManager UpdateResourcesListAndCount");
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
      Debug.Log("CraftManager AddCraftedItemToOutput: " + recipe.RecipeName);
      station.RemoveCountFromCraftTotal(recipe.Result, count);

      var outputInventory = station.OutputInventory;
      var item = new Item(outputInventory.database.ItemObjects[recipe.Result.data.Id]);

      outputInventory.AddItem(item, count, null, null);
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
        playerInventory.inventory.AddItem(item.Material.data, totalCount, null, null);
      }
    }

    private void AddOutputUpdateEvents() {
      foreach (var output in station.OutputInventory.GetSlots) {
        output.onAfterUpdated += OutputUpdateSlotHandler;
      }
    }

    private void RemoveOutputUpdateEvents() {
      foreach (var output in station.OutputInventory.GetSlots) {
        output.onAfterUpdated -= OutputUpdateSlotHandler;
      }
    }

    private void OutputUpdateSlotHandler(InventorySlot slot) {
      Debug.Log("OutputUpdateSlotHandler slot.item.Id:" + slot.item.Id);
      if (slot.item.Id < 0) {
        craftActions.UpdateAndPrintInputCount();
      }
    }

    private void OnTakeAllButtonClickHandler() {
      station.OutputInventory.MoveAllItemsTo(playerInventory.inventory);
    }
  }
}