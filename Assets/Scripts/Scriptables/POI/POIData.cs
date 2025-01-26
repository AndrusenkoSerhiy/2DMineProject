using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scriptables.POI {
  [Serializable]
  public class POIData : ScriptableObject {
    [Header("POI Spawn Data")] public int maxCount;
    [Header("POI Cells Data")] public int sizeX;
    public int sizeY;
    private POICell[,] cells;
    public POICell[,] Cells => cells;

    public void CreateCells() {
      cells = new POICell[sizeX, sizeY];
    }

    public void SetCell(POICell cell) {
      cells[cell.localX, cell.localY] = cell;
    }
  }
}