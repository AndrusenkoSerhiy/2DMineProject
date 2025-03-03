using System;
using System.Collections.Generic;
using SaveSystem;
using Scriptables.Craft;
using Scriptables.Items;
using UnityEngine;
using UnityEngine.Rendering;

namespace Craft.Recipes {
  public class RecipesManager : MonoBehaviour, ISaveLoad {
    [SerializeField] private RecipesDatabaseObject recipesDB;

    [Header("Debug Options")] [SerializeField]
    private bool unlockAllRecipes = false;

    private SerializedDictionary<string, RecipeState> recipeStates = new();
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

      foreach (var recipe in recipesDB.ItemObjects) {
        recipeStates[recipe.Id] = RecipeState.Locked;
      }

      foreach (var recipe in recipesDB.DefaultUnlockedRecipes) {
        recipeStates[recipe.Id] = RecipeState.Unlocked;
      }
    }

    /// <summary>
    /// Call when player picked up new material.
    /// </summary>
    public void DiscoverMaterial(ItemObject material) {
      if (material.Id == string.Empty || discoveredMaterials.Contains(material.Id)) {
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
    /// Returns recipe state.
    /// </summary>
    public RecipeState GetRecipeState(string id) {
      if (unlockAllRecipes) {
        return RecipeState.Unlocked;
      }

      return recipeStates.TryGetValue(id, out var state) ? state : RecipeState.Locked;
    }

    /// <summary>
    /// Returns all recipes for specific station.
    /// </summary>
    public List<Recipe> GetRecipesForStation(RecipeType stationType, bool onlyUnlocked = true) {
      if (unlockAllRecipes) {
        return GetAllStationRecipes(stationType);
      }

      var recipes = new List<Recipe>();
      if (!unlockedStations.Contains(stationType)) {
        return recipes;
      }

      foreach (var (recipeId, state) in recipeStates) {
        var recipe = recipesDB.RecipesMap[recipeId];

        if (recipe.RecipeType != stationType) {
          continue;
        }

        if (onlyUnlocked && state != RecipeState.Unlocked) {
          continue;
        }

        recipes.Add(recipe);
      }

      return recipes;
    }

    public Recipe GetByID(string id) {
      return recipesDB.RecipesMap[id];
    }

    private List<Recipe> GetAllStationRecipes(RecipeType stationType) {
      var recipes = new List<Recipe>();
      foreach (var (id, recipe) in recipesDB.RecipesMap) {
        if (recipe.RecipeType != stationType) {
          continue;
        }

        recipes.Add(recipe);
      }

      return recipes;
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
        var recipeId = kvp.Key;
        var recipe = recipesDB.RecipesMap[recipeId];

        if (recipe.RecipeType != stationType) {
          continue;
        }

        if (recipeStates[recipeId] == RecipeState.Unlocked) {
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
      recipeStates[recipe.Id] = RecipeState.Unlocked;
      OnRecipeUnlocked?.Invoke(recipe);
      Debug.Log($"Recipe unlocked: {recipe.RecipeName}");
    }

    public string Id => "RecipesManager";

    public void Save() {
      var data = new RecipesData {
        RecipeStates = recipeStates,
        UnlockedStations = unlockedStations,
        DiscoveredMaterials = discoveredMaterials,
        FullyUnlockedStations = fullyUnlockedStations
      };

      SaveLoadSystem.Instance.gameData.Recipes = data;
    }

    public void Load() {
      var data = SaveLoadSystem.Instance.gameData.Recipes;
      if (data == null) {
        return;
      }

      recipeStates = data.RecipeStates;
      unlockedStations = data.UnlockedStations;
      discoveredMaterials = data.DiscoveredMaterials;
      fullyUnlockedStations = data.FullyUnlockedStations;
    }
  }
}