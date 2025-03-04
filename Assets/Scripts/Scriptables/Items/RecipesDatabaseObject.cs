using Scriptables.Craft;
using UnityEngine;

namespace Scriptables.Items {
  [CreateAssetMenu(fileName = "New Recipes Database", menuName = "Inventory System/RecipesDatabase")]
  public class RecipesDatabaseObject : Database<Recipe> {
  }
}