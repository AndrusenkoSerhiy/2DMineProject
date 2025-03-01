using Interaction;
using UnityEngine;

namespace Craft {
  public class Workbench : Crafter, IInteractable {
    [SerializeField] private string interactText;
    public string InteractionPrompt => interactText;

    public bool Interact(PlayerInteractor playerInteractor) {
      CheckInteract();

      return true;
    }
  }
}