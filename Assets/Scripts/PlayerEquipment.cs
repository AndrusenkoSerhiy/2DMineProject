using Scriptables.Inventory;
using Scriptables.Items;
using UnityEngine;

namespace Equipment {
  public class PlayerEquipment : MonoBehaviour {
    private InventoryObject _equipment;

    [Header("Equip Transforms")]
    [SerializeField] private Transform offhandWristTransform;
    [SerializeField] private Transform offhandHandTransform;
    [SerializeField] private Transform weaponTransform;

    private Transform _pants;
    private Transform _gloves;
    private Transform _boots;
    private Transform _chest;
    private Transform _helmet;
    private Transform _offhand;
    private Transform _weapon;

    public Transform Weapon { get => _weapon; private set => _weapon = value; }

    void Start() {
      _equipment = GetComponent<Interface.PlayerInventory>().equipment;

      for (int i = 0; i < _equipment.GetSlots.Length; i++) {
        _equipment.GetSlots[i].onBeforeUpdated += OnRemoveItem;
        _equipment.GetSlots[i].onAfterUpdated += OnEquipItem;
      }

      // Manually instantiate equipped items after loading the equipment
      for (int i = 0; i < _equipment.GetSlots.Length; i++) {
        OnEquipItem(_equipment.GetSlots[i]);
      }
    }

    private void OnEquipItem(InventorySlot slot) {
      var itemObject = slot.GetItemObject();
      if (itemObject == null)
        return;
      switch (slot.parent.inventory.type) {
        case InterfaceType.Equipment:

          if (itemObject.CharacterDisplay != null) {
            switch (slot.AllowedItems[0]) {
              case ItemType.Tool:
              case ItemType.Weapon:
                Weapon = Instantiate(itemObject.CharacterDisplay, weaponTransform).transform;
                break;
            }
          }

          break;
      }
    }

    private void OnRemoveItem(InventorySlot slot) {
      if (slot.GetItemObject() == null) {
        return;
      }

      switch (slot.parent.inventory.type) {
        case InterfaceType.Equipment:
          if (slot.GetItemObject().CharacterDisplay != null) {
            switch (slot.AllowedItems[0]) {
              case ItemType.Shield:
                Destroy(_offhand.gameObject);
                break;

              case ItemType.Tool:
              case ItemType.Weapon:
                Destroy(Weapon.gameObject);
                break;
            }
          }

          break;
      }
    }
  }
}
