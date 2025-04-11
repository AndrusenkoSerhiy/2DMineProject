using System;
using System.Collections;
using System.Collections.Generic;
using Windows;
using Inventory;
using Scriptables.Repair;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Repair {
  public class RepairWindow : MonoBehaviour {
    [SerializeField] private UserInterface resourcesInterface;
    [SerializeField] private Button repairButton;
    [SerializeField] private List<RepairResourceSlot> items;
    [SerializeField] private Color blinkBgColor;
    [SerializeField] private float blinkTime = 1.5f;

    private Inventory.Inventory resourcesInventory;
    private RobotObject robotObject;
    private Coroutine blinkCoroutine;
    private List<int> blinkItems;
    public event Action OnRepaired;

    public void Setup(RobotObject settings) {
      robotObject = settings;
      resourcesInventory =
        GameManager.Instance.PlayerInventory.GetInventoryByTypeAndId(robotObject.InventoryType, robotObject.Id);
    }

    private void Awake() {
      DisableRepairButton();
      resourcesInterface.Setup(robotObject.InventoryType, robotObject.Id);
      InitItems();
      blinkItems = new List<int>();
    }

    private void Start() {
      PrepareResourcesInterface();
    }

    private void OnEnable() {
      CheckSlots();
      StartBlink();

      repairButton.onClick.AddListener(OnRepairButtonClickHandler);
      AddSlotsEvents();
      GameManager.Instance.UserInput.controls.UI.Craft.performed += ClickOnKeyboard;
    }

    private void ClickOnKeyboard(InputAction.CallbackContext obj) {
      if (blinkItems.Count <= 0) OnRepairButtonClickHandler();
    }

    private void OnDisable() {
      ClearBlinkEffect();

      RemoveSlotsEvents();
      repairButton.onClick.RemoveAllListeners();
      if (GameManager.HasInstance) {
        GameManager.Instance.UserInput.controls.UI.Craft.performed -= ClickOnKeyboard;
      }
    }

    private void AddSlotsEvents() {
      foreach (var slot in resourcesInventory.Slots) {
        slot.OnAfterUpdated += OnSlotUpdatedHandler;
      }
    }

    private void EnableRepairButton() {
      repairButton.enabled = true;
    }

    private void DisableRepairButton() {
      repairButton.enabled = false;
    }

    private void OnRepairButtonClickHandler() {
      DisableRepairButton();
      gameObject.GetComponent<WindowBase>().Hide();

      OnRepaired?.Invoke();
    }

    private void RemoveSlotsEvents() {
      foreach (var slot in resourcesInventory.Slots) {
        slot.OnAfterUpdated -= OnSlotUpdatedHandler;
      }
    }

    private void OnSlotUpdatedHandler(SlotUpdateEventData data) {
      var slotIndex = data.after.index;
      var item = items[slotIndex];
      if (item.IsEnough() && blinkItems.Contains(slotIndex)) {
        blinkItems.Remove(slotIndex);
        item.ResetBackgroundColor();
      }
      else if (!blinkItems.Contains(slotIndex)) {
        blinkItems.Add(slotIndex);
      }

      if (blinkItems.Count > 0) {
        StartBlink();
        DisableRepairButton();
      }
      else {
        ClearBlinkEffect();
        EnableRepairButton();
      }
    }

    private void CheckSlots() {
      for (var i = 0; i < items.Count; i++) {
        var item = items[i];
        if (item.IsEnough() || blinkItems.Contains(i)) {
          continue;
        }

        blinkItems.Add(i);
      }
    }

    private void InitItems() {
      for (var i = 0; i < items.Count; i++) {
        var item = items[i];
        var count = robotObject.RepairResourcesAmount[i];
        item.Init(count, resourcesInventory.Slots[i]);
      }
    }

    private void PrepareResourcesInterface() {
      var slots = resourcesInventory.Slots;

      if (robotObject.RepairResources.Count != slots.Length) {
        Debug.LogError("Repair resources count doesn't match inventory slots count.");
        return;
      }

      for (var i = 0; i < slots.Length; i++) {
        var slot = slots[i];
        var allowedItem = robotObject.RepairResources[i];
        var requiredAmount = robotObject.RepairResourcesAmount[i];
        slot.SlotDisplay.AllowedItems.Add(allowedItem);
        slot.SlotDisplay.MaxAllowedAmount = requiredAmount;
        slot.SlotDisplay.EmptySlotIcon = allowedItem.UiDisplay;
      }

      resourcesInterface.UpdateInventoryUI();
    }

    private void StartBlink() {
      if (blinkItems.Count == 0 || blinkCoroutine != null) {
        return;
      }

      blinkCoroutine = StartCoroutine(BlinkBackgroundColor());
    }

    private void ResetBackgroundColor() {
      foreach (var index in blinkItems) {
        items[index].ResetBackgroundColor();
      }
    }

    private void Blink() {
      foreach (var index in blinkItems) {
        items[index].Blink(blinkBgColor);
      }
    }

    private void ClearBlinkEffect() {
      if (blinkCoroutine != null) {
        StopCoroutine(blinkCoroutine);
        blinkCoroutine = null;
      }

      ResetBackgroundColor();
    }

    private IEnumerator BlinkBackgroundColor() {
      while (blinkItems.Count > 0) {
        Blink();
        yield return new WaitForSeconds(blinkTime);
        ResetBackgroundColor();
        yield return new WaitForSeconds(blinkTime);
      }
    }
  }
}