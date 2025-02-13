namespace Interaction
{
  public interface IInteractable {
    public string InteractionPrompt {get;}
    public bool Interact(PlayerInteractor playerInteractor);
  }
}
