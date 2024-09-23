using System;
using UnityEngine;

public class AnimationEventManager : MonoBehaviour {
  public static Action<AnimationEvent> onAttackStarted;
  public static Action<AnimationEvent> onAttackEnded;
  public static Action onFootstep;

  public static void TriggerAttackStarted(AnimationEvent animationEvent) {
    onAttackStarted?.Invoke(animationEvent);
  }

  public static void TriggerAttackEnded(AnimationEvent animationEvent) {
    onAttackEnded?.Invoke(animationEvent);
  }

  public static void StartFootstepEffect() {
    onFootstep?.Invoke();
  }
}