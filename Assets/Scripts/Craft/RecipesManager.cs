using System;
using System.Collections.Generic;
using Scriptables.Craft;
using UnityEngine;
using UnityEngine.UI;
using UnityServiceLocator;

namespace Craft {
  public class RecipesManager : MonoBehaviour, IRecipesManager {
    [SerializeField] private GameObject recipesListContainerPrefab;
    [SerializeField] private Button recipesListItemPrefab;

    private Workstation station;
    private List<Button> recipesListButtons = new();
    private RecipeListItem selectedRecipeListItem;

    public Recipe Recipe { get; private set; }
    public event Action<Recipe> OnSelected;

    public void Awake() {
      ServiceLocator.For(this).Register<IRecipesManager>(this);
      station = ServiceLocator.For(this).Get<Workstation>();

      BuildList();
    }

    public void InitComponent() {
      AddEvents();
      SelectFirst();
    }

    public void ClearComponent() {
      RemoveEvents();
    }

    private void BuildList() {
      if (recipesListButtons.Count > 0) {
        return;
      }

      foreach (var recipe in station.recipes) {
        var listItem = Instantiate<Button>(recipesListItemPrefab, recipesListContainerPrefab.transform);

        var recipeListItem = listItem.GetComponent<RecipeListItem>();
        recipeListItem.SetRecipeDetails(recipe.RecipeName, recipe.Result.UiDisplay, recipe);

        recipesListButtons.Add(listItem);
      }
    }

    private void Select(Button button, bool force = false) {
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
      OnSelected?.Invoke(selectedRecipeListItem.Recipe);
    }

    private void SelectFirst() {
      Select(recipesListButtons[0], true);
    }

    private void AddEvents() {
      foreach (var button in recipesListButtons) {
        button.onClick.AddListener(() => ListItemClickHandler(button));
      }
    }

    private void RemoveEvents() {
      foreach (var button in recipesListButtons) {
        button.onClick.RemoveAllListeners();
      }
    }

    private void ListItemClickHandler(Button button) {
      Select(button);
    }
  }
}