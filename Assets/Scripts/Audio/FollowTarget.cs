using UnityEngine;

namespace Audio {
  public class FollowTarget : MonoBehaviour {
    public Transform target;

    private void LateUpdate() {
      if (target) {
        transform.position = target.position;
      }
    }
  }
}