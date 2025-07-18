using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scriptables.POI {
  [Serializable]
  public class POIData : ScriptableObject {
    [Header("POI Spawn Data")] public int minCount = 100;
    public int SizeX;
    public int SizeY;
    [SerializeField] private List<POICell> cells = new();
    public List<POICell> Cells => cells;

    public void SetCell(POICell cell) {
      cells.Add(cell);
    }
  }
}