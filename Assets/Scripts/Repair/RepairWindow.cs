using System.Collections;
using System.Collections.Generic;
using Windows;
using Inventory;
using Scriptables.Repair;
using UnityEngine;
using UnityEngine.UI;
using World;

namespace Repair {
  public class RepairWindow : MonoBehaviour {
    [SerializeField] private UserInterface resourcesInterface;
    [SerializeField] private Button repairButton;
    [SerializeField] private List<RepairResourceSlot> items;
    [SerializeField] private Color blinkBgColor;
    [SerializeField] private float blinkTime = 1.5f;

    private RobotRepair robotRepair;
    private Coroutine blinkCoroutine;
    private List<int> blinkItems;
    private bool repaired;

    public bool Repaired => repaired;

    public void Setup(CellObject cellObject, RobotRepairObject repairSettings) {
      robotRepair = new RobotRepair(cellObject, repairSettings);
    }

    private void Awake() {
      DisableRepairButton();
      resourcesInterface.Setup(robotRepair.RobotRepairObject.InventoryType, robotRepair.Id);
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
    }

    private void OnDisable() {
      ClearBlinkEffect();

      RemoveSlotsEvents();
      repairButton.onClick.RemoveAllListeners();
    }

    private void AddSlotsEvents() {
      foreach (var slot in robotRepair.ResourcesInventory.Slots) {
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
      repaired = true;
      gameObject.GetComponent<WindowBase>().Hide();

      Instantiate(robotRepair.RobotRepairObject.RobotPrefab,
        GameManager.Instance.PlayerController.transform.position + new Vector3(6, 8, 0), Quaternion.identity);
    }

    private void RemoveSlotsEvents() {
      foreach (var slot in robotRepair.ResourcesInventory.Slots) {
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
        if (item.IsEnough()) {
          continue;
        }

        blinkItems.Add(i);
      }
    }

    private void InitItems() {
      for (var i = 0; i < items.Count; i++) {
        var item = items[i];
        var count = robotRepair.RobotRepairObject.RepairResourcesAmount[i];
        item.Init(count, robotRepair.ResourcesInventory.Slots[i]);
      }
    }

    private void PrepareResourcesInterface() {
      var slots = robotRepair.ResourcesInventory.Slots;

      if (robotRepair.RobotRepairObject.RepairResources.Count != slots.Length) {
        Debug.LogError("Repair resources count doesn't match inventory slots count.");
        return;
      }

      for (var i = 0; i < slots.Length; i++) {
        var slot = slots[i];
        var allowedItem = robotRepair.RobotRepairObject.RepairResources[i];
        var requiredAmount = robotRepair.RobotRepairObject.RepairResourcesAmount[i];
        slot.AllowedItem = allowedItem;
        slot.MaxAllowedAmount = requiredAmount;
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