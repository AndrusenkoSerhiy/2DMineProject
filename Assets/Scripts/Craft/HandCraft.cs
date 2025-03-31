using Interaction;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Craft {
  public class HandCraft : Crafter {
    [SerializeField] private InteractionPrompt interactionPrompt;
    [SerializeField] private string actionName;
    private string buttonName;
    public void Start() {
      GameManager.Instance.UserInput.controls.UI.HandCraft.performed += ctx => CheckInteract();
      InputSystem.onActionChange += InputActionChangeCallback;
      buttonName =
        GameManager.Instance.UserInput.controls.UI.HandCraft.GetBindingDisplayString(
          (int)GameManager.Instance.UserInput.ActiveGameDevice);
      interactionPrompt.ShowPrompt(true, actionName + " " + "<sprite name=" + buttonName + ">");
    }

    private void InputActionChangeCallback(object arg1, InputActionChange arg2) {
      buttonName = GameManager.Instance.UserInput.controls.UI.HandCraft.GetBindingDisplayString((int)GameManager.Instance.UserInput.ActiveGameDevice);
      interactionPrompt.UpdateSpriteAsset();
      interactionPrompt.ShowPrompt(true, actionName + " " + " " + "<sprite name=" + buttonName + ">");
    }
  }
}