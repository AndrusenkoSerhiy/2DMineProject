using System;
using UnityEngine;

public class AnimationEventManager : MonoBehaviour {
  public static Action<AnimationEvent> OnAttackStarted;
  public static Action<AnimationEvent> OnAttackEnded;

  public static void TriggerAttackStarted(AnimationEvent animationEvent) {
    OnAttackStarted?.Invoke(animationEvent);
  }

  public static void TriggerAttackEnded(AnimationEvent animationEvent) {
    OnAttackEnded?.Invoke(animationEvent);
  }
}