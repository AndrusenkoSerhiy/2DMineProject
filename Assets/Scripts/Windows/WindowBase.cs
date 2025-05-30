using System;
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
    protected string buttonName;

    public virtual void Init() {
      
    }
    private void GetInteractionPrompt() {
      if (!interactionPromtUI) {
        interactionPromtUI = GameManager.Instance.InteractionPromptUI;
      }
      GetInteractionText();
      SetInteractionText();
    }

    private void GetInteractionText() {
      if (string.IsNullOrEmpty(actionName))
        return;
      
      interactionPromtUI.UpdateSpriteAsset();
      buttonName = ButtonPromptSprite.GetSpriteName(GameManager.Instance.UserInput.controls.UI.Craft);
    }

    protected virtual void SetInteractionText() {
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
      GameManager.Instance.UserInput.OnGameDeviceChanged += InputActionChangeCallback;
      //GameManager.Instance.UserInput.ShowCursor(true);
    }

    private void InputActionChangeCallback(object sender, EventArgs e) {
      OnChangeDevice();
    }

    protected virtual void OnChangeDevice() {
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
      interactionPromtUI?.ShowPrompt(false);
      GameManager.Instance.UserInput.OnGameDeviceChanged -= InputActionChangeCallback;
      //GameManager.Instance.UserInput.ShowCursor(false);
    }
    
    private void LockPlayer(bool state) {
      GetCurrPlayerController().SetLockPlayer(state);
      GameManager.Instance.UserInput.EnableGamePlayControls(!state);
    }

    private void LockHighlight(bool state) {
      GetCurrPlayerController().SetLockHighlight(state);
    }

    private void OnDestroy() {
      if (GameManager.HasInstance) {
        GameManager.Instance.UserInput.OnGameDeviceChanged -= InputActionChangeCallback;
      }
    }
  }
}