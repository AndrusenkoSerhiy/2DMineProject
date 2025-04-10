using System;
using Interaction;
using UnityEngine;

namespace Craft {
  public class HandCraft : Crafter {
    [SerializeField] private InteractionPrompt interactionPrompt;
    [SerializeField] private string actionName;
    private string buttonName;
    private GameManager gameManager;
    
    //TODO
    //for inventory prompt
    [SerializeField] private InteractionPrompt inventoryPrompt;

    [SerializeField] private string inventoryActionName;
    private string inventoryButtonName;
    public void Start() {
      gameManager = GameManager.Instance;
      gameManager.UserInput.controls.UI.HandCraft.performed += ctx => CheckInteract();
      gameManager.UserInput.OnGameDeviceChanged += OnGameDeviceChanged;
      
      OnGameDeviceChanged(null, null);
    }

    private void OnGameDeviceChanged(object sender, EventArgs e) {
      buttonName = ButtonPromptSprite.GetSpriteName(gameManager.UserInput.controls.UI.HandCraft);
      interactionPrompt.UpdateSpriteAsset();
      interactionPrompt.ShowPrompt(true, ButtonPromptSprite.GetFullPrompt(actionName,buttonName));
      UpdateInventoryPrompt();
    }

    //TODO
    private void UpdateInventoryPrompt() {
      inventoryButtonName = ButtonPromptSprite.GetSpriteName(gameManager.UserInput.controls.UI.Inventory);
      inventoryPrompt.UpdateSpriteAsset();
      inventoryPrompt.ShowPrompt(true, ButtonPromptSprite.GetFullPrompt(inventoryActionName,inventoryButtonName));
    }
    private void OnDestroy() {
      if (GameManager.HasInstance) {
        gameManager.UserInput.OnGameDeviceChanged -= OnGameDeviceChanged;
      }
    }
  }
}