using System.Collections.Generic;
using System.Linq;

namespace Stats {
  public interface IStatModifierApplicationOrder {
    public float Apply(IEnumerable<StatModifier> statModifiers, float baseValue);
  }

  public class NormalStatModifierOrder : IStatModifierApplicationOrder {
    public float Apply(IEnumerable<StatModifier> statModifiers, float baseValue) {
      var allModifiers = statModifiers.ToList();

      foreach (var modifier in allModifiers.Where(modifier => modifier.Strategy is AddOperation)) {
        baseValue = modifier.Strategy.Calculate(baseValue);
      }

      foreach (var modifier in allModifiers.Where(modifier => modifier.Strategy is MultiplyOperation)) {
        baseValue = modifier.Strategy.Calculate(baseValue);
      }

      return baseValue;
    }
  }
}