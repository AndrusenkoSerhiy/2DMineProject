using System;
using System.Collections.Generic;
using System.Linq;
using Inventory;
using Scriptables.Craft;
using Scriptables.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Craft {
  public class CraftManager : MonoBehaviour, IInventoryDropZoneUI {
    [SerializeField] private Workstation station;
    [SerializeField] private GameObject recipesListContainerPrefab;
    [SerializeField] private RecipeDetail detail;
    [SerializeField] private Button recipesListItemPrefab;

    [SerializeField] private TMP_InputField countInput;
    [SerializeField] private Button craftButton;
    [SerializeField] private Button incrementButton;
    [SerializeField] private Button decrementButton;
    [SerializeField] private Button maxCountButton;
    [SerializeField] private Color buttonsActiveColor;
    [SerializeField] private Color buttonsDisabledColor;

    [SerializeField] private GameObject inputItemsContainer;
    [SerializeField] private GameObject inputItemPrefab;

    [SerializeField] private GameObject outputItemsContainer;
    // [SerializeField] private InventoryObject outputInventory;

    private PlayerInventory playerInventory;
    private RecipesManager recipesManager;
    private CraftActions craftActions;
    private InputItems inputItems;

    [SerializeField] private bool preventItemDrop;
    public bool PreventItemDrop => preventItemDrop;

    private void Init() {
      if (playerInventory != null) {
        return;
      }
      //Debug.Log("CraftManager Init");

      playerInventory = GameManager.instance.PlayerInventory;
      recipesManager = new RecipesManager(station, recipesListItemPrefab, recipesListContainerPrefab);
      craftActions = new CraftActions(station, countInput, craftButton, incrementButton, decrementButton,
        maxCountButton, buttonsActiveColor, buttonsDisabledColor);
      inputItems = new InputItems(station, inputItemsContainer, inputItemPrefab);
    }

    private void OnEnable() {
      //Debug.Log("CraftManager OnEnable");

      Init();

      recipesManager.onSelected += OnRecipeSelectedHandler;
      recipesManager.BuildList();
      recipesManager.AddEvents();
      recipesManager.SelectFirst();

      SlotsUpdateEvents();
      craftActions.AddCraftActionsEvents();
      craftActions.onCraftRequested += OnCraftRequestedHandler;

      AddInputEvents();

      AddOutputUpdateEvents();
    }

    private void OnDisable() {
      RemoveOutputUpdateEvents();

      RemoveInputEvents();

      craftActions.onCraftRequested -= OnCraftRequestedHandler;
      craftActions.RemoveCraftActionsEvents();
      RemoveSlotsUpdateEvents();

      recipesManager.RemoveEvents();
      recipesManager.onSelected -= OnRecipeSelectedHandler;
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

      craftActions.UpdateAndPrintInputCount();
    }

    private void AddCraftedItemToOutput(Recipe recipe, int count) {
      //Debug.Log("CraftManager AddCraftedItemToOutput: " + recipe.RecipeName);
      var outputInventory = station.OutputInventory;
      var item = new Item(outputInventory.database.ItemObjects[recipe.Result.data.Id]);

      outputInventory.AddItem(item, count, null, null);
      //outputInterface.UpdateUI();
    }

    private void OnRecipeSelectedHandler(Recipe recipe) {
      Debug.Log("Recipe selected OnRecipeSelectedHandler: " + recipe.RecipeName);
      detail.SetRecipeDetails(recipe);

      craftActions.SetRecipe(recipe);
      craftActions.UpdateAndPrintInputCount();
    }

    private void SlotsUpdateEvents() {
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
        input.onItemCrafted += AddCraftedItemToOutput;
        input.onInputAllCrafted += OnInputAllCraftedHandler;
      }
    }

    private void RemoveInputEvents() {
      foreach (var input in inputItems.Items) {
        input.onItemCrafted -= AddCraftedItemToOutput;
        input.onInputAllCrafted -= OnInputAllCraftedHandler;
      }
    }

    private void OnInputAllCraftedHandler() {
      craftActions.UpdateAndPrintInputCount();
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
  }
}