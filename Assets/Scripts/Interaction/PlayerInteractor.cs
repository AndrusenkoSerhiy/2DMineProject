using System;
using Inventory;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

namespace Interaction {
  public class PlayerInteractor : MonoBehaviour {
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private float radius = .5f;
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] private InteractionPrompt interactionPromtUI;
    [SerializeField] private InteractionPrompt holdInteractionPromtUI;

    private Collider2D[] colliders = new Collider2D[3];
    [SerializeField] private int numFound;

    private GameManager gameManager;
    private IInteractable interactable;
    private string actionName;
    private PlayerEquipment playerEquipment;

    private void Start() {
      gameManager = GameManager.Instance;
      GetActionName();
      gameManager.UserInput.controls.GamePlay.Interact.performed += OnInteractPerformed;
      gameManager.UserInput.OnGameDeviceChanged += OnGameDeviceChanged;
      playerEquipment = gameManager.PlayerEquipment;
    }

    private void OnGameDeviceChanged(object sender, EventArgs e) {
      GetActionName();
    }

    private void GetActionName() {
      actionName = ButtonPromptSprite.GetSpriteName(gameManager.UserInput.controls.GamePlay.Interact);
      interactionPromtUI.UpdateSpriteAsset();
      holdInteractionPromtUI.UpdateSpriteAsset();
    }

    private void Update() {
      //if any window is open don't allow to find items and show interaction message
      if (GameManager.Instance.WindowsController.IsAnyWindowOpen) {
        interactionPromtUI.ShowPrompt(false);
        holdInteractionPromtUI.ShowPrompt(false);
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
        ShowEquipmentHoldActionPrompt();
        return;
      }

      foreach (var col in colliders) {
        interactable = col.GetComponent<IInteractable>();
        if (interactable != null) {
          break;
        }
      }

      if (interactable == null) {
        return;
      }

      interactionPromtUI.ShowPrompt(true, ButtonPromptSprite.GetFullPrompt(interactable.InteractionText, actionName));

      if (!interactable.HasHoldInteraction) {
        ShowEquipmentHoldActionPrompt();
        return;
      }

      holdInteractionPromtUI.ShowPrompt(true,
        ButtonPromptSprite.GetFullPrompt(interactable.HoldInteractionText, actionName));
      /*if (GameManager.Instance.UserInput.controls.GamePlay.Interact.WasPressedThisFrame()) {
        interactable.Interact(this);
      }*/
    }

    private void ShowEquipmentHoldActionPrompt() {
      if (playerEquipment.ShowEquippedItemHoldAction()) {
        holdInteractionPromtUI.ShowPrompt(true,
          ButtonPromptSprite.GetFullPrompt(playerEquipment.EquippedItemHoldActionText(), actionName));
      }
      else {
        holdInteractionPromtUI.ShowPrompt(false);
      }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context) {
      if (context.interaction is HoldInteraction) {
        HoldInteract();
      }
      else {
        SimpleInteract();
      }
    }

    private void HoldInteract() {
      if (interactable is { HasHoldInteraction: true }) {
        interactable.HoldInteract(this);
      }
      else {
        playerEquipment.EquippedItemHoldAction();
      }
    }

    private void SimpleInteract() {
      interactable?.Interact(this);
    }

    private void OnDrawGizmos() {
      Gizmos.color = Color.red;
      Gizmos.DrawWireSphere(interactionPoint.position, radius);
    }

    private void OnDestroy() {
      if (!GameManager.HasInstance) {
        return;
      }

      gameManager.UserInput.controls.GamePlay.Interact.performed -= OnInteractPerformed;
      gameManager.UserInput.OnGameDeviceChanged -= OnGameDeviceChanged;
    }
  }
}