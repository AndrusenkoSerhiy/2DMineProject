using UnityEngine;

namespace Pool {
  public class PoolObjectBase : MonoBehaviour {
    public virtual void ReturnToPool() {
      // Deactivate the object and return it to the pool
      gameObject.SetActive(false);
    }
  }
}