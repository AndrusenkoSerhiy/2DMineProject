using System;
using System.Collections.Generic;
using Scriptables.Craft;
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
    private List<Recipe> previousRecipes;
    private List<Recipe> availableRecipes = new List<Recipe>();
    private List<Button> recipesListButtons = new List<Button>();
    private RecipeListItem selectedRecipeListItem;

    // private void Awake() {
    //   Debug.Log("CraftManager Awake");
    //   LoadAvailableRecipes();

    //   BuildRecipeList();
    // }

    private void OnEnable() {
      Debug.Log("CraftManager OnEnable");
      LoadAvailableRecipes();
      BuildRecipeList();

      AddEvents();
    }

    private void OnDisable() {
      RemoveEvents();
    }

    //TODO: Load recipes from file
    private void LoadAvailableRecipes() {
      availableRecipes = recipes;
    }

    private void BuildRecipeList() {
      if (availableRecipes == previousRecipes) {
        Debug.Log("CraftManager BuildRecipeList without rebuild");
        return;
      }

      previousRecipes = availableRecipes;

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
      AddRecipesListEvents();
    }

    private void RemoveEvents() {
      RemoveRecipesListEvents();
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
  }
}