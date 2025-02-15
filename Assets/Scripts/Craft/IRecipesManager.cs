using System;
using System.Collections.Generic;
using Scriptables.Craft;

namespace Craft {
  public interface IRecipesManager : ICraftComponent {
    public event Action<Recipe> OnSelected;
    public Recipe Recipe { get; }
  }
}