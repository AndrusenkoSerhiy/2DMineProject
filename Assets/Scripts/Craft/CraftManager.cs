using System.Linq;
using Inventory;
using Scriptables.Craft;
using Scriptables.Items;
using UnityEngine;
using UnityEngine.UI;
using UnityServiceLocator;

namespace Craft {
  public class CraftManager : MonoBehaviour, IInventoryDropZoneUI {
    [SerializeField] private Button takeAllButton;
    [SerializeField] private bool preventItemDrop;
    [SerializeField] private UserInterface outputInterface;

    private Workstation station;
    private IRecipesList recipesList;
    private IRecipeDetail detail;
    private ICraftActions craftActions;
    private PlayerInventory playerInventory;
    private IInputItems inputItems;
    private ITotalAmount totalAmount;
    private IFuelItems fuelItems;
    
    private InventoryObject outputInventory;
    private InventoryObject fuelInventory;

    private bool started;

    public bool PreventItemDropIn => preventItemDrop;

    public void Setup(Workstation station) {
      this.station = station;
      outputInterface.Setup(station.OutputInventoryType);
    }

    public void Awake() {
      ServiceLocator.For(this).Register(station);
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
      recipesList.ClearComponent();
      totalAmount.ClearComponent();
      inputItems.ClearComponent();
      fuelItems?.ClearComponent();

      RemoveEvents();
    }

    private void Init() {
      InitReferences();
      AddEvents();
      totalAmount.InitComponent();
      recipesList.InitComponent();
      craftActions.InitComponent();
      inputItems.InitComponent();

      Load();

      fuelItems?.InitComponent();
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
      recipesList = ServiceLocator.For(this).Get<IRecipesList>();
      
      outputInventory = GameManager.Instance.PlayerInventory.GetInventoryByType(station.OutputInventoryType);
      fuelInventory = GameManager.Instance.PlayerInventory.GetInventoryByType(station.FuelInventoryType);

      ServiceLocator.For(this).TryGet(out fuelItems);
    }

    private void Load() {
      ProcessCraftedInputs();
    }

    private void ProcessCraftedInputs() {
      station.ProcessCraftedInputs();

      var inputs = station.Inputs;
      if (inputs.Count == 0) {
        return;
      }

      foreach (var input in inputs) {
        if (inputItems.InputInProgress == 0) {
          station.UpdateMillisecondsLeftByInput(input);
        }
        
        // Debug.Log($"SetRecipe input.Count {input.Count}");
        inputItems.SetRecipe(input.Count, input.Recipe);
      }
    }

    private void AddEvents() {
      //recipes list
      recipesList.OnSelected += OnRecipeSelectedHandler;
      //inventory slots
      AddSlotsUpdateEvents();
      //Fuel slots
      AddFuelUpdateEvents();
      //craft input actions
      craftActions.OnCraftRequested += OnCraftRequestedHandler;
      //craft input slots
      AddInputEvents();
      //craft output slots
      AddOutputUpdateEvents();
      takeAllButton.onClick.AddListener(OnTakeAllButtonClickHandler);
    }

    private void RemoveEvents() {
      //craft output slots
      takeAllButton.onClick.RemoveAllListeners();
      RemoveOutputUpdateEvents();
      //craft input slots
      RemoveInputEvents();
      //craft input actions
      craftActions.OnCraftRequested -= OnCraftRequestedHandler;
      //Fuel slots
      RemoveFuelUpdateEvents();
      //inventory slots
      RemoveSlotsUpdateEvents();
      //recipes list
      recipesList.OnSelected -= OnRecipeSelectedHandler;
    }

    private void OnCraftRequestedHandler(int count) {
      var recipe = recipesList.Recipe;

      //remove resources from inventory
      foreach (var item in recipe.RequiredMaterials) {
        var totalCount = count * item.Amount;
        totalAmount.RemoveFromInventoriesPool(item.Material.Id, totalCount);
      }

      inputItems.SetRecipe(count, recipe);
      station.AddItemToInputs(recipe, count);

      craftActions.UpdateAndPrintInputCount();
    }

    private void OnRecipeSelectedHandler(Recipe recipe) {
      detail.SetRecipeDetails(recipe);

      craftActions.SetRecipe(recipe);
      craftActions.UpdateAndPrintInputCount(true);

      fuelItems?.UpdateInterface(recipe);
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
        input.OnCanceled += OnInputCanceledHandler;
      }

      var craftInput = inputItems.CraftInput;
      craftInput.OnItemCrafted += AddCraftedItemToOutput;
      craftInput.OnInputAllCrafted += OnInputAllCraftedHandler;
    }

    private void RemoveInputEvents() {
      foreach (var input in inputItems.Items) {
        input.OnCanceled -= OnInputCanceledHandler;
      }

      var craftInput = inputItems.CraftInput;
      craftInput.OnInputAllCrafted -= OnInputAllCraftedHandler;
      craftInput.OnItemCrafted -= AddCraftedItemToOutput;
    }

    private void AddCraftedItemToOutput(ItemCraftedEventData data) {
      station.RemoveInputCountFromInputs(data.Count);
      
      var item = new Item(data.Recipe.Result);

      fuelItems?.ConsumeFuel(data.Recipe, data.Count);
      outputInventory.AddItem(item, data.Count);
    }

    private void OnInputAllCraftedHandler() {
      inputItems.UpdateWaitInputs();
      craftActions.UpdateAndPrintInputCount();
    }

    private void OnInputCanceledHandler(ItemCanceledEventData data) {
      station.RemoveInputFromInputs(data.Position);

      //remove resources from inventory
      foreach (var item in data.Recipe.RequiredMaterials) {
        var totalCount = data.CountLeft * item.Amount;
        var addItem = new Item(item.Material);
        playerInventory.GetInventory().AddItem(addItem, totalCount);
      }

      inputItems.UpdateWaitInputs(data.Position);
    }

    private void AddOutputUpdateEvents() {
      foreach (var output in outputInventory.GetSlots) {
        output.OnAfterUpdated += OutputUpdateSlotHandler;
      }
    }

    private void RemoveOutputUpdateEvents() {
      foreach (var output in outputInventory.GetSlots) {
        output.OnAfterUpdated -= OutputUpdateSlotHandler;
      }
    }

    private void OutputUpdateSlotHandler(SlotUpdateEventData data) {
      if (data.after.isEmpty) {
        craftActions.UpdateAndPrintInputCount();
      }
    }

    private void OnTakeAllButtonClickHandler() {
      outputInventory.MoveAllItemsTo(playerInventory.GetInventory());
    }

    private void AddFuelUpdateEvents() {
      if (fuelInventory == null) {
        return;
      }

      foreach (var slot in fuelInventory.GetSlots) {
        slot.OnAfterUpdated += FuelSlotUpdateHandler;
      }
    }

    private void RemoveFuelUpdateEvents() {
      if (fuelInventory == null) {
        return;
      }

      foreach (var slot in fuelInventory.GetSlots) {
        slot.OnAfterUpdated -= FuelSlotUpdateHandler;
      }
    }

    private void FuelSlotUpdateHandler(SlotUpdateEventData data) {
      craftActions.UpdateAndPrintInputCount();
    }
  }
}