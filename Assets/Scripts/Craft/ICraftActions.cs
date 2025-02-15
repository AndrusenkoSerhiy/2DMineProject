using System;
using Scriptables.Craft;

namespace Craft {
  public interface ICraftActions : ICraftComponent {
    public void UpdateAndPrintInputCount(bool resetCurrentCount = false);
    public void SetRecipe(Recipe recipe);

    public event Action<int> OnCraftRequested;
  }
}