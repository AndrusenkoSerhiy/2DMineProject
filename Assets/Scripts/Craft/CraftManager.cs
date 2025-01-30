using System;
using System.Linq;
using Inventory;
using Scriptables.Craft;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Craft {
  public class CraftManager : MonoBehaviour {
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

    private RecipesManager recipesManager;
    private CraftActions craftActions;
    private PlayerInventory playerInventory;

    private void Init() {
      if (playerInventory != null) {
        return;
      }
      Debug.Log("CraftManager Init");

      playerInventory = GameManager.instance.PlayerInventory;
      recipesManager = new RecipesManager(station.recipes, recipesListItemPrefab, recipesListContainerPrefab);
      craftActions = new CraftActions(countInput, craftButton, incrementButton, decrementButton,
        maxCountButton, buttonsActiveColor, buttonsDisabledColor);
    }

    private void OnEnable() {
      Debug.Log("CraftManager OnEnable");

      Init();

      recipesManager.onSelected += OnRecipeSelectedHandler;
      recipesManager.BuildList();
      recipesManager.AddEvents();
      recipesManager.SelectFirst();

      SlotsUpdateEvents();
      craftActions.AddCraftActionsEvents();
    }

    private void OnDisable() {
      craftActions.RemoveCraftActionsEvents();
      RemoveSlotsUpdateEvents();

      recipesManager.RemoveEvents();
      recipesManager.onSelected -= OnRecipeSelectedHandler;
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
  }
}