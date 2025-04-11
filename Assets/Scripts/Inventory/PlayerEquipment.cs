using System;
using Player;
using Scriptables.Items;
using Stats;
using UnityEngine;

namespace Inventory {
  public class PlayerEquipment : MonoBehaviour {
    [SerializeField] private UserInterface userInterface;
    [SerializeField] private Transform itemInHand;
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private Transform leftHandTransform;
    [SerializeField] private Transform rightHandTransform;

    private GameManager gameManager;
    private PlayerController playerController;

    public Transform ItemInHand {
      get => itemInHand;
      private set => itemInHand = value;
    }

    public event Action OnEquippedWeapon;
    public event Action OnUnequippedWeapon;

    private void Start() {
      gameManager = GameManager.Instance;
      playerController = gameManager.PlayerController;

      userInterface.OnLoaded += AddEvents;
      userInterface.OnDisabled += RemoveEvents;
    }

    private void OnDisable() {
      userInterface.OnLoaded -= AddEvents;
      userInterface.OnDisabled -= RemoveEvents;
    }

    public void EquipTool(ItemObject itemObject) {
      if (itemObject == null) {
        return;
      }

      if (itemObject.Type != ItemType.Tool) {
        return;
      }

      itemInHand = Instantiate(itemObject.CharacterDisplay, GetParent(itemObject)).transform;
      itemInHand.localPosition = itemObject.SpawnPosition;
      itemInHand.localEulerAngles = itemObject.SpawnRotation;
      itemInHand.gameObject.layer = LayerMask.NameToLayer("Character");

      playerController.PlayerStats.Mediator.ApplyModifiers(ApplyType.Equip, itemObject);

      OnEquippedWeapon?.Invoke();
    }

    public void UnEquipTool(ItemObject itemObject) {
      if (itemObject == null) {
        return;
      }

      if (itemObject.Type != ItemType.Tool) {
        return;
      }

      Destroy(itemInHand.gameObject);
      playerController.PlayerStats.Mediator.RemoveModifiersByItemId(itemObject.Id);

      OnUnequippedWeapon?.Invoke();
    }

    private Transform GetParent(ItemObject item) {
      Transform tr = null;
      switch (item.ParentType) {
        case ParentType.Body:
          tr = bodyTransform;
          break;

        case ParentType.LeftHand:
          tr = leftHandTransform;
          break;

        case ParentType.RightHand:
          tr = rightHandTransform;
          break;
      }

      return tr;
    }

    private void EquipArmor(SlotUpdateEventData data) {
      if (data.before.amount == data.after.amount) {
        return;
      }

      if (data.after.amount == 0) {
        playerController.PlayerStats.Mediator.RemoveModifiersByItemId(data.before.Item.id);
      }
      else {
        playerController.PlayerStats.Mediator.ApplyModifiers(ApplyType.Equip, data.after.Item.info);
      }
    }

    private void AddEvents() {
      foreach (var (_, slot) in userInterface.SlotsOnInterface) {
        slot.OnAfterUpdated += EquipArmor;
      }
    }

    private void RemoveEvents() {
      foreach (var (_, slot) in userInterface.SlotsOnInterface) {
        slot.OnAfterUpdated -= EquipArmor;
      }
    }
  }
}