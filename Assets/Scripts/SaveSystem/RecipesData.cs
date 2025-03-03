using System;
using System.Collections.Generic;
using Craft.Recipes;
using Scriptables.Craft;
using UnityEngine.Rendering;

namespace SaveSystem {
  [Serializable]
  public class RecipesData {
    public SerializedDictionary<string, RecipeState> RecipeStates = new();
    public HashSet<string> DiscoveredMaterials = new();
    public HashSet<RecipeType> UnlockedStations = new();
    public HashSet<RecipeType> FullyUnlockedStations = new();
  }
}