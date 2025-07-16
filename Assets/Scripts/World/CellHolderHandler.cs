using System;
using System.Collections.Generic;
using Scriptables.Craft;
using Scriptables.Items;
using UnityEngine;

namespace World {
  public class CellHolderHandler {
    private List<CellObject> baseCells = new();
    private readonly Dictionary<CellObject, Action> cellDestroyedHandlers = new();
    private readonly Action onAllDestroyed;
    private readonly Recipe recipe;
    private Vector3 spawnPosition;

    public CellHolderHandler(Action onAllDestroyed, Recipe recipe, Vector3 spawnPosition) {
      this.onAllDestroyed = onAllDestroyed;
      this.recipe = recipe;
      this.spawnPosition = spawnPosition;
    }

    public void SetBaseCells(List<CellObject> cells, Vector3 position) {
      this.spawnPosition = position;

      ClearBaseCells();

      baseCells = cells;

      foreach (var cell in cells) {
        Action handler = () => OnBaseCellDestroyedHandler(cell);
        cellDestroyedHandlers[cell] = handler;
        cell.OnDestroyed += handler;
      }
    }

    public void ClearBaseCells() {
      foreach (var kvp in cellDestroyedHandlers) {
        kvp.Key.OnDestroyed -= kvp.Value;
      }

      cellDestroyedHandlers.Clear();
      baseCells.Clear();
    }

    private void OnBaseCellDestroyedHandler(CellObject cell) {
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