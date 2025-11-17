using System;
using System.Collections.Generic;
using Scriptables.Craft;
using Scriptables.Items;
using UnityEngine;

namespace World {
  public class CellHolderHandler {
    private List<CellData> baseCells = new();
    private readonly Dictionary<CellData, Action> cellDestroyedHandlers = new();
    private readonly Action onAllDestroyed;
    private readonly Recipe recipe;
    private Vector3 spawnPosition;
    private bool canDestroyCellsBelow;

    public CellHolderHandler(Action onAllDestroyed, Recipe recipe, Vector3 spawnPosition) {
      this.onAllDestroyed = onAllDestroyed;
      this.recipe = recipe;
      this.spawnPosition = spawnPosition;
    }

    public void SetBaseCells(List<CellData> cells, Vector3 position, bool canDestroyCellsBelow) {
      spawnPosition = position;
      this.canDestroyCellsBelow = canDestroyCellsBelow;
      ClearBaseCells();

      baseCells = cells;
      //subscribe to destroy cells
      if (canDestroyCellsBelow) {
        foreach (var cell in cells) {
          Action handler = () => OnBaseCellDestroyedHandler(cell);
          cellDestroyedHandlers[cell] = handler;
          cell.OnDestroyed += handler;
        }
      }
      else {
        //just lock cells from the damage (for example stoneCutter)
        foreach (var cellData in cells) {
          var cell = GameManager.Instance.ChunkController.GetCell(cellData.x, cellData.y);
          if (cell != null) {
            GameManager.Instance.ChunkController.GetCell(cellData.x, cellData.y).CanGetDamage = false;
          }
        }
      }
    }

    public void ClearBaseCells() {
      foreach (var kvp in cellDestroyedHandlers) {
        kvp.Key.OnDestroyed -= kvp.Value;
      }

      if (!canDestroyCellsBelow) {
        foreach (var cellData in baseCells) {
          var cell = GameManager.Instance.ChunkController.GetCell(cellData.x, cellData.y);
          if (cell != null) {
            GameManager.Instance.ChunkController.GetCell(cellData.x, cellData.y).CanGetDamage = true;
          }
        } 
      }

      cellDestroyedHandlers.Clear();
      baseCells.Clear();
    }

    private void OnBaseCellDestroyedHandler(CellData cell) {
      if (baseCells == null || baseCells.Count == 0) return;

      if (cellDestroyedHandlers.TryGetValue(cell, out var handler)) {
        cell.OnDestroyed -= handler;
        cellDestroyedHandlers.Remove(cell);
      }

      baseCells.Remove(cell);

      if (baseCells.Count != 0) {
        return;
      }

      onAllDestroyed?.Invoke();

      //drop materials that were used for crafting station
      if (recipe == null || recipe.RequiredMaterials.Count <= 0) {
        return;
      }

      foreach (var material in recipe.RequiredMaterials) {
        var item = new Item(material.Material);
        GameManager.Instance.PlayerInventory.SpawnItem(item, material.Amount, spawnPosition);
      }
    }
  }
}