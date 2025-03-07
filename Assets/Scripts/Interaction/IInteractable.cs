namespace Interaction
{
  public interface IInteractable {
    public string InteractionText {get;}
    public string InteractionHeader {get;}
    public bool Interact(PlayerInteractor playerInteractor);
  }
}
