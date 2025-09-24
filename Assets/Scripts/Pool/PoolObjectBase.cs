using UnityEngine;

namespace Pool {
  public class PoolObjectBase : MonoBehaviour {

    [HideInInspector] public GameObject target;

    public virtual void ReturnToPool() {
      // Deactivate the object and return it to the pool
      gameObject.SetActive(false);
    }
  }
}