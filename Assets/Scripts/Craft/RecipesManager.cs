using System;
using System.Collections.Generic;
using Scriptables.Craft;
using UnityEngine;
using UnityEngine.UI;

namespace Craft {
  public class RecipesManager {
    private readonly List<Recipe> recipes;
    private readonly Button recipesListItemPrefab;
    private readonly GameObject recipesListContainerPrefab;
    private List<Button> recipesListButtons = new List<Button>();
    private RecipeListItem selectedRecipeListItem;
    public Action<Recipe> onSelected { get; set; }
    public Recipe Recipe { get; private set; }

    public RecipesManager(Workstation station, Button recipesListItemPrefab, GameObject recipesListContainerPrefab) {
      recipes = station.recipes;
      this.recipesListItemPrefab = recipesListItemPrefab;
      this.recipesListContainerPrefab = recipesListContainerPrefab;
    }

    public void BuildList() {
      if (recipesListButtons.Count > 0) {
        return;
      }

      Debug.Log("RecipesManager BuildList");
      foreach (Recipe recipe in recipes) {
        var listItem = GameObject.Instantiate<Button>(recipesListItemPrefab, recipesListContainerPrefab.transform);

        var recipeListItem = listItem.GetComponent<RecipeListItem>();
        recipeListItem.SetRecipeDetails(recipe.RecipeName, recipe.Result.UiDisplay, recipe);

        recipesListButtons.Add(listItem);
      }
    }

    public void Select(Button button, bool force = false) {
      Debug.Log("RecipesManager Select");
      var recipeListItem = button.GetComponent<RecipeListItem>();

      if (!force && selectedRecipeListItem == recipeListItem) {
        return;
      }

      if (selectedRecipeListItem) {
        selectedRecipeListItem.ResetStyles();
      }

      selectedRecipeListItem = recipeListItem;
      recipeListItem.SetActiveStyles();

      Recipe = selectedRecipeListItem.Recipe;
      onSelected?.Invoke(selectedRecipeListItem.Recipe);
    }

    public void SelectFirst() {
      Select(recipesListButtons[0], true);
    }

    public void AddEvents() {
      foreach (var button in recipesListButtons) {
        button.onClick.AddListener(() => ListItemClickHandler(button));
      }
    }

    public void RemoveEvents() {
      foreach (var button in recipesListButtons) {
        button.onClick.RemoveAllListeners();
      }
    }

    private void ListItemClickHandler(Button button) {
      Debug.Log("RecipesManager Clicked: ListItemClickHandler");

      Select(button);
    }
  }
}