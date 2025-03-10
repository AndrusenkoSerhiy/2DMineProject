using Inventory;
using Scriptables.Craft;

namespace Craft {
  public interface IFuelItems : ICraftComponent {
    public void ConsumeFuel(Recipe recipe, int count);
    public void UpdateInterface(Recipe recipe);
    public InventoryObject Inventory { get; }
    public void RunFuelEffect(Recipe recipe);
  }
}