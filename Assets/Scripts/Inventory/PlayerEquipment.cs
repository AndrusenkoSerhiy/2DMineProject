using System;
using Animation;
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
    [SerializeField] private string repairItemText;

    private GameManager gameManager;
    private PlayerController playerController;
    private PlayerInventory playerInventory;
    private Item equippedItem;

    public Transform ItemInHand {
      get => itemInHand;
      private set => itemInHand = value;
    }

    public Item EquippedItem => equippedItem;

    public event Action OnEquippedWeapon;
    public event Action OnUnequippedWeapon;

    private void Start() {
      gameManager = GameManager.Instance;
      playerController = gameManager.PlayerController;
      playerInventory = gameManager.PlayerInventory;

      userInterface.OnLoaded += AddEvents;
      userInterface.OnDisabled += RemoveEvents;
    }

    private void OnDisable() {
      userInterface.OnLoaded -= AddEvents;
      userInterface.OnDisabled -= RemoveEvents;
    }

    public void EquipTool(Item item) {
      if (item == null || item.isEmpty) {
        return;
      }

      var itemObject = item.info;

      if (itemObject.Type != ItemType.Tool) {
        return;
      }

      if (!item.IsBroken) {
        PlaceItemInHand(item);
      }

      equippedItem = item;
      AddWeaponEvents();
    }

    public void UnEquipTool() {
      if (equippedItem == null) {
        return;
      }

      RemoveItemFromHand();

      RemoveWeaponEvents();
      equippedItem = null;
    }

    private void RemoveItemFromHand() {
      if (!itemInHand) {
        return;
      }

      var itemObject = equippedItem.info;
      Destroy(itemInHand.gameObject);
      playerController.PlayerStats.Mediator.RemoveModifiersByItemId(itemObject.Id);

      OnUnequippedWeapon?.Invoke();
    }

    private void PlaceItemInHand(Item item) {
      var itemObject = item.info;
      itemInHand = Instantiate(itemObject.CharacterDisplay, GetParent(itemObject)).transform;
      itemInHand.localPosition = itemObject.SpawnPosition;
      itemInHand.localEulerAngles = itemObject.SpawnRotation;
      itemInHand.gameObject.layer = LayerMask.NameToLayer("Character");

      playerController.PlayerStats.Mediator.ApplyModifiers(ApplyType.Equip, itemObject);

      OnEquippedWeapon?.Invoke();
    }

    private void AddWeaponEvents() {
      if (equippedItem == null) {
        return;
      }

      equippedItem.OnItemBroken += OnItemBrokenHandler;
      equippedItem.OnItemRepaired += OnItemRepairedHandler;
    }

    private void RemoveWeaponEvents() {
      if (equippedItem == null) {
        return;
      }

      equippedItem.OnItemBroken -= OnItemBrokenHandler;
      equippedItem.OnItemRepaired -= OnItemRepairedHandler;
    }

    private void OnItemBrokenHandler() {
      RemoveItemFromHand();
    }

    private void OnItemRepairedHandler() {
      PlaceItemInHand(equippedItem);
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

    public void EquippedItemHoldAction() {
      if (equippedItem == null) {
        return;
      }

      if (EquippedItemCanBeRepaired()) {
        RepairEquippedItem();
      }
    }

    private void RepairEquippedItem() {
      var repairValue =
        playerInventory.Repair(equippedItem.MaxDurability, equippedItem.Durability, equippedItem.RepairCost);

      if (repairValue == 0) {
        return;
      }

      equippedItem.AddDurability(repairValue);
    }

    public bool ShowEquippedItemHoldAction() {
      return EquippedItemCanBeRepaired();
    }

    public string EquippedItemHoldActionText() {
      return repairItemText;
    }

    private bool EquippedItemCanBeRepaired() {
      return equippedItem is { CanBeRepaired: true };
    }
  }
}