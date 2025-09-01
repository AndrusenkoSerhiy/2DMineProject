using UnityEngine;

namespace Interaction {
  public interface IInteractable {
    public string InteractionText { get; }

    // public string InteractionHeader { get; }
    public bool HasHoldInteraction { get; }

    public string HoldInteractionText { get; }
    public string HoldProcessText { get; }

    // public string HoldInteractionHeader { get; }
    public bool Interact(PlayerInteractor playerInteractor);
    public bool HoldInteract(PlayerInteractor playerInteractor);
    public Bounds GetBounds();
  }
}