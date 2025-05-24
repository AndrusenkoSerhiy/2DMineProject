using System;
using UnityEngine;

namespace Animation {
  public class AnimationEventManager : MonoBehaviour {
    public static Action<AnimationEvent, GameObject> onAttackStarted;
    public static Action<AnimationEvent, GameObject> onAttackEnded;
    public static Action onFootstep;
    public static Action onRobotRepaired;
    public static Action onLeftStep;
    public static Action onRightStep;

    public static void TriggerAttackStarted(AnimationEvent animationEvent, GameObject go) {
      onAttackStarted?.Invoke(animationEvent, go);
    }

    public static void TriggerAttackEnded(AnimationEvent animationEvent, GameObject go) {
      onAttackEnded?.Invoke(animationEvent, go);
    }

    public static void StartFootstepEffect() {
      onFootstep?.Invoke();
    }

    public static void RobotRepaired() {
      onRobotRepaired?.Invoke();
    }

    public static void LeftStep() {
      onLeftStep?.Invoke();
    }

    public static void RightStep() {
      onRightStep?.Invoke();
    }
  }
}