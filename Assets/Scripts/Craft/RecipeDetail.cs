using System;
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
    private string[] recipeIngredientsIds;
    private ITotalAmount totalAmount;

    public void Awake() {
      Debug.Log("RecipeDetail Awake");
      ServiceLocator.For(this).Register<IRecipeDetail>(this);

      totalAmount = ServiceLocator.For(this).Get<ITotalAmount>();
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
      recipeIngredientsIds = new string[currentRecipe.RequiredMaterials.Count];

      foreach (var resource in currentRecipe.RequiredMaterials) {
        var row = rows[rowIndex++];
        var recipeDetailRow = row.GetComponent<RecipeDetailRow>();
        var totalAmountValue = totalAmount.GetResourceTotalAmount(resource.Material.Id);
        recipeDetailRow.SetRow(resource, totalAmountValue);

        recipeIngredientsIds[rowIndex - 1] = resource.Material.Id;
      }

      while (rowIndex + 1 < rows.Count) {
        var row = rows[rowIndex++];
        var recipeDetailRow = row.GetComponent<RecipeDetailRow>();
        recipeDetailRow.ClearRow();
      }
    }

    public string[] GetRecipeIngredientsIds() => recipeIngredientsIds;
  }
}