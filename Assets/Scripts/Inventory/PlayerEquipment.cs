using System;
using System.Collections;
using Interaction;
using Player;
using SaveSystem;
using Scriptables.Items;
using Stats;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Inventory {
  public class PlayerEquipment : MonoBehaviour, ISaveLoad {
    [SerializeField] private UserInterface userInterface;
    [SerializeField] private Transform itemInHand;
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private Transform leftHandTransform;
    [SerializeField] private Transform rightHandTransform;
    [SerializeField] private string repairItemText;
    [SerializeField] private InteractionPrompt reloadInteractionPromtUI;
    [SerializeField] private string reloadText;
    [SerializeField] private Ammo ammoUI;

    private GameManager gameManager;
    private PlayerController playerController;
    private PlayerInventory playerInventory;
    private Item equippedItem;
    private string reloadButtonName;
    private bool isReloading = false;

    public Transform ItemInHand {
      get => itemInHand;
      private set => itemInHand = value;
    }

    public Item EquippedItem => equippedItem;

    public event Action OnEquippedWeapon;
    public event Action OnUnequippedWeapon;

    #region Save/Load

    public int Priority => LoadPriority.EQUIPMENT;

    public void Save() {
    }

    public void Clear() {
    }

    public void Load() {
      var equipment = GetPlayerInventory().GetEquipment();
      foreach (var slot in equipment.Slots) {
        if (slot.isEmpty) {
          continue;
        }

        GetPlayerController().PlayerStats.Mediator.ApplyModifiers(ApplyType.Equip, slot.Item.info);
      }
    }

    #endregion

    private void Awake() {
      SaveLoadSystem.Instance.Register(this);
      gameManager = GameManager.Instance;
    }

    private void Start() {
      userInterface.OnLoaded += AddEvents;
      userInterface.OnDisabled += RemoveEvents;

      reloadButtonName = ButtonPromptSprite.GetSpriteName(gameManager.UserInput.controls.UI.Reload);
    }

    private void OnDisable() {
      userInterface.OnLoaded -= AddEvents;
      userInterface.OnDisabled -= RemoveEvents;
    }

    private PlayerController GetPlayerController() {
      if (!playerController) {
        playerController = gameManager.PlayerController;
      }

      return playerController;
    }

    private PlayerInventory GetPlayerInventory() {
      if (!playerInventory) {
        playerInventory = gameManager.PlayerInventory;
      }

      return playerInventory;
    }

    public void EquipTool(Item item) {
      if (item == null || item.isEmpty) {
        return;
      }

      var itemObject = item.info;

      if (!item.IsBroken) {
        PlaceItemInHand(item);
      }

      equippedItem = item;

      if (itemObject.Type != ItemType.Tool)
        return;

      AddWeaponEvents();
      CheckIfReloadNeeded();
      CheckIfNeedToShowAmmoUI();
    }

    public void UnEquipTool() {
      if (equippedItem == null) {
        return;
      }

      RemoveItemFromHand();

      RemoveWeaponEvents();
      equippedItem = null;
      CheckIfReloadNeeded();
      CheckIfNeedToShowAmmoUI();
    }

    private void RemoveItemFromHand() {
      if (!itemInHand) {
        return;
      }

      var itemObject = equippedItem.info;
      Destroy(itemInHand.gameObject);

      if (itemObject.Type != ItemType.Tool)
        return;

      GetPlayerController().PlayerStats.Mediator.RemoveModifiersByItemId(itemObject.Id);
      OnUnequippedWeapon?.Invoke();
    }

    private void PlaceItemInHand(Item item) {
      var itemObject = item.info;
      itemInHand = Instantiate(itemObject.CharacterDisplay, GetParent(itemObject)).transform;
      itemInHand.localPosition = itemObject.SpawnPosition;
      itemInHand.localEulerAngles = itemObject.SpawnRotation;
      itemInHand.gameObject.layer = LayerMask.NameToLayer("Character");

      if (item.info.Type != ItemType.Tool)
        return;

      GetPlayerController().PlayerStats.Mediator.ApplyModifiers(ApplyType.Equip, itemObject);
      OnEquippedWeapon?.Invoke();
    }

    private void AddWeaponEvents() {
      if (equippedItem == null) {
        return;
      }

      equippedItem.OnAmmoUsed += OnAmmoUsedHandler;
      equippedItem.OnItemBroken += OnItemBrokenHandler;
      equippedItem.OnItemRepaired += OnItemRepairedHandler;
    }

    private void RemoveWeaponEvents() {
      if (equippedItem == null) {
        return;
      }

      equippedItem.OnAmmoUsed -= OnAmmoUsedHandler;
      equippedItem.OnItemBroken -= OnItemBrokenHandler;
      equippedItem.OnItemRepaired -= OnItemRepairedHandler;
    }

    private void OnAmmoUsedHandler() {
      CheckIfReloadNeeded();
    }

    private void OnItemBrokenHandler() {
      RemoveItemFromHand();
    }

    private void OnItemRepairedHandler() {
      if (itemInHand != null) {
        return;
      }

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
        GetPlayerController().PlayerStats.Mediator.RemoveModifiersByItemId(data.before.Item.id);
      }
      else {
        GetPlayerController().PlayerStats.Mediator.ApplyModifiers(ApplyType.Equip, data.after.Item.info);
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
        GetPlayerInventory().Repair(equippedItem.MaxDurability, equippedItem.Durability, equippedItem.RepairCost);

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

    public void ConsumeAmmo() {
      equippedItem.ApplyDurabilityLoss(false);
      equippedItem.ConsumeAmmo();
      ammoUI.UpdateCount(equippedItem.CurrentAmmoCount);

      if ((equippedItem.CurrentAmmoCount + 1) == equippedItem.MagazineSize) {
        CheckIfReloadNeeded();
      }
    }

    private bool EquippedItemCanBeRepaired() {
      return equippedItem is { CanBeRepaired: true };
    }

    private void CheckIfNeedToShowAmmoUI() {
      if (equippedItem == null || equippedItem.IsBroken || equippedItem.MagazineSize <= 0) {
        ammoUI.Hide();
        return;
      }

      ammoUI.Show(equippedItem.GetAmmoItemObject(), equippedItem.CurrentAmmoCount, equippedItem.MagazineSize);
    }

    private void CheckIfReloadNeeded() {
      if (equippedItem == null || equippedItem.IsBroken || !equippedItem.ReloadNeeded()) {
        HideReloadPrompt();
        UnsubscribeToChangeBlockType();
        return;
      }

      ShowReloadPrompt();
      SubscribeToChangeBlockType();
    }

    private void ReloadHandler(InputAction.CallbackContext obj) {
      if (isReloading || equippedItem == null) {
        return;
      }

      StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine() {
      isReloading = true;

      var ammo = equippedItem.GetAmmoItemObject();
      var ammoAmount = GetPlayerInventory().InventoriesPool.GetResourceTotalAmount(ammo.Id);

      if (ammoAmount <= 0) {
        gameManager.MessagesManager.ShowSimpleMessage("You don't have ammo.");
        isReloading = false;
        yield break;
      }

      gameManager.AudioController.PlayAudio(equippedItem.ReloadSound);

      yield return new WaitForSeconds(equippedItem.ReloadTime);

      var ammoNeeded = equippedItem.MagazineSize - equippedItem.CurrentAmmoCount;
      var reloadAmount = ammoAmount > ammoNeeded ? ammoNeeded : ammoAmount;

      equippedItem.Reload(reloadAmount);
      GetPlayerInventory().InventoriesPool.RemoveFromInventoriesPool(ammo.Id, reloadAmount);
      ammoUI.UpdateCount(equippedItem.CurrentAmmoCount, ammoAmount - reloadAmount);
      CheckIfReloadNeeded();

      isReloading = false;
    }

    private void ShowReloadPrompt() {
      reloadInteractionPromtUI.ShowPrompt(true, ButtonPromptSprite.GetFullPrompt(reloadText, reloadButtonName));
    }

    private void HideReloadPrompt() {
      reloadInteractionPromtUI.ShowPrompt(false);
    }

    private void SubscribeToChangeBlockType() {
      gameManager.UserInput.controls.UI.Reload.performed += ReloadHandler;
    }

    private void UnsubscribeToChangeBlockType() {
      gameManager.UserInput.controls.UI.Reload.performed -= ReloadHandler;
    }
  }
}