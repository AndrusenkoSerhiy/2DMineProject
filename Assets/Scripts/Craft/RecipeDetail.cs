using System;
using System.Collections.Generic;
using Inventory;
using Scriptables.Craft;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityServiceLocator;

namespace Craft {
  public class RecipeDetail : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI craftTime;
    [SerializeField] private GameObject listContainer;
    [SerializeField] private List<RecipeDetailRow> rows;

    private Workstation station;
    private InventoriesPool inventoriesPool;

    private void Awake() {
      station = ServiceLocator.For(this).Get<Workstation>();
      inventoriesPool = GameManager.Instance.PlayerInventory.InventoriesPool;
    }

    private void OnEnable() {
      SetRecipeDetails(station.CurrentRecipe);
      station.OnRecipeChanged += SetRecipeDetails;
      // inventoriesPool.OnResourcesTotalUpdate += OnResourcesTotalUpdateHandler;
      AddInventoryPoolEvents();
    }

    private void OnDisable() {
      station.OnRecipeChanged -= SetRecipeDetails;
      // inventoriesPool.OnResourcesTotalUpdate -= OnResourcesTotalUpdateHandler;
      RemoveInventoryPoolEvents();
    }

    private void OnResourcesTotalUpdateHandler(string resourceId) {
      var recipeIngredientsIds = station.GetRecipeIngredientsIds();
      if (Array.IndexOf(recipeIngredientsIds, resourceId) != -1) {
        PrintList();
      }
    }

    private void SetRecipeDetails(Recipe recipe) {
      PrintDetails();
      PrintList();
    }

    private void PrintDetails() {
      var currentRecipe = station.CurrentRecipe;
      var img = currentRecipe.detailImg != null ? currentRecipe.detailImg : currentRecipe.Result.UiDisplay;

      title.text = currentRecipe.RecipeName;
      icon.sprite = img;
      craftTime.text = Helper.SecondsToTimeString(currentRecipe.CraftingTime);
    }

    private void PrintList() {
      PrintList(rows, station.CurrentRecipe.RequiredMaterials);
    }

    private void PrintList(List<RecipeDetailRow> rows, List<Recipe.CraftingMaterial> materials) {
      var rowIndex = 0;

      foreach (var resource in materials) {
        var recipeDetailRow = rows[rowIndex++];
        var totalAmountValue = inventoriesPool.GetResourceTotalAmount(resource.Material.Id);
        recipeDetailRow.SetRow(resource, totalAmountValue);
      }

      while (rowIndex + 1 < rows.Count) {
        var recipeDetailRow = rows[rowIndex++];
        recipeDetailRow.ClearRow();
      }
    }

    private void AddInventoryPoolEvents() {
      foreach (var inventory in inventoriesPool.Inventories) {
        foreach (var slot in inventory.Slots) {
          slot.OnAfterUpdated += OnAfterUpdatedHandler;
        }
      }
    }

    private void RemoveInventoryPoolEvents() {
      foreach (var inventory in inventoriesPool.Inventories) {
        foreach (var slot in inventory.Slots) {
          slot.OnAfterUpdated -= OnAfterUpdatedHandler;
        }
      }
    }

    private void OnAfterUpdatedHandler(SlotUpdateEventData obj) {
      var slot = obj.after;
      if (slot.isEmpty) {
        return;
      }

      OnResourcesTotalUpdateHandler(slot.Item.info.Id);
    }
  }
}