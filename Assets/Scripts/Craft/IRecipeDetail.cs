using Scriptables.Craft;

namespace Craft {
  public interface IRecipeDetail {
    public void SetRecipeDetails(Recipe recipe);
    public void PrintList();
    public string[] GetRecipeIngredientsIds();
  }
}