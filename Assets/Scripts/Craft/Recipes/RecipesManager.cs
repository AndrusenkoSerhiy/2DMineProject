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
    [SerializeField] private List<RecipeType> defaultUnlockedStations;

    [Header("Debug Options")] [SerializeField]
    private bool unlockAllRecipes = false;

    private SerializedDictionary<string, RecipeState> recipeStates = new();
    private List<string> discoveredMaterials = new();
    private List<RecipeType> unlockedStations = new();
    private List<RecipeType> fullyUnlockedStations = new();

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
        recipeStates.Add(recipe.Id, RecipeState.Locked);
      }

      foreach (var station in defaultUnlockedStations) {
        UnlockStation(station);
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
        var recipe = recipesDB.ItemsMap[recipeId];

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

    public bool HasUnlockedRecipesForStation(RecipeType stationType) {
      if (unlockAllRecipes) {
        return true;
      }

      if (!unlockedStations.Contains(stationType)) {
        return false;
      }

      foreach (var (recipeId, state) in recipeStates) {
        var recipe = recipesDB.ItemsMap[recipeId];

        if (recipe.RecipeType == stationType && state == RecipeState.Unlocked) {
          return true;
        }
      }

      return false;
    }

    public Recipe GetByID(string id) {
      return recipesDB.ItemsMap[id];
    }

    private List<Recipe> GetAllStationRecipes(RecipeType stationType) {
      var recipes = new List<Recipe>();
      foreach (var (id, recipe) in recipesDB.ItemsMap) {
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
      var recipesToUnlock = new List<Recipe>();

      foreach (var (recipeId, state) in recipeStates) {
        var recipe = recipesDB.ItemsMap[recipeId];

        if (recipe.RecipeType != stationType) {
          continue;
        }

        if (state == RecipeState.Unlocked) {
          continue;
        }

        if (AreAllMaterialsDiscovered(recipe)) {
          recipesToUnlock.Add(recipe);
        }
        else {
          allUnlocked = false;
        }
      }

      // Unlock after the iteration is complete
      foreach (var recipe in recipesToUnlock) {
        UnlockRecipe(recipe);
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
      GameManager.Instance.MessagesManager.ShowNewRecipeMessage(recipe);
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