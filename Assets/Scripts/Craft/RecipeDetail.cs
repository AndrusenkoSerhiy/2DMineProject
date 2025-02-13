using System.Collections.Generic;
using Scriptables.Craft;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityServiceLocator;

namespace Craft {
  public class RecipeDetail : MonoBehaviour, IRecipeDetail {
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI craftTime;
    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private GameObject rowEmptyPrefab;
    [SerializeField] private GameObject listContainer;
    [SerializeField] private List<GameObject> rows;
    
    private Recipe currentRecipe;
    private int[] recipeIngredientsIds;

    public void Awake() {
      Debug.Log("RecipeDetail Awake");
      ServiceLocator.For(this).Register<IRecipeDetail>(this);
    }

    public void SetRecipeDetails(Recipe recipe) {
      currentRecipe = recipe;

      PrintDetails();
      PrintList();
    }

    private void PrintDetails() {
      var img = currentRecipe.detailImg != null ? currentRecipe.detailImg : currentRecipe.Result.UiDisplay;

      title.text = currentRecipe.RecipeName;
      icon.sprite = img;
      craftTime.text = Helper.SecondsToTimeString(currentRecipe.CraftingTime);
    }

    public void PrintList() {
      var rowIndex = 0;
      recipeIngredientsIds = new int[currentRecipe.RequiredMaterials.Count];

      foreach (var resource in currentRecipe.RequiredMaterials) {
        var row = rows[rowIndex++];
        var recipeDetailRow = row.GetComponent<RecipeDetailRow>();
        recipeDetailRow.SetRow(resource);

        recipeIngredientsIds[rowIndex - 1] = resource.Material.data.Id;
      }

      while (rowIndex + 1 < rows.Count) {
        var row = rows[rowIndex++];
        var recipeDetailRow = row.GetComponent<RecipeDetailRow>();
        recipeDetailRow.ClearRow();
      }
    }

    public int[] GetRecipeIngredientsIds() => recipeIngredientsIds;
  }
}