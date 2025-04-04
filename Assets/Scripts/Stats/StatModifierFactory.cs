using System;
using Modifier = Scriptables.Stats.StatModifier;

namespace Stats {
  public interface IStatModifierFactory {
    public StatModifier Create(Modifier statModifier, string itemObjectId);
  }

  public class StatModifierFactory : IStatModifierFactory {
    public StatModifier Create(Modifier statModifier, string itemObjectId) {
      IOperationStrategy strategy = statModifier.operatorType switch {
        OperatorType.Add => new AddOperation(statModifier.value),
        OperatorType.Multiply => new MultiplyOperation(statModifier.value),
        _ => throw new ArgumentOutOfRangeException()
      };

      return new StatModifier(statModifier, strategy, itemObjectId);
    }
  }
}