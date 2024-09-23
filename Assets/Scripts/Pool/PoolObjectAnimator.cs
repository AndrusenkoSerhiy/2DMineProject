using UnityEngine;

namespace Pool {
  public class PoolObjectAnimator : PoolObjectBase {
    [SerializeField] private Animator animator;
    [Tooltip("Return parent object to pool")][SerializeField] private bool _returnParent;

    private void Update() {
      if (animator != null) {
        // Check if the current animation is finished
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1 && !animator.IsInTransition(0)) {
          ReturnToPool();
        }
      }
    }

    public override void ReturnToPool() {
      if (_returnParent) {
        gameObject.transform.parent.gameObject.SetActive(false);
      }
      else base.ReturnToPool();
    }
  }
}