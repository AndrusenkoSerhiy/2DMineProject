using System;
using System.Collections.Generic;
using Interaction;
using UnityEngine;
using World;

namespace Craft {
  public class Workbench : Crafter, IInteractable, IBaseCellHolder {
    [SerializeField] private string interactText;
    [SerializeField] private string holdInteractText;
    [SerializeField] private bool hasHoldInteraction = true;

    private List<CellObject> baseCells = new();
    private readonly Dictionary<CellObject, Action> cellDestroyedHandlers = new();

    public string InteractionText => interactText;
    public bool HasHoldInteraction => hasHoldInteraction;
    public string HoldInteractionText => holdInteractText;

    public bool Interact(PlayerInteractor playerInteractor) {
      CheckInteract();

      return true;
    }

    public bool HoldInteract(PlayerInteractor playerInteractor) {
      CheckHoldInteract();

      return true;
    }

    public void ClearBaseCells() {
      foreach (var kvp in cellDestroyedHandlers) {
        kvp.Key.OnDestroyed -= kvp.Value;
      }

      cellDestroyedHandlers.Clear();
      baseCells.Clear();
    }

    public void SetBaseCells(List<CellObject> cells) {
      ClearBaseCells();

      baseCells = cells;

      foreach (var cell in cells) {
        Action handler = () => OnBaseCellDestroyedHandler(cell);
        cellDestroyedHandlers[cell] = handler;
        cell.OnDestroyed += handler;
      }
    }

    private void OnBaseCellDestroyedHandler(CellObject cell) {
      if (baseCells == null || baseCells.Count == 0) {
        return;
      }

      if (cellDestroyedHandlers.TryGetValue(cell, out var handler)) {
        cell.OnDestroyed -= handler;
        cellDestroyedHandlers.Remove(cell);
      }

      baseCells.Remove(cell);

      if (baseCells.Count > 0) {
        return;
      }

      station?.StopAndDropItems(transform.position);

      gameManager.PlaceCell.RemoveBuilding(buildObject, stationObject.InventoryItem);

      gameManager.MessagesManager.ShowSimpleMessage(stationObject.Title + " destroyed");

      gameManager.PoolEffects.SpawnFromPool("PlaceCellEffect", transform.position, Quaternion.identity);
      gameManager.AudioController.PlayWorkstationDestroyed();
    }
  }
}