using System.Collections.Generic;
using Scriptables.POI;
using UnityEditor;
using UnityEngine;

namespace World {
  public class POISaver : MonoBehaviour {
    public string Name;
    public int sizeX;
    public int sizeY;
    public List<CellObject> targetCellObjects= new ();

    [ContextMenu("Create POIData")]
    private void SavePOIData() {
      var poiData = ScriptableObject.CreateInstance<POIData>();
      poiData.name = Name;
      poiData.sizeX = sizeX;
      poiData.sizeY = sizeY;
      poiData.CreateCells();
      int listIndex = 0;
      for (int i = 0; i < sizeX; i++) {
        for (int j = 0; j < sizeY; j++) {
          //todo add step check position
          //if(div stepX div StepY)
          var poiCell = new POICell() {
            localX = i,
            localY = j,
            resourceData = targetCellObjects[listIndex].resourceData
          };
          poiData.SetCell(poiCell);
          listIndex++;
        }
      }
      string path = "Assets/ScriptableObjects/POIData/"+Name+".asset";
      AssetDatabase.CreateAsset(poiData, path);

      // Save and refresh the AssetDatabase
      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();
    }
  }
}