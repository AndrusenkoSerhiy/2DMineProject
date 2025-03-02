using Scriptables.Craft;

namespace Craft {
  public interface IFuelItems : ICraftComponent {
    public void ConsumeFuel(Recipe recipe, int count);
    public void UpdateInterface(Recipe recipe);
    public void StartBlink();
    public void StopBlink();
  }
}