using System.Collections.Generic;
using UnityEngine;

namespace Scriptables.POI {
  [CreateAssetMenu(menuName = "Create POIDataLibrary", fileName = "POIDataLibrary", order = 0)]
  public class POIDataLibrary : ScriptableObject {
    public int RadiusBetweenPoints = 50;
    public List<POIData> POIDataList = new();
  }
}