using System;
using System.Collections.Generic;
using Inventory;
using Scriptables.Inventory;
using UnityEngine;
using UnityEngine.Rendering;
using UnityServiceLocator;

namespace Craft {
  public class TotalAmount : MonoBehaviour, ITotalAmount {
    [SerializeField] private List<InventoryObject> checkInventories = new();

    private SerializedDictionary<string, int> resourcesTotal = new();
    public event Action<string> onResourcesTotalUpdate;

    public void Awake() {
      Debug.Log("Craft TotalAmount Awake");
      ServiceLocator.For(this).Register<ITotalAmount>(this);
    }

    public void InitComponent() {
      CalculateResourcesTotal();
      AddEvents();
    }

    public void ClearComponent() {
      RemoveEvents();
      resourcesTotal.Clear();
    }

    public int GetResourceTotalAmount(string resourceId) {
      return resourcesTotal.GetValueOrDefault(resourceId, 0);
    }

    private void CalculateResourcesTotal() {
      Debug.Log("Craft TotalAmount CalculateResourcesTotal");
      resourcesTotal.Clear();

      foreach (var inventory in checkInventories) {
        foreach (var inventorySlot in inventory.GetSlots) {
          var item = inventorySlot.Item;

          if (item.isEmpty) {
            continue;
          }

          UpdateResourceTotal(item.info.Id, inventorySlot.amount, false);
        }
      }
    }

    private void AddEvents() {
      if (checkInventories.Count == 0) {
        Debug.LogWarning("Craft TotalAmount: No inventories to check");
        return;
      }

      foreach (var inventory in checkInventories) {
        foreach (var inventorySlot in inventory.GetSlots) {
          inventorySlot.OnAfterUpdated += SlotAmountUpdateHandler;
        }
      }
    }

    private void RemoveEvents() {
      if (checkInventories.Count == 0) {
        Debug.LogWarning("Craft TotalAmount: No inventories to check");
        return;
      }

      foreach (var inventory in checkInventories) {
        foreach (var inventorySlot in inventory.GetSlots) {
          inventorySlot.OnAfterUpdated -= SlotAmountUpdateHandler;
        }
      }
    }

    private void SlotAmountUpdateHandler(InventorySlot slotBefore, InventorySlot slotAfter) {
      var itemBefore = slotBefore.Item.info;
      var itemAfter = slotAfter.Item.info;

      Debug.LogWarning(
        $"SlotAmountUpdateHandler slotBefore Inventory {slotBefore.Item.InventoryType}, amount {slotBefore.amount}");
      Debug.LogWarning(
        $"SlotAmountUpdateHandler slotAfter Inventory {slotAfter.Item.InventoryType}, amount {slotAfter.amount}");

      if (itemBefore == null && itemAfter == null) {
        return;
      }

      if (itemBefore == itemAfter) {
        // Same item, update the amount difference
        UpdateResourceTotal(itemAfter.Id, slotAfter.amount - slotBefore.amount);
      }
      else {
        // Remove old item amount (if any)
        if (itemBefore != null) {
          UpdateResourceTotal(itemBefore.Id, -slotBefore.amount);
        }

        // Add new item amount (if any)
        if (itemAfter != null) {
          UpdateResourceTotal(itemAfter.Id, slotAfter.amount);
        }
      }
    }

    private void UpdateResourceTotal(string resourceId, int amount, bool triggerEvent = true) {
      if (resourcesTotal.ContainsKey(resourceId)) {
        resourcesTotal[resourceId] += amount;

        if (resourcesTotal[resourceId] <= 0) {
          resourcesTotal.Remove(resourceId);
        }
      }
      else if (amount > 0) {
        resourcesTotal[resourceId] = amount;
      }

      if (triggerEvent) {
        onResourcesTotalUpdate?.Invoke(resourceId);
      }
    }
  }
}