using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scriptables.POI {
  [Serializable]
  public class POIData : ScriptableObject {
    [Header("POI Spawn Data")] 
    public int maxCount;
    [Header("POI Cells Data")]
    public int sizeX;
    public int sizeY;
    public List<POICell> cells = new();
  }
}