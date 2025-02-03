using System.Collections.Generic;
using System.Linq;
using Scriptables.POI;
using UnityEditor;
using UnityEngine;

namespace World {
  public class POISaver : MonoBehaviour {
    public string Name;
    public List<POICellObject> targetCellObjects = new();

    [ContextMenu("Create POIData")]
    private void SavePOIData() {
      var poiData = ScriptableObject.CreateInstance<POIData>();
      poiData.name = Name;
      for (int i = 0; i < targetCellObjects.Count; i++) {
        var poiCell = new POICell() {
          localX = targetCellObjects[i].LocalX,
          localY = targetCellObjects[i].LocalY,
          resourceData = targetCellObjects[i].CellObject.resourceData
        };
        poiData.SetCell(poiCell);
      }

      string path = "Assets/ScriptableObjects/POIData/" + Name + ".asset";
      AssetDatabase.CreateAsset(poiData, path);

      // Save and refresh the AssetDatabase
      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();
    }
  }
}