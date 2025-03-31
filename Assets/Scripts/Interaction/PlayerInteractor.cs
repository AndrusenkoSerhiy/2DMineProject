using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Interaction
{
  public class PlayerInteractor : MonoBehaviour {
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private float radius = .5f;
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] private InteractionPrompt interactionPromtUI;

    private Collider2D[] colliders = new Collider2D[3];
    [SerializeField] private int numFound;

    private IInteractable interactable;
    private string actionName;

    private void Start() {
      GetActionName();
      InputSystem.onActionChange += InputActionChangeCallback;
    }

    private void GetActionName() {
      actionName = GameManager.Instance.UserInput.controls.GamePlay.Interact.GetBindingDisplayString((int)GameManager.Instance.UserInput.ActiveGameDevice);
      interactionPromtUI.UpdateSpriteAsset();
    }

    private void Update() {
      //if any window is open don't allow to find items and show interaction message
      if (GameManager.Instance.WindowsController.IsAnyWindowOpen) {
        interactionPromtUI.ShowPrompt(false);
        return; 
      }
      
      UpdateInteractionPrompt();
    }

    private void UpdateInteractionPrompt() {
      colliders = Physics2D.OverlapCircleAll(interactionPoint.position, radius, interactableMask);
      numFound = colliders.Length;

      if (numFound <= 0) {
        interactable = null;
        interactionPromtUI.ShowPrompt(false);
        return;
      }

      foreach (var col in colliders) {
        interactable = col.GetComponent<IInteractable>();
        if (interactable != null) break;
      }
      
      if (interactable == null)
        return;

      interactionPromtUI.ShowPrompt(true, interactable.InteractionText + "<sprite name=" + actionName + ">");
      if (GameManager.Instance.UserInput.controls.GamePlay.Interact.WasPressedThisFrame()) {
        interactable.Interact(this);
      }
    }
    
    private void InputActionChangeCallback(object arg1, InputActionChange arg2) {
      GetActionName();
    }

    private void OnDrawGizmos() {
      Gizmos.color = Color.red;
      Gizmos.DrawWireSphere(interactionPoint.position, radius);
    }

    private void OnDestroy() {
      InputSystem.onActionChange -= InputActionChangeCallback;
    }
  }
}
