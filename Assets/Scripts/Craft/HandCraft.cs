namespace Craft {
  public class HandCraft : Crafter {
    public void Start() {
      GameManager.Instance.UserInput.controls.UI.HandCraft.performed += ctx => CheckInteract();
    }
  }
}