using System;
using System.Collections.Generic;
using System.Linq;
using Inventory;
using Scriptables.Craft;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Craft {
  public class CraftManager : MonoBehaviour {
    [SerializeField]
    private Workstation station;

    [SerializeField]
    private List<Recipe> recipes = new List<Recipe>();

    [SerializeField]
    private GameObject recipesListContainerPrefab;

    [SerializeField]
    private RecipeDetail detail;

    [SerializeField]
    private Button recipesListItemPrefab;

    private List<Recipe> availableRecipes = new List<Recipe>();
    private List<Button> recipesListButtons = new List<Button>();
    private RecipeListItem selectedRecipeListItem;

    #region Craft Actions
    [SerializeField]
    private TMP_InputField countInput;

    [SerializeField]
    private Button craftButton;

    [SerializeField]
    private Button incrementButton;

    [SerializeField]
    private Button decrementButton;

    private PlayerInventory playerInventory;
    private int minCount = 1;
    private int maxCount = -1;
    private int currentCount = 1;
    #endregion

    private void Awake() {
      playerInventory = GameManager.instance.PlayerInventory;
    }

    private void OnEnable() {
      Debug.Log("CraftManager OnEnable");

      LoadAvailableRecipes();
      BuildOrUpdateRecipeList();

      AddEvents();

      #region Craft Actions
      UpdateAndPrintInputCount();
      #endregion
    }

    private void OnDisable() {
      RemoveEvents();
    }

    #region Craft Actions
    private void UpdateAndPrintInputCount() {
      Debug.Log("CraftManager UpdateAndPrintInputCount");
      ResetCurrentCount();
      CalculateMaxCount();
      SetCurrentCount();
      PrintInputCount();
    }

    private void AddCraftActionsEvents() {
      countInput.onValueChanged.AddListener(OnCountInputChangeHandler);
      craftButton.onClick.AddListener(OnCraftClickHandler);
      incrementButton.onClick.AddListener(OnIncrementClickHandler);
      decrementButton.onClick.AddListener(OnDecrementClickHandler);
    }

    private void RemoveCraftActionsEvents() {
      countInput.onValueChanged.RemoveAllListeners();
      craftButton.onClick.RemoveAllListeners();
      incrementButton.onClick.RemoveAllListeners();
      decrementButton.onClick.RemoveAllListeners();
    }

    private void OnCountInputChangeHandler(string value) {
      Debug.Log("Value changed: " + value);
      if (value == string.Empty) {
        return;
      }

      var count = int.Parse(value);

      if (count == minCount || count == maxCount) {
        return;
      }

      if (count > maxCount) {
        count = maxCount;
      }
      else if (count < minCount) {
        count = minCount;
      }

      SetCurrentCount(count);
      countInput.text = currentCount.ToString();
    }

    private void OnCraftClickHandler() {
      Debug.Log("Craft Clicked");
    }

    private void OnIncrementClickHandler() {
      Debug.Log("Increment Clicked");
      if (currentCount >= maxCount) {
        return;
      }
      currentCount++;
      PrintInputCount();
    }

    private void OnDecrementClickHandler() {
      Debug.Log("Decrement Clicked");
      if (currentCount <= minCount) {
        return;
      }
      currentCount--;
      PrintInputCount();
    }

    private void CalculateMaxCount() {
      foreach (var resource in selectedRecipeListItem.Recipe.RequiredMaterials) {
        var amount = playerInventory.GetResourceTotalAmount(resource.Material.data.Id);
        var max = amount / resource.Amount;
        maxCount = maxCount != -1 ? Math.Min(maxCount, max) : max;
      }
    }

    private void SetCurrentCount() {
      if (currentCount <= 0 && maxCount > 0) {
        currentCount = 1;
        return;
      }
      currentCount = currentCount > maxCount ? maxCount : currentCount;
    }

    private void SetCurrentCount(int count) {
      currentCount = count;
    }

    private void ResetCurrentCount() {
      currentCount = 1;
      maxCount = -1;
    }

    private void PrintInputCount() {
      countInput.text = currentCount.ToString();
    }
    #endregion

    //TODO: Load recipes from file
    private void LoadAvailableRecipes() {
      availableRecipes = recipes;
    }

    private void BuildOrUpdateRecipeList() {
      if (recipesListButtons.Count <= 0) {
        BuildRecipeList();
        return;
      }

      detail.PrintList();
    }

    private void BuildRecipeList() {
      Debug.Log("CraftManager BuildRecipeList");
      var count = 0;
      foreach (Recipe recipe in availableRecipes) {
        var listItem = Instantiate<Button>(recipesListItemPrefab, recipesListContainerPrefab.transform);

        var recipeListItem = listItem.GetComponent<RecipeListItem>();
        recipeListItem.SetRecipeDetails(recipe.RecipeName, recipe.Result.UiDisplay, recipe);

        recipesListButtons.Add(listItem);

        // Activate first recipe
        if (count == 0) {
          ActivateRecipe(listItem);
        }

        count++;
      }
    }

    private void AddEvents() {
      SlotsUpdateEvents();
      AddRecipesListEvents();

      #region Craft Actions
      AddCraftActionsEvents();
      #endregion
    }

    private void RemoveEvents() {
      RemoveSlotsUpdateEvents();
      RemoveRecipesListEvents();

      #region Craft Actions
      RemoveCraftActionsEvents();
      #endregion
    }

    private void AddRecipesListEvents() {
      foreach (var button in recipesListButtons) {
        button.onClick.AddListener(() => RecipesListItemClickHandler(button));
      }
    }

    private void RemoveRecipesListEvents() {
      foreach (var button in recipesListButtons) {
        button.onClick.RemoveAllListeners();
      }
    }

    private void RecipesListItemClickHandler(Button button) {
      Debug.Log("Clicked: recipesListItemClickHandler");

      ActivateRecipe(button);

      #region Craft Actions
      UpdateAndPrintInputCount();
      #endregion
    }

    private void ActivateRecipe(Button button) {
      SelectRecipe(button);
      PrintDetailInfo();
    }

    private void SelectRecipe(Button button) {
      var recipeListItem = button.GetComponent<RecipeListItem>();

      if (selectedRecipeListItem == recipeListItem) {
        Debug.Log("Clicked: return");
        return;
      }

      if (selectedRecipeListItem) {
        selectedRecipeListItem.ResetStyles();
      }

      selectedRecipeListItem = recipeListItem;
      recipeListItem.SetActiveStyles();
    }

    private void PrintDetailInfo() {
      detail.SetRecipeDetails(selectedRecipeListItem.Recipe);
    }

    #region Inventory slots update
    private void SlotsUpdateEvents() {
      GameManager.instance.PlayerInventory.onResourcesTotalUpdate += SlotAmountUpdateHandler;
    }

    private void RemoveSlotsUpdateEvents() {
      GameManager.instance.PlayerInventory.onResourcesTotalUpdate -= SlotAmountUpdateHandler;
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

      #region Craft Actions
      UpdateAndPrintInputCount();
      #endregion
    }
    #endregion
  }
}