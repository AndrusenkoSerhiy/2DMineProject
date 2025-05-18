using System.Collections.Generic;
using Craft.Recipes;
using Scriptables.Craft;
using UnityEngine;
using UnityEngine.UI;
using UnityServiceLocator;

namespace Craft {
  public class RecipesList : MonoBehaviour {
    [SerializeField] private GameObject recipesListContainerPrefab;
    [SerializeField] private Button recipesListItemPrefab;
    [SerializeField] private ScrollRect scrollRect;

    private Workstation station;
    private List<Button> recipesListButtons = new();
    private List<string> recipesListIds = new();
    private RecipeListItem selectedRecipeListItem;
    private GameManager gameManager;
    private bool isListBuilt;

    private void Awake() {
      station = ServiceLocator.For(this).Get<Workstation>();
      gameManager = GameManager.Instance;
    }

    private void OnEnable() => InitComponent();

    private void OnDisable() {
      if (selectedRecipeListItem) {
        selectedRecipeListItem.MarkAsSeen();
        gameManager.RecipesManager.MarkRecipeAsSeen(selectedRecipeListItem.Recipe);
      }

      RemoveEvents();
    }

    private void InitComponent() {
      BuildList();
      AddEvents();
    }

    private void BuildList() {
      var recipes = GameManager.Instance.RecipesManager.GetRecipesForStation(station.RecipeType);

      if (recipes.Count == 0) {
        Debug.LogError($"No recipes for station - {station.RecipeType}");
        return;
      }

      if (recipesListButtons.Count > 0 && recipes.Count == recipesListButtons.Count) {
        return;
      }

      Button previousButton = null;

      foreach (var recipe in recipes) {
        var findIndex = recipesListIds.IndexOf(recipe.Id);
        if (findIndex != -1) {
          previousButton = recipesListButtons[findIndex];
          continue;
        }

        var listItem = CreateButton(recipe);

        if (isListBuilt && previousButton != null) {
          InsertButton(previousButton, listItem, recipe);
        }
        else {
          AddButton(listItem, recipe);
        }

        previousButton = listItem;
      }

      if (isListBuilt) {
        return;
      }

      SelectFirst();
      isListBuilt = true;
    }

    private void AddButton(Button button, Recipe recipe) {
      recipesListIds.Add(recipe.Id);
      recipesListButtons.Add(button);
    }

    private void InsertButton(Button previousButton, Button button, Recipe recipe) {
      var index = previousButton.transform.GetSiblingIndex() + 1;
      InsertButtonInPosition(index, button, recipe);
    }

    private void InsertButtonInPosition(int position, Button button, Recipe recipe) {
      button.transform.SetSiblingIndex(position);
      recipesListIds.Insert(position, recipe.Id);
      recipesListButtons.Insert(position, button);
    }

    private Button CreateButton(Recipe recipe) {
      var listItem = Instantiate(recipesListItemPrefab, recipesListContainerPrefab.transform);
      var recipeListItem = listItem.GetComponent<RecipeListItem>();
      var isNew = gameManager.RecipesManager.GetRecipeState(recipe.Id) == RecipeState.New;
      recipeListItem.SetRecipeDetails(recipe.RecipeName, recipe.Result.UiDisplay, recipe, isNew);

      return listItem;
    }

    private void Select(Button button, bool force = false) {
      var recipeListItem = button.GetComponent<RecipeListItem>();

      if (!force && selectedRecipeListItem == recipeListItem) {
        return;
      }

      if (selectedRecipeListItem) {
        selectedRecipeListItem.ResetStyles();
        gameManager.RecipesManager.MarkRecipeAsSeen(selectedRecipeListItem.Recipe);
      }

      selectedRecipeListItem = recipeListItem;
      recipeListItem.SetActiveStyles();

      station.SetRecipe(selectedRecipeListItem.Recipe);
    }

    private void SelectFirst() {
      if (recipesListButtons.Count == 0) {
        return;
      }

      Select(recipesListButtons[0], true);
    }

    private void AddEvents() {
      gameManager.RecipesManager.OnRecipeUnlocked += OnRecipeUnlockedHandler;

      foreach (var button in recipesListButtons) {
        button.onClick.AddListener(() => ListItemClickHandler(button));
      }
    }

    private void RemoveEvents() {
      foreach (var button in recipesListButtons) {
        button.onClick.RemoveAllListeners();
      }

      gameManager.RecipesManager.OnRecipeUnlocked -= OnRecipeUnlockedHandler;
    }

    private void OnRecipeUnlockedHandler(Recipe recipe) {
      if (!gameManager.RecipesManager.IsRecipeOfType(recipe, station.RecipeType)) {
        return;
      }

      if (recipesListIds.Contains(recipe.Id)) {
        return;
      }

      var recipes = GameManager.Instance.RecipesManager.GetRecipesForStation(station.RecipeType);
      var newRecipeIndex = recipes.IndexOf(recipe);

      Button previousButton = null;

      for (var i = newRecipeIndex - 1; i >= 0; i--) {
        var previousRecipeId = recipes[i].Id;
        var previousIndexInList = recipesListIds.IndexOf(previousRecipeId);

        if (previousIndexInList == -1) {
          continue;
        }

        previousButton = recipesListButtons[previousIndexInList];
        break;
      }

      var listItem = CreateButton(recipe);

      if (previousButton != null) {
        InsertButton(previousButton, listItem, recipe);
      }
      else {
        InsertButtonInPosition(0, listItem, recipe);
      }

      listItem.onClick.AddListener(() => ListItemClickHandler(listItem));
      
      // ScrollToButton(listItem);
    }

    private void ListItemClickHandler(Button button) {
      Select(button);
    }
    
    /*private void ScrollToButton(Button button) {
      Canvas.ForceUpdateCanvases();

      var content = scrollRect.content;
      var target = button.GetComponent<RectTransform>();

      var localPosition = (Vector2)content.InverseTransformPoint(content.position) 
                          - (Vector2)content.InverseTransformPoint(target.position);

      var scrollHeight = content.rect.height - scrollRect.viewport.rect.height;

      var normalizedY = Mathf.Clamp01(1 - (localPosition.y / scrollHeight));
      scrollRect.verticalNormalizedPosition = normalizedY;
    }*/
  }
}