using System;
using System.Collections.Generic;
using Scriptables.Craft;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Craft {
  public class RecipeDetail : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI craftTime;
    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private GameObject rowEmptyPrefab;
    [SerializeField] private GameObject listContainer;
    [SerializeField] private List<GameObject> rows;
    private Recipe currentRecipe;

    public void SetRecipeDetails(Recipe recipe) {
      currentRecipe = recipe;

      PrintDetails();
      PrintList();
    }

    private void PrintDetails() {
      var img = currentRecipe.detailImg != null ? currentRecipe.detailImg : currentRecipe.Result.UiDisplay;

      title.text = currentRecipe.RecipeName;
      this.icon.sprite = img;
      this.craftTime.text = Helper.SecondsToTimeString(currentRecipe.CraftingTime);
    }

    private void PrintList() {
      var rowIndex = 0;
      foreach (var resource in currentRecipe.RequiredMaterials) {
        var row = rows[rowIndex++];
        var recipeDetailRow = row.GetComponent<RecipeDetailRow>();
        recipeDetailRow.SetRow(resource);
      }

      while (rowIndex + 1 < rows.Count) {
        var row = rows[rowIndex++];
        var recipeDetailRow = row.GetComponent<RecipeDetailRow>();
        recipeDetailRow.ClearRow();
      }
    }
  }
}