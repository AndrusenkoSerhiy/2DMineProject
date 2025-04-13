using Player;
using Scriptables.Items;
using Stats;
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

      if (itemObject is not IConsumableItem) {
        return;
      }

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
        playerController.SetLockHighlight(false);
      }
      else {
        playerController.SetLockHighlight(true);
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
    }

    private void AddLeftMouseClickHandler() {
      if (isClickHandlerAdded) {
        return;
      }

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

      activeSlot.RemoveAmount(1);
    }

    private void RemoveLeftMouseClickHandler() {
      if (!isClickHandlerAdded) {
        return;
      }

      gameManager.UserInput.controls.GamePlay.Attack.performed -= leftClickHandler;
      isClickHandlerAdded = false;
    }

    private void UseBuildingBlock(BuildingBlock buildingBlock) {
      var prefab = buildingBlock.CharacterDisplay ? buildingBlock.CharacterDisplay : buildingBlock.spawnPrefab;
      gameManager.PlaceCell.ActivateBuildMode(buildingBlock.BuildingData,buildingBlock.ResourceData, prefab);
    }
  }
}