using System;
using Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using Slider = UnityEngine.UI.Slider;

namespace Interaction {
  public class PlayerInteractor : MonoBehaviour {
    [SerializeField] private ObjectInteractionPrompts objectInteractionPrompts;
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private float radius = .5f;
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] private InteractionPrompt interactionPromtUI;
    [SerializeField] private InteractionPrompt holdInteractionPromtUI;
    [SerializeField] private TextMeshProUGUI holdActionProgressText;
    [SerializeField] private Slider holdActionProgressSlider;

    private Collider2D[] colliders = new Collider2D[3];
    [SerializeField] private int numFound;

    private GameManager gameManager;
    private IInteractable interactable;
    private string actionName;
    private PlayerEquipment playerEquipment;

    private bool isHolding;
    private float holdTime;
    private float requiredHoldDuration;

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

      objectInteractionPrompts.UpdateSpriteAsset();
    }

    private void Update() {
      //if any window is open don't allow to find items and show interaction message
      if (GameManager.Instance.WindowsController.IsAnyWindowOpen ||
          gameManager.MiningRobotController.GetExitInteract()) {
        interactionPromtUI.ShowPrompt(false);

        objectInteractionPrompts.HideInteractionPrompt();

        holdInteractionPromtUI.ShowPrompt(false);

        objectInteractionPrompts.HideHoldInteractionPrompt();
        return;
      }

      UpdateInteractionPrompt();
      HandleHoldProgress();
    }

    private void UpdateInteractionPrompt() {
      colliders = Physics2D.OverlapCircleAll(interactionPoint.position, radius, interactableMask);
      numFound = colliders.Length;

      if (numFound <= 0) {
        interactable = null;
        interactionPromtUI.ShowPrompt(false);

        objectInteractionPrompts.HideInteractionPrompt();
        objectInteractionPrompts.HideHoldInteractionPrompt();

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

      // interactionPromtUI.ShowPrompt(true, ButtonPromptSprite.GetFullPrompt(interactable.InteractionText, actionName));
      if (!String.IsNullOrEmpty(interactable.InteractionText))
        objectInteractionPrompts.ShowInteractionPrompt(interactable,
          ButtonPromptSprite.GetFullPrompt(interactable.InteractionText, actionName, true));

      if (!interactable.HasHoldInteraction) {
        ShowEquipmentHoldActionPrompt();
        //return;
      }

      //when you repair robot the prompt is hided
      var str = interactable.HasHoldInteraction
        ? ButtonPromptSprite.GetFullPrompt(interactable.HoldInteractionText, actionName, true, true)
        : string.Empty;
      objectInteractionPrompts.ShowHoldInteractionPrompt(interactable, str);
    }

    private void ShowEquipmentHoldActionPrompt() {
      if (playerEquipment.ShowEquippedItemHoldAction()) {
        holdInteractionPromtUI.ShowPrompt(true,
          ButtonPromptSprite.GetFullPrompt(playerEquipment.EquippedItemHoldActionText(),
            actionName + "_hold"));
      }
      else {
        holdInteractionPromtUI.ShowPrompt(false);
      }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context) {
      if (context.interaction is HoldInteraction holdInteraction) {
        HoldInteract(holdInteraction);
      }
      else {
        SimpleInteract();
      }
    }

    private void HoldInteract(HoldInteraction holdInteraction) {
      StartHoldProgress(holdInteraction);
    }

    private void HoldAction() {
      if (interactable is { HasHoldInteraction: true }) {
        interactable.HoldInteract(this);
      }
      else {
        playerEquipment.EquippedItemHoldAction();
      }
    }

    private string HoldText() {
      return interactable is { HasHoldInteraction: true }
        ? interactable.HoldProcessText
        : playerEquipment.EquippedItemRepairingActionText();
    }

    private void HandleHoldProgress() {
      if (!isHolding) {
        return;
      }

      //skip hold progress if we don't have equipped item
      if (interactable == null && (playerEquipment.EquippedItem == null ||
                                   playerEquipment.EquippedItem.Durability.Equals(playerEquipment.EquippedItem
                                     .MaxDurability)) ||
          //when robot is repaired to max health
          interactable is { HasHoldInteraction: false }) {
        CancelHoldProgress();
        return;
      }

      holdTime += Time.deltaTime;
      holdActionProgressSlider.value = holdTime / requiredHoldDuration;

      if (holdTime >= requiredHoldDuration) {
        HoldAction();
        CancelHoldProgress();
      }

      // Cancel if button released early
      if (!gameManager.UserInput.controls.GamePlay.Interact.IsPressed()) {
        CancelHoldProgress();
      }
    }

    private void StartHoldProgress(HoldInteraction holdInteraction) {
      requiredHoldDuration = holdInteraction.duration;
      isHolding = true;
      holdTime = 0f;

      holdActionProgressSlider.gameObject.SetActive(true);
      holdActionProgressText.gameObject.SetActive(true);
      holdActionProgressText.text = HoldText();
    }

    private void CancelHoldProgress() {
      isHolding = false;
      holdActionProgressSlider.value = 0f;
      holdActionProgressSlider.gameObject.SetActive(false);
      holdActionProgressText.gameObject.SetActive(false);
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