using System.Collections.Generic;
using UnityEngine;

namespace Scriptables.POI {
  [CreateAssetMenu(menuName = "Create POIDataLibrary", fileName = "POIDataLibrary", order = 0)]
  public class POIDataLibrary : ScriptableObject {
    public int POICountForGeneration = 100;
    public int RadiusX = 15;
    public int RadiusY = 10;
    public List<POIData> POIDataList = new();
  }
}