using Interaction;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Windows {
  public class WindowBase : MonoBehaviour {
    [SerializeField] private bool isShow;
    public bool IsShow => isShow;
    public delegate void ShowWindow(WindowBase window);
    public event ShowWindow OnShow;
    public event ShowWindow OnHide;
    public InteractionPrompt interactionPromtUI;

    [SerializeField] private string actionName = string.Empty;
    private string buttonName;

    private void GetInteractionPrompt() {
      if (!interactionPromtUI) {
        interactionPromtUI = GameManager.Instance.InteractionPromptUI;
        GetInteractionText();
      }
      
      SetInteractionText();
    }

    private void GetInteractionText() {
      if (string.IsNullOrEmpty(actionName))
        return;
      interactionPromtUI.UpdateSpriteAsset();
      //buttonName = "<sprite name=" + GameManager.Instance.UserInput.controls.UI.Craft.GetBindingDisplayString((int)GameManager.Instance.UserInput.ActiveGameDevice) + ">";
      buttonName = ButtonPromptSprite.GetSpriteName(GameManager.Instance.UserInput.controls.UI.Craft);
    }

    private void SetInteractionText() {
      /*if (actionName == string.Empty)
        return;*/
      
      interactionPromtUI.ShowPrompt(true, ButtonPromptSprite.GetFullPrompt(actionName, buttonName));
    }

    private PlayerControllerBase GetCurrPlayerController() {
      return GameManager.Instance.CurrPlayerController;
    }
    
    public virtual void Show() {
      GetInteractionPrompt();
      isShow = true;
      gameObject.SetActive(true);
      OnShow?.Invoke(this);
      LockPlayer(true);
      LockHighlight(true);
      InputSystem.onActionChange += InputActionChangeCallback;
    }

    private void InputActionChangeCallback(object arg1, InputActionChange arg2) {
      buttonName = ButtonPromptSprite.GetSpriteName(GameManager.Instance.UserInput.controls.UI.Craft);
      interactionPromtUI.UpdateSpriteAsset();
      SetInteractionText();
    }

    public virtual void Hide() {
      isShow = false;
      gameObject.SetActive(false);
      OnHide?.Invoke(this);
      LockPlayer(false);
      LockHighlight(false);
      interactionPromtUI.ShowPrompt(false);
      InputSystem.onActionChange -= InputActionChangeCallback;
    }
    
    private void LockPlayer(bool state) {
      GetCurrPlayerController().SetLockPlayer(state);
      GameManager.Instance.UserInput.EnableGamePlayControls(!state);
    }

    private void LockHighlight(bool state) {
      GetCurrPlayerController().SetLockHighlight(state);
    }

    private void OnDestroy() {
      InputSystem.onActionChange -= InputActionChangeCallback;
    }
  }
}