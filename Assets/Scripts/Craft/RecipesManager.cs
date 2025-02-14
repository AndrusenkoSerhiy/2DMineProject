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
    private List<Button> recipesListButtons = new List<Button>();
    private RecipeListItem selectedRecipeListItem;
    private bool isInitialized;

    public Recipe Recipe { get; private set; }
    public event Action<Recipe> OnSelected;

    public void Awake() {
      Debug.Log("RecipesManager Awake");
      ServiceLocator.For(this).Register<IRecipesManager>(this);
      station = ServiceLocator.For(this).Get<Workstation>();
    }
    
    public void OnEnable() {
      if (!isInitialized) {
        return;
      }
      Debug.Log("RecipesManager OnEnable");

      ProcessRecipesList();
    }

    public void Start() => ProcessRecipesList();
    
    public void OnDisable() => RemoveEvents();

    private void ProcessRecipesList() {
      Debug.Log("RecipesManager ProcessRecipesList");
      BuildList();
      AddEvents();
      SelectFirst();

      isInitialized = true;
    }

    private void BuildList() {
      if (recipesListButtons.Count > 0) {
        return;
      }

      Debug.Log("RecipesManager BuildList");
      foreach (var recipe in station.recipes) {
        var listItem = Instantiate<Button>(recipesListItemPrefab, recipesListContainerPrefab.transform);

        var recipeListItem = listItem.GetComponent<RecipeListItem>();
        recipeListItem.SetRecipeDetails(recipe.RecipeName, recipe.Result.UiDisplay, recipe);

        recipesListButtons.Add(listItem);
      }
    }

    private void Select(Button button, bool force = false) {
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
      Debug.Log("RecipesManager Clicked: ListItemClickHandler");

      Select(button);
    }
  }
}