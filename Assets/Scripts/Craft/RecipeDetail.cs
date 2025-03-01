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
    [SerializeField] private GameObject listContainer;
    [SerializeField] private List<RecipeDetailRow> rows;
    [SerializeField] private List<RecipeDetailRow> fuelRows;

    protected Recipe currentRecipe;
    private string[] recipeIngredientsIds;
    private ITotalAmount totalAmount;

    public void Awake() {
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
      PrintList(rows, currentRecipe.RequiredMaterials);

      if (currentRecipe.Fuel == null || fuelRows.Count <= 0) {
        return;
      }

      var fuels = new List<Recipe.CraftingMaterial> { currentRecipe.Fuel };
      PrintList(fuelRows, fuels);
    }

    protected void PrintList(List<RecipeDetailRow> rows, List<Recipe.CraftingMaterial> materials) {
      var rowIndex = 0;
      recipeIngredientsIds = new string[materials.Count];

      foreach (var resource in materials) {
        var recipeDetailRow = rows[rowIndex++];
        var totalAmountValue = totalAmount.GetResourceTotalAmount(resource.Material.Id);
        recipeDetailRow.SetRow(resource, totalAmountValue);

        recipeIngredientsIds[rowIndex - 1] = resource.Material.Id;
      }

      while (rowIndex + 1 < rows.Count) {
        var recipeDetailRow = rows[rowIndex++];
        recipeDetailRow.ClearRow();
      }
    }

    public string[] GetRecipeIngredientsIds() => recipeIngredientsIds;
  }
}