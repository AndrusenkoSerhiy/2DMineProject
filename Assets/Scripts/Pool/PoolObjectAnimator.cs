using UnityEngine;

namespace Pool {
  public class PoolObjectAnimator : PoolObjectBase {
    [SerializeField] private Animator animator;
    private bool animationFinished;

    private void Update() {
      if (animator != null) {
        // Check if the current animation is finished
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1 && !animator.IsInTransition(0)) {
          ReturnToPool();
        }
      }
    }

  }
}