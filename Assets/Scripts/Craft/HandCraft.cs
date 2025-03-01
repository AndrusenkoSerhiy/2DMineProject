using Settings;

namespace Craft {
  public class HandCraft : Crafter {
    public void Start() {
      UserInput.instance.controls.UI.HandCraft.performed += ctx => CheckInteract();
    }
  }
}