using System;
using Scriptables.Items;
using Stats;
using UnityEngine;

namespace Inventory {
  public class PlayerEquipment : MonoBehaviour {
    //private InventoryObject _equipment;
    private Inventory quickSlots;

    [Header("Equip Transforms")] [SerializeField]
    private Transform offhandWristTransform;

    [SerializeField] private Transform offhandHandTransform;
    [SerializeField] private Transform leftHandTransform;
    [SerializeField] private Transform rightHandTransform;
    [SerializeField] private Transform bodyTransform;

    private Transform _pants;
    private Transform _gloves;
    private Transform _boots;
    private Transform _chest;
    private Transform _helmet;
    private Transform _offhand;
    [SerializeField] private Transform itemInHand;
    public event Action OnEquippedWeapon;
    public event Action OnUnequippedWeapon;

    public Transform ItemInHand {
      get => itemInHand;
      private set => itemInHand = value;
    }

    private void Start() {
      quickSlots = GameManager.Instance.PlayerInventory.GetQuickSlots();
      /*for (int i = 0; i < quickSlots.GetSlots.Length; i++) {
        if (quickSlots.GetSlots[i].IsSelected) {
          OnEquipItem(quickSlots.GetSlots[i]);
        }
      }*/
    }

    public void OnEquipItem(InventorySlot slot) {
      var itemObject = slot.GetItemObject();
      if (itemObject == null)
        return;

      switch (slot.Parent.Inventory.Type) {
        case InventoryType.QuickSlots: //InterfaceType.Equipment
          switch (slot.GetItemObject().Type) {
            case ItemType.Tool:
              itemInHand = Instantiate(itemObject.CharacterDisplay, GetParent(itemObject)).transform;
              itemInHand.localPosition = itemObject.SpawnPosition;
              itemInHand.localEulerAngles = itemObject.SpawnRotation;
              itemInHand.gameObject.layer = LayerMask.NameToLayer("Character");

              GameManager.Instance.PlayerController.EntityStats.Mediator.ApplyModifiers(ApplyType.Equip, itemObject);
              
              OnEquippedWeapon?.Invoke();
              break;
          }

          break;
      }
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

    public void OnRemoveItem(Item item, InventoryType type) {
      if (item.info == null) {
        return;
      }

      switch (type) {
        case InventoryType.QuickSlots: //InterfaceType.Equipment
          if (item.info.CharacterDisplay != null) {
            switch (item.info.Type) {
              case ItemType.Shield:
                Destroy(_offhand.gameObject);
                break;

              case ItemType.Tool:
                Destroy(itemInHand.gameObject);
                
                GameManager.Instance.PlayerController.EntityStats.Mediator.RemoveModifiersByItemId(item.info.Id);
                
                OnUnequippedWeapon?.Invoke();
                break;
              case ItemType.Weapon:
                //Destroy(Weapon.gameObject);
                //OnUnequippedWeapon?.Invoke();
                break;
            }
          }

          break;
      }
    }

    public void OnRemoveItem(InventorySlot slot) {
      OnRemoveItem(slot.Item, slot.InventoryType);
    }
  }
}