using System;
using Scriptables.Stats;
using Utils;
using Modifier = Scriptables.Stats.StatModifier;

namespace Stats {
  public class StatModifier : IDisposable {
    private readonly CountdownTimer timer;
    private readonly string id;
    private readonly string itemId;
    private readonly float value;
    private bool markedForRemoval;
    private Modifier modifier;

    public string Id => id;
    public string ItemId => itemId;
    public float Value => value;
    public float TimeLeft => timer?.Time ?? 0;
    public float Duration => timer?.Duration ?? 0;
    public float Progress => timer?.Progress ?? 0;
    public StatType Type { get; }
    public ModifierDisplayObject modifierDisplayObject { get; }
    public IOperationStrategy Strategy { get; }
    public bool MarkedForRemoval => markedForRemoval;
    public event Action<StatModifier> OnDispose = delegate { };
    public Modifier Modifier => modifier;

    public StatModifier(Modifier statModifier, IOperationStrategy strategy, string itemObjectId) {
      modifier = statModifier;
      itemId = itemObjectId;
      value = statModifier.value;
      Type = statModifier.type;
      modifierDisplayObject = statModifier.modifierDisplayObject;
      Strategy = strategy;
      id = GenerateId();

      if (statModifier.duration <= 0) {
        return;
      }

      var timeLeft = statModifier.timeLeft > 0
        ? statModifier.timeLeft
        : statModifier.duration;

      timer = new CountdownTimer(statModifier.duration, timeLeft);
      timer.OnTimerStop += () => markedForRemoval = true;
      timer.Start();
    }

    public void Update(float deltaTime) => timer?.Tick(deltaTime);

    public void Dispose() {
      OnDispose.Invoke(this);
    }

    public void Pause() {
      timer?.Pause();
    }

    public void Resume() {
      timer?.Resume();
    }

    public void ResetMarkForRemoval() {
      markedForRemoval = false;
    }

    public bool HasDisplay() {
      return modifierDisplayObject != null;
    }

    private string GenerateId() {
      var prefix = $"{Type}_{Strategy}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
      return HasDisplay()
        ? $"{prefix}_{modifierDisplayObject.Id}".ToLower()
        : prefix.ToLower();
    }
  }
}