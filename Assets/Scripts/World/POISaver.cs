using System.Collections.Generic;
using UnityEngine;

namespace World {
  public class POISaver : MonoBehaviour {
    public int sizeX;
    public int sizeY;
    public List<CellObject> targetCellObjects= new ();

    [ContextMenu("Create POIData")]
    private void SavePOIData() {
      
    }
  }
}