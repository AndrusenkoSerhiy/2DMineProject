using System;
using Stats;
using UnityEngine;

namespace Scriptables.Stats {
  [Serializable]
  public class StatModifier {
    public StatType type;
    public ApplyType applyType;
    public OperatorType operatorType;
    public float value;

    [Tooltip(
      "Immediately applies the modifier to stats, duration ignored, canApplyWhenPreviousAt ignored, removePrevious ignored")]
    public bool permanent;

    [Tooltip("Duration in seconds, if 0 - modifier without lifetime")]
    public float duration;
    [Tooltip("Need only for load from file")]
    public float timeLeft;

    [Tooltip("Percentage of the previous modifier(same DisplayObject, if not set - same type) " +
             "duration when the new modifier can be applied, " +
             "if 100 - can apply multiple times")]
    public float canApplyWhenPreviousAt = 100f;

    public RemovePreviousModifiersType removePrevious = RemovePreviousModifiersType.None;

    [Tooltip("For modifier display")] public ModifierDisplayObject modifierDisplayObject;
  }
}