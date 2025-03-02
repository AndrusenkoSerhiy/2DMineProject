using System;
using System.Collections.Generic;
using System.Linq;
using SaveSystem;
using Scriptables.Craft;
using Scriptables.Items;
using UnityEngine;

namespace Craft.Recipes {
  public class RecipesManager : MonoBehaviour, ISaveLoad {
    [SerializeField] private List<Recipe> allRecipes = new();
    [SerializeField] private List<Recipe> defaultUnlockedRecipes = new();

    private Dictionary<Recipe, RecipeState> recipeStates = new();
    private HashSet<string> discoveredMaterials = new();
    private HashSet<RecipeType> unlockedStations = new();
    private HashSet<RecipeType> fullyUnlockedStations = new();

    public event Action<Recipe> OnRecipeUnlocked;

    private void Awake() {
      Load();
      Initialize();
    }

    private void Initialize() {
      if (recipeStates.Count > 0) {
        return;
      }

      foreach (var recipe in allRecipes) {
        recipeStates[recipe] = RecipeState.Locked;
      }

      foreach (var recipe in defaultUnlockedRecipes) {
        recipeStates[recipe] = RecipeState.Unlocked;
      }
    }

    /// <summary>
    /// Call when player picked up new material.
    /// </summary>
    public void DiscoverMaterial(ItemObject material) {
      if (discoveredMaterials.Contains(material.Id)) {
        return;
      }

      discoveredMaterials.Add(material.Id);
      RecheckRecipesForAllUnlockedStations();
    }

    /// <summary>
    /// Call when player build new station.
    /// </summary>
    public void UnlockStation(RecipeType stationType) {
      if (unlockedStations.Contains(stationType)) {
        return;
      }

      unlockedStations.Add(stationType);
      RecheckRecipesForStation(stationType);
    }

    /// <summary>
    /// Checks recipes for all unlocked stations.
    /// </summary>
    private void RecheckRecipesForAllUnlockedStations() {
      foreach (var station in unlockedStations) {
        RecheckRecipesForStation(station);
      }
    }

    /// <summary>
    /// Checks recipes for specific station.
    /// If all materials are discovered, unlocks recipe.
    /// </summary>
    private void RecheckRecipesForStation(RecipeType stationType) {
      if (fullyUnlockedStations.Contains(stationType)) {
        return;
      }

      var allUnlocked = true;

      foreach (var kvp in recipeStates) {
        var recipe = kvp.Key;

        if (recipe.RecipeType != stationType) {
          continue;
        }

        if (recipeStates[recipe] == RecipeState.Unlocked) {
          continue;
        }

        if (AreAllMaterialsDiscovered(recipe)) {
          UnlockRecipe(recipe);
        }
        else {
          allUnlocked = false;
        }
      }

      if (allUnlocked) {
        fullyUnlockedStations.Add(stationType);
      }
    }

    private bool AreAllMaterialsDiscovered(Recipe recipe) {
      foreach (var material in recipe.RequiredMaterials) {
        if (!discoveredMaterials.Contains(material.Material.Id)) {
          return false;
        }
      }

      return true;
    }

    private void UnlockRecipe(Recipe recipe) {
      recipeStates[recipe] = RecipeState.Unlocked;
      OnRecipeUnlocked?.Invoke(recipe);
      Debug.Log($"Рецепт відкрито: {recipe.RecipeName}");
    }

    /// <summary>
    /// Returns recipe state.
    /// </summary>
    public RecipeState GetRecipeState(Recipe recipe) {
      return recipeStates.TryGetValue(recipe, out var state) ? state : RecipeState.Locked;
    }

    /// <summary>
    /// Returns all recipes for specific station.
    /// </summary>
    public List<Recipe> GetRecipesForStation(RecipeType stationType, bool onlyUnlocked) {
      return recipeStates
        .Where(kvp => kvp.Key.RecipeType == stationType && (!onlyUnlocked || kvp.Value == RecipeState.Unlocked))
        .Select(kvp => kvp.Key)
        .ToList();
    }

    /// <summary>
    /// Are all recipes for specific station unlocked.
    /// </summary>
    public bool AreAllStationRecipesUnlocked(RecipeType stationType) {
      if (fullyUnlockedStations.Contains(stationType)) {
        return true;
      }

      foreach (var kvp in recipeStates) {
        if (kvp.Key.RecipeType != stationType) {
          continue;
        }

        if (kvp.Value != RecipeState.Unlocked) {
          return false;
        }
      }

      fullyUnlockedStations.Add(stationType);
      return true;
    }

    public string Id => "RecipesManager";

    public void Save() {
      // throw new NotImplementedException();
    }

    public void Load() {
      // throw new NotImplementedException();
    }
  }
}