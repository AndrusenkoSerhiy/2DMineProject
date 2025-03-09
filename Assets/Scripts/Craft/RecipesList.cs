using System;
using System.Collections.Generic;
using Scriptables.Craft;
using UnityEngine;
using UnityEngine.UI;
using UnityServiceLocator;

namespace Craft {
  public class RecipesList : MonoBehaviour, IRecipesList {
    [SerializeField] private GameObject recipesListContainerPrefab;
    [SerializeField] private Button recipesListItemPrefab;

    private Workstation station;
    private List<Button> recipesListButtons = new();
    private RecipeListItem selectedRecipeListItem;

    public Recipe Recipe { get; private set; }

    public event Action<Recipe> OnSelected;

    public void Awake() {
      ServiceLocator.For(this).Register<IRecipesList>(this);
      station = ServiceLocator.For(this).Get<Workstation>();
    }

    public void InitComponent() {
      BuildList();
      AddEvents();
      SelectFirst();
    }

    public void ClearComponent() {
      RemoveEvents();
    }

    public void UpdateList() {
      InitComponent();
    }

    private void BuildList() {
      var recipes = GameManager.Instance.RecipesManager.GetRecipesForStation(station.RecipeType);
      if (recipesListButtons.Count > 0 && recipes.Count == recipesListButtons.Count) {
        return;
      }

      Debug.Log("BuildList");

      ClearList();

      if (recipes.Count == 0) {
        Debug.LogError($"No recipes for station - {station.RecipeType}");
        return;
      }

      foreach (var recipe in recipes) {
        var listItem = Instantiate<Button>(recipesListItemPrefab, recipesListContainerPrefab.transform);

        var recipeListItem = listItem.GetComponent<RecipeListItem>();
        recipeListItem.SetRecipeDetails(recipe.RecipeName, recipe.Result.UiDisplay, recipe);

        recipesListButtons.Add(listItem);
      }
    }

    private void ClearList() {
      RemoveEvents();

      foreach (var button in recipesListButtons) {
        Destroy(button.gameObject);
      }

      recipesListButtons.Clear();
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
      if (recipesListButtons.Count == 0) {
        return;
      }

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