using Scriptables.Craft;

namespace Craft {
  public interface IFuelItems : ICraftComponent {
    public void ConsumeFuel(Recipe recipe, int count);
  }
}