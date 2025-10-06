using Menu;
using Player;
using Scriptables.Items;
using Stats;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Inventory {
  //TODO: Add consume animation, get animation type from IConsumableItem
  public class ItemConsumer {
    private readonly GameManager gameManager;
    private readonly PlayerController playerController;
    private readonly PlayerStats playerStats;
    private readonly System.Action<InputAction.CallbackContext> leftClickHandler;
    private InventorySlot activeSlot;
    private bool isClickHandlerAdded;
    private IConsumableItem consumableItem;

    public ItemConsumer() {
      gameManager = GameManager.Instance;
      playerController = gameManager.PlayerController;
      playerStats = playerController.PlayerStats;
      leftClickHandler = ctx => DefaultConsume();
    }

    public void SetActiveSlot(InventorySlot slot) {
      activeSlot = slot;
      OnOffPlayerAttack();
      RemoveLeftMouseClickHandler();

      if (slot == null) {
        return;
      }
      
      if (slot.isEmpty) {
        return;
      }

      var itemObject = slot.Item.info;

      if (itemObject is not IConsumableItem iConsumableItem) {
        return;
      }

      consumableItem = iConsumableItem;

      switch (itemObject) {
        case BuildingBlock buildingBlock:
          UseBuildingBlock(buildingBlock);
          break;
        default:
          DefaultConsumeItemSet();
          break;
      }
    }

    private void OnOffPlayerAttack() {
      if (activeSlot == null || activeSlot.isEmpty || activeSlot.Item.info is IAttackableItem) {
        playerController.SetLockHighlight(false, "ItemConsumer");
      }
      else {
        playerController.SetLockHighlight(true, "ItemConsumer");
      }
    }

    public void DeactivateItem(Item item) {
      if (item == null || item.isEmpty) {
        return;
      }

      switch (item.info) {
        case BuildingBlock:
          gameManager.PlaceCell.DisableBuildMode();
          break;
      }
    }

    private void DefaultConsumeItemSet() {
      AddLeftMouseClickHandler();
      MenuController.OnExitToMainMenu += ExitToMainMenu;
    }

    private void ExitToMainMenu() {
      MenuController.OnExitToMainMenu -= RemoveLeftMouseClickHandler;
      RemoveLeftMouseClickHandler();
    }
    private void AddLeftMouseClickHandler() {
      if (isClickHandlerAdded) {
        return;
      }
      //Debug.LogError("AddLeftMouseClickHandler");
      gameManager.UserInput.controls.GamePlay.Attack.performed += leftClickHandler;
      isClickHandlerAdded = true;
    }

    private void DefaultConsume() {
      if (activeSlot.isEmpty) {
        RemoveLeftMouseClickHandler();
        return;
      }

      if (!playerStats.Mediator.ApplyModifiers(ApplyType.Use, activeSlot.Item.info)) {
        return;
      }

      SpawnEffect();
      
      gameManager.AudioController.PlayAudio(consumableItem?.ConsumeSound);
      gameManager.ObjectivesSystem.ReportItemUse(activeSlot.Item.info, 1);

      activeSlot.RemoveAmount(1);
    }

    private void SpawnEffect() {
      var statsType = activeSlot.Item.info.statModifiers[0];
      var particleName = statsType.modifierDisplayObject != null ? 
        statsType.modifierDisplayObject.particleName :"HealingParticleEffect";

      var effectParticle = GameManager.Instance.PoolEffects.SpawnFromPool(particleName,
        playerController.gameObject.transform.position, Quaternion.identity);
      effectParticle.target = playerController.gameObject;
    }

    private void RemoveLeftMouseClickHandler() {
      if (!isClickHandlerAdded) {
        return;
      }
      //Debug.LogError("RemoveLeftMouseClickHandler");
      gameManager.UserInput.controls.GamePlay.Attack.performed -= leftClickHandler;
      isClickHandlerAdded = false;
    }

    private void UseBuildingBlock(BuildingBlock buildingBlock) {
      var prefab = buildingBlock.CharacterDisplay ? buildingBlock.CharacterDisplay : buildingBlock.spawnPrefab;
      gameManager.PlaceCell.ActivateBuildMode(buildingBlock, prefab);
    }
  }
}