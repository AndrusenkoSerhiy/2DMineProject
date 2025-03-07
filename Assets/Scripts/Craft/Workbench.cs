using Interaction;
using UnityEngine;

namespace Craft {
  public class Workbench : Crafter, IInteractable {
    [SerializeField] private string interactText;
    [SerializeField] private string interactHeader;
    public string InteractionText => interactText;
    public string InteractionHeader => interactHeader;

    public bool Interact(PlayerInteractor playerInteractor) {
      CheckInteract();

      return true;
    }
  }
}