using UnityEngine;

namespace Animation {
  public class AnimatorEventReceiver : MonoBehaviour {
    public void TriggerAttackEnd(AnimationEvent animationEvent) {
      AnimationEventManager.TriggerAttackEnded(animationEvent);
    }

    public void TriggerAttackStart(AnimationEvent animationEvent) {
      AnimationEventManager.TriggerAttackStarted(animationEvent);
    }

    public void Footstep() {
      AnimationEventManager.StartFootstepEffect();
    }
  }
}