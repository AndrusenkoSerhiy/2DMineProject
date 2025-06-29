using System.Collections.Generic;
using Scriptables.POI;
using UnityEditor;
using UnityEngine;

namespace World {
  public class POISaver : MonoBehaviour {
    public string Name;
    public int SizeX;
    public int SizeY;
    public List<POICellObject> targetCellObjects = new();

#if UNITY_EDITOR
    [ContextMenu("Create POIData")]
    private void SavePOIData() {
      var poiData = ScriptableObject.CreateInstance<POIData>();
      poiData.name = Name;
      poiData.SizeX = SizeX;
      poiData.SizeY = SizeY;
      for (int i = 0; i < targetCellObjects.Count; i++) {
        var poiCell = new POICell() {
          localX = Mathf.RoundToInt(targetCellObjects[i].transform.position.x/3.44f),
          localY = SizeY - 1 - Mathf.RoundToInt(targetCellObjects[i].transform.position.y/3.44f),
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
#endif
  }
}