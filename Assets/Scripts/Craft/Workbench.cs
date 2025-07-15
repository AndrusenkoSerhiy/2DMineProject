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

    public void SetBaseCells(List<CellObject> cells) {
      baseCells.Clear();

      if (cells == null || cells.Count == 0) {
        return;
      }

      baseCells = cells;

      foreach (var cell in cells) {
        cell.OnDestroyed += () => OnBaseCellDestroyedHandler(cell);
      }
    }

    private void OnBaseCellDestroyedHandler(CellObject cell) {
      if (baseCells == null || baseCells.Count == 0) {
        return;
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