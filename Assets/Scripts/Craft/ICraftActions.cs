using System;
using Scriptables.Craft;

namespace Craft {
  public interface ICraftActions {
    public void UpdateAndPrintInputCount(bool resetCurrentCount = false);
    public void SetRecipe(Recipe recipe);

    public event Action<int> OnCraftRequested;
  }
}