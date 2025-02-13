using UnityEngine;

namespace UnityServiceLocator {
  [DisallowMultipleComponent]
  [RequireComponent(typeof(ServiceLocator))]
  public abstract class Bootstrapper : MonoBehaviour {
    private ServiceLocator container;
    protected internal ServiceLocator Container => container.OrNull() ?? (container = GetComponent<ServiceLocator>());

    private bool hasBeenBootstrapped;

    public void Awake() => BootstrapOnDemand();

    public void BootstrapOnDemand() {
      if (hasBeenBootstrapped) return;
      hasBeenBootstrapped = true;
      Bootstrap();
    }

    protected abstract void Bootstrap();
  }
}