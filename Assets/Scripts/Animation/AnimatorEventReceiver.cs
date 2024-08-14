using System;
using UnityEngine;

namespace Animation {
  public class AnimatorEventReceiver : MonoBehaviour {
    public event Action<AnimationEvent> OnAnimationEnded;
    public event Action<AnimationEvent> OnAnimationStarted;

    void OnAnimationEnd(AnimationEvent animationEvent) {
      OnAnimationEnded?.Invoke(animationEvent);
    }

    void OnAnimationStart(AnimationEvent animationEvent) {
      OnAnimationStarted?.Invoke(animationEvent);
    }
  }
}