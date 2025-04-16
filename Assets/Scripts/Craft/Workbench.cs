using Interaction;
using UnityEngine;

namespace Craft {
  public class Workbench : Crafter, IInteractable {
    [SerializeField] private string interactText;
    [SerializeField] private string holdInteractText;

    public string InteractionText => interactText;
    public bool HasHoldInteraction { get; }
    public string HoldInteractionText => holdInteractText;

    public bool Interact(PlayerInteractor playerInteractor) {
      CheckInteract();

      return true;
    }

    public bool HoldInteract(PlayerInteractor playerInteractor) {
      return false;
    }
  }
}