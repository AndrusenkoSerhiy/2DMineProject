using UnityEngine;

namespace Animation {
  public class AnimatorEventReceiver : MonoBehaviour {
    public void TriggerAttackEnd(AnimationEvent animationEvent) {
      AnimationEventManager.TriggerAttackEnded(animationEvent, transform.parent.gameObject);
    }

    public void TriggerAttackStart(AnimationEvent animationEvent) {
      AnimationEventManager.TriggerAttackStarted(animationEvent, transform.parent.gameObject);
    }

    public void Footstep() {
      AnimationEventManager.StartFootstepEffect();
    }

    public void RobotRepaired() {
      AnimationEventManager.RobotRepaired();
    }
  }
}