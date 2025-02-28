using System;
using System.Collections.Generic;
using Scriptables.Craft;

namespace Craft {
  public interface IInputItems : ICraftComponent {
    public List<InputItem> Items { get; }
    public int InputInProgress { get; }
    public TimerInputItem CraftInput { get; }
    public void SetRecipe(int count, Recipe recipe);

    public void UpdateWaitInputs(int fromPosition = 0);
    // public void UpdateTimersStartTimes(InputItem inputItem);
    // public int GetItemsCountBeforeInputPosition(int position);
  }
}