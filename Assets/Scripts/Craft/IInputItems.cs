using System.Collections.Generic;
using Scriptables.Craft;

namespace Craft {
  public interface IInputItems {
    public List<InputItem> Items { get; }
    public void SetRecipe(int count, Recipe recipe);
    public void UpdateWaitInputs(int fromPosition = 0);
    public void UpdateTimersStartTimes(InputItem inputItem);
  }
}