using System;
using System.Collections.Generic;
using Craft.Recipes;
using Scriptables.Craft;
using UnityEngine.Rendering;

namespace SaveSystem {
  [Serializable]
  public class RecipesData {
    public SerializedDictionary<string, RecipeState> RecipeStates = new();
    public List<string> DiscoveredMaterials = new();
    public List<RecipeType> UnlockedStations = new();
    public List<RecipeType> FullyUnlockedStations = new();
  }
}